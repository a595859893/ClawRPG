using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ArenaTournamentSystem
{
    private ArenaTournamentData _data;
    private Random _random = new Random();
    
    public ArenaTournamentSystem()
    {
        _data = new ArenaTournamentData();
    }
    
    // 创建锦标赛
    public bool CreateTournament(string name, ArenaTournamentType type, int maxParticipants = 16)
    {
        if (_data.State != ArenaTournamentState.Completed && 
            _data.State != ArenaTournamentState.Cancelled &&
            _data.Participants.Count > 0)
        {
            GD.Print("Cannot create new tournament while one is in progress");
            return false;
        }
        
        _data = new ArenaTournamentData
        {
            TournamentName = name,
            TournamentType = type,
            State = ArenaTournamentState.Registration,
            MaxParticipants = maxParticipants,
            CurrentRound = 0
        };
        
        // 计算预期轮数
        switch (type)
        {
            case ArenaTournamentType.SingleElimination:
                _data.TotalRounds = (int)Math.Ceiling(Math.Log2(maxParticipants));
                break;
            case ArenaTournamentType.DoubleElimination:
                _data.TotalRounds = (int)(Math.Ceiling(Math.Log2(maxParticipants)) * 2);
                break;
            case ArenaTournamentType.RoundRobin:
                _data.TotalRounds = maxParticipants - 1;
                break;
            case ArenaTournamentType.Swiss:
                _data.TotalRounds = (int)Math.Ceiling(Math.Log2(maxParticipants));
                break;
        }
        
        return true;
    }
    
    // 注册选手
    public bool RegisterParticipant(int playerId, string playerName)
    {
        if (_data.State != ArenaTournamentState.Registration)
        {
            GD.Print("Registration is not open");
            return false;
        }
        
        if (_data.Participants.Count >= _data.MaxParticipants)
        {
            GD.Print("Tournament is full");
            return false;
        }
        
        if (_data.Participants.Any(p => p.Id == playerId))
        {
            GD.Print("Player already registered");
            return false;
        }
        
        _data.Participants.Add(new ArenaTournamentParticipant
        {
            Id = playerId,
            Name = playerName,
            Seed = _data.Participants.Count + 1
        });
        
        return true;
    }
    
    // 开始抽签
    public bool StartSeeding()
    {
        if (_data.State != ArenaTournamentState.Registration)
        {
            GD.Print("Cannot start seeding");
            return false;
        }
        
        if (_data.Participants.Count < _data.MinParticipants)
        {
            GD.Print("Not enough participants");
            return false;
        }
        
        // 随机打乱种子
        var shuffled = _data.Participants.OrderBy(x => _random.Next()).ToList();
        for (int i = 0; i < shuffled.Count; i++)
        {
            shuffled[i].Seed = i + 1;
        }
        _data.Participants = shuffled;
        
        _data.State = ArenaTournamentState.Seeding;
        
        // 根据类型生成分组
        if (_data.TournamentType == ArenaTournamentType.RoundRobin || 
            _data.TournamentType == ArenaTournamentType.Swiss)
        {
            GenerateGroups();
        }
        
        return true;
    }
    
    // 生成分组（循环赛/瑞士制）
    private void GenerateGroups()
    {
        int participantCount = _data.Participants.Count;
        int groupCount = 4;
        int playersPerGroup = participantCount / groupCount;
        
        _data.GroupA.Clear();
        _data.GroupB.Clear();
        _data.GroupC.Clear();
        _data.GroupD.Clear();
        
        for (int i = 0; i < participantCount; i++)
        {
            int groupId = i / playersPerGroup;
            _data.Participants[i].GroupId = groupId;
            
            switch (groupId)
            {
                case 0: _data.GroupA.Add(i); break;
                case 1: _data.GroupB.Add(i); break;
                case 2: _data.GroupC.Add(i); break;
                default: _data.GroupD.Add(i); break;
            }
        }
    }
    
    // 开始锦标赛
    public bool StartTournament()
    {
        if (_data.State != ArenaTournamentState.Seeding)
        {
            GD.Print("Cannot start tournament");
            return false;
        }
        
        _data.State = ArenaTournamentState.InProgress;
        _data.CurrentRound = 1;
        _data.TotalTournaments++;
        
        // 生成第一轮比赛
        GenerateRoundMatches();
        
        return true;
    }
    
    // 生成轮次比赛
    private void GenerateRoundMatches()
    {
        _data.Matches.Clear();
        
        switch (_data.TournamentType)
        {
            case ArenaTournamentType.SingleElimination:
                GenerateSingleEliminationMatches();
                break;
            case ArenaTournamentType.DoubleElimination:
                GenerateDoubleEliminationMatches();
                break;
            case ArenaTournamentType.RoundRobin:
                GenerateRoundRobinMatches();
                break;
            case ArenaTournamentType.Swiss:
                GenerateSwissMatches();
                break;
        }
    }
    
    // 单败淘汰赛生成
    private void GenerateSingleEliminationMatches()
    {
        int participantCount = _data.Participants.Count;
        int rounds = (int)Math.Ceiling(Math.Log2(participantCount));
        int bracketSize = (int)Math.Pow(2, rounds);
        
        int matchId = 0;
        int round1Matches = bracketSize / 2;
        
        // 第一轮
        for (int i = 0; i < round1Matches; i++)
        {
            int player1Idx = i * 2;
            int player2Idx = i * 2 + 1;
            
            var match = new ArenaTournamentMatch
            {
                MatchId = matchId++,
                Round = 1,
                Player1Id = player1Idx < participantCount ? player1Idx : -1,
                Player2Id = player2Idx < participantCount ? player2Idx : -1,
                MatchState = (player1Idx < participantCount && player2Idx < participantCount) 
                    ? ArenaTournamentMatchState.Ready 
                    : ArenaTournamentMatchState.Bye
            };
            
            // 自动处理轮空
            if (match.Player1Id == -1 || match.Player2Id == -1)
            {
                int winnerIdx = match.Player1Id >= 0 ? match.Player1Id : match.Player2Id;
                match.WinnerId = winnerIdx;
                match.IsCompleted = true;
                _data.Participants[winnerIdx].Placement = 1;
            }
            
            _data.Matches.Add(match);
        }
        
        // 后续轮次（空壳，等待填充）
        for (int round = 2; round <= rounds; round++)
        {
            int matchesInRound = bracketSize / (int)Math.Pow(2, round);
            for (int i = 0; i < matchesInRound; i++)
            {
                _data.Matches.Add(new ArenaTournamentMatch
                {
                    MatchId = matchId++,
                    Round = round,
                    MatchState = ArenaTournamentMatchState.Pending
                });
            }
        }
    }
    
    // 双败淘汰赛生成
    private void GenerateDoubleEliminationMatches()
    {
        // 类似单败淘汰，但标记胜者组/败者组
        GenerateSingleEliminationMatches();
        foreach (var p in _data.Participants)
        {
            p.IsWinnerBracket = true;
        }
    }
    
    // 循环赛生成
    private void GenerateRoundRobinMatches()
    {
        int participantCount = _data.Participants.Count;
        int matchId = 0;
        
        for (int round = 0; round < participantCount - 1; round++)
        {
            for (int i = 0; i < participantCount / 2; i++)
            {
                int player1Idx = i;
                int player2Idx = participantCount - 1 - i;
                
                // 轮换对阵
                if (round % 2 == 1)
                {
                    (player1Idx, player2Idx) = (player2Idx, player1Idx);
                }
                
                if (i == 0)
                {
                    player2Idx = (participantCount - 1 - round) % (participantCount - 1);
                    if (player2Idx >= player1Idx) player2Idx++;
                }
                
                _data.Matches.Add(new ArenaTournamentMatch
                {
                    MatchId = matchId++,
                    Round = round + 1,
                    Player1Id = player1Idx,
                    Player2Id = player2Idx,
                    MatchState = ArenaTournamentMatchState.Ready
                });
            }
        }
    }
    
    // 瑞士制生成
    private void GenerateSwissMatches()
    {
        // 第一轮随机配对
        if (_data.CurrentRound == 1)
        {
            var shuffled = _data.Participants.OrderBy(x => _random.Next()).ToList();
            int matchId = 0;
            
            for (int i = 0; i < shuffled.Count / 2; i++)
            {
                _data.Matches.Add(new ArenaTournamentMatch
                {
                    MatchId = matchId++,
                    Round = 1,
                    Player1Id = shuffled[i * 2].Id,
                    Player2Id = shuffled[i * 2 + 1].Id,
                    MatchState = ArenaTournamentMatchState.Ready
                });
            }
        }
        else
        {
            // 根据战绩配对
            GenerateSwissPairings();
        }
    }
    
    // 瑞士制配对
    private void GenerateSwissPairings()
    {
        // 按积分排序
        var sorted = _data.Participants
            .Where(p => !p.IsEliminated)
            .OrderByDescending(p => p.Points)
            .ThenByDescending(p => p.GoalsFor - p.GoalsAgainst)
            .ToList();
        
        int matchId = _data.Matches.Count;
        
        for (int i = 0; i < sorted.Count / 2; i++)
        {
            _data.Matches.Add(new ArenaTournamentMatch
            {
                MatchId = matchId++,
                Round = _data.CurrentRound,
                Player1Id = sorted[i * 2].Id,
                Player2Id = sorted[i * 2 + 1].Id,
                MatchState = ArenaTournamentMatchState.Ready
            });
        }
    }
    
    // 完成比赛
    public bool CompleteMatch(int matchId, int player1Score, int player2Score)
    {
        var match = _data.Matches.FirstOrDefault(m => m.MatchId == matchId);
        if (match == null || match.IsCompleted)
        {
            return false;
        }
        
        match.Player1Score = player1Score;
        match.Player2Score = player2Score;
        match.IsCompleted = true;
        
        if (player1Score == player2Score)
        {
            match.IsDraw = true;
            match.WinnerId = -1;
            
            var player1 = _data.Participants.FirstOrDefault(p => p.Id == match.Player1Id);
            var player2 = _data.Participants.FirstOrDefault(p => p.Id == match.Player2Id);
            
            if (player1 != null)
            {
                player1.Draws++;
                player1.Points += _data.PointsPerDraw;
            }
            if (player2 != null)
            {
                player2.Draws++;
                player2.Points += _data.PointsPerDraw;
            }
            
            _data.TotalDraws++;
        }
        else
        {
            match.WinnerId = player1Score > player2Score ? match.Player1Id : match.Player2Id;
            int loserId = player1Score > player2Score ? match.Player2Id : match.Player1Id;
            
            var winner = _data.Participants.FirstOrDefault(p => p.Id == match.WinnerId);
            var loser = _data.Participants.FirstOrDefault(p => p.Id == loserId);
            
            if (winner != null)
            {
                winner.Wins++;
                winner.Points += _data.PointsPerWin;
                winner.GoalsFor += player1Score > player2Score ? player1Score : player2Score;
                winner.GoalsAgainst += player1Score > player2Score ? player2Score : player1Score;
            }
            if (loser != null)
            {
                loser.Losses++;
                loser.Points += _data.PointsPerLoss;
                loser.GoalsFor += player1Score > player2Score ? player2Score : player1Score;
                loser.GoalsAgainst += player1Score > player2Score ? player1Score : player2Score;
                
                // 单败淘汰中被淘汰
                if (_data.TournamentType == ArenaTournamentType.SingleElimination)
                {
                    loser.IsEliminated = true;
                }
            }
            
            _data.TotalWins++;
            _data.TotalLosses++;
            
            // 单败淘汰中胜者进入下一轮
            if (_data.TournamentType == ArenaTournamentType.SingleElimination)
            {
                AdvanceSingleElimination(match);
            }
        }
        
        _data.TotalMatchesPlayed++;
        
        // 检查是否需要进入下一轮
        CheckRoundCompletion();
        
        return true;
    }
    
    // 单败淘汰赛晋级
    private void AdvanceSingleElimination(ArenaTournamentMatch match)
    {
        int currentRound = match.Round;
        int matchesInRound = (int)(_data.Participants.Count / Math.Pow(2, currentRound));
        int matchIndex = _data.Matches.FindIndex(m => m.Round == currentRound && m.MatchId == match.MatchId);
        int nextMatchIndex = matchIndex + matchesInRound;
        
        if (nextMatchIndex < _data.Matches.Count)
        {
            var nextMatch = _data.Matches[nextMatchIndex];
            if (nextMatch.Player1Id == -1)
            {
                nextMatch.Player1Id = match.WinnerId;
                nextMatch.MatchState = ArenaTournamentMatchState.Ready;
            }
            else if (nextMatch.Player2Id == -1)
            {
                nextMatch.Player2Id = match.WinnerId;
                nextMatch.MatchState = ArenaTournamentMatchState.Ready;
            }
        }
        
        // 检查是否产生冠军
        if (currentRound == _data.TotalRounds)
        {
            var champion = _data.Participants.FirstOrDefault(p => p.Id == match.WinnerId);
            if (champion != null)
            {
                champion.Placement = 1;
                CompleteTournament();
            }
        }
    }
    
    // 检查轮次完成
    private void CheckRoundCompletion()
    {
        var currentRoundMatches = _data.Matches.Where(m => m.Round == _data.CurrentRound);
        if (currentRoundMatches.All(m => m.IsCompleted))
        {
            // 循环赛/瑞士制进入下一轮
            if (_data.TournamentType == ArenaTournamentType.RoundRobin ||
                _data.TournamentType == ArenaTournamentType.Swiss)
            {
                if (_data.CurrentRound < _data.TotalRounds)
                {
                    _data.CurrentRound++;
                    GenerateRoundMatches();
                }
                else
                {
                    CompleteTournament();
                }
            }
        }
    }
    
    // 完成锦标赛
    private void CompleteTournament()
    {
        _data.State = ArenaTournamentState.Completed;
        _data.TournamentsParticipated++;
        
        // 计算排名
        var sorted = _data.Participants.OrderByDescending(p => p.Points).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].Placement = i + 1;
        }
        
        // 记录历史
        var champion = _data.Participants.FirstOrDefault(p => p.Placement == 1);
        if (champion != null)
        {
            _data.TournamentsWon++;
            var (gold, exp) = ArenaTournamentDatabase.GetReward(1);
            _data.History.Add(new ArenaTournamentHistory
            {
                TournamentName = _data.TournamentName,
                Type = _data.TournamentType,
                Placement = 1,
                Participants = _data.Participants.Count,
                Reward = gold,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }
    }
    
    // 获取当前轮次比赛
    public List<ArenaTournamentMatch> GetCurrentRoundMatches()
    {
        return _data.Matches.Where(m => m.Round == _data.CurrentRound).ToList();
    }
    
    // 获取选手信息
    public ArenaTournamentParticipant GetParticipant(int playerId)
    {
        return _data.Participants.FirstOrDefault(p => p.Id == playerId);
    }
    
    // 获取排名
    public List<ArenaTournamentParticipant> GetRankings()
    {
        return _data.Participants.OrderByDescending(p => p.Points).ToList();
    }
    
    // 获取统计
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "total_tournaments", _data.TotalTournaments },
            { "tournaments_won", _data.TournamentsWon },
            { "tournaments_participated", _data.TournamentsParticipated },
            { "total_matches", _data.TotalMatchesPlayed },
            { "total_wins", _data.TotalWins },
            { "total_losses", _data.TotalLosses },
            { "total_draws", _data.TotalDraws },
            { "highest_placement", _data.HighestPlacement }
        };
    }
    
    // 获取数据
    public ArenaTournamentData GetData() => _data;
}
