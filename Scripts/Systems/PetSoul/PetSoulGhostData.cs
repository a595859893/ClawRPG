using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetSoul
{
    /// <summary>
    /// 宠物灵魂状态枚举
    /// </summary>
    public enum SoulState
    {
        /// <summary>游荡状态 — 在 Safe House 中随机飘动</summary>
        Wandering = 0,
        /// <summary>靠近玩家状态 — 显示独白气泡</summary>
        NearPlayer = 1,
        /// <summary>互动中 — 玩家触发了交互动画</summary>
        Interacting = 2,
        /// <summary>升华状态 — 已转化为守护灵，固定在玩家身上</summary>
        Transcended = 3
    }

    /// <summary>
    /// 单个宠物灵魂数据条目
    /// </summary>
    [System.Serializable]
    public class PetSoulGhostEntry
    {
        /// <summary>宠物ID</summary>
        public int PetId;
        /// <summary>宠物名称</summary>
        public string PetName;
        /// <summary>宠物类型</summary>
        public string PetType;
        /// <summary>宠物颜色（用于灵魂着色）</summary>
        public string PetColor;
        /// <summary>宠物死亡次数</summary>
        public int DeathCount;
        /// <summary>当前灵魂状态</summary>
        public SoulState State;
        /// <summary>是否已升华</summary>
        public bool IsTranscended;
        /// <summary>升华时间戳</summary>
        public float TranscendedTimestamp;
        /// <summary>在 Safe House 中的游荡位置（相对坐标）</summary>
        public Vector2 WanderPosition;
        /// <summary>最后游荡时间</summary>
        public float LastWanderTime;
        /// <summary>最后独白时间（用于控制独白频率）</summary>
        public float LastMonologueTime;
        /// <summary>最后互动时间（冷却控制）</summary>
        public float LastInteractionTime;
        /// <summary>友谊等级（用于升华阈值计算）</summary>
        public int FriendshipLevel;
        /// <summary>首次死亡时间戳</summary>
        public float FirstDeathTimestamp;

        public PetSoulGhostEntry()
        {
            PetId = 0;
            PetName = "";
            PetType = "";
            PetColor = "#FFFFFF";
            DeathCount = 0;
            State = SoulState.Wandering;
            IsTranscended = false;
            TranscendedTimestamp = 0f;
            WanderPosition = new Vector2(0, 0);
            LastWanderTime = 0f;
            LastMonologueTime = 0f;
            LastInteractionTime = 0f;
            FriendshipLevel = 0;
            FirstDeathTimestamp = 0f;
        }

        public PetSoulGhostEntry(int petId, string petName, string petType, string petColor, int friendshipLevel)
        {
            PetId = petId;
            PetName = petName;
            PetType = petType;
            PetColor = petColor;
            DeathCount = 1;
            State = SoulState.Wandering;
            IsTranscended = false;
            TranscendedTimestamp = 0f;
            WanderPosition = new Vector2(GD.Randf() * 400 - 200, GD.Randf() * 200 - 100);
            LastWanderTime = Time.GetUnixTimeFromSystem();
            LastMonologueTime = 0f;
            LastInteractionTime = 0f;
            FriendshipLevel = friendshipLevel;
            FirstDeathTimestamp = Time.GetUnixTimeFromSystem();
        }

        /// <summary>
        /// 升华所需死亡次数 = 宠物最大友谊等级
        /// </summary>
        public int TranscendenceThreshold => Mathf.Max(5, FriendshipLevel);

        /// <summary>
        /// 检查是否达到升华条件
        /// </summary>
        public bool CanTranscend => !IsTranscended && DeathCount >= TranscendenceThreshold;
    }

    /// <summary>
    /// 独白文本库 — 每种宠物类型3-5条内心独白
    /// </summary>
    public static class SoulMonologueLibrary
    {
        private static readonly Dictionary<string, List<string>> _monologues = new Dictionary<string, List<string>>
        {
            {
                "Default", new List<string>
                {
                    "还想再战斗一次...",
                    "这次一定要打赢...",
                    "主人，我还在这里。",
                    "下一场仗，一定..."
                }
            },
            {
                "Beast", new List<string>
                {
                    "嗷...还想再扑上去。",
                    "那种追逐的感觉，我还记得。",
                    "我的爪子还没钝。",
                    "下次我会更快的..."
                }
            },
            {
                "Dragon", new List<string>
                {
                    "我的火焰还没有熄灭...",
                    "下次我会烧得更旺。",
                    "这片战场是我的。",
                    "他们逃不掉的..."
                }
            },
            {
                "Spirit", new List<string>
                {
                    "我能感觉到...我还在。",
                    "物质的束缚已经消失，但意志还在。",
                    "下一个身体，会更强。",
                    "我已经看到了下一条路。"
                }
            },
            {
                "Elemental", new List<string>
                {
                    "形态会消散，元素不会。",
                    "下一场战斗，让风雨来得更猛烈些。",
                    "我在风中等待。",
                    "雷鸣还在我心中回响..."
                }
            }
        };

        /// <summary>
        /// 获取指定宠物类型的随机独白
        /// </summary>
        public static string GetRandomMonologue(string petType)
        {
            string key = _monologues.ContainsKey(petType) ? petType : "Default";
            var lines = _monologues[key];
            return lines[(int)(GD.Randi() % lines.Count)];
        }

        /// <summary>
        /// 获取所有宠物类型
        /// </summary>
        public static List<string> GetAllPetTypes() => new List<string>(_monologues.Keys);
    }

    /// <summary>
    /// 宠物灵魂数据库 — 管理所有宠物灵魂条目
    /// </summary>
    public class PetSoulGhostDatabase
    {
        /// <summary>所有宠物灵魂条目（PetId -> Entry）</summary>
        public Dictionary<int, PetSoulGhostEntry> Ghosts = new Dictionary<int, PetSoulGhostEntry>();

        /// <summary>升华的守护灵列表</summary>
        public List<int> TranscendedPetIds = new List<int>();

        /// <summary>
        /// 添加或更新宠物灵魂
        /// </summary>
        public void AddOrUpdateGhost(int petId, string petName, string petType, string petColor, int friendshipLevel)
        {
            if (Ghosts.TryGetValue(petId, out var existing))
            {
                existing.DeathCount++;
                existing.State = SoulState.Wandering;
            }
            else
            {
                Ghosts[petId] = new PetSoulGhostEntry(petId, petName, petType, petColor, friendshipLevel);
            }
        }

        /// <summary>
        /// 升华宠物灵魂
        /// </summary>
        public void TranscendGhost(int petId)
        {
            if (Ghosts.TryGetValue(petId, out var ghost))
            {
                ghost.IsTranscended = true;
                ghost.State = SoulState.Transcended;
                ghost.TranscendedTimestamp = Time.GetUnixTimeFromSystem();
                if (!TranscendedPetIds.Contains(petId))
                    TranscendedPetIds.Add(petId);
            }
        }

        /// <summary>
        /// 获取宠物灵魂
        /// </summary>
        public PetSoulGhostEntry GetGhost(int petId)
        {
            return Ghosts.TryGetValue(petId, out var ghost) ? ghost : null;
        }

        /// <summary>
        /// 获取所有游荡中的灵魂
        /// </summary>
        public List<PetSoulGhostEntry> GetWanderingGhosts()
        {
            var result = new List<PetSoulGhostEntry>();
            foreach (var ghost in Ghosts.Values)
            {
                if (!ghost.IsTranscended && ghost.State == SoulState.Wandering)
                    result.Add(ghost);
            }
            return result;
        }

        /// <summary>
        /// 获取所有升华的守护灵
        /// </summary>
        public List<PetSoulGhostEntry> GetTranscendedGhosts()
        {
            var result = new List<PetSoulGhostEntry>();
            foreach (var ghost in Ghosts.Values)
            {
                if (ghost.IsTranscended)
                    result.Add(ghost);
            }
            return result;
        }

        /// <summary>
        /// 更新灵魂在 Safe House 中的位置
        /// </summary>
        public void UpdateWanderPosition(int petId, Vector2 position)
        {
            if (Ghosts.TryGetValue(petId, out var ghost))
            {
                ghost.WanderPosition = position;
                ghost.LastWanderTime = Time.GetUnixTimeFromSystem();
            }
        }

        /// <summary>
        /// 更新灵魂状态
        /// </summary>
        public void UpdateGhostState(int petId, SoulState state)
        {
            if (Ghosts.TryGetValue(petId, out var ghost))
            {
                ghost.State = state;
            }
        }
    }
}
