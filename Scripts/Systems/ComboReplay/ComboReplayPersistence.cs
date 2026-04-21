using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Systems.ComboReplay
{
    /// <summary>
    /// Combo 回放持久化系统（REQ-114-05）
    /// 职责：保存/加载回放到 user://replays/ 目录，命名格式 {seed}_{timestamp}.json
    /// </summary>
    public partial class ComboReplayPersistence : BaseSystem
    {
        private static ComboReplayPersistence _instance;
        public static ComboReplayPersistence Instance => _instance ??= new ComboReplayPersistence();

        /// <summary>回放文件存放目录</summary>
        private const string REPLAY_DIR = "replays";

        /// <summary>最大保存回放数量（防止目录膨胀）</summary>
        private const int MAX_SAVED_REPLAYS = 50;

        /// <summary>所有已加载的回放元数据缓存</summary>
        private List<ReplayFileInfo> _cachedReplayList = new List<ReplayFileInfo>();

        public override void _Ready()
        {
            _instance = this;

            // 确保目录存在
            EnsureReplayDirectory();

            // 订阅录制完成信号
            ComboReplayRecorder.OnReplayRecorded += OnReplayRecorded;

            // 启动时扫描已有回放
            ScanReplayDirectory();

            GD.Print("[ComboReplayPersistence] Initialized");
        }

        public override void _ExitTree()
        {
            ComboReplayRecorder.OnReplayRecorded -= OnReplayRecorded;
        }

        /// <summary>
        /// 确保回放目录存在
        /// </summary>
        private void EnsureReplayDirectory()
        {
            var dir = GetReplayDirectory();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                GD.Print($"[ComboReplayPersistence] Created replay directory: {dir}");
            }
        }

        /// <summary>
        /// 获取回放目录路径
        /// </summary>
        private string GetReplayDirectory()
        {
            return Path.Combine(ProjectSettings.GlobalizePath("user://"), REPLAY_DIR);
        }

        /// <summary>
        /// 回放文件信息（用于列表展示）
        /// </summary>
        public class ReplayFileInfo
        {
            public string FileName { get; set; } = "";
            public int Seed { get; set; }
            public double Timestamp { get; set; }
            public float DurationSeconds { get; set; }
            public string SceneName { get; set; } = "";
            public string Result { get; set; } = "";
            public int ActionCount { get; set; }
            public int ComboCount { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        /// <summary>
        /// 录制完成回调：自动保存回放
        /// </summary>
        private void OnReplayRecorded(ComboReplayData replay)
        {
            if (replay == null || replay.Actions.Count == 0)
            {
                GD.Print("[ComboReplayPersistence] Replay has no actions, skipping save.");
                return;
            }

            SaveReplay(replay);
        }

        /// <summary>
        /// 保存回放到文件
        /// </summary>
        public bool SaveReplay(ComboReplayData replay)
        {
            try
            {
                EnsureReplayDirectory();

                // 文件名格式：{seed}_{timestamp}.json
                string fileName = $"{replay.Seed}_{replay.StartTimestamp:F0}.json";
                string fullPath = Path.Combine(GetReplayDirectory(), fileName);

                // 手动序列化（避免 System.Text.Json 依赖 Godot 特有类型）
                string json = ReplayToJson(replay);
                System.IO.File.WriteAllText(fullPath, json);

                GD.Print($"[ComboReplayPersistence] Saved replay: {fileName} ({replay.Actions.Count} actions, {replay.Combos.Count} combos)");

                // 更新缓存
                var info = BuildReplayFileInfo(fileName, replay);
                _cachedReplayList.Insert(0, info);

                // 清理过期回放
                CleanupOldReplays();

                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[ComboReplayPersistence] Failed to save replay: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 将 ComboReplayData 序列化为 JSON
        /// </summary>
        private string ReplayToJson(ComboReplayData replay)
        {
            var obj = new Dictionary<string, object>
            {
                ["Version"] = replay.Version,
                ["Seed"] = replay.Seed,
                ["StartTimestamp"] = replay.StartTimestamp,
                ["DurationSeconds"] = replay.DurationSeconds,
                ["Actions"] = replay.Actions.Select(a => new Dictionary<string, object>
                {
                    ["Time"] = a.Time,
                    ["Type"] = (int)a.Type,
                    ["SkillId"] = a.SkillId ?? "",
                    ["TargetId"] = a.TargetId ?? "",
                    ["PlayerPosX"] = a.PlayerPosX,
                    ["PlayerPosY"] = a.PlayerPosY,
                    ["ExtraData"] = a.ExtraData ?? ""
                }).ToList(),
                ["Combos"] = replay.Combos.Select(c => new Dictionary<string, object>
                {
                    ["Time"] = c.Time,
                    ["ComboId"] = c.ComboId ?? "",
                    ["ComboName"] = c.ComboName ?? "",
                    ["SkillSequence"] = c.SkillSequence ?? new List<string>(),
                    ["Damage"] = c.Damage,
                    ["Killed"] = c.Killed
                }).ToList(),
                ["Metadata"] = new Dictionary<string, object>
                {
                    ["CreatedAt"] = replay.Metadata.CreatedAt,
                    ["GameVersion"] = replay.Metadata.GameVersion ?? "1.0.0",
                    ["PlayerLevel"] = replay.Metadata.PlayerLevel,
                    ["Result"] = replay.Metadata.Result ?? "victory",
                    ["SceneName"] = replay.Metadata.SceneName ?? "",
                    ["EnemyCount"] = replay.Metadata.EnemyCount
                }
            };

            return Godot.Json.Print(obj, "  ");
        }

        /// <summary>
        /// 从 JSON 反序列化为 ComboReplayData
        /// </summary>
        private ComboReplayData JsonToReplay(string json)
        {
            var result = (Godot.Collections.Dictionary)Godot.Json.Parse(json).AsGodotDictionary();

            var replay = new ComboReplayData
            {
                Version = result.Contains("Version") ? System.Convert.ToInt32(result["Version"]) : 1,
                Seed = result.Contains("Seed") ? System.Convert.ToInt32(result["Seed"]) : 0,
                StartTimestamp = result.Contains("StartTimestamp") ? System.Convert.ToDouble(result["StartTimestamp"]) : 0,
                DurationSeconds = result.Contains("DurationSeconds") ? (float)System.Convert.ToDouble(result["DurationSeconds"]) : 0f
            };

            if (result.Contains("Actions"))
            {
                var actions = (Godot.Collections.Array)result["Actions"];
                foreach (Godot.Collections.Dictionary ad in actions)
                {
                    replay.Actions.Add(new PlayerActionRecord
                    {
                        Time = ad.Contains("Time") ? (float)System.Convert.ToDouble(ad["Time"]) : 0f,
                        Type = ad.Contains("Type") ? (PlayerActionType)System.Convert.ToInt32(ad["Type"]) : PlayerActionType.SkillUse,
                        SkillId = ad.Contains("SkillId") ? ad["SkillId"]?.ToString() ?? "" : "",
                        TargetId = ad.Contains("TargetId") ? ad["TargetId"]?.ToString() ?? "" : "",
                        PlayerPosX = ad.Contains("PlayerPosX") ? (float)System.Convert.ToDouble(ad["PlayerPosX"]) : 0f,
                        PlayerPosY = ad.Contains("PlayerPosY") ? (float)System.Convert.ToDouble(ad["PlayerPosY"]) : 0f,
                        ExtraData = ad.Contains("ExtraData") ? ad["ExtraData"]?.ToString() ?? "" : ""
                    });
                }
            }

            if (result.Contains("Combos"))
            {
                var combos = (Godot.Collections.Array)result["Combos"];
                foreach (Godot.Collections.Dictionary cd in combos)
                {
                    var combo = new ComboRecord
                    {
                        Time = cd.Contains("Time") ? (float)System.Convert.ToDouble(cd["Time"]) : 0f,
                        ComboId = cd.Contains("ComboId") ? cd["ComboId"]?.ToString() ?? "" : "",
                        ComboName = cd.Contains("ComboName") ? cd["ComboName"]?.ToString() ?? "" : "",
                        Damage = cd.Contains("Damage") ? System.Convert.ToInt32(cd["Damage"]) : 0,
                        Killed = cd.Contains("Killed") ? System.Convert.ToBoolean(cd["Killed"]) : false
                    };
                    if (cd.Contains("SkillSequence"))
                    {
                        var seq = (Godot.Collections.Array)cd["SkillSequence"];
                        foreach (var s in seq)
                            combo.SkillSequence.Add(s?.ToString() ?? "");
                    }
                    replay.Combos.Add(combo);
                }
            }

            if (result.Contains("Metadata"))
            {
                var meta = (Godot.Collections.Dictionary)result["Metadata"];
                replay.Metadata = new ReplayMetadata
                {
                    CreatedAt = meta.Contains("CreatedAt") ? System.Convert.ToDouble(meta["CreatedAt"]) : 0,
                    GameVersion = meta.Contains("GameVersion") ? meta["GameVersion"]?.ToString() ?? "1.0.0" : "1.0.0",
                    PlayerLevel = meta.Contains("PlayerLevel") ? System.Convert.ToInt32(meta["PlayerLevel"]) : 0,
                    Result = meta.Contains("Result") ? meta["Result"]?.ToString() ?? "victory" : "victory",
                    SceneName = meta.Contains("SceneName") ? meta["SceneName"]?.ToString() ?? "" : "",
                    EnemyCount = meta.Contains("EnemyCount") ? System.Convert.ToInt32(meta["EnemyCount"]) : 0
                };
            }

            return replay;
        }

        /// <summary>
        /// 加载指定文件名的回放
        /// </summary>
        public ComboReplayData LoadReplay(string fileName)
        {
            try
            {
                string fullPath = Path.Combine(GetReplayDirectory(), fileName);
                if (!System.IO.File.Exists(fullPath))
                {
                    GD.PrintErr($"[ComboReplayPersistence] Replay file not found: {fileName}");
                    return null;
                }

                string json = System.IO.File.ReadAllText(fullPath);
                var replay = JsonToReplay(json);
                GD.Print($"[ComboReplayPersistence] Loaded replay: {fileName}");
                return replay;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[ComboReplayPersistence] Failed to load replay {fileName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取回放文件列表（按时间倒序）
        /// </summary>
        public List<ReplayFileInfo> GetReplayList()
        {
            return _cachedReplayList.OrderByDescending(r => r.Timestamp).ToList();
        }

        /// <summary>
        /// 扫描回放目录并更新缓存
        /// </summary>
        private void ScanReplayDirectory()
        {
            _cachedReplayList.Clear();
            try
            {
                EnsureReplayDirectory();
                var dir = new DirectoryInfo(GetReplayDirectory());
                var files = dir.GetFiles("*.json").OrderByDescending(f => f.LastWriteTime).ToList();

                foreach (var file in files)
                {
                    try
                    {
                        var replay = LoadReplay(file.Name);
                        if (replay != null)
                        {
                            _cachedReplayList.Add(BuildReplayFileInfo(file.Name, replay));
                        }
                    }
                    catch
                    {
                        // 跳过损坏的文件
                    }
                }

                GD.Print($"[ComboReplayPersistence] Scanned {_cachedReplayList.Count} replays");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[ComboReplayPersistence] Failed to scan replay directory: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据文件名和实际数据构建元信息
        /// </summary>
        private ReplayFileInfo BuildReplayFileInfo(string fileName, ComboReplayData replay)
        {
            return new ReplayFileInfo
            {
                FileName = fileName,
                Seed = replay.Seed,
                Timestamp = replay.StartTimestamp,
                DurationSeconds = replay.DurationSeconds,
                SceneName = replay.Metadata.SceneName,
                Result = replay.Metadata.Result,
                ActionCount = replay.Actions.Count,
                ComboCount = replay.Combos.Count,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds((long)replay.Metadata.CreatedAt).LocalDateTime
            };
        }

        /// <summary>
        /// 删除指定回放
        /// </summary>
        public bool DeleteReplay(string fileName)
        {
            try
            {
                string fullPath = Path.Combine(GetReplayDirectory(), fileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _cachedReplayList.RemoveAll(r => r.FileName == fileName);
                    GD.Print($"[ComboReplayPersistence] Deleted replay: {fileName}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[ComboReplayPersistence] Failed to delete replay {fileName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清理超过数量限制的最旧回放
        /// </summary>
        private void CleanupOldReplays()
        {
            if (_cachedReplayList.Count <= MAX_SAVED_REPLAYS) return;

            var toDelete = _cachedReplayList
                .OrderBy(r => r.Timestamp)
                .Take(_cachedReplayList.Count - MAX_SAVED_REPLAYS)
                .ToList();

            foreach (var info in toDelete)
            {
                DeleteReplay(info.FileName);
            }

            GD.Print($"[ComboReplayPersistence] Cleaned up {toDelete.Count} old replays");
        }

        /// <summary>
        /// 删除所有回放
        /// </summary>
        public void ClearAllReplays()
        {
            try
            {
                var dir = new DirectoryInfo(GetReplayDirectory());
                foreach (var file in dir.GetFiles("*.json"))
                {
                    file.Delete();
                }
                _cachedReplayList.Clear();
                GD.Print("[ComboReplayPersistence] Cleared all replays");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[ComboReplayPersistence] Failed to clear replays: {ex.Message}");
            }
        }
    }
}
