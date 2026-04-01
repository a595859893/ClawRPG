using System;
using System.Linq;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 锦标赛匹配报名系统 - 负责玩家报名和锦标赛创建
    /// </summary>
    public partial class TournamentMatchmakingSystem : BaseSystem
    {
        private static TournamentMatchmakingSystem _instance;
        public static TournamentMatchmakingSystem Instance => _instance;

        // 事件：玩家报名成功
        public event Action<string, string, string> OnPlayerRegistered;
        // 事件：锦标赛创建成功
        public event Action<string> OnTournamentCreated;
        // 事件：锦标赛开始
        public event Action<string, int> OnTournamentStarted;

        public override void _Ready()
        {
            _instance = this;
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            GD.Print("[TournamentMatchmakingSystem] 匹配报名系统初始化");
            IsInitialized = true;
        }

        #region Tournament Creation

        /// <summary>
        /// 从模板创建锦标赛
        /// </summary>
        public Tournament CreateTournamentFromTemplate(string templateId, string organizerId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            var template = ArenaTournamentDatabase.GetTemplate(templateId);
            if (template == null)
            {
                GD.PrintErr($"[TournamentMatchmakingSystem] 模板不存在: {templateId}");
                return null;
            }

            var tournament = new Tournament
            {
                tournamentId = GenerateTournamentId(),
                tournamentName = template.name,
                description = template.description,
                format = template.format,
                status = TournamentStatus.Pending,
                currentStage = TournamentStage.Registration,
                maxPlayers = template.maxPlayers,
                minPlayers = template.minPlayers,
                currentPlayerCount = 0,
                registrationStart = DateTime.Now,
                registrationEnd = DateTime.Now.AddSeconds(template.registrationDuration),
                rounds = template.rounds,
                currentRound = 0,
                prizePool = template.prizePool,
                entryFee = template.entryFee,
                organizerId = organizerId,
                createdAt = DateTime.Now,
                updatedAt = DateTime.Now,
                rewards = ArenaTournamentDatabase.GetRewardPool(template.maxPlayers)
            };

            core.Tournaments[tournament.tournamentId] = tournament;
            
            GD.Print($"[TournamentMatchmakingSystem] 创建锦标赛: {tournament.tournamentName} ({tournament.tournamentId})");
            OnTournamentCreated?.Invoke(tournament.tournamentId);
            
            return tournament;
        }

        /// <summary>
        /// 创建自定义锦标赛
        /// </summary>
        public Tournament CreateCustomTournament(string name, string description, TournamentFormat format, 
            int maxPlayers, int minPlayers, int prizePool, int entryFee, string organizerId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            var tournament = new Tournament
            {
                tournamentId = GenerateTournamentId(),
                tournamentName = name,
                description = description,
                format = format,
                status = TournamentStatus.Pending,
                currentStage = TournamentStage.Registration,
                maxPlayers = maxPlayers,
                minPlayers = minPlayers,
                currentPlayerCount = 0,
                registrationStart = DateTime.Now,
                registrationEnd = DateTime.Now.AddHours(2),
                rounds = CalculateRounds(format, maxPlayers),
                currentRound = 0,
                prizePool = prizePool,
                entryFee = entryFee,
                organizerId = organizerId,
                createdAt = DateTime.Now,
                updatedAt = DateTime.Now,
                rewards = ArenaTournamentDatabase.GetRewardPool(maxPlayers)
            };

            core.Tournaments[tournament.tournamentId] = tournament;
            
            GD.Print($"[TournamentMatchmakingSystem] 创建自定义锦标赛: {tournament.tournamentName}");
            OnTournamentCreated?.Invoke(tournament.tournamentId);
            
            return tournament;
        }

        #endregion

        #region Player Registration

        /// <summary>
        /// 玩家报名锦标赛
        /// </summary>
        public bool RegisterPlayer(string tournamentId, string playerId, string playerName)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (!core.Tournaments.ContainsKey(tournamentId))
            {
                GD.PrintErr($"[TournamentMatchmakingSystem] 锦标赛不存在: {tournamentId}");
                return false;
            }

            var tournament = core.Tournaments[tournamentId];
            
            // 验证锦标赛状态
            if (tournament.status != TournamentStatus.Pending)
            {
                GD.PrintErr($"[TournamentMatchmakingSystem] 锦标赛无法报名: {tournament.status}");
                return false;
            }

            if (DateTime.Now > tournament.registrationEnd)
            {
                GD.PrintErr("[TournamentMatchmakingSystem] 报名已结束");
                return false;
            }

            if (tournament.currentPlayerCount >= tournament.maxPlayers)
            {
                GD.PrintErr("[TournamentMatchmakingSystem] 锦标赛已满");
                return false;
            }

            if (tournament.registeredPlayers.Any(p => p.playerId == playerId))
            {
                GD.PrintErr("[TournamentMatchmakingSystem] 玩家已报名");
                return false;
            }

            // 创建玩家并添加到锦标赛
            var player = new TournamentPlayer
            {
                playerId = playerId,
                playerName = playerName,
                seedNumber = tournament.currentPlayerCount + 1,
                registrationTime = DateTime.Now
            };

            tournament.registeredPlayers.Add(player);
            tournament.currentPlayerCount++;
            tournament.updatedAt = DateTime.Now;
            
            GD.Print($"[TournamentMatchmakingSystem] 玩家 {playerName} 报名锦标赛 {tournament.tournamentName}");
            OnPlayerRegistered?.Invoke(tournamentId, playerId, playerName);
            
            return true;
        }

        /// <summary>
        /// 玩家取消报名
        /// </summary>
        public bool UnregisterPlayer(string tournamentId, string playerId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (!core.Tournaments.ContainsKey(tournamentId))
            {
                GD.PrintErr($"[TournamentMatchmakingSystem] 锦标赛不存在: {tournamentId}");
                return false;
            }

            var tournament = core.Tournaments[tournamentId];
            
            if (tournament.status != TournamentStatus.Pending)
            {
                GD.PrintErr("[TournamentMatchmakingSystem] 锦标赛已开始，无法取消报名");
                return false;
            }

            var player = tournament.registeredPlayers.FirstOrDefault(p => p.playerId == playerId);
            if (player == null)
            {
                GD.PrintErr("[TournamentMatchmakingSystem] 玩家未报名");
                return false;
            }

            tournament.registeredPlayers.Remove(player);
            tournament.currentPlayerCount--;
            tournament.updatedAt = DateTime.Now;
            
            GD.Print($"[TournamentMatchmakingSystem] 玩家 {playerId} 取消报名锦标赛 {tournament.tournamentName}");
            return true;
        }

        /// <summary>
        /// 检查报名是否已满
        /// </summary>
        public bool IsRegistrationFull(string tournamentId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (!core.Tournaments.TryGetValue(tournamentId, out var tournament))
                return false;
            
            return tournament.currentPlayerCount >= tournament.maxPlayers;
        }

        /// <summary>
        /// 检查是否满足开始条件
        /// </summary>
        public bool CanStartTournament(string tournamentId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (!core.Tournaments.TryGetValue(tournamentId, out var tournament))
                return false;
            
            return tournament.currentPlayerCount >= tournament.minPlayers;
        }

        /// <summary>
        /// 获取锦标赛报名状态
        /// </summary>
        public (int current, int max, int min) GetRegistrationStatus(string tournamentId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (!core.Tournaments.TryGetValue(tournamentId, out var tournament))
                return (0, 0, 0);
            
            return (tournament.currentPlayerCount, tournament.maxPlayers, tournament.minPlayers);
        }

        #endregion

        #region Helpers

        private string GenerateTournamentId()
        {
            return $"T_{DateTime.Now:yyyyMMddHHmmss}_{Godot.GD.Randomi(1000, 9999)}";
        }

        private int CalculateRounds(TournamentFormat format, int playerCount)
        {
            var config = ArenaTournamentDatabase.GetFormatConfig(format);
            if (config != null)
            {
                return (int)Math.Ceiling(Math.Log(playerCount, 2));
            }
            return 4;
        }

        #endregion

        #region 存档支持

        public override Dictionary<string, object> ExportSaveData()
        {
            // 赛程数据由 TournamentPersistenceSystem 统一管理
            // 本系统为无状态协调器，无独立数据需要持久化
            return new Dictionary<string, object>();
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // 无自有状态，无需导入
        }

        #endregion
    }
}
