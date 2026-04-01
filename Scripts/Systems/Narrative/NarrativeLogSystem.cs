using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Systems.Narrative
{
    /// <summary>
    /// 叙事日志系统 - 跨 run 收集和展示房间叙事碎片
    ///
    /// 设计原则：
    /// - 同一房间模板在不同楼层的叙事内容不同
    /// - 碎片数据跨游戏局次持久化
    /// - 预写叙事，不依赖 LLM 生成
    /// </summary>
    public class NarrativeLogSystem : BaseSystem
    {
        public static NarrativeLogSystem Instance { get; private set; }

        /// <summary>跨 run 收集的所有碎片ID</summary>
        private HashSet<string> _collectedFragmentIds = new HashSet<string>();

        /// <summary>当前 run 已访问的房间（去重）</summary>
        private HashSet<string> _currentRunVisitedRooms = new HashSet<string>();

        /// <summary>当前 run 已分配的碎片（roomId → fragmentId）</summary>
        private Dictionary<string, string> _currentRunAssignedFragments = new Dictionary<string, string>();

        /// <summary>碎片数据库（预写内容）</summary>
        private List<NarrativeFragment> _fragments = new List<NarrativeFragment>();

        /// <summary>碎片按 (RoomType, FloorRange) 分组的索引</summary>
        private Dictionary<string, List<NarrativeFragment>> _fragmentIndex = new Dictionary<string, List<NarrativeFragment>>();

        /// <summary>Signal: 新碎片被收集</summary>
        [Signal]
        public delegate void FragmentCollectedDelegate(string fragmentId, string roomType);

        /// <summary>Signal: 玩家进入一个已分配碎片的房间</summary>
        [Signal]
        public delegate void RoomFragmentDiscoveredDelegate(string roomId, string fragmentId);

        public override void _Ready()
        {
            Instance = this;
            IsInitialized = false;
            LoadFragments();
            LoadPersistedData();
            IsInitialized = true;
            Connect("FragmentCollected", this, nameof(_OnFragmentCollected));
        }

        /// <summary>
        /// 加载预写碎片数据
        /// </summary>
        private void LoadFragments()
        {
            _fragments = new List<NarrativeFragment>
            {
                // ===== 图书馆 Library =====
                new NarrativeFragment {
                    FragmentId = "library_burn_01",
                    RoomType = "Library",
                    FloorRange = "1-5",
                    Loop = 0,
                    NarrativeText = "「燃烧的图书馆」\n\n书页在火焰中翻卷。有什么东西……曾经在这里发生过。",
                    Theme = "fire"
                },
                new NarrativeFragment {
                    FragmentId = "library_burn_02",
                    RoomType = "Library",
                    FloorRange = "6-10",
                    Loop = 0,
                    NarrativeText = "「被遗忘的书架」\n\n书架上的灰尘下藏着一行字：'她知道火焰会来'",
                    Theme = "forewarning"
                },
                new NarrativeFragment {
                    FragmentId = "library_burn_03",
                    RoomType = "Library",
                    FloorRange = "11+",
                    Loop = 0,
                    NarrativeText = "「灰烬中的书脊」\n\n在所有烧毁的文字中，只有一句话幸存：'记住我'",
                    Theme = "memory"
                },
                new NarrativeFragment {
                    FragmentId = "library_secret_01",
                    RoomType = "Library",
                    FloorRange = "1-5",
                    Loop = 1,
                    NarrativeText = "「未寄出的信」\n\n抽屉里有一封未拆开的信。收件人的名字已被烧焦，但日期清晰可见——那是灾难发生的前一天。",
                    Theme = "loss"
                },
                new NarrativeFragment {
                    FragmentId = "library_secret_02",
                    RoomType = "Library",
                    FloorRange = "6-10",
                    Loop = 1,
                    NarrativeText = "「作家的遗稿」\n\n手稿的最后一页停在句子的中间：'当世界重启，我会在——'",
                    Theme = "incomplete"
                },
                new NarrativeFragment {
                    FragmentId = "library_secret_03",
                    RoomType = "Library",
                    FloorRange = "11+",
                    Loop = 1,
                    NarrativeText = "「全世界的故事」\n\n一面墙的书脊上刻着所有来过这里的人的名字。他们的故事，都在这里。",
                    Theme = "legacy"
                },

                // ===== Boss 房间 BossRoom =====
                new NarrativeFragment {
                    FragmentId = "boss_arena_01",
                    RoomType = "BossRoom",
                    FloorRange = "1-5",
                    Loop = 0,
                    NarrativeText = "「血迹的形状」\n\n地面上的血迹不是倒下的形状——而是向上喷射的。有人……或什么东西，曾在这里战斗到最后一刻。",
                    Theme = "struggle"
                },
                new NarrativeFragment {
                    FragmentId = "boss_arena_02",
                    RoomType = "BossRoom",
                    FloorRange = "6-10",
                    Loop = 0,
                    NarrativeText = "「无主的王座」\n\n王座上没有人，但扶手上的磨损痕迹说明曾有人长期坐于此处。也许是统治者。也许是囚徒。",
                    Theme = "power"
                },
                new NarrativeFragment {
                    FragmentId = "boss_arena_03",
                    RoomType = "BossRoom",
                    FloorRange = "11+",
                    Loop = 0,
                    NarrativeText = "「循环的终点」\n\n墙上的刻痕记录着次数。你是第 47 个试图打破这个循环的人。但你是第一个……带着记忆来的。",
                    Theme = "time_loop"
                },
                new NarrativeFragment {
                    FragmentId = "boss_arena_04",
                    RoomType = "BossRoom",
                    FloorRange = "1-5",
                    Loop = 1,
                    NarrativeText = "「前任的遗物」\n\n一把断剑。握柄处还残留着体温。也许这是上一个'你'留下的。",
                    Theme = "self"
                },
                new NarrativeFragment {
                    FragmentId = "boss_arena_05",
                    RoomType = "BossRoom",
                    FloorRange = "6-10",
                    Loop = 1,
                    NarrativeText = "「镜中身影」\n\n镜子里的人影没有跟着你移动。它只是站在那里，看着你，微笑着。",
                    Theme = "reflection"
                },
                new NarrativeFragment {
                    FragmentId = "boss_arena_06",
                    RoomType = "BossRoom",
                    FloorRange = "11+",
                    Loop = 1,
                    NarrativeText = "「第零次循环」\n\n这里没有战斗的痕迹。只有一张桌子，一把椅子，和一封写给你的信。你还没打开它。",
                    Theme = "origin"
                },

                // ===== 商人 Merchant =====
                new NarrativeFragment {
                    FragmentId = "merchant_note_01",
                    RoomType = "Merchant",
                    FloorRange = "1-5",
                    Loop = 0,
                    NarrativeText = "「商人的账本」\n\n'已赊账：灵魂 x 7。备注：均已还清。但不是用钱。'",
                    Theme = "trade"
                },
                new NarrativeFragment {
                    FragmentId = "merchant_note_02",
                    RoomType = "Merchant",
                    FloorRange = "6-10",
                    Loop = 0,
                    NarrativeText = "「不再出售的东西」\n\n展示柜里有一个空位。标签写着：'时间'。商人不卖时间。",
                    Theme = "time"
                },
                new NarrativeFragment {
                    FragmentId = "merchant_note_03",
                    RoomType = "Merchant",
                    FloorRange = "11+",
                    Loop = 0,
                    NarrativeText = "「最好的商品」\n\n'我卖的最多的东西？'商人笑了。'希望。每个人都觉得自己能改变什么。'",
                    Theme = "hope"
                },
                new NarrativeFragment {
                    FragmentId = "merchant_note_04",
                    RoomType = "Merchant",
                    FloorRange = "1-5",
                    Loop = 1,
                    NarrativeText = "「回头客」\n\n商人看着你，停顿了一下。'我们以前见过吗？……不，大概没有。我只是有种奇怪的感觉。'",
                    Theme = "recognition"
                },
                new NarrativeFragment {
                    FragmentId = "merchant_note_05",
                    RoomType = "Merchant",
                    FloorRange = "6-10",
                    Loop = 1,
                    NarrativeText = "「最后一件物品」\n\n柜台上放着一件没有标价的东西。商人说：'这个不在货架上。它在等某个人。'",
                    Theme = "destiny"
                },

                // ===== 宝箱室 TreasureRoom =====
                new NarrativeFragment {
                    FragmentId = "treasure_cursed_01",
                    RoomType = "TreasureRoom",
                    FloorRange = "1-5",
                    Loop = 0,
                    NarrativeText = "「被拒绝的礼物」\n\n宝箱是空的。但锁上有一个牙印——有人曾试图用嘴撬开它。",
                    Theme = "desperation"
                },
                new NarrativeFragment {
                    FragmentId = "treasure_cursed_02",
                    RoomType = "TreasureRoom",
                    FloorRange = "6-10",
                    Loop = 0,
                    NarrativeText = "「诅咒的重量」\n\n宝箱的盖子很轻。但打开它之后，你感觉肩膀上的重量增加了一分。",
                    Theme = "burden"
                },
                new NarrativeFragment {
                    FragmentId = "treasure_cursed_03",
                    RoomType = "TreasureRoom",
                    FloorRange = "11+",
                    Loop = 0,
                    NarrativeText = "「属于你的东西」\n\n宝箱里是一面小镜子。你往里看——看到的不是你自己，而是你曾经的某个决定。",
                    Theme = "choice"
                },

                // ===== 休息室 RestSite =====
                new NarrativeFragment {
                    FragmentId = "rest_dream_01",
                    RoomType = "RestSite",
                    FloorRange = "1-5",
                    Loop = 0,
                    NarrativeText = "「半梦半醒」\n\n你在半梦半醒之间听到一个声音：'你不是第一次来到这里。'你睡着了，忘记了这句话。",
                    Theme = "subconscious"
                },
                new NarrativeFragment {
                    FragmentId = "rest_dream_02",
                    RoomType = "RestSite",
                    FloorRange = "6-10",
                    Loop = 0,
                    NarrativeText = "「梦中梦」\n\n你梦到自己在休息。然后你梦到自己在梦中休息。再然后——你醒了，但不确定哪个是真实。",
                    Theme = "uncertainty"
                },
                new NarrativeFragment {
                    FragmentId = "rest_dream_03",
                    RoomType = "RestSite",
                    FloorRange = "11+",
                    Loop = 0,
                    NarrativeText = "「清醒梦」\n\n这次休息，你完全清醒。你知道自己在塔里。你知道你曾来过这里很多次。你知道——你还不知道的事情。",
                    Theme = "awareness"
                },
                new NarrativeFragment {
                    FragmentId = "rest_dream_04",
                    RoomType = "RestSite",
                    FloorRange = "1-5",
                    Loop = 1,
                    NarrativeText = "「另一个记忆」\n\n你梦到了另一个'你'。他们在对你说话，但你醒来后一个字也记不得。只记得——那很重要。",
                    Theme = "connection"
                },
                new NarrativeFragment {
                    FragmentId = "rest_dream_05",
                    RoomType = "RestSite",
                    FloorRange = "6-10",
                    Loop = 1,
                    NarrativeText = "「第一次休息」\n\n这真的是你第一次在这里休息吗？地板上的磨损痕迹说明不只数十人曾在这里坐过。数百人？数千人？",
                    Theme = "repetition"
                },

                // ===== 普通战斗房间 CombatRoom =====
                new NarrativeFragment {
                    FragmentId = "combat_aftermath_01",
                    RoomType = "CombatRoom",
                    FloorRange = "1-5",
                    Loop = 0,
                    NarrativeText = "「战场清理」\n\n战斗结束后，你注意到地板上的图案不像是自然磨损。更像是一个仪式。",
                    Theme = "ritual"
                },
                new NarrativeFragment {
                    FragmentId = "combat_aftermath_02",
                    RoomType = "CombatRoom",
                    FloorRange = "6-10",
                    Loop = 0,
                    NarrativeText = "「倒下的方向」\n\n倒下的敌人面向同一个方向。不是逃跑的方向。像是……在跪拜什么。",
                    Theme = "worship"
                },
                new NarrativeFragment {
                    FragmentId = "combat_aftermath_03",
                    RoomType = "CombatRoom",
                    FloorRange = "11+",
                    Loop = 0,
                    NarrativeText = "「你自己的血迹」\n\n墙上有你的血迹。但你不记得在这里受伤。也许是另一个你。另一个循环的你。",
                    Theme = "self_loop"
                },
                new NarrativeFragment {
                    FragmentId = "combat_aftermath_04",
                    RoomType = "CombatRoom",
                    FloorRange = "1-5",
                    Loop = 1,
                    NarrativeText = "「复活的痕迹」\n\n你在这里死过。但你活着。这里的血迹证明了这一点。",
                    Theme = "death"
                },
                new NarrativeFragment {
                    FragmentId = "combat_aftermath_05",
                    RoomType = "CombatRoom",
                    FloorRange = "6-10",
                    Loop = 1,
                    NarrativeText = "「战损不对」\n\n击败的敌人数量和地面痕迹不符。有些敌人倒下的位置——没有尸体。",
                    Theme = "disappearance"
                }
            };

            BuildIndex();
        }

        /// <summary>
        /// 构建 (RoomType + FloorRange) → fragments 索引
        /// </summary>
        private void BuildIndex()
        {
            _fragmentIndex.Clear();
            foreach (var frag in _fragments)
            {
                string key = frag.RoomType + "_" + frag.FloorRange;
                if (!_fragmentIndex.ContainsKey(key))
                    _fragmentIndex[key] = new List<NarrativeFragment>();
                _fragmentIndex[key].Add(frag);
            }
        }

        /// <summary>
        /// 获取指定房间和楼层应出现的叙事碎片
        /// 使用确定性伪随机，同一 (roomId) 总是返回相同碎片
        /// </summary>
        public NarrativeFragment GetFragmentForRoom(string roomId, string roomType, int floor)
        {
            string floorRange = GetFloorRange(floor);
            string key = roomType + "_" + floorRange;

            if (!_fragmentIndex.ContainsKey(key) || _fragmentIndex[key].Count == 0)
            {
                // 找不到精确匹配，尝试获取该房间类型的任意碎片
                var candidates = _fragments.Where(f => f.RoomType == roomType).ToList();
                if (candidates.Count == 0)
                    return null;
                int idx = Mathf.Abs(roomId.GetHashCode()) % candidates.Count;
                return candidates[idx];
            }

            var fragments = _fragmentIndex[key];
            // 确定性选择：基于 roomId 的 hash
            int selectedIdx = Mathf.Abs(roomId.GetHashCode()) % fragments.Count;
            return fragments[selectedIdx];
        }

        /// <summary>
        /// 根据楼层返回楼层范围字符串
        /// </summary>
        public string GetFloorRange(int floor)
        {
            if (floor <= 5) return "1-5";
            if (floor <= 10) return "6-10";
            return "11+";
        }

        /// <summary>
        /// 玩家进入房间时调用，分配并返回碎片（若尚未分配）
        /// </summary>
        public NarrativeFragment OnRoomEntered(string roomId, string roomType, int floor)
        {
            if (_currentRunVisitedRooms.Contains(roomId))
                return null; // 已访问过，不重复分配

            _currentRunVisitedRooms.Add(roomId);
            var fragment = GetFragmentForRoom(roomId, roomType, floor);
            if (fragment != null)
            {
                _currentRunAssignedFragments[roomId] = fragment.FragmentId;
                EmitSignal(nameof(RoomFragmentDiscoveredDelegate), roomId, fragment.FragmentId);
            }
            return fragment;
        }

        /// <summary>
        /// 玩家收集碎片时调用（路过房间出口）
        /// </summary>
        public void CollectFragment(string fragmentId)
        {
            if (_collectedFragmentIds.Contains(fragmentId))
                return; // 已收集过

            _collectedFragmentIds.Add(fragmentId);
            var fragment = _fragments.FirstOrDefault(f => f.FragmentId == fragmentId);
            if (fragment != null)
            {
                EmitSignal(nameof(FragmentCollectedDelegate), fragmentId, fragment.RoomType);
            }
        }

        /// <summary>
        /// 获取当前 run 中已分配但未收集的碎片
        /// </summary>
        public List<string> GetUncollectedFragmentIds()
        {
            return _currentRunAssignedFragments.Values
                .Where(fid => !_collectedFragmentIds.Contains(fid))
                .ToList();
        }

        /// <summary>
        /// 获取所有已收集碎片
        /// </summary>
        public List<NarrativeFragment> GetCollectedFragments()
        {
            return _fragments.Where(f => _collectedFragmentIds.Contains(f.FragmentId)).ToList();
        }

        /// <summary>
        /// 获取所有碎片（已收集显示完整，未收集显示???）
        /// </summary>
        public List<NarrativeFragment> GetAllFragments()
        {
            return _fragments;
        }

        /// <summary>
        /// 获取按房间类型分组的碎片
        /// </summary>
        public Dictionary<string, List<NarrativeFragment>> GetFragmentsByRoomType()
        {
            var result = new Dictionary<string, List<NarrativeFragment>>();
            foreach (var frag in _fragments)
            {
                if (!result.ContainsKey(frag.RoomType))
                    result[frag.RoomType] = new List<NarrativeFragment>();
                result[frag.RoomType].Add(frag);
            }
            return result;
        }

        /// <summary>
        /// 获取收集进度
        /// </summary>
        public (int collected, int total) GetCollectionProgress()
        {
            return (_collectedFragmentIds.Count, _fragments.Count);
        }

        /// <summary>
        /// 新 run 开始时调用，重置当前 run 状态
        /// </summary>
        public void OnNewRunStarted()
        {
            _currentRunVisitedRooms.Clear();
            _currentRunAssignedFragments.Clear();
        }

        /// <summary>
        /// 获取已收集碎片的主题分布（用于叙事菜单显示）
        /// </summary>
        public Dictionary<string, int> GetThemeDistribution()
        {
            var collected = GetCollectedFragments();
            var dist = new Dictionary<string, int>();
            foreach (var frag in collected)
            {
                string theme = string.IsNullOrEmpty(frag.Theme) ? "unknown" : frag.Theme;
                if (!dist.ContainsKey(theme))
                    dist[theme] = 0;
                dist[theme]++;
            }
            return dist;
        }

        // ===== 持久化 =====

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new NarrativeLogSaveData
            {
                CollectedFragmentIds = _collectedFragmentIds.ToList(),
                TotalFragments = _collectedFragmentIds.Count,
                UniqueRoomsVisited = _currentRunVisitedRooms.Count
            };
            return new Dictionary<string, object> { { "NarrativeLog", data } };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (!data.TryGetValue("NarrativeLog", out var logData))
                return;
            var saveData = (Godot.Collections.Dictionary)logData;
            if (saveData == null)
                return;

            if (saveData.TryGetValue("CollectedFragmentIds", out var ids))
            {
                _collectedFragmentIds = new HashSet<string>((Godot.Collections.Array)ids);
            }
        }

        public override void ImportSaveDataRaw(Dictionary<string, object> data)
        {
            ImportSaveData(data);
        }

        private void _OnFragmentCollected(string fragmentId, string roomType)
        {
            SaveIfReady();
        }

        public override string[] _GetDependencies()
        {
            return new string[] { };
        }
    }
}
