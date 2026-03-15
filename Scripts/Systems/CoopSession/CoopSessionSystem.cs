using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 合作冒险会话系统 - 管理多人合作冒险
    /// </summary>
    public class CoopSessionSystem
    {
        private static CoopSessionSystem _instance;
        public static CoopSessionSystem Instance => _instance ??= new CoopSessionSystem();

        private readonly Dictionary<string, CoopSession> _activeSessions;
        private readonly Dictionary<int, CoopSessionHistory> _playerHistories;
        private string _currentSessionId;

        public event Action<CoopSession>? OnSessionCreated;
        public event Action<string>? OnSessionStarted;
        public event Action<string>? OnSessionCompleted;
        public event Action<string, int>? OnPlayerJoined;
        public event Action<string, int>? OnPlayerLeft;
        public event Action<CoopRewardResult>? OnRewardsDistributed;

        public CoopSessionSystem()
        {
            _activeSessions = new Dictionary<string, CoopSession>();
            _playerHistories = new Dictionary<int, CoopSessionHistory>();
            _currentSessionId = "";
        }

        /// <summary>
        /// 创建新的合作会话
        /// </summary>
        public CoopSession CreateSession(string sessionName, string dungeonId, int creatorId, string creatorName, CoopSessionConfig config)
        {
            var session = new CoopSession
            {
                SessionId = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                SessionName = sessionName,
                DungeonId = dungeonId,
                CreatorId = creatorId,
                CreatorName = creatorName,
                MaxPlayers = config.MaxPlayers,
                IsQuickMode = config.IsQuickMode,
                TimeLimitMinutes = config.TimeLimitMinutes,
                ExpMultiplier = config.ExpMultiplier,
                DropRateMultiplier = config.DropRateMultiplier,
                State = CoopSessionState.Forming,
                StartTime = DateTime.Now,
                Party = new CoopPartyData
                {
                    PartyId = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    PartyName = $"{creatorName}'s Party",
                    LeaderId = creatorId
                }
            };

            _activeSessions[session.SessionId] = session;
            OnSessionCreated?.Invoke(session);
            return session;
        }

        /// <summary>
        /// 玩家加入会话
        /// </summary>
        public bool JoinSession(string sessionId, int playerId, string playerName, int level, int classId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return false;

            if (session.Party.Members.Count >= session.MaxPlayers)
                return false;

            if (session.Party.Members.Any(m => m.PlayerId == playerId))
                return false;

            var playerData = new CoopPlayerData
            {
                PlayerId = playerId,
                PlayerName = playerName,
                Level = level,
                ClassId = classId,
                State = CoopPlayerState.Waiting,
                LastUpdate = DateTime.Now
            };

            session.Party.Members.Add(playerData);
            OnPlayerJoined?.Invoke(sessionId, playerId);
            return true;
        }

        /// <summary>
        /// 玩家准备
        /// </summary>
        public bool SetPlayerReady(string sessionId, int playerId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return false;

            var player = session.Party.Members.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null) return false;

            player.State = CoopPlayerState.Ready;
            player.LastUpdate = DateTime.Now;

            // 检查是否所有玩家都准备了
            if (session.Party.Members.All(p => p.State == CoopPlayerState.Ready))
            {
                session.State = CoopSessionState.Starting;
            }

            return true;
        }

        /// <summary>
        /// 开始会话
        /// </summary>
        public bool StartSession(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return false;

            if (session.State != CoopSessionState.Starting)
                return false;

            session.State = CoopSessionState.InProgress;
            session.StartTime = DateTime.Now;

            foreach (var member in session.Party.Members)
            {
                member.State = CoopPlayerState.InDungeon;
            }

            _currentSessionId = sessionId;
            OnSessionStarted?.Invoke(sessionId);
            return true;
        }

        /// <summary>
        /// 玩家进入地下城
        /// </summary>
        public bool PlayerEnterDungeon(string sessionId, int playerId, string roomId, float x, float y)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return false;

            var player = session.Party.Members.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null) return false;

            player.State = CoopPlayerState.InDungeon;
            player.CurrentRoomId = roomId;
            player.PositionX = x;
            player.PositionY = y;
            player.LastUpdate = DateTime.Now;
            player.RoomsExplored++;

            return true;
        }

        /// <summary>
        /// 更新玩家位置
        /// </summary>
        public bool UpdatePlayerPosition(string sessionId, int playerId, float x, float y)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return false;

            var player = session.Party.Members.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null) return false;

            player.PositionX = x;
            player.PositionY = y;
            player.LastUpdate = DateTime.Now;
            return true;
        }

        /// <summary>
        /// 更新玩家生命值
        /// </summary>
        public bool UpdatePlayerHealth(string sessionId, int playerId, float healthPercent)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return false;

            var player = session.Party.Members.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null) return false;

            player.HealthPercent = healthPercent;
            if (healthPercent <= 0)
            {
                player.State = CoopPlayerState.Dead;
            }
            player.LastUpdate = DateTime.Now;
            return true;
        }

        /// <summary>
        /// 记录玩家贡献
        /// </summary>
        public bool RecordContribution(string sessionId, int playerId, int damage = 0, int healing = 0, int kills = 0, int treasures = 0)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return false;

            var player = session.Party.Members.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null) return false;

            player.DamageDealt += damage;
            player.HealingDone += healing;
            player.EnemiesKilled += kills;
            player.TreasuresCollected += treasures;
            player.LastUpdate = DateTime.Now;

            session.TotalEnemiesDefeated += kills;
            session.TotalTreasuresFound += treasures;

            return true;
        }

        /// <summary>
        /// 玩家复活
        /// </summary>
        public bool RevivePlayer(string sessionId, int playerId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return false;

            var player = session.Party.Members.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null) return false;

            player.State = CoopPlayerState.InDungeon;
            player.HealthPercent = 1.0f;
            player.TimesRevived++;
            player.LastUpdate = DateTime.Now;

            return true;
        }

        /// <summary>
        /// 进入新楼层
        /// </summary>
        public bool AdvanceToFloor(string sessionId, int floor)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return false;

            session.CurrentFloor = floor;
            session.TotalRoomsCleared = 0;
            return true;
        }

        /// <summary>
        /// 房间清理完成
        /// </summary>
        public void RoomCleared(string sessionId)
        {
            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                session.TotalRoomsCleared++;
            }
        }

        /// <summary>
        /// 发现秘密
        /// </summary>
        public void SecretDiscovered(string sessionId)
        {
            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                session.TotalSecretsDiscovered++;
            }
        }

        /// <summary>
        /// 完成会话
        /// </summary>
        public CoopRewardResult CompleteSession(string sessionId, bool isVictory, List<string> sharedItems)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return new CoopRewardResult { Success = false };

            session.State = isVictory ? CoopSessionState.Completed : CoopSessionState.Failed;
            session.EndTime = DateTime.Now;
            session.ElapsedTime = session.EndTime - session.StartTime;

            var result = CalculateRewards(session, isVictory, sharedItems);

            // 记录到玩家历史
            foreach (var member in session.Party.Members)
            {
                RecordToHistory(member.PlayerId, session, result);
            }

            OnSessionCompleted?.Invoke(sessionId);
            OnRewardsDistributed?.Invoke(result);

            return result;
        }

        /// <summary>
        /// 计算收益分配
        /// </summary>
        private CoopRewardResult CalculateRewards(CoopSession session, bool isVictory, List<string> sharedItems)
        {
            var result = new CoopRewardResult
            {
                SessionId = session.SessionId,
                Success = true,
                IsVictory = isVictory,
                CompletionTime = session.ElapsedTime,
                SharedItems = sharedItems ?? new List<string>()
            };

            if (!isVictory)
            {
                // 失败时只给少量安慰奖励
                foreach (var member in session.Party.Members)
                {
                    result.Distributions.Add(new RewardDistribution
                    {
                        PlayerId = member.PlayerId,
                        PlayerName = member.PlayerName,
                        ContributionScore = 50,
                        ShareRatio = 1.0f / session.Party.Members.Count,
                        BaseExp = 10,
                        BonusExp = 0,
                        TotalExp = 10,
                        BaseGold = 5,
                        BonusGold = 0,
                        TotalGold = 5
                    });
                    result.TotalExp += 10;
                    result.TotalGold += 5;
                }
                return result;
            }

            // 计算贡献度分数
            int totalDamage = session.Party.Members.Sum(m => m.DamageDealt);
            int totalHealing = session.Party.Members.Sum(m => m.HealingDone);
            int totalKills = session.Party.Members.Sum(m => m.EnemiesKilled);
            int totalRooms = session.Party.Members.Sum(m => m.RoomsExplored);
            int totalTreasures = session.Party.Members.Sum(m => m.TreasuresCollected);

            float totalContribution = totalDamage + totalHealing * 1.5f + totalKills * 10 + totalRooms + totalTreasures * 20;
            if (totalContribution <= 0) totalContribution = 1;

            // 基础奖励（根据楼层和难度）
            int baseExp = session.CurrentFloor * 100;
            int baseGold = session.CurrentFloor * 50;

            // 应用倍率
            baseExp = (int)(baseExp * session.ExpMultiplier);
            baseGold = (int)(baseGold * session.DropRateMultiplier);

            // 时间奖励
            if (session.ElapsedTime.TotalMinutes < session.TimeLimitMinutes * 0.5f)
            {
                baseExp = (int)(baseExp * 1.5f);
                baseGold = (int)(baseGold * 1.5f);
            }

            // 分配奖励
            foreach (var member in session.Party.Members)
            {
                float contributionScore = (member.DamageDealt + member.HealingDone * 1.5f + 
                    member.EnemiesKilled * 10 + member.RoomsExplored + member.TreasuresCollected * 20) / totalContribution * 100;

                float shareRatio = contributionScore / 100f;
                if (shareRatio < 0.2f) shareRatio = 0.2f; // 最低保底20%

                int exp = (int)(baseExp * shareRatio);
                int gold = (int)(baseGold * shareRatio);

                // 奖励助攻者（治疗者）
                if (member.HealingDone > totalHealing * 0.3f)
                {
                    exp = (int)(exp * 1.1f);
                }

                result.Distributions.Add(new RewardDistribution
                {
                    PlayerId = member.PlayerId,
                    PlayerName = member.PlayerName,
                    ContributionScore = contributionScore,
                    ShareRatio = shareRatio,
                    BaseExp = (int)(baseExp / session.Party.Members.Count),
                    BonusExp = exp - (int)(baseExp / session.Party.Members.Count),
                    TotalExp = exp,
                    BaseGold = (int)(baseGold / session.Party.Members.Count),
                    BonusGold = gold - (int)(baseGold / session.Party.Members.Count),
                    TotalGold = gold
                });

                result.TotalExp += exp;
                result.TotalGold += gold;
            }

            return result;
        }

        /// <summary>
        /// 记录到玩家历史
        /// </summary>
        private void RecordToHistory(int playerId, CoopSession session, CoopRewardResult result)
        {
            if (!_playerHistories.TryGetValue(playerId, out var history))
            {
                history = new CoopSessionHistory { PlayerId = playerId };
                _playerHistories[playerId] = history;
            }

            var dist = result.Distributions.FirstOrDefault(d => d.PlayerId == playerId);
            if (dist == null) return;

            history.Sessions.Add(new CoopSessionRecord
            {
                SessionId = session.SessionId,
                DungeonName = session.DungeonName,
                AdventureType = session.AdventureType,
                WasVictory = result.IsVictory,
                FloorReached = session.CurrentFloor,
                Duration = session.ElapsedTime,
                ExpEarned = dist.TotalExp,
                GoldEarned = dist.TotalGold,
                PlayedAt = DateTime.Now
            });

            history.TotalSessionsJoined++;
            if (result.IsVictory) history.TotalSessionsCompleted++;
            if (result.IsVictory) history.TotalSessionsWon++;
            history.TotalExpEarned += dist.TotalExp;
            history.TotalGoldEarned += dist.TotalGold;
        }

        /// <summary>
        /// 获取玩家历史
        /// </summary>
        public CoopSessionHistory? GetPlayerHistory(int playerId)
        {
            return _playerHistories.TryGetValue(playerId, out var history) ? history : null;
        }

        /// <summary>
        /// 获取当前会话
        /// </summary>
        public CoopSession? GetCurrentSession()
        {
            if (string.IsNullOrEmpty(_currentSessionId)) return null;
            return _activeSessions.TryGetValue(_currentSessionId, out var session) ? session : null;
        }

        /// <summary>
        /// 获取会话
        /// </summary>
        public CoopSession? GetSession(string sessionId)
        {
            return _activeSessions.TryGetValue(sessionId, out var session) ? session : null;
        }

        /// <summary>
        /// 获取所有活跃会话
        /// </summary>
        public List<CoopSession> GetActiveSessions()
        {
            return _activeSessions.Values.ToList();
        }

        /// <summary>
        /// 离开当前会话
        /// </summary>
        public bool LeaveSession(string sessionId, int playerId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return false;

            var player = session.Party.Members.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null) return false;

            session.Party.Members.Remove(player);
            OnPlayerLeft?.Invoke(sessionId, playerId);

            // 如果没有玩家了，关闭会话
            if (session.Party.Members.Count == 0)
            {
                session.State = CoopSessionState.Cancelled;
                _activeSessions.Remove(sessionId);
            }

            if (_currentSessionId == sessionId)
            {
                _currentSessionId = "";
            }

            return true;
        }

        /// <summary>
        /// 存档支持
        /// </summary>
        public Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["ActiveSessions"] = _activeSessions.Values.ToList();
            data["PlayerHistories"] = _playerHistories.Values.ToList();
            data["CurrentSessionId"] = _currentSessionId;
            return data;
        }

        /// <summary>
        /// 读档支持
        /// </summary>
        public void ImportSaveData(Dictionary<string, object> data)
        {
            _activeSessions.Clear();
            _playerHistories.Clear();

            if (data.TryGetValue("ActiveSessions", out var sessionsObj) && sessionsObj is List<CoopSession> sessions)
            {
                foreach (var session in sessions)
                {
                    _activeSessions[session.SessionId] = session;
                }
            }

            if (data.TryGetValue("PlayerHistories", out var historiesObj) && historiesObj is List<CoopSessionHistory> histories)
            {
                foreach (var history in histories)
                {
                    _playerHistories[history.PlayerId] = history;
                }
            }

            if (data.TryGetValue("CurrentSessionId", out var currentIdObj))
            {
                _currentSessionId = currentIdObj?.ToString() ?? "";
            }
        }
    }
}
