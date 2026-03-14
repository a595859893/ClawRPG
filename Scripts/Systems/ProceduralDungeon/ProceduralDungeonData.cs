using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ProceduralDungeon
{
    /// <summary>
    /// 地下城房间类型
    /// </summary>
    public enum RoomType
    {
        Entrance,        // 入口
        Corridor,       // 走廊
        Combat,          // 战斗房间
        Treasure,        // 宝箱房间
        Elite,           // 精英敌人
        Boss,            // Boss房间
        Puzzle,          // 谜题房间
        Merchant,        // 商人房间
        Rest,            // 休息点/篝火
        Secret,          // 密室
        Trap,            // 陷阱房间
        Event            // 随机事件
    }

    /// <summary>
    /// 房间难度等级
    /// </summary>
    public enum RoomDifficulty
    {
        Easy,        // 简单
        Normal,      // 普通
        Hard,        // 困难
        Nightmare,   // 噩梦
        Legendary    // 传奇
    }

    /// <summary>
    /// 地下城楼层配置
    /// </summary>
    public class DungeonFloorConfig
    {
        public int FloorNumber { get; set; }
        public string FloorName { get; set; }
        public int MinRooms { get; set; }
        public int MaxRooms { get; set; }
        public float RoomScaleFactor { get; set; }  // 房间规模因子
        public float EnemyStrengthMultiplier { get; set; }
        public float TreasureMultiplier { get; set; }
        public List<RoomType> AllowedRoomTypes { get; set; }
        public List<RoomDifficulty> AllowedDifficulties { get; set; }
        
        public DungeonFloorConfig()
        {
            FloorName = "Floor";
            AllowedRoomTypes = new List<RoomType>();
            AllowedDifficulties = new List<RoomDifficulty>();
        }
    }

    /// <summary>
    /// 地下城房间数据
    /// </summary>
    public class DungeonRoom
    {
        public string RoomId { get; set; }
        public RoomType Type { get; set; }
        public RoomDifficulty Difficulty { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
        public List<string> ConnectedRooms { get; set; }
        public List<string> Enemies { get; set; }
        public string TreasureId { get; set; }
        public string EventId { get; set; }
        public bool IsCleared { get; set; }
        public bool IsDiscovered { get; set; }
        public Dictionary<string, float> RoomModifiers { get; set; }
        
        public DungeonRoom()
        {
            ConnectedRooms = new List<string>();
            Enemies = new List<string>();
            RoomModifiers = new Dictionary<string, float>();
        }
    }

    /// <summary>
    /// 完整地下城实例
    /// </summary>
    public class GeneratedDungeon
    {
        public string DungeonId { get; set; }
        public string DungeonName { get; set; }
        public int CurrentFloor { get; set; }
        public int TotalFloors { get; set; }
        public List<DungeonFloor> Floors { get; set; }
        public DungeonRoom CurrentRoom { get; set; }
        public List<string> VisitedRooms { get; set; }
        public int TotalTreasuresFound { get; set; }
        public int TotalEnemiesDefeated { get; set; }
        public TimeSpan TimeElapsed { get; set; }
        
        public GeneratedDungeon()
        {
            Floors = new List<DungeonFloor>();
            VisitedRooms = new List<string>();
        }
    }

    /// <summary>
    /// 地下城楼层
    /// </summary>
    public class DungeonFloor
    {
        public int FloorNumber { get; set; }
        public string FloorName { get; set; }
        public List<DungeonRoom> Rooms { get; set; }
        public DungeonRoom EntranceRoom { get; set; }
        public DungeonRoom ExitRoom { get; set; }
        public bool IsCompleted { get; set; }
        
        public DungeonFloor()
        {
            Rooms = new List<DungeonRoom>();
        }
    }

    /// <summary>
    /// 地下城进度数据
    /// </summary>
    public class DungeonProgress
    {
        public string DungeonId { get; set; }
        public int CurrentFloor { get; set; }
        public string CurrentRoomId { get; set; }
        public List<string> ClearedRooms { get; set; }
        public List<string> DiscoveredSecrets { get; set; }
        public int KeysCollected { get; set; }
        public Dictionary<string, int> ItemsLooted { get; set; }
        public DateTime StartTime { get; set; }
        
        public DungeonProgress()
        {
            ClearedRooms = new List<string>();
            DiscoveredSecrets = new List<string>();
            ItemsLooted = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// 玩家地下城统计
    /// </summary>
    public class DungeonStatistics
    {
        public int TotalDungeonsEntered { get; set; }
        public int TotalDungeonsCompleted { get; set; }
        public int TotalFloorsCleared { get; set; }
        public int TotalEnemiesDefeated { get; set; }
        public int TotalTreasuresFound { get; set; }
        public int TotalSecretsDiscovered { get; set; }
        public Dictionary<string, int> DungeonTypeCompletions { get; set; }
        public TimeSpan TotalTimeInDungeons { get; set; }
        
        public DungeonStatistics()
        {
            DungeonTypeCompletions = new Dictionary<string, int>();
        }
    }
}
