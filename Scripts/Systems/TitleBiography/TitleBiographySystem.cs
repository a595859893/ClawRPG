using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Framework;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems.TitleBiography
{
    /// <summary>
    /// 称号传记系统 - 在称号解锁时生成动态传记
    /// 订阅 TitleSystem.TitleUnlocked 信号，生成并存储传记
    /// </summary>
    public partial class TitleBiographySystem : BaseSystem
    {
        private static TitleBiographySystem _instance;
        public static TitleBiographySystem Instance => _instance ??= new TitleBiographySystem();

        // 称号ID → 传记数据
        private Dictionary<string, TitleBiographyData> _biographies = new Dictionary<string, TitleBiographyData>();

        // Signals
        [Signal]
        public delegate void BiographyUnlockedEventHandler(string titleId, string biographyTitleId);
        [Signal]
        public delegate void BiographyPanelRequestedEventHandler();

        // 玩家数据源（由外部系统注入）
        private Dictionary<string, object> _playerStats = new Dictionary<string, object>();

        public override void _Ready()
        {
            _instance = this;
            SubscribeToTitleSystem();
        }

        /// <summary>
        /// 订阅 TitleSystem 的解锁信号
        /// </summary>
        private void SubscribeToTitleSystem()
        {
            var ts = TitleSystem.Instance;
            if (ts == null)
            {
                GD.PrintErr("[TitleBiographySystem] TitleSystem.Instance is null, retrying in 1s...");
                ToSignal(GetTree().CreateTimer(1.0f), "timeout").OnCompleted(() => SubscribeToTitleSystem());
                return;
            }

            // 检查是否已有信号连接方法
            if (ts.HasSignal("TitleUnlocked"))
            {
                ts.Connect("TitleUnlocked", new Callable(this, nameof(OnTitleUnlocked)), (uint)ConnectFlags.Deferred);
            }
            else
            {
                // 轮询模式：定期检查新解锁的称号
                GD.Print("[TitleBiographySystem] TitleUnlocked signal not found, using polling mode");
                _ = new System.Threading.Timer(_ => CheckForNewUnlocks(), null, 5000, 5000);
            }
        }

        /// <summary>
        /// TitleSystem.TitleUnlocked 信号的回调
        /// </summary>
        private void OnTitleUnlocked(string playerId, TitleData titleData)
        {
            if (titleData == null) return;
            GenerateAndStoreBiography(titleData);
        }

        /// <summary>
        /// 轮询模式：检测新解锁的称号
        /// </summary>
        private void CheckForNewUnlocks()
        {
            var ts = TitleSystem.Instance;
            if (ts == null) return;

            var allTitles = ts.GetAllTitles();
            foreach (var title in allTitles)
            {
                if (title.IsUnlocked && !_biographies.ContainsKey(title.TitleId))
                {
                    GenerateAndStoreBiography(title);
                }
            }
        }

        /// <summary>
        /// 生成并存储指定称号的传记
        /// </summary>
        public void GenerateAndStoreBiography(TitleData titleData)
        {
            if (titleData == null || string.IsNullOrEmpty(titleData.TitleId)) return;
            if (_biographies.ContainsKey(titleData.TitleId)) return; // 已有传记，不重复生成

            var bioText = TitleBiographyDatabase.Instance.GenerateBiography(
                titleData.TitleId,
                _playerStats,
                titleData.RequiredValue);

            var bioData = new TitleBiographyData(
                titleData.TitleId,
                titleData.TitleName,
                bioText ?? $"你解锁了「{titleData.TitleName}」。\n{titleData.Description}",
                titleData.UnlockTime,
                titleData.Rarity.ToString(),
                titleData.Category.ToString());

            _biographies[titleData.TitleId] = bioData;

            GD.Print($"[TitleBiographySystem] Biography generated for: {titleData.TitleName}");
            EmitSignal(SignalName.BiographyUnlocked, titleData.TitleId, bioData.TitleId);
        }

        /// <summary>
        /// 手动生成指定称号的传记（不触发信号，用于初始化）
        /// </summary>
        public TitleBiographyData GenerateBiographySilent(string titleId, string titleName, int requiredValue)
        {
            if (_biographies.ContainsKey(titleId)) return _biographies[titleId];

            var bioText = TitleBiographyDatabase.Instance.GenerateBiography(titleId, _playerStats, requiredValue);
            var bioData = new TitleBiographyData(titleId, titleName, bioText ?? $"你解锁了「{titleName}」",
                DateTime.Now, "Common", "Combat");

            _biographies[titleId] = bioData;
            return bioData;
        }

        /// <summary>
        /// 注入玩家数据（由其他系统调用，传递当前玩家属性）
        /// </summary>
        public void SetPlayerStats(Dictionary<string, object> stats)
        {
            _playerStats = stats ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// 更新单个玩家属性
        /// </summary>
        public void UpdatePlayerStat(string key, object value)
        {
            _playerStats[key] = value;
        }

        /// <summary>
        /// 获取已解锁的传记列表（按解锁时间倒序）
        /// </summary>
        public List<TitleBiographyData> GetUnlockedBiographies()
        {
            var list = new List<TitleBiographyData>(_biographies.Values);
            list.Sort((a, b) => b.UnlockTime.CompareTo(a.UnlockTime)); // 最新解锁在前
            return list;
        }

        /// <summary>
        /// 获取指定称号的传记数据
        /// </summary>
        public TitleBiographyData GetBiography(string titleId)
        {
            return _biographies.TryGetValue(titleId, out var bio) ? bio : null;
        }

        /// <summary>
        /// 检查指定称号是否有传记
        /// </summary>
        public bool HasBiography(string titleId)
        {
            return _biographies.ContainsKey(titleId);
        }

        /// <summary>
        /// 获取已解锁传记总数
        /// </summary>
        public int GetUnlockedCount()
        {
            return _biographies.Count;
        }

        /// <summary>
        /// 请求显示传记面板（由UI调用）
        /// </summary>
        public void RequestPanel()
        {
            EmitSignal(SignalName.BiographyPanelRequested);
        }

        // ===== Persistence =====

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new TitleBiographySaveData
            {
                TotalBiographiesUnlocked = _biographies.Count
            };

            foreach (var kvp in _biographies)
            {
                data.UnlockedBiographies.Add(kvp.Value);
            }

            // 序列化为 Dictionary
            var result = new Dictionary<string, object>
            {
                ["biographies"] = data.UnlockedBiographies,
                ["count"] = data.TotalBiographiesUnlocked
            };

            return result;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null || !data.ContainsKey("biographies")) return;

            _biographies.Clear();

            try
            {
                var bioList = data["biographies"] as Godot.Collections.Array;
                if (bioList != null)
                {
                    foreach (Godot.Collections.Dictionary raw in bioList)
                    {
                        var bio = new TitleBiographyData
                        {
                            TitleId = raw.ContainsKey("TitleId") ? raw["TitleId"].ToString() : "",
                            TitleName = raw.ContainsKey("TitleName") ? raw["TitleName"].ToString() : "",
                            BiographyText = raw.ContainsKey("BiographyText") ? raw["BiographyText"].ToString() : "",
                            Rarity = raw.ContainsKey("Rarity") ? raw["Rarity"].ToString() : "Common",
                            Category = raw.ContainsKey("Category") ? raw["Category"].ToString() : "Combat"
                        };

                        if (raw.ContainsKey("UnlockTime") && raw["UnlockTime"] != null)
                        {
                            if (raw["UnlockTime"] is double ticks)
                                bio.UnlockTime = DateTime.FromOADate(ticks);
                            else if (double.TryParse(raw["UnlockTime"].ToString(), out double parsed))
                                bio.UnlockTime = DateTime.FromOADate(parsed);
                        }

                        if (!string.IsNullOrEmpty(bio.TitleId))
                            _biographies[bio.TitleId] = bio;
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[TitleBiographySystem] Failed to import save data: {ex.Message}");
            }

            GD.Print($"[TitleBiographySystem] Loaded {_biographies.Count} biographies from save");
        }
    }
}
