using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 锦标赛持久化系统 - 负责所有数据的序列化和反序列化
    /// </summary>
    public class TournamentPersistenceSystem : BaseSystem
    {
        private static TournamentPersistenceSystem _instance;
        public static TournamentPersistenceSystem Instance => _instance;

        public override void _Ready()
        {
            _instance = this;
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            GD.Print("[TournamentPersistenceSystem] 持久化系统初始化");
            IsInitialized = true;
        }

        /// <summary>
        /// 导出保存数据 - 序列化所有锦标赛和玩家进度
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            var core = ArenaTournamentCoreSystem.Instance;
            if (core == null) return data;

            // 序列化所有锦标赛
            var tournamentsData = new ArrayList();
            foreach (var kvp in core.Tournaments)
            {
                var t = kvp.Value;
                var tournamentDict = new Dictionary
                {
                    { "tournamentId", t.tournamentId },
                    { "tournamentName", t.tournamentName },
                    { "description", t.description },
                    { "format", (int)t.format },
                    { "status", (int)t.status },
                    { "currentStage", (int)t.currentStage },
                    { "maxPlayers", t.maxPlayers },
                    { "minPlayers", t.minPlayers },
                    { "currentPlayerCount", t.currentPlayerCount },
                    { "registrationStart", t.registrationStart.ToString("o") },
                    { "registrationEnd", t.registrationEnd.ToString("o") },
                    { "startTime", t.startTime?.ToString("o") },
                    { "endTime", t.endTime?.ToString("o") },
                    { "rounds", t.rounds },
                    { "currentRound", t.currentRound },
                    { "prizePool", t.prizePool },
                    { "entryFee", t.entryFee },
                    { "organizerId", t.organizerId },
                    { "createdAt", t.createdAt.ToString("o") },
                    { "updatedAt", t.updatedAt.ToString("o") }
                };

                // 序列化玩家
                var playersData = new ArrayList();
                foreach (var p in t.registeredPlayers)
                {
                    playersData.Add(new Dictionary
                    {
                        { "playerId", p.playerId },
                        { "playerName", p.playerName },
                        { "seedNumber", p.seedNumber },
                        { "score", p.score },
                        { "wins", p.wins },
                        { "losses", p.losses },
                        { "matchesPlayed", p.matchesPlayed },
                        { "isEliminated", p.isEliminated },
                        { "hasLostOnce", p.hasLostOnce },
                        { "registrationTime", p.registrationTime.ToString("o") },
                        { "matchHistory", new ArrayList(p.matchHistory) }
                    });
                }
                tournamentDict["registeredPlayers"] = playersData;

                // 序列化比赛
                var matchesData = new ArrayList();
                foreach (var m in t.matches)
                {
                    matchesData.Add(new Dictionary
                    {
                        { "matchId", m.matchId },
                        { "roundNumber", m.roundNumber },
                        { "matchNumber", m.matchNumber },
                        { "stage", (int)m.stage },
                        { "player1Id", m.player1Id },
                        { "player2Id", m.player2Id },
                        { "winnerId", m.winnerId },
                        { "player1Score", m.player1Score },
                        { "player2Score", m.player2Score },
                        { "isCompleted", m.isCompleted },
                        { "scheduledTime", m.scheduledTime.ToString("o") },
                        { "completedTime", m.completedTime?.ToString("o") }
                    });
                }
                tournamentDict["matches"] = matchesData;

                // 序列化奖励
                var rewardsData = new ArrayList();
                foreach (var r in t.rewards)
                {
                    rewardsData.Add(new Dictionary
                    {
                        { "rankStart", r.rankStart },
                        { "rankEnd", r.rankEnd },
                        { "rewardType", r.rewardType },
                        { "rewardId", r.rewardId },
                        { "rewardAmount", r.rewardAmount }
                    });
                }
                tournamentDict["rewards"] = rewardsData;

                // 序列化分组
                var groupsData = new ArrayList();
                foreach (var g in t.groups)
                {
                    groupsData.Add(new Dictionary
                    {
                        { "groupId", g.groupId },
                        { "groupName", g.groupName },
                        { "playerIds", new ArrayList(g.playerIds) }
                    });
                }
                tournamentDict["groups"] = groupsData;

                tournamentsData.Add(tournamentDict);
            }
            data["tournaments"] = tournamentsData;

            // 序列化活动锦标赛索引
            var activeTournamentIds = new ArrayList();
            foreach (var t in core.ActiveTournaments)
            {
                activeTournamentIds.Add(t.tournamentId);
            }
            data["activeTournamentIds"] = activeTournamentIds;

            // 序列化玩家进度
            var progressData = new ArrayList();
            foreach (var kvp in core.PlayerProgress)
            {
                var p = kvp.Value;
                var progressDict = new Dictionary
                {
                    { "playerId", p.playerId },
                    { "participatedTournaments", new ArrayList(p.participatedTournaments) }
                };

                // 序列化最近记录
                var recordsData = new ArrayList();
                foreach (var r in p.recentRecords)
                {
                    recordsData.Add(new Dictionary
                    {
                        { "playerId", r.playerId },
                        { "tournamentId", r.tournamentId },
                        { "tournamentName", r.tournamentName },
                        { "finalRank", r.finalRank },
                        { "score", r.score },
                        { "wins", r.wins },
                        { "losses", r.losses },
                        { "participatedAt", r.participatedAt.ToString("o") }
                    });
                }
                progressDict["recentRecords"] = recordsData;

                // 序列化统计
                if (p.statistics != null)
                {
                    progressDict["statistics"] = new Dictionary
                    {
                        { "playerId", p.statistics.playerId },
                        { "totalTournaments", p.statistics.totalTournaments },
                        { "firstPlace", p.statistics.firstPlace },
                        { "secondPlace", p.statistics.secondPlace },
                        { "thirdPlace", p.statistics.thirdPlace },
                        { "top4", p.statistics.top4 },
                        { "top8", p.statistics.top8 },
                        { "top16", p.statistics.top16 },
                        { "totalWins", p.statistics.totalWins },
                        { "totalLosses", p.statistics.totalLosses },
                        { "highestRank", p.statistics.highestRank },
                        { "totalPrizeWon", p.statistics.totalPrizeWon }
                    };
                }

                progressData.Add(progressDict);
            }
            data["playerProgress"] = progressData;

            GD.Print($"[TournamentPersistenceSystem] 导出 {core.Tournaments.Count} 个锦标赛, {core.PlayerProgress.Count} 个玩家进度");
            return data;
        }

        /// <summary>
        /// 导入保存数据 - 反序列化所有锦标赛和玩家进度
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            var core = ArenaTournamentCoreSystem.Instance;
            if (core == null) return;

            // 导入锦标赛
            if (data.Contains("tournaments"))
            {
                var tournamentsData = (ArrayList)data["tournaments"];
                foreach (Dictionary td in tournamentsData)
                {
                    var tournament = new Tournament
                    {
                        tournamentId = td["tournamentId"]?.ToString() ?? "",
                        tournamentName = td["tournamentName"]?.ToString() ?? "",
                        description = td["description"]?.ToString() ?? "",
                        format = (TournamentFormat)(td["format"] as int? ?? 0),
                        status = (TournamentStatus)(td["status"] as int? ?? 0),
                        currentStage = (TournamentStage)(td["currentStage"] as int? ?? 0),
                        maxPlayers = td["maxPlayers"] as int? ?? 0,
                        minPlayers = td["minPlayers"] as int? ?? 0,
                        currentPlayerCount = td["currentPlayerCount"] as int? ?? 0,
                        registrationStart = DateTime.Parse(td["registrationStart"]?.ToString() ?? DateTime.Now.ToString("o")),
                        registrationEnd = DateTime.Parse(td["registrationEnd"]?.ToString() ?? DateTime.Now.AddHours(2).ToString("o")),
                        rounds = td["rounds"] as int? ?? 0,
                        currentRound = td["currentRound"] as int? ?? 0,
                        prizePool = td["prizePool"] as int? ?? 0,
                        entryFee = td["entryFee"] as int? ?? 0,
                        organizerId = td["organizerId"]?.ToString() ?? "",
                        createdAt = DateTime.Parse(td["createdAt"]?.ToString() ?? DateTime.Now.ToString("o")),
                        updatedAt = DateTime.Parse(td["updatedAt"]?.ToString() ?? DateTime.Now.ToString("o"))
                    };

                    if (td["startTime"] != null && !string.IsNullOrEmpty(td["startTime"]?.ToString()))
                        tournament.startTime = DateTime.Parse(td["startTime"]?.ToString());
                    if (td["endTime"] != null && !string.IsNullOrEmpty(td["endTime"]?.ToString()))
                        tournament.endTime = DateTime.Parse(td["endTime"]?.ToString());

                    // 导入玩家
                    if (td.Contains("registeredPlayers"))
                    {
                        foreach (Dictionary pd in (ArrayList)td["registeredPlayers"])
                        {
                            tournament.registeredPlayers.Add(new TournamentPlayer
                            {
                                playerId = pd["playerId"]?.ToString() ?? "",
                                playerName = pd["playerName"]?.ToString() ?? "",
                                seedNumber = pd["seedNumber"] as int? ?? 0,
                                score = pd["score"] as int? ?? 0,
                                wins = pd["wins"] as int? ?? 0,
                                losses = pd["losses"] as int? ?? 0,
                                matchesPlayed = pd["matchesPlayed"] as int? ?? 0,
                                isEliminated = pd["isEliminated"] as bool? ?? false,
                                hasLostOnce = pd["hasLostOnce"] as bool? ?? false,
                                registrationTime = DateTime.Parse(pd["registrationTime"]?.ToString() ?? DateTime.Now.ToString("o")),
                                matchHistory = pd["matchHistory"] != null ? new List<string>((ArrayList)pd["matchHistory"]) : new List<string>()
                            });
                        }
                    }

                    // 导入比赛
                    if (td.Contains("matches"))
                    {
                        foreach (Dictionary md in (ArrayList)td["matches"])
                        {
                            var match = new TournamentMatch
                            {
                                matchId = md["matchId"]?.ToString() ?? "",
                                roundNumber = md["roundNumber"] as int? ?? 0,
                                matchNumber = md["matchNumber"] as int? ?? 0,
                                stage = (TournamentStage)(md["stage"] as int? ?? 0),
                                player1Id = md["player1Id"]?.ToString() ?? "",
                                player2Id = md["player2Id"]?.ToString() ?? "",
                                winnerId = md["winnerId"]?.ToString() ?? "",
                                player1Score = md["player1Score"] as int? ?? 0,
                                player2Score = md["player2Score"] as int? ?? 0,
                                isCompleted = md["isCompleted"] as bool? ?? false,
                                scheduledTime = DateTime.Parse(md["scheduledTime"]?.ToString() ?? DateTime.Now.ToString("o"))
                            };
                            if (md["completedTime"] != null && !string.IsNullOrEmpty(md["completedTime"]?.ToString()))
                                match.completedTime = DateTime.Parse(md["completedTime"]?.ToString());
                            tournament.matches.Add(match);
                        }
                    }

                    // 导入奖励
                    if (td.Contains("rewards"))
                    {
                        foreach (Dictionary rd in (ArrayList)td["rewards"])
                        {
                            tournament.rewards.Add(new TournamentReward
                            {
                                rankStart = rd["rankStart"] as int? ?? 0,
                                rankEnd = rd["rankEnd"] as int? ?? 0,
                                rewardType = rd["rewardType"]?.ToString() ?? "",
                                rewardId = rd["rewardId"]?.ToString() ?? "",
                                rewardAmount = rd["rewardAmount"] as int? ?? 0
                            });
                        }
                    }

                    // 导入分组
                    if (td.Contains("groups"))
                    {
                        foreach (Dictionary gd in (ArrayList)td["groups"])
                        {
                            var group = new TournamentGroup
                            {
                                groupId = gd["groupId"]?.ToString() ?? "",
                                groupName = gd["groupName"]?.ToString() ?? ""
                            };
                            if (gd.Contains("playerIds"))
                            {
                                foreach (string pid in (ArrayList)gd["playerIds"])
                                {
                                    group.playerIds.Add(pid);
                                }
                            }
                            tournament.groups.Add(group);
                        }
                    }

                    core.Tournaments[tournament.tournamentId] = tournament;
                }
            }

            // 恢复活动锦标赛
            if (data.Contains("activeTournamentIds"))
            {
                core.ActiveTournaments.Clear();
                foreach (string tid in (ArrayList)data["activeTournamentIds"])
                {
                    if (core.Tournaments.TryGetValue(tid, out var tournament))
                    {
                        core.ActiveTournaments.Add(tournament);
                    }
                }
            }

            // 导入玩家进度
            if (data.Contains("playerProgress"))
            {
                foreach (Dictionary pd in (ArrayList)data["playerProgress"])
                {
                    var participated = pd["participatedTournaments"] as ArrayList;
                    var progress = new TournamentProgress
                    {
                        playerId = pd["playerId"]?.ToString() ?? "",
                        participatedTournaments = participated != null 
                            ? new List<string>(participated.Cast<string>()) 
                            : new List<string>()
                    };

                    // 导入最近记录
                    if (pd.Contains("recentRecords"))
                    {
                        foreach (Dictionary rd in (ArrayList)pd["recentRecords"])
                        {
                            progress.recentRecords.Add(new PlayerTournamentRecord
                            {
                                playerId = rd["playerId"]?.ToString() ?? "",
                                tournamentId = rd["tournamentId"]?.ToString() ?? "",
                                tournamentName = rd["tournamentName"]?.ToString() ?? "",
                                finalRank = rd["finalRank"] as int? ?? 0,
                                score = rd["score"] as int? ?? 0,
                                wins = rd["wins"] as int? ?? 0,
                                losses = rd["losses"] as int? ?? 0,
                                participatedAt = DateTime.Parse(rd["participatedAt"]?.ToString() ?? DateTime.Now.ToString("o"))
                            });
                        }
                    }

                    // 导入统计
                    if (pd.Contains("statistics"))
                    {
                        var sd = (Dictionary)pd["statistics"];
                        progress.statistics = new TournamentStatistics
                        {
                            playerId = sd["playerId"]?.ToString() ?? "",
                            totalTournaments = sd["totalTournaments"] as int? ?? 0,
                            firstPlace = sd["firstPlace"] as int? ?? 0,
                            secondPlace = sd["secondPlace"] as int? ?? 0,
                            thirdPlace = sd["thirdPlace"] as int? ?? 0,
                            top4 = sd["top4"] as int? ?? 0,
                            top8 = sd["top8"] as int? ?? 0,
                            top16 = sd["top16"] as int? ?? 0,
                            totalWins = sd["totalWins"] as int? ?? 0,
                            totalLosses = sd["totalLosses"] as int? ?? 0,
                            highestRank = sd["highestRank"] as int? ?? 0,
                            totalPrizeWon = sd["totalPrizeWon"] as int? ?? 0
                        };
                    }

                    core.PlayerProgress[progress.playerId] = progress;
                }
            }

            GD.Print($"[TournamentPersistenceSystem] 导入 {core.Tournaments.Count} 个锦标赛, {core.PlayerProgress.Count} 个玩家进度");
        }

        /// <summary>
        /// 保存数据到文件
        /// </summary>
        public void SaveToFile()
        {
            try
            {
                var savePath = "user://tournament_save.dat";
                using var file = Godot.FileAccess.Open(savePath, Godot.FileAccess.ModeFlags.Write);
                
                var data = ExportSaveData();
                var json = new Godot.Json();
                var jsonString = json.Stringify(new Godot.Variant(data));
                
                file.StoreString(jsonString);
                GD.Print("[TournamentPersistenceSystem] 数据保存完成");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[TournamentPersistenceSystem] 保存数据时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 从文件加载数据
        /// </summary>
        public void LoadFromFile()
        {
            try
            {
                var savePath = "user://tournament_save.dat";
                if (Godot.FileAccess.FileExists(savePath))
                {
                    using var file = Godot.FileAccess.Open(savePath, Godot.FileAccess.ModeFlags.Read);
                    var jsonString = file.GetAsText();
                    var json = new Godot.Json();
                    var result = json.Parse(jsonString);
                    if (result == Godot.Error.Ok && json.Data is Dictionary data)
                    {
                        ImportSaveData(data);
                        GD.Print("[TournamentPersistenceSystem] 数据加载完成");
                    }
                    else
                    {
                        GD.PrintErr("[TournamentPersistenceSystem] 数据解析失败");
                    }
                }
                else
                {
                    GD.Print("[TournamentPersistenceSystem] 无保存数据，开始新游戏");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[TournamentPersistenceSystem] 加载数据时出错: {ex.Message}");
            }
        }
    }
}
