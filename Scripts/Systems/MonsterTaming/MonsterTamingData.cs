using Godot;
using System;
using System.Collections.Generic;

public partial class MonsterTamingData : BaseSystem
{
    // Tamed Monsters
    public Dictionary<int, TamedMonster> TamedMonsters = new Dictionary<int, TamedMonster>();
    
    // Capture Attempts
    public int TotalCaptureAttempts = 0;
    public int SuccessfulCaptures = 0;
    
    // Statistics
    public int TotalMonstersTamed = 0;
    public int LegendaryCaptures = 0;
    public int EpicCaptures = 0;
    public int RareCaptures = 0;
    public int UncommonCaptures = 0;
    public int CommonCaptures = 0;
    
    // Active Capture
    public bool IsCapturing = false;
    public int CapturingMonsterId = -1;
    public float CaptureProgress = 0f;
    
    public class TamedMonster
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int BondLevel { get; set; }
        public int BattlesWon { get; set; }
        public DateTime TamedAt { get; set; }
        public Dictionary<string, int> Stats { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();

        // 已捕获的怪物
        var tamedMonstersList = new Godot.Collections.Array();
        foreach (var monster in TamedMonsters)
        {
            var monsterDict = new Dictionary
            {
                { "id", monster.Id },
                { "name", monster.Name },
                { "type", monster.Type },
                { "rarity", monster.Rarity },
                { "level", monster.Level },
                { "experience", monster.Experience },
                { "bond_level", monster.BondLevel },
                { "battles_won", monster.BattlesWon },
                { "tamed_at", monster.TamedAt.ToString("o") }
            };
            tamedMonstersList.Add(monsterDict);
        }
        data["tamed_monsters"] = tamedMonstersList;

        // 统计数据
        data["total_monsters_captured"] = TotalMonstersCaptured;
        data["total_battles_won"] = TotalBattlesWon;
        data["total_capture_attempts"] = TotalCaptureAttempts;
        data["successful_captures"] = SuccessfulCaptures;
        data["legendary_captures"] = LegendaryCaptures;

        // 活跃捕捉状态
        data["is_capturing"] = IsCapturing;
        data["capturing_monster_id"] = CapturingMonsterId;
        data["capture_progress"] = CaptureProgress;

        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        // 已捕获的怪物
        TamedMonsters = new List<TamedMonster>();
        if (data.Contains("tamed_monsters"))
        {
            var monstersArray = (Array)data["tamed_monsters"];
            foreach (Dictionary monsterDict in monstersArray)
            {
                var monster = new TamedMonster
                {
                    Id = (int)monsterDict["id"],
                    Name = (string)monsterDict["name"],
                    Type = (string)monsterDict["type"],
                    Rarity = (string)monsterDict["rarity"],
                    Level = (int)monsterDict["level"],
                    Experience = (int)monsterDict["experience"],
                    BondLevel = (int)monsterDict["bond_level"],
                    BattlesWon = (int)monsterDict["battles_won"]
                };
                if (monsterDict.Contains("tamed_at") && DateTime.TryParse(monsterDict["tamed_at"].ToString(), out var tamedAt))
                {
                    monster.TamedAt = tamedAt;
                }
                TamedMonsters.Add(monster);
            }
        }

        // 统计数据
        TotalMonstersCaptured = (int)data.GetValueOrDefault("total_monsters_captured", 0);
        TotalBattlesWon = (int)data.GetValueOrDefault("total_battles_won", 0);
        TotalCaptureAttempts = (int)data.GetValueOrDefault("total_capture_attempts", 0);
        SuccessfulCaptures = (int)data.GetValueOrDefault("successful_captures", 0);
        LegendaryCaptures = (int)data.GetValueOrDefault("legendary_captures", 0);

        // 活跃捕捉状态
        IsCapturing = (bool)data.GetValueOrDefault("is_capturing", false);
        CapturingMonsterId = (int)data.GetValueOrDefault("capturing_monster_id", -1);
        CaptureProgress = (float)data.GetValueOrDefault("capture_progress", 0f);
    }
}
