using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 自动使用药水系统 - 根据阈值自动使用背包中的药水
    /// </summary>
    public partial class AutoPotionSystem : Node
    {
        public static AutoPotionSystem Instance { get; private set; }

        // 自动使用设置
        public bool AutoUseHealthPotion { get; set; } = true;
        public bool AutoUseManaPotion { get; set; } = true;
        public bool AutoUseBuffPotions { get; set; } = false;

        // 阈值设置 (0-100)
        public int HealthPotionThreshold { get; set; } = 30;  // 30%血量时自动使用
        public int ManaPotionThreshold { get; set; } = 30;    // 30%魔法时自动使用

        // 冷却时间
        private float _healthPotionCooldown = 0f;
        private float _manaPotionCooldown = 0f;
        private float _buffPotionCooldown = 0f;
        private const float POTION_COOLDOWN = 2f;  // 2秒冷却
        private const float BUFF_COOLDOWN = 30f;   // 30秒buff药水冷却

        // 药水ID范围
        private const int HEALTH_POTION_MIN = 201;
        private const int HEALTH_POTION_MAX = 203;
        private const int MANA_POTION_MIN = 204;
        private const int MANA_POTION_MAX = 206;
        private const int BUFF_POTION_MIN = 207;
        private const int BUFF_POTION_MAX = 212;

        // 信号
        [Signal] public delegate void AutoPotionUsedEventHandler(string potionType, int itemId);
        [Signal] public delegate void AutoPotionSettingsChangedEventHandler();

        public override void _Ready()
        {
            Instance = this;
        }

        public override void _Process(float delta)
        {
            // 更新冷却
            if (_healthPotionCooldown > 0) _healthPotionCooldown -= delta;
            if (_manaPotionCooldown > 0) _manaPotionCooldown -= delta;
            if (_buffPotionCooldown > 0) _buffPotionCooldown -= delta;

            // 获取玩家
            var player = GetPlayer();
            if (player == null) return;

            // 检查是否需要自动使用药水
            CheckAutoUsePotions(player);
        }

        private Character GetPlayer()
        {
            var player = GetTree().GetFirstNodeInGroup("Player");
            return player as Character;
        }

        private void CheckAutoUsePotions(Character player)
        {
            // 获取玩家当前血量/魔法百分比
            float healthPercent = (float)player.Health / player.MaxHealth * 100f;
            float manaPercent = player is Characters.Player p ? (float)p.Mana / p.MaxMana * 100f : 100f;

            // 自动使用生命药水
            if (AutoUseHealthPotion && _healthPotionCooldown <= 0 && healthPercent <= HealthPotionThreshold)
            {
                TryUseHealthPotion(player);
            }

            // 自动使用魔法药水
            if (AutoUseManaPotion && _manaPotionCooldown <= 0 && manaPercent <= ManaPotionThreshold)
            {
                TryUseManaPotion(player);
            }

            // 自动使用增益药水
            if (AutoUseBuffPotions && _buffPotionCooldown <= 0)
            {
                TryUseBuffPotion(player);
            }
        }

        private void TryUseHealthPotion(Character player)
        {
            var itemSystem = Items.ItemSystem.Instance;
            if (itemSystem == null) return;

            // 查找背包中的生命药水
            int bestPotionId = -1;
            int bestHealAmount = 0;

            for (int itemId = HEALTH_POTION_MIN; itemId <= HEALTH_POTION_MAX; itemId++)
            {
                int count = itemSystem.GetItemCount(itemId);
                if (count > 0)
                {
                    var item = itemSystem.GetItem(itemId);
                    if (item != null)
                    {
                        int healAmount = GetPotionHealAmount(itemId);
                        if (healAmount > bestHealAmount)
                        {
                            bestHealAmount = healAmount;
                            bestPotionId = itemId;
                        }
                    }
                }
            }

            if (bestPotionId > 0)
            {
                // 使用药水
                itemSystem.RemoveItem(bestPotionId, 1);
                
                int actualHeal = player.Heal(bestHealAmount);
                _healthPotionCooldown = POTION_COOLDOWN;
                
                EmitSignal(SignalName.AutoPotionUsed, "health", bestPotionId);
                
                // 显示消息
                ShowPotionMessage("自动使用生命药水", actualHeal);
            }
        }

        private void TryUseManaPotion(Character player)
        {
            if (player is not Characters.Player p) return;

            var itemSystem = Items.ItemSystem.Instance;
            if (itemSystem == null) return;

            // 查找背包中的魔法药水
            int bestPotionId = -1;
            int bestManaAmount = 0;

            for (int itemId = MANA_POTION_MIN; itemId <= MANA_POTION_MAX; itemId++)
            {
                int count = itemSystem.GetItemCount(itemId);
                if (count > 0)
                {
                    var item = itemSystem.GetItem(itemId);
                    if (item != null)
                    {
                        int manaAmount = GetPotionManaAmount(itemId);
                        if (manaAmount > bestManaAmount)
                        {
                            bestManaAmount = manaAmount;
                            bestPotionId = itemId;
                        }
                    }
                }
            }

            if (bestPotionId > 0)
            {
                // 使用药水
                itemSystem.RemoveItem(bestPotionId, 1);
                
                int actualMana = p.AddMana(bestManaAmount);
                _manaPotionCooldown = POTION_COOLDOWN;
                
                EmitSignal(SignalName.AutoPotionUsed, "mana", bestPotionId);
                
                // 显示消息
                ShowPotionMessage("自动使用魔法药水", actualMana);
            }
        }

        private void TryUseBuffPotion(Character player)
        {
            var itemSystem = Items.ItemSystem.Instance;
            if (itemSystem == null) return;

            // 查找背包中的增益药水
            for (int itemId = BUFF_POTION_MIN; itemId <= BUFF_POTION_MAX; itemId++)
            {
                int count = itemSystem.GetItemCount(itemId);
                if (count > 0)
                {
                    // 使用第一个可用的增益药水
                    itemSystem.RemoveItem(itemId, 1);
                    ApplyBuffPotion(player, itemId);
                    _buffPotionCooldown = BUFF_COOLDOWN;
                    
                    EmitSignal(SignalName.AutoPotionUsed, "buff", itemId);
                    
                    var item = itemSystem.GetItem(itemId);
                    ShowPotionMessage("自动使用增益药水: " + (item?.Name ?? "Unknown"), 0);
                    return;
                }
            }
        }

        private void ApplyBuffPotion(Character player, int itemId)
        {
            var skillSystem = Systems.SkillSystem.Instance;
            if (skillSystem == null) return;

            // 根据药水ID应用不同的增益效果
            switch (itemId)
            {
                case 207: // 力量药水
                    skillSystem.ApplyStatusEffect(player, StatusEffectType.Buff, 5f, 0, 0, 0, 0, 0, 20f);
                    break;
                case 208: // 防御药水
                    skillSystem.ApplyStatusEffect(player, StatusEffectType.Buff, 5f, 0, 0, 20f, 0, 0);
                    break;
                case 209: // 速度药水
                    skillSystem.ApplyStatusEffect(player, StatusEffectType.Buff, 5f, 0, 0, 0, 0, 30f);
                    break;
                case 210: // 生命上限药水
                    // 临时增加最大生命
                    break;
                case 211: // 魔法上限药水
                    // 临时增加最大魔法
                    break;
                case 212: // 全属性药水
                    skillSystem.ApplyStatusEffect(player, StatusEffectType.Buff, 5f, 10f, 10f, 10f, 0, 10f);
                    break;
            }
        }

        private int GetPotionHealAmount(int itemId)
        {
            return itemId switch
            {
                201 => 50,   // 小型生命药水
                202 => 150,  // 中型生命药水
                203 => 400,  // 大型生命药水
                _ => 0
            };
        }

        private int GetPotionManaAmount(int itemId)
        {
            return itemId switch
            {
                204 => 30,   // 小型魔法药水
                205 => 80,   // 中型魔法药水
                206 => 200,  // 大型魔法药水
                _ => 0
            };
        }

        private void ShowPotionMessage(string message, int amount)
        {
            var gameMessage = GetTree().GetFirstNodeInGroup("GameMessageSystem") as UI.GameMessageSystem;
            if (gameMessage != null)
            {
                if (amount > 0)
                {
                    gameMessage.ShowPositive(message + $" +{amount}");
                }
                else
                {
                    gameMessage.ShowInfo(message);
                }
            }
        }

        // 设置方法
        public void SetAutoHealthPotion(bool enabled)
        {
            AutoUseHealthPotion = enabled;
            EmitSignal(SignalName.AutoPotionSettingsChanged);
        }

        public void SetAutoManaPotion(bool enabled)
        {
            AutoUseManaPotion = enabled;
            EmitSignal(SignalName.AutoPotionSettingsChanged);
        }

        public void SetAutoBuffPotions(bool enabled)
        {
            AutoUseBuffPotions = enabled;
            EmitSignal(SignalName.AutoPotionSettingsChanged);
        }

        public void SetHealthThreshold(int threshold)
        {
            HealthPotionThreshold = Mathf.Clamp(threshold, 5, 95);
            EmitSignal(SignalName.AutoPotionSettingsChanged);
        }

        public void SetManaThreshold(int threshold)
        {
            ManaPotionThreshold = Mathf.Clamp(threshold, 5, 95);
            EmitSignal(SignalName.AutoPotionSettingsChanged);
        }

        // 序列化
        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "autoHealthPotion", AutoUseHealthPotion },
                { "autoManaPotion", AutoUseManaPotion },
                { "autoBuffPotions", AutoUseBuffPotions },
                { "healthThreshold", HealthPotionThreshold },
                { "manaThreshold", ManaPotionThreshold }
            };
        }

        public void Deserialize(Dictionary<string, object> data)
        {
            if (data.ContainsKey("autoHealthPotion"))
                AutoUseHealthPotion = Convert.ToBoolean(data["autoHealthPotion"]);
            if (data.ContainsKey("autoManaPotion"))
                AutoUseManaPotion = Convert.ToBoolean(data["autoManaPotion"]);
            if (data.ContainsKey("autoBuffPotions"))
                AutoUseBuffPotions = Convert.ToBoolean(data["autoBuffPotions"]);
            if (data.ContainsKey("healthThreshold"))
                HealthPotionThreshold = Convert.ToInt32(data["healthThreshold"]);
            if (data.ContainsKey("manaThreshold"))
                ManaPotionThreshold = Convert.ToInt32(data["manaThreshold"]);
        }
    }
}
