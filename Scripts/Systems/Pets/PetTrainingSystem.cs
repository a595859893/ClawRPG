using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物训练系统管理器
    /// </summary>
    public class PetTrainingSystem : BaseSystem
    {
        public static PetTrainingSystem Instance { get; private set; }

        // 玩家训练数据
        private PetTrainingData.PlayerTrainingData _playerData = new();

        // 信号
        public Action<int> OnTrainingPointsChanged;
        public Action<string> OnTrainingStarted;
        public Action<string> OnTrainingCompleted;
        public Action<string> OnTrainingClaimed;
        public Action OnDataLoaded;

        // 配置
        private const int MaxActiveSessions = 3;  // 最大同时训练数
        private const int TrainingPointsPerLevel = 1;  // 每级获得训练点数

        public override void _Ready()
        {
            Instance = this;
            PetTrainingDatabase.Initialize();
            GD.Print("宠物训练系统已初始化");
        }

        // 获取训练点数
        public int GetTrainingPoints() => _playerData.TrainingPoints;

        // 获得训练点数（宠物升级时调用）
        public void GrantTrainingPoints(int petLevel)
        {
            int points = petLevel * TrainingPointsPerLevel;
            _playerData.TrainingPoints += points;
            _playerData.TotalTrainingPoints += points;
            OnTrainingPointsChanged?.Invoke(_playerData.TrainingPoints);
            GD.Print($"获得训练点数: {points}, 总计: {_playerData.TrainingPoints}");
        }

        // 开始训练
        public bool StartTraining(Pet pet, string projectId)
        {
            if (pet == null)
            {
                GD.PrintErr("宠物为空，无法开始训练");
                return false;
            }

            var project = PetTrainingDatabase.GetProject(projectId);
            if (project == null)
            {
                GD.PrintErr($"训练项目不存在: {projectId}");
                return false;
            }

            // 检查宠物等级
            if (pet.Level < project.RequiredLevel)
            {
                GD.PrintErr($"宠物等级不足，需要 {project.RequiredLevel} 级，当前 {pet.Level} 级");
                return false;
            }

            // 检查训练点数
            if (_playerData.TrainingPoints < project.TrainingPoints)
            {
                GD.PrintErr($"训练点数不足，需要 {project.TrainingPoints} 点，当前 {_playerData.TrainingPoints} 点");
                return false;
            }

            // 检查金币
            var player = GetTree().GetFirstNodeInGroup("player");
            if (player != null)
            {
                var playerGold = player.Get("Gold") as int? ?? 0;
                if (playerGold < project.GoldCost)
                {
                    GD.PrintErr($"金币不足，需要 {project.GoldCost}，当前 {playerGold}");
                    return false;
                }
                player.Set("Gold", playerGold - project.GoldCost);
                _playerData.GoldSpentOnTraining += project.GoldCost;
            }

            // 检查最大同时训练数
            if (_playerData.ActiveSessions.Count >= MaxActiveSessions)
            {
                GD.PrintErr($"同时训练数已达上限: {MaxActiveSessions}");
                return false;
            }

            // 扣除训练点数
            _playerData.TrainingPoints -= project.TrainingPoints;

            // 创建训练会话
            var session = new PetTrainingData.TrainingSession
            {
                Id = Guid.NewGuid().ToString(),
                PetId = pet.Id,
                ProjectId = projectId,
                StartTime = DateTime.Now,
                Duration = project.Duration,
                Completed = false,
                Claimed = false
            };

            _playerData.ActiveSessions.Add(session);
            _playerData.TotalTrainingCount++;

            OnTrainingStarted?.Invoke(session.Id);
            OnTrainingPointsChanged?.Invoke(_playerData.TrainingPoints);
            
            GD.Print($"开始训练: {pet.PetName} - {project.Name}, 持续 {project.Duration} 秒");
            return true;
        }

        // 处理训练（每帧调用）
        public void _Process(double delta)
        {
            var now = DateTime.Now;
            var completedSessions = new List<PetTrainingData.TrainingSession>();

            foreach (var session in _playerData.ActiveSessions)
            {
                if (!session.Completed)
                {
                    var elapsed = (now - session.StartTime).TotalSeconds;
                    if (elapsed >= session.Duration)
                    {
                        session.Completed = true;
                        completedSessions.Add(session);
                        OnTrainingCompleted?.Invoke(session.Id);
                        GD.Print($"训练完成: {session.Id}");
                    }
                }
            }

            // 自动移动到历史记录
            foreach (var session in completedSessions)
            {
                _playerData.ActiveSessions.Remove(session);
                _playerData.CompletedSessions.Add(session);
            }
        }

        // 领取训练奖励
        public bool ClaimTrainingReward(string sessionId)
        {
            var session = _playerData.CompletedSessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null)
            {
                GD.PrintErr($"找不到已完成训练: {sessionId}");
                return false;
            }

            if (session.Claimed)
            {
                GD.PrintErr($"训练奖励已领取: {sessionId}");
                return false;
            }

            var project = PetTrainingDatabase.GetProject(session.ProjectId);
            if (project == null)
            {
                GD.PrintErr($"训练项目不存在: {session.ProjectId}");
                return false;
            }

            // 找到对应的宠物
            var petManager = PetManager.Instance;
            if (petManager == null)
            {
                GD.PrintErr("宠物管理器不存在");
                return false;
            }

            var pet = petManager.GetPetById(session.PetId);
            if (pet == null)
            {
                GD.PrintErr($"宠物不存在: {session.PetId}");
                return false;
            }

            // 应用属性加成
            pet.Attack += (int)project.AttackBonus;
            pet.Defense += (int)project.DefenseBonus;
            pet.Health += (int)project.HealthBonus;
            pet.Speed += (int)project.SpeedBonus;
            pet.CriticalRate += project.CriticalRateBonus;
            pet.CriticalDamage += project.CriticalDamageBonus;
            pet.LifeSteal += project.LifeStealBonus;

            // 更新项目等级
            if (_playerData.ProjectLevels.ContainsKey(session.ProjectId))
                _playerData.ProjectLevels[session.ProjectId]++;
            else
                _playerData.ProjectLevels[session.ProjectId] = 1;

            session.Claimed = true;
            OnTrainingClaimed?.Invoke(session.Id);

            GD.Print($"领取训练奖励: {pet.PetName} - {project.Name}, 攻击+{project.AttackBonus}, 防御+{project.DefenseBonus}");
            return true;
        }

        // 获取训练进度
        public float GetTrainingProgress(string sessionId)
        {
            var session = _playerData.ActiveSessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null)
                return 0f;

            var elapsed = (DateTime.Now - session.StartTime).TotalSeconds;
            return Mathf.Min(1f, (float)(elapsed / session.Duration));
        }

        // 获取剩余时间
        public int GetRemainingTime(string sessionId)
        {
            var session = _playerData.ActiveSessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null)
                return 0;

            var elapsed = (DateTime.Now - session.StartTime).TotalSeconds;
            var remaining = session.Duration - elapsed;
            return Mathf.Max(0, (int)remaining);
        }

        // 获取活跃训练会话
        public List<PetTrainingData.TrainingSession> GetActiveSessions() => _playerData.ActiveSessions;

        // 获取已完成训练会话
        public List<PetTrainingData.TrainingSession> GetCompletedSessions() => _playerData.CompletedSessions;

        // 获取项目等级
        public int GetProjectLevel(string projectId)
        {
            return _playerData.ProjectLevels.ContainsKey(projectId) ? _playerData.ProjectLevels[projectId] : 0;
        }

        // 获取统计信息
        public Dictionary<string, int> GetStatistics()
        {
            return new Dictionary<string, int>
            {
                { "totalTrainingPoints", _playerData.TotalTrainingPoints },
                { "availableTrainingPoints", _playerData.TrainingPoints },
                { "totalTrainingCount", _playerData.TotalTrainingCount },
                { "activeSessions", _playerData.ActiveSessions.Count },
                { "completedSessions", _playerData.CompletedSessions.Count },
                { "goldSpent", _playerData.GoldSpentOnTraining }
            };
        }

        // 保存数据
        public Dictionary<string, Variant> GetSaveData()
        {
            var data = new Dictionary<string, Variant>
            {
                { "trainingPoints", _playerData.TrainingPoints },
                { "totalTrainingPoints", _playerData.TotalTrainingPoints },
                { "totalTrainingCount", _playerData.TotalTrainingCount },
                { "goldSpentOnTraining", _playerData.GoldSpentOnTraining }
            };

            // 保存活跃会话
            var activeSessions = new List<Dictionary<string, Variant>>();
            foreach (var session in _playerData.ActiveSessions)
            {
                activeSessions.Add(new Dictionary<string, Variant>
                {
                    { "id", session.Id },
                    { "petId", session.PetId },
                    { "projectId", session.ProjectId },
                    { "startTime", session.StartTime.ToString("o") },
                    { "duration", session.Duration },
                    { "completed", session.Completed }
                });
            }
            data["activeSessions"] = activeSessions;

            // 保存已完成会话
            var completedSessions = new List<Dictionary<string, Variant>>();
            foreach (var session in _playerData.CompletedSessions)
            {
                completedSessions.Add(new Dictionary<string, Variant>
                {
                    { "id", session.Id },
                    { "petId", session.PetId },
                    { "projectId", session.ProjectId },
                    { "startTime", session.StartTime.ToString("o") },
                    { "duration", session.Duration },
                    { "completed", session.Completed },
                    { "claimed", session.Claimed }
                });
            }
            data["completedSessions"] = completedSessions;

            // 保存项目等级
            var projectLevels = new Dictionary<string, int>();
            foreach (var kvp in _playerData.ProjectLevels)
            {
                projectLevels[kvp.Key] = kvp.Value;
            }
            data["projectLevels"] = projectLevels;

            return data;
        }

        // 加载数据
        public void LoadSaveData(Dictionary<string, Variant> data)
        {
            if (data == null) return;

            _playerData.TrainingPoints = data.GetValueOrDefault("trainingPoints", 0);
            _playerData.TotalTrainingPoints = data.GetValueOrDefault("totalTrainingPoints", 0);
            _playerData.TotalTrainingCount = data.GetValueOrDefault("totalTrainingCount", 0);
            _playerData.GoldSpentOnTraining = data.GetValueOrDefault("goldSpentOnTraining", 0);

            // 加载活跃会话
            _playerData.ActiveSessions.Clear();
            if (data.ContainsKey("activeSessions"))
            {
                var sessions = data["activeSessions"] as List<Variant>;
                if (sessions != null)
                {
                    foreach (var sessionData in sessions)
                    {
                        var dict = sessionData as Dictionary<string, Variant>;
                        if (dict != null)
                        {
                            var session = new PetTrainingData.TrainingSession
                            {
                                Id = dict.GetValueOrDefault("id", ""),
                                PetId = dict.GetValueOrDefault("petId", ""),
                                ProjectId = dict.GetValueOrDefault("projectId", ""),
                                StartTime = DateTime.Parse(dict.GetValueOrDefault("startTime", DateTime.Now.ToString("o"))),
                                Duration = dict.GetValueOrDefault("duration", 60),
                                Completed = dict.GetValueOrDefault("completed", false),
                                Claimed = false
                            };
                            _playerData.ActiveSessions.Add(session);
                        }
                    }
                }
            }

            // 加载已完成会话
            _playerData.CompletedSessions.Clear();
            if (data.ContainsKey("completedSessions"))
            {
                var sessions = data["completedSessions"] as List<Variant>;
                if (sessions != null)
                {
                    foreach (var sessionData in sessions)
                    {
                        var dict = sessionData as Dictionary<string, Variant>;
                        if (dict != null)
                        {
                            var session = new PetTrainingData.TrainingSession
                            {
                                Id = dict.GetValueOrDefault("id", ""),
                                PetId = dict.GetValueOrDefault("petId", ""),
                                ProjectId = dict.GetValueOrDefault("projectId", ""),
                                StartTime = DateTime.Parse(dict.GetValueOrDefault("startTime", DateTime.Now.ToString("o"))),
                                Duration = dict.GetValueOrDefault("duration", 60),
                                Completed = dict.GetValueOrDefault("completed", true),
                                Claimed = dict.GetValueOrDefault("claimed", false)
                            };
                            _playerData.CompletedSessions.Add(session);
                        }
                    }
                }
            }

            // 加载项目等级
            _playerData.ProjectLevels.Clear();
            if (data.ContainsKey("projectLevels"))
            {
                var levels = data["projectLevels"] as Dictionary<string, int>;
                if (levels != null)
                {
                    foreach (var kvp in levels)
                    {
                        _playerData.ProjectLevels[kvp.Key] = kvp.Value;
                    }
                }
            }

            OnDataLoaded?.Invoke();
            GD.Print($"宠物训练数据已加载: {_playerData.TrainingPoints} 点训练点数, {_playerData.ActiveSessions.Count} 个活跃训练");
        }
    }

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary<string, Variant>();

        if (_playerData == null) return data;

        // 保存训练点数
        data["training_points"] = _playerData.TrainingPoints;
        data["total_training_points"] = _playerData.TotalTrainingPoints;

        // 保存活跃训练会话
        var activeSessions = new List<Dictionary<string, Variant>>();
        foreach (var session in _playerData.ActiveSessions)
        {
            activeSessions.Add(new Dictionary<string, Variant>
            {
                ["id"] = session.Id ?? "",
                ["pet_id"] = session.PetId ?? "",
                ["project_id"] = session.ProjectId ?? "",
                ["start_time"] = session.StartTime.ToString("o"),
                ["duration"] = session.Duration,
                ["progress"] = session.Progress
            });
        }
        data["active_sessions"] = activeSessions;

        // 保存完成训练会话
        var completedSessions = new List<Dictionary<string, Variant>>();
        foreach (var session in _playerData.CompletedSessions)
        {
            completedSessions.Add(new Dictionary<string, Variant>
            {
                ["id"] = session.Id ?? "",
                ["pet_id"] = session.PetId ?? "",
                ["project_id"] = session.ProjectId ?? "",
                ["start_time"] = session.StartTime.ToString("o"),
                ["duration"] = session.Duration,
                ["progress"] = session.Progress
            });
        }
        data["completed_sessions"] = completedSessions;

        // 保存项目等级
        var projectLevels = new Dictionary<string, int>();
        foreach (var kvp in _playerData.ProjectLevels)
        {
            projectLevels[kvp.Key] = kvp.Value;
        }
        data["project_levels"] = projectLevels;

        // 保存统计
        data["total_training_count"] = _playerData.TotalTrainingCount;
        data["gold_spent"] = _playerData.GoldSpentOnTraining;

        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null || _playerData == null) return;

        // 加载训练点数
        if (data.TryGetValue("training_points", out var trainingPoints))
            _playerData.TrainingPoints = (int)trainingPoints;
        if (data.TryGetValue("total_training_points", out var totalPoints))
            _playerData.TotalTrainingPoints = (int)totalPoints;

        // 加载活跃训练会话
        if (data.TryGetValue("active_sessions", out var activeData))
        {
            _playerData.ActiveSessions = new List<PetTrainingData.TrainingSession>();
            var sessionsList = (List<Variant>)activeData;
            foreach (var sessionVar in sessionsList)
            {
                var sessionDict = (Dictionary<string, Variant>)sessionVar;
                var session = new PetTrainingData.TrainingSession();

                if (sessionDict.TryGetValue("id", out var id))
                    session.Id = (string)id;
                if (sessionDict.TryGetValue("pet_id", out var petId))
                    session.PetId = (string)petId;
                if (sessionDict.TryGetValue("project_id", out var projectId))
                    session.ProjectId = (string)projectId;
                if (sessionDict.TryGetValue("start_time", out var startTime) && DateTime.TryParse((string)startTime, out var parsed))
                    session.StartTime = parsed;
                if (sessionDict.TryGetValue("duration", out var duration))
                    session.Duration = (float)duration;
                if (sessionDict.TryGetValue("progress", out var progress))
                    session.Progress = (float)progress;

                _playerData.ActiveSessions.Add(session);
            }
        }

        // 加载完成训练会话
        if (data.TryGetValue("completed_sessions", out var completedData))
        {
            _playerData.CompletedSessions = new List<PetTrainingData.TrainingSession>();
            var sessionsList = (List<Variant>)completedData;
            foreach (var sessionVar in sessionsList)
            {
                var sessionDict = (Dictionary<string, Variant>)sessionVar;
                var session = new PetTrainingData.TrainingSession();

                if (sessionDict.TryGetValue("id", out var id))
                    session.Id = (string)id;
                if (sessionDict.TryGetValue("pet_id", out var petId))
                    session.PetId = (string)petId;
                if (sessionDict.TryGetValue("project_id", out var projectId))
                    session.ProjectId = (string)projectId;
                if (sessionDict.TryGetValue("start_time", out var startTime) && DateTime.TryParse((string)startTime, out var parsed))
                    session.StartTime = parsed;
                if (sessionDict.TryGetValue("duration", out var duration))
                    session.Duration = (float)duration;
                if (sessionDict.TryGetValue("progress", out var progress))
                    session.Progress = (float)progress;

                _playerData.CompletedSessions.Add(session);
            }
        }

        // 加载项目等级
        if (data.TryGetValue("project_levels", out var levelsData))
        {
            _playerData.ProjectLevels = new Dictionary<string, int>();
            var levelsDict = (Dictionary<string, Variant>)levelsData;
            foreach (var kvp in levelsDict)
            {
                _playerData.ProjectLevels[kvp.Key] = (int)kvp.Value;
            }
        }

        // 加载统计
        if (data.TryGetValue("total_training_count", out var totalCount))
            _playerData.TotalTrainingCount = (int)totalCount;
        if (data.TryGetValue("gold_spent", out var goldSpent))
            _playerData.GoldSpentOnTraining = (int)goldSpent;
    }
}
