using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Items
{
    /// <summary>
    /// 药水管理器 - 管理玩家拥有的药水
    /// </summary>
    public class PotionManager
    {
        private static PotionManager _instance;
        public static PotionManager Instance => _instance ??= new PotionManager();

        // 玩家拥有的药水列表
        private List<PotionInstance> _ownedPotions = new List<PotionInstance>();
        
        // 当前激活的药水效果
        private Dictionary<int, float> _activeEffects = new Dictionary<int, float>();
        
        // 药水冷却
        private Dictionary<int, float> _cooldowns = new Dictionary<int, float>();
        
        // Tutorial tracking
        private bool _hasTriggeredFirstPotion = false;

        // 信号系统
        public Action<PotionInstance> OnPotionAdded;
        public Action<PotionInstance> OnPotionRemoved;
        public Action<Potion> OnPotionUsed;
        public Action<Potion> OnBuffActivated;
        public Action<Potion> OnBuffExpired;

        public List<PotionInstance> OwnedPotions => _ownedPotions;

        public PotionManager()
        {
            _instance = this;
        }

        /// <summary>
        /// 添加药水到背包
        /// </summary>
        public bool AddPotion(int potionId, int quantity = 1)
        {
            var potionTemplate = PotionDatabase.Instance.GetPotion(potionId);
            if (potionTemplate == null)
            {
                GD.PrintErr($"PotionManager: Invalid potion ID {potionId}");
                return false;
            }

            // 检查是否已经拥有这种药水
            foreach (var owned in _ownedPotions)
            {
                if (owned.PotionId == potionId)
                {
                    // 检查是否达到堆叠上限
                    if (owned.Quantity < potionTemplate.MaxStack)
                    {
                        int canAdd = Math.Min(quantity, potionTemplate.MaxStack - owned.Quantity);
                        owned.Quantity += canAdd;
                        OnPotionAdded?.Invoke(owned);
                        return true;
                    }
                    return false;
                }
            }

            // 新药水
            int addQuantity = Math.Min(quantity, potionTemplate.MaxStack);
            var newPotion = new PotionInstance(potionId, addQuantity);
            _ownedPotions.Add(newPotion);
            OnPotionAdded?.Invoke(newPotion);
            return true;
        }

        /// <summary>
        /// 移除药水
        /// </summary>
        public bool RemovePotion(int potionId, int quantity = 1)
        {
            for (int i = _ownedPotions.Count - 1; i >= 0; i--)
            {
                if (_ownedPotions[i].PotionId == potionId)
                {
                    _ownedPotions[i].Quantity -= quantity;
                    if (_ownedPotions[i].Quantity <= 0)
                    {
                        var removed = _ownedPotions[i];
                        _ownedPotions.RemoveAt(i);
                        OnPotionRemoved?.Invoke(removed);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 使用药水
        /// </summary>
        public bool UsePotion(int potionId, Node player)
        {
            // 检查冷却
            if (_cooldowns.ContainsKey(potionId) && _cooldowns[potionId] > 0)
            {
                GD.Print($"Potion {potionId} is on cooldown");
                return false;
            }

            // 检查是否拥有药水
            PotionInstance potionInstance = null;
            foreach (var owned in _ownedPotions)
            {
                if (owned.PotionId == potionId)
                {
                    potionInstance = owned;
                    break;
                }
            }

            if (potionInstance == null || potionInstance.Quantity <= 0)
            {
                GD.Print("No potion available");
                return false;
            }

            var potion = PotionDatabase.Instance.GetPotion(potionId);
            if (potion == null)
            {
                GD.PrintErr($"Potion template not found for ID {potionId}");
                return false;
            }

            // 应用药水效果
            ApplyPotionEffect(potion, player);

            // 设置冷却
            if (potion.Cooldown > 0)
            {
                _cooldowns[potionId] = potion.Cooldown;
            }

            // 消耗药水
            RemovePotion(potionId, 1);
            OnPotionUsed?.Invoke(potion);
            
            // Trigger tutorial for first potion use
            if (!_hasTriggeredFirstPotion)
            {
                _hasTriggeredFirstPotion = true;
                TutorialSystem.Trigger(TutorialTrigger.FirstPotion);
            }

            return true;
        }

        /// <summary>
        /// 应用药水效果
        /// </summary>
        private void ApplyPotionEffect(Potion potion, Node player)
        {
            // 直接恢复
            if (potion.HealthRestore > 0)
            {
                var playerScript = player as Characters.Player;
                if (playerScript != null)
                {
                    playerScript.Heal((int)potion.HealthRestore);
                }
            }

            if (potion.ManaRestore > 0)
            {
                var playerScript = player as Characters.Player;
                if (playerScript != null)
                {
                    playerScript.RestoreMana((int)potion.ManaRestore);
                }
            }

            // 持续效果
            if (potion.Duration > 0)
            {
                if (!_activeEffects.ContainsKey(potion.Id))
                {
                    _activeEffects[potion.Id] = potion.Duration;
                    OnBuffActivated?.Invoke(potion);
                }
                else
                {
                    // 刷新持续时间
                    _activeEffects[potion.Id] = Math.Max(_activeEffects[potion.Id], potion.Duration);
                }
            }
        }

        /// <summary>
        /// 更新药水效果（每帧调用）
        /// </summary>
        public void UpdatePotionEffects(float delta, Node player)
        {
            // 更新冷却
            foreach (var key in new List<int>(_cooldowns.Keys))
            {
                _cooldowns[key] -= delta;
                if (_cooldowns[key] <= 0)
                {
                    _cooldowns.Remove(key);
                }
            }

            // 更新持续效果
            var expiredEffects = new List<int>();
            foreach (var effect in _activeEffects)
            {
                float remainingTime = effect.Value - delta;
                if (remainingTime <= 0)
                {
                    expiredEffects.Add(effect.Key);
                }
                else
                {
                    _activeEffects[effect.Key] = remainingTime;
                    
                    // 应用周期性效果
                    var potion = PotionDatabase.Instance.GetPotion(effect.Key);
                    if (potion != null)
                    {
                        ApplyPeriodicEffect(potion, player, delta);
                    }
                }
            }

            // 处理过期效果
            foreach (var expiredId in expiredEffects)
            {
                _activeEffects.Remove(expiredId);
                var potion = PotionDatabase.Instance.GetPotion(expiredId);
                if (potion != null)
                {
                    OnBuffExpired?.Invoke(potion);
                }
            }
        }

        /// <summary>
        /// 应用周期性效果（如再生）
        /// </summary>
        private void ApplyPeriodicEffect(Potion potion, Node player, float delta)
        {
            var playerScript = player as Characters.Player;
            if (playerScript == null) return;

            if (potion.HealthRegen > 0)
            {
                playerScript.Heal((int)(potion.HealthRegen * delta));
            }

            if (potion.ManaRegen > 0)
            {
                playerScript.RestoreMana((int)(potion.ManaRegen * delta));
            }
        }

        /// <summary>
        /// 获取药水剩余冷却时间
        /// </summary>
        public float GetCooldownRemaining(int potionId)
        {
            return _cooldowns.ContainsKey(potionId) ? _cooldowns[potionId] : 0;
        }

        /// <summary>
        /// 检查是否有某种药水
        /// </summary>
        public bool HasPotion(int potionId)
        {
            foreach (var owned in _ownedPotions)
            {
                if (owned.PotionId == potionId && owned.Quantity > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取药水数量
        /// </summary>
        public int GetPotionQuantity(int potionId)
        {
            foreach (var owned in _ownedPotions)
            {
                if (owned.PotionId == potionId)
                    return owned.Quantity;
            }
            return 0;
        }

        /// <summary>
        /// 获取所有激活的药水效果
        /// </summary>
        public List<Potion> GetActiveBuffs()
        {
            List<Potion> activeBuffs = new List<Potion>();
            foreach (var effect in _activeEffects.Keys)
            {
                var potion = PotionDatabase.Instance.GetPotion(effect);
                if (potion != null && potion.Duration > 0)
                {
                    activeBuffs.Add(potion);
                }
            }
            return activeBuffs;
        }

        /// <summary>
        /// 获取激活效果的剩余时间
        /// </summary>
        public float GetBuffRemainingTime(int potionId)
        {
            return _activeEffects.ContainsKey(potionId) ? _activeEffects[potionId] : 0;
        }

        /// <summary>
        /// 设置自动使用药水
        /// </summary>
        public void SetAutoUse(int potionId, bool autoUse)
        {
            foreach (var owned in _ownedPotions)
            {
                if (owned.PotionId == potionId)
                {
                    owned.IsAutoUse = autoUse;
                    break;
                }
            }
        }

        /// <summary>
        /// 序列化存档
        /// </summary>
        public Dictionary<string, object> Serialize()
        {
            var data = new Dictionary<string, object>();
            
            var potionsData = new List<Dictionary<string, object>>();
            foreach (var potion in _ownedPotions)
            {
                potionsData.Add(new Dictionary<string, object>
                {
                    { "id", potion.PotionId },
                    { "quantity", potion.Quantity },
                    { "autoUse", potion.IsAutoUse }
                });
            }
            data["potions"] = potionsData;

            return data;
        }

        /// <summary>
        /// 反序列化存档
        /// </summary>
        public void Deserialize(Dictionary<string, object> data)
        {
            _ownedPotions.Clear();
            _activeEffects.Clear();
            _cooldowns.Clear();

            if (!data.ContainsKey("potions")) return;

            var potionsData = (List<object>)data["potions"];
            foreach (var potionData in potionsData)
            {
                var dict = (Dictionary<string, object>)potionData;
                var potion = new PotionInstance(
                    (int)dict["id"],
                    (int)dict["quantity"]
                );
                potion.IsAutoUse = (bool)dict["autoUse"];
                _ownedPotions.Add(potion);
            }
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear()
        {
            _ownedPotions.Clear();
            _activeEffects.Clear();
            _cooldowns.Clear();
        }
    }
}
