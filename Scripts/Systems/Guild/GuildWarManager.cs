using Godot;
using System;
using System.Collections.Generic;

public partial class GuildWarManager : BaseSystem
{
    public static GuildWarManager Instance { get; private set; }
    
    // Guild war data
    private Dictionary<int, GuildWarData> guildWars = new Dictionary<int, GuildWarData>();
    private List<GuildWarEntry> warHistory = new List<GuildWarEntry>();
    private int nextWarId = 1;
    
    // War configurations
    private int maxConcurrentWars = 5;
    private int warDurationHours = 24;
    private int preparationHours = 2;
    
    // Signals
    public static void WarStarted(int warId) { }
    public static void WarEnded(int warId, int winningGuildId) { }
    public static void BattleJoined(int warId, int guildId, int playerId) { }
    public static void PointsUpdated(int warId, int guildId, int points) { }
    
    public override void _Ready()
    {
        Instance = this;
    }
    
    public class GuildWarData
    {
        public int warId;
        public int attackerGuildId;
        public int defenderGuildId;
        public DateTime startTime;
        public DateTime endTime;
        public GuildWarState state;
        public int attackerPoints;
        public int defenderPoints;
        public List<WarParticipant> attackerParticipants = new List<WarParticipant>();
        public List<WarParticipant> defenderParticipants = new List<WarParticipant>();
        public Dictionary<int, int> contributions = new Dictionary<int, int>(); // playerId -> points
        
        public GuildWarData(int warId, int attacker, int defender)
        {
            this.warId = warId;
            this.attackerGuildId = attacker;
            this.defenderGuildId = defender;
            this.startTime = DateTime.Now;
            this.endTime = startTime.AddHours(24);
            this.state = GuildWarState.Preparation;
        }
    }
    
    public class WarParticipant
    {
        public int playerId;
        public string playerName;
        public int kills;
        public int deaths;
        public int damageDealt;
        public int healingDone;
        public int contribution;
        
        public WarParticipant(int playerId, string playerName)
        {
            this.playerId = playerId;
            this.playerName = playerName;
        }
    }
    
    public class GuildWarEntry
    {
        public int warId;
        public int attackerGuildId;
        public int defenderGuildId;
        public int winningGuildId;
        public int attackerPoints;
        public int defenderPoints;
        public DateTime startTime;
        public DateTime endTime;
    }
    
    public enum GuildWarState
    {
        Preparation,
        Active,
        Ended
    }
    
    // Declare signals
public delegate void WarStartedSignal(int warId, int attackerGuildId, int defenderGuildId);
public delegate void WarEndedSignal(int warId, int winningGuildId, int attackerPoints, int defenderPoints);
public delegate void PointsUpdatedSignal(int warId, int guildId, int points);
    
    // Declare signal invocations
    private void EmitWarStarted(int warId, int attackerGuildId, int defenderGuildId)
    {
        EmitSignal(nameof(WarStartedSignal), warId, attackerGuildId, defenderGuildId);
    }
    
    private void EmitWarEnded(int warId, int winningGuildId, int attackerPoints, int defenderPoints)
    {
        EmitSignal(nameof(WarEndedSignal), warId, winningGuildId, attackerPoints, defenderPoints);
    }
    
    private void EmitPointsUpdated(int warId, int guildId, int points)
    {
        EmitSignal(nameof(PointsUpdatedSignal), warId, guildId, points);
    }
    
    // Create a new guild war
    public int CreateWar(int attackerGuildId, int defenderGuildId)
    {
        if (guildWars.Count >= maxConcurrentWars)
        {
            GD.Print("Maximum concurrent wars reached");
            return -1;
        }
        
        int warId = nextWarId++;
        GuildWarData war = new GuildWarData(warId, attackerGuildId, defenderGuildId);
        guildWars.Add(warId, war);
        
        EmitWarStarted(warId, attackerGuildId, defenderGuildId);
        GD.Print($"Guild War {warId} created: Guild {attackerGuildId} vs Guild {defenderGuildId}");
        
        // Auto-start war after preparation period
        CallDeferred(nameof(StartWarAfterDelay), warId);
        
        return warId;
    }
    
    private void StartWarAfterDelay(int warId)
    {
        await ToSignal(GetTree().CreateTimer(preparationHours), "timeout");
        
        if (guildWars.ContainsKey(warId))
        {
            guildWars[warId].state = GuildWarState.Active;
            GD.Print($"Guild War {warId} started!");
        }
    }
    
    // Join a guild war as a player
    public bool JoinWar(int warId, int guildId, int playerId, string playerName)
    {
        if (!guildWars.ContainsKey(warId))
            return false;
            
        GuildWarData war = guildWars[warId];
        
        if (war.state != GuildWarState.Active)
            return false;
        
        List<WarParticipant> participants = (war.attackerGuildId == guildId) 
            ? war.attackerParticipants 
            : war.defenderParticipants;
        
        // Check if already joined
        foreach (var p in participants)
        {
            if (p.playerId == playerId)
                return false;
        }
        
        WarParticipant newParticipant = new WarParticipant(playerId, playerName);
        participants.Add(newParticipant);
        
        GD.Print($"Player {playerName} joined Guild War {warId} for guild {guildId}");
        return true;
    }
    
    // Record battle results
    public void RecordBattle(int warId, int guildId, int playerId, int kills, int deaths, int damage, int healing)
    {
        if (!guildWars.ContainsKey(warId))
            return;
            
        GuildWarData war = guildWars[warId];
        
        if (war.state != GuildWarState.Active)
            return;
        
        List<WarParticipant> participants = (war.attackerGuildId == guildId) 
            ? war.attackerParticipants 
            : war.defenderParticipants;
        
        WarParticipant participant = null;
        foreach (var p in participants)
        {
            if (p.playerId == playerId)
            {
                participant = p;
                break;
            }
        }
        
        if (participant == null)
            return;
        
        // Update stats
        participant.kills += kills;
        participant.deaths += deaths;
        participant.damageDealt += damage;
        participant.healingDone += healing;
        
        // Calculate contribution points
        int points = kills * 10 + damage / 100 + healing / 50;
        participant.contribution += points;
        
        // Update guild total points
        if (war.attackerGuildId == guildId)
            war.attackerPoints += points;
        else
            war.defenderPoints += points;
        
        // Update overall contributions
        if (!war.contributions.ContainsKey(playerId))
            war.contributions[playerId] = 0;
        war.contributions[playerId] += points;
        
        EmitPointsUpdated(warId, guildId, (war.attackerGuildId == guildId) ? war.attackerPoints : war.defenderPoints);
    }
    
    // Get war info
    public GuildWarData GetWarInfo(int warId)
    {
        if (guildWars.ContainsKey(warId))
            return guildWars[warId];
        return null;
    }
    
    // Get player's contribution in a war
    public int GetPlayerContribution(int warId, int playerId)
    {
        if (guildWars.ContainsKey(warId) && guildWars[warId].contributions.ContainsKey(playerId))
            return guildWars[warId].contributions[playerId];
        return 0;
    }
    
    // Get active wars for a guild
    public List<GuildWarData> GetActiveWarsForGuild(int guildId)
    {
        List<GuildWarData> result = new List<GuildWarData>();
        foreach (var war in guildWars.Values)
        {
            if (war.state == GuildWarState.Active && 
                (war.attackerGuildId == guildId || war.defenderGuildId == guildId))
            {
                result.Add(war);
            }
        }
        return result;
    }
    
    // End war and get results
    public void EndWar(int warId)
    {
        if (!guildWars.ContainsKey(warId))
            return;
            
        GuildWarData war = guildWars[warId];
        
        if (war.state == GuildWarState.Ended)
            return;
        
        war.state = GuildWarState.Ended;
        
        int winner = (war.attackerPoints >= war.defenderPoints) 
            ? war.attackerGuildId 
            : war.defenderGuildId;
        
        // Add to history
        GuildWarEntry entry = new GuildWarEntry
        {
            warId = war.warId,
            attackerGuildId = war.attackerGuildId,
            defenderGuildId = war.defenderGuildId,
            winningGuildId = winner,
            attackerPoints = war.attackerPoints,
            defenderPoints = war.defenderPoints,
            startTime = war.startTime,
            endTime = DateTime.Now
        };
        warHistory.Add(entry);
        
        EmitWarEnded(warId, winner, war.attackerPoints, war.defenderPoints);
        
        GD.Print($"Guild War {warId} ended! Winner: Guild {winner} ({war.attackerPoints} vs {war.defenderPoints})");
    }
    
    // Process wars (check for expiration)
    public void ProcessWars()
    {
        DateTime now = DateTime.Now;
        List<int> warsToEnd = new List<int>();
        
        foreach (var war in guildWars.Values)
        {
            if (war.state == GuildWarState.Active && now >= war.endTime)
            {
                warsToEnd.Add(war.warId);
            }
        }
        
        foreach (int warId in warsToEnd)
        {
            EndWar(warId);
        }
    }
    
    // Get war history
    public List<GuildWarEntry> GetWarHistory()
    {
        return new List<GuildWarEntry>(warHistory);
    }
    
    // Get war leaderboard
    public List<KeyValuePair<int, int>> GetWarLeaderboard(int warId)
    {
        if (!guildWars.ContainsKey(warId))
            return new List<KeyValuePair<int, int>>();
        
        GuildWarData war = guildWars[warId];
        List<KeyValuePair<int, int>> leaderboard = new List<KeyValuePair<int, int>>();
        
        foreach (var contribution in war.contributions)
        {
            leaderboard.Add(contribution);
        }
        
        leaderboard.Sort((a, b) => b.Value.CompareTo(a.Value));
        return leaderboard;
    }
    
    // Save war data
    public Dictionary SaveData()
    {
        Dictionary data = new Dictionary<string, object>();
        
        // Save active wars
        Array warsArray = new Godot.Array();
        foreach (var war in guildWars.Values)
        {
            Dictionary warDict = new Dictionary<string, object>();
            warDict["warId"] = war.warId;
            warDict["attackerGuildId"] = war.attackerGuildId;
            warDict["defenderGuildId"] = war.defenderGuildId;
            warDict["startTime"] = war.startTime.ToString();
            warDict["endTime"] = war.endTime.ToString();
            warDict["state"] = (int)war.state;
            warDict["attackerPoints"] = war.attackerPoints;
            warDict["defenderPoints"] = war.defenderPoints;
            warsArray.Append(warDict);
        }
        data["wars"] = warsArray;
        
        // Save history
        Array historyArray = new Godot.Array();
        foreach (var entry in warHistory)
        {
            Dictionary entryDict = new Dictionary<string, object>();
            entryDict["warId"] = entry.warId;
            entryDict["attackerGuildId"] = entry.attackerGuildId;
            entryDict["defenderGuildId"] = entry.defenderGuildId;
            entryDict["winningGuildId"] = entry.winningGuildId;
            entryDict["attackerPoints"] = entry.attackerPoints;
            entryDict["defenderPoints"] = entry.defenderPoints;
            entryDict["startTime"] = entry.startTime.ToString();
            entryDict["endTime"] = entry.endTime.ToString();
            historyArray.Append(entryDict);
        }
        data["history"] = historyArray;
        
        data["nextWarId"] = nextWarId;
        
        return data;
    }
    
    // Load war data
    public void LoadData(Dictionary data)
    {
        if (data == null) return;
        
        guildWars.Clear();
        warHistory.Clear();
        
        if (data.Contains("wars"))
        {
            Godot.Array warsArray = (Godot.Array)data["wars"];
            foreach (Dictionary warDict in warsArray)
            {
                GuildWarData war = new GuildWarData(
                    (int)warDict["warId"],
                    (int)warDict["attackerGuildId"],
                    (int)warDict["defenderGuildId"]
                );
                war.startTime = DateTime.Parse((string)warDict["startTime"]);
                war.endTime = DateTime.Parse((string)warDict["endTime"]);
                war.state = (GuildWarState)(int)warDict["state"];
                war.attackerPoints = (int)warDict["attackerPoints"];
                war.defenderPoints = (int)warDict["defenderPoints"];
                guildWars[war.warId] = war;
            }
        }
        
        if (data.Contains("history"))
        {
            Godot.Array historyArray = (Godot.Array)data["history"];
            foreach (Dictionary entryDict in historyArray)
            {
                GuildWarEntry entry = new GuildWarEntry();
                entry.warId = (int)entryDict["warId"];
                entry.attackerGuildId = (int)entryDict["attackerGuildId"];
                entry.defenderGuildId = (int)entryDict["defenderGuildId"];
                entry.winningGuildId = (int)entryDict["winningGuildId"];
                entry.attackerPoints = (int)entryDict["attackerPoints"];
                entry.defenderPoints = (int)entryDict["defenderPoints"];
                entry.startTime = DateTime.Parse((string)entryDict["startTime"]);
                entry.endTime = DateTime.Parse((string)entryDict["endTime"]);
                warHistory.Add(entry);
            }
        }
        
        if (data.Contains("nextWarId"))
        {
            nextWarId = (int)data["nextWarId"];
        }
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return SaveData();
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        LoadData(data);
    }
}
