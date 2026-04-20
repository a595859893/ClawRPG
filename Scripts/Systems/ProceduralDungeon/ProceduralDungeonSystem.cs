using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.Managers;

namespace ClawRPG.Scripts.Systems.ProceduralDungeon
{
    /// <summary>
    /// 程序化地下城生成系统 - 协调者
    /// 委托给子系统：DungeonGeneratorSystem, RoomLayoutSystem, DungeonDifficultySystem
    /// </summary>
    public partial class ProceduralDungeonSystem : BaseSystem
    {
        private static ProceduralDungeonSystem _instance;
        public static ProceduralDungeonSystem Instance => _instance;
        
        // 子系统引用
        private DungeonGeneratorSystem _generatorSystem;
        private RoomLayoutSystem _layoutSystem;
        private DungeonDifficultySystem _difficultySystem;
        
        // 当前地下城实例
        public GeneratedDungeon CurrentDungeon { get; private set; }
        
        // 玩家进度
        public DungeonProgress CurrentProgress { get; private set; }
        
        // 统计数据
        public DungeonStatistics Statistics { get; private set; }
        
        // 信号事件
        public static Signal DungeonGenerated => new("dungeon_generated");
        public static Signal RoomEntered => new("room_entered");
        public static Signal RoomCleared => new("room_cleared");
        public static Signal FloorCompleted => new("floor_completed");
        public static Signal DungeonCompleted => new("dungeon_completed");
        public static Signal TreasureFound => new("treasure_found");
        public static Signal SecretDiscovered => new("secret_discovered");
        
        private ProceduralDungeonDatabase _database;
        private GameModeConfig _gameModeConfig;
        
        public ProceduralDungeonSystem()
        {
            _instance = this;
            _database = ProceduralDungeonDatabase.Instance;
            _gameModeConfig = GameModeConfig.Instance;
            Statistics = new DungeonStatistics();
        }
        
        public override void _Ready()
        {
            // 初始化子系统
            _generatorSystem = new DungeonGeneratorSystem();
            _layoutSystem = new RoomLayoutSystem();
            _difficultySystem = new DungeonDifficultySystem();
            
            // 将子系统添加为子节点
            AddChild(_generatorSystem);
            AddChild(_layoutSystem);
            AddChild(_difficultySystem);
            
            GD.Print("Procedural Dungeon System initialized");
        }
        
        /// <summary>
        /// 生成新地下城
        /// </summary>
        public GeneratedDungeon GenerateDungeon(string dungeonTypeId, int seed = -1)
        {
            var config = _database.GetDungeonType(dungeonTypeId);
            if (config == null)
            {
                GD.PrintErr($"Unknown dungeon type: {dungeonTypeId}");
                return null;
            }
            
            // 设置子系统种子
            _layoutSystem.SetSeed(seed);
            _difficultySystem.SetSeed(seed);
            
            CurrentDungeon = new GeneratedDungeon
            {
                DungeonId = Guid.NewGuid().ToString(),
                DungeonName = config.DisplayName,
                TotalFloors = (int)(config.TotalFloors * _gameModeConfig.GetFloorCountMultiplier()),
                CurrentFloor = 1
            };
            
            CurrentProgress = new DungeonProgress
            {
                DungeonId = CurrentDungeon.DungeonId,
                CurrentFloor = 1,
                StartTime = DateTime.Now
            };
            
            // 委托给生成系统
            CurrentDungeon = _generatorSystem.GenerateDungeon(dungeonTypeId, seed);
            
            // 恢复进度数据
            CurrentProgress.DungeonId = CurrentDungeon.DungeonId;
            CurrentProgress.CurrentFloor = 1;
            CurrentProgress.StartTime = DateTime.Now;
            
            // 设置初始房间
            if (CurrentDungeon.Floors.Count > 0 && CurrentDungeon.Floors[0].Rooms.Count > 0)
            {
                CurrentDungeon.CurrentRoom = CurrentDungeon.Floors[0].EntranceRoom;
                CurrentDungeon.CurrentRoom.IsDiscovered = true;
                CurrentProgress.CurrentRoomId = CurrentDungeon.CurrentRoom.RoomId;
            }
            
            Statistics.TotalDungeonsEntered++;
            
            DungeonGenerated?.Emit();
            
            GD.Print($"Generated dungeon: {CurrentDungeon.DungeonName} with {CurrentDungeon.TotalFloors} floors");
            
            return CurrentDungeon;
        }
        
        /// <summary>
        /// 进入房间
        /// </summary>
        public bool EnterRoom(string roomId)
        {
            if (CurrentDungeon == null) return false;
            
            var room = _generatorSystem.FindRoomById(CurrentDungeon, roomId);
            if (room == null) return false;
            
            CurrentDungeon.CurrentRoom = room;
            room.IsDiscovered = true;
            
            if (!CurrentProgress.ClearedRooms.Contains(roomId))
            {
                CurrentProgress.CurrentRoomId = roomId;
            }
            
            if (!CurrentDungeon.VisitedRooms.Contains(roomId))
            {
                CurrentDungeon.VisitedRooms.Add(roomId);
            }
            
            RoomEntered?.Emit();
            
            // REQ-117-05: 战斗开始流程触发预览 - 进入战斗房间时请求Combo预览
            if (_IsCombatRoom(room.Type))
            {
                EventBusManager.Instance?.Emit("combat_preload_requested");
                GD.Print($"[ProceduralDungeonSystem] Combat room entered - combat preload requested");
            }
            
            GD.Print($"Entered room: {room.Type} ({room.RoomId})");
            
            return true;
        }
        
        /// <summary>
        /// 判断房间类型是否为战斗房间
        /// </summary>
        private bool _IsCombatRoom(RoomType type)
        {
            return type == RoomType.Combat || type == RoomType.Elite || type == RoomType.Boss;
        }
        
        /// <summary>
        /// 清理房间
        /// </summary>
        public void ClearCurrentRoom()
        {
            if (CurrentDungeon?.CurrentRoom == null) return;
            
            var room = CurrentDungeon.CurrentRoom;
            if (!room.IsCleared)
            {
                room.IsCleared = true;
                
                if (!CurrentProgress.ClearedRooms.Contains(room.RoomId))
                {
                    CurrentProgress.ClearedRooms.Add(room.RoomId);
                }
                
                CurrentDungeon.TotalEnemiesDefeated++;
                Statistics.TotalEnemiesDefeated++;
                
                // 宝藏检查
                if (!string.IsNullOrEmpty(room.TreasureId))
                {
                    CurrentDungeon.TotalTreasuresFound++;
                    Statistics.TotalTreasuresFound++;
                    TreasureFound?.Emit();
                }
                
                // 密室检查
                if (room.Type == RoomType.Secret && !CurrentProgress.DiscoveredSecrets.Contains(room.RoomId))
                {
                    CurrentProgress.DiscoveredSecrets.Add(room.RoomId);
                    Statistics.TotalSecretsDiscovered++;
                    SecretDiscovered?.Emit();
                }
                
                RoomCleared?.Emit();
                
                GD.Print($"Room cleared: {room.Type}");
                
                // 检查是否是出口房间
                var currentFloor = CurrentDungeon.Floors[CurrentDungeon.CurrentFloor - 1];
                if (room == currentFloor.ExitRoom && room.Type == RoomType.Boss)
                {
                    CompleteFloor();
                }
            }
        }
        
        /// <summary>
        /// 完成楼层
        /// </summary>
        public void CompleteFloor()
        {
            if (CurrentDungeon == null) return;
            
            var currentFloor = CurrentDungeon.Floors[CurrentDungeon.CurrentFloor - 1];
            currentFloor.IsCompleted = true;
            CurrentDungeon.CurrentFloor++;
            Statistics.TotalFloorsCleared++;
            
            FloorCompleted?.Emit();
            
            GD.Print($"Floor {currentFloor.FloorNumber} completed");
            
            // 检查是否完成整个地下城
            if (CurrentDungeon.CurrentFloor > CurrentDungeon.TotalFloors)
            {
                CompleteDungeon();
            }
            else
            {
                // 进入下一层
                var nextFloor = CurrentDungeon.Floors[CurrentDungeon.CurrentFloor - 1];
                if (nextFloor?.EntranceRoom != null)
                {
                    EnterRoom(nextFloor.EntranceRoom.RoomId);
                }
                CurrentProgress.CurrentFloor = CurrentDungeon.CurrentFloor;
            }
        }
        
        /// <summary>
        /// 完成地下城
        /// </summary>
        public void CompleteDungeon()
        {
            if (CurrentDungeon == null) return;
            
            Statistics.TotalDungeonsCompleted++;
            CurrentDungeon.TimeElapsed = DateTime.Now - CurrentProgress.StartTime;
            Statistics.TotalTimeInDungeons += CurrentDungeon.TimeElapsed;
            
            DungeonCompleted?.Emit();
            
            GD.Print($"Dungeon completed: {CurrentDungeon.DungeonName}");
        }
        
        /// <summary>
        /// 获取当前可用的连接房间
        /// </summary>
        public List<DungeonRoom> GetConnectedRooms()
        {
            if (CurrentDungeon?.CurrentRoom == null) return new List<DungeonRoom>();
            
            return _generatorSystem.GetConnectedRooms(CurrentDungeon, CurrentDungeon.CurrentRoom);
        }
        
        /// <summary>
        /// 获取房间信息
        /// </summary>
        public Dictionary GetRoomInfo()
        {
            if (CurrentDungeon?.CurrentRoom == null) return null;
            
            var room = CurrentDungeon.CurrentRoom;
            return new Dictionary
            {
                ["room_id"] = room.RoomId,
                ["type"] = room.Type.ToString(),
                ["difficulty"] = room.Difficulty.ToString(),
                ["is_cleared"] = room.IsCleared,
                ["is_discovered"] = room.IsDiscovered,
                ["width"] = room.Width,
                ["height"] = room.Height,
                ["enemy_count"] = room.Enemies.Count,
                ["has_treasure"] = !string.IsNullOrEmpty(room.TreasureId),
                ["has_event"] = !string.IsNullOrEmpty(room.EventId)
            };
        }
        
        /// <summary>
        /// 获取地下城信息
        /// </summary>
        public Dictionary GetDungeonInfo()
        {
            if (CurrentDungeon == null) return null;
            
            return new Dictionary
            {
                ["dungeon_id"] = CurrentDungeon.DungeonId,
                ["dungeon_name"] = CurrentDungeon.DungeonName,
                ["current_floor"] = CurrentDungeon.CurrentFloor,
                ["total_floors"] = CurrentDungeon.TotalFloors,
                ["rooms_visited"] = CurrentDungeon.VisitedRooms.Count,
                ["treasures_found"] = CurrentDungeon.TotalTreasuresFound,
                ["enemies_defeated"] = CurrentDungeon.TotalEnemiesDefeated
            };
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
        {
            var data = new System.Collections.Generic.Dictionary<string, object>();
            
            // 统计数据
            if (Statistics != null)
            {
                data["total_dungeons_entered"] = Statistics.TotalDungeonsEntered;
                data["total_dungeons_completed"] = Statistics.TotalDungeonsCompleted;
                data["total_floors_cleared"] = Statistics.TotalFloorsCleared;
                data["total_enemies_defeated"] = Statistics.TotalEnemiesDefeated;
                data["total_treasures_found"] = Statistics.TotalTreasuresFound;
                data["total_secrets_discovered"] = Statistics.TotalSecretsDiscovered;
                data["total_time_in_dungeons"] = Statistics.TotalTimeInDungeons.Ticks;
            }
            
            // 当前进度
            if (CurrentProgress != null)
            {
                data["current_dungeon_id"] = CurrentProgress.DungeonId;
                data["current_floor"] = CurrentProgress.CurrentFloor;
                data["current_room_id"] = CurrentProgress.CurrentRoomId;
                data["start_time"] = CurrentProgress.StartTime.ToString("o");
                
                // 已清理房间
                data["cleared_rooms"] = new Godot.Collections.Array(CurrentProgress.ClearedRooms);
                
                // 发现的秘密
                data["discovered_secrets"] = new Godot.Collections.Array(CurrentProgress.DiscoveredSecrets);
            }
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 统计数据
            if (Statistics == null)
            {
                Statistics = new DungeonStatistics();
            }
            
            Statistics.TotalDungeonsEntered = (int)data.GetValueOrDefault("total_dungeons_entered", 0);
            Statistics.TotalDungeonsCompleted = (int)data.GetValueOrDefault("total_dungeons_completed", 0);
            Statistics.TotalFloorsCleared = (int)data.GetValueOrDefault("total_floors_cleared", 0);
            Statistics.TotalEnemiesDefeated = (int)data.GetValueOrDefault("total_enemies_defeated", 0);
            Statistics.TotalTreasuresFound = (int)data.GetValueOrDefault("total_treasures_found", 0);
            Statistics.TotalSecretsDiscovered = (int)data.GetValueOrDefault("total_secrets_discovered", 0);
            
            if (data.Contains("total_time_in_dungeons"))
            {
                Statistics.TotalTimeInDungeons = TimeSpan.FromTicks((long)data["total_time_in_dungeons"]);
            }
            
            // 当前进度
            if (data.Contains("current_dungeon_id"))
            {
                if (CurrentProgress == null)
                {
                    CurrentProgress = new DungeonProgress();
                }
                
                CurrentProgress.DungeonId = (string)data["current_dungeon_id"];
                CurrentProgress.CurrentFloor = (int)data.GetValueOrDefault("current_floor", 1);
                CurrentProgress.CurrentRoomId = (string)data.GetValueOrDefault("current_room_id", "");
                
                if (data.Contains("start_time") && DateTime.TryParse((string)data["start_time"], out var startTime))
                {
                    CurrentProgress.StartTime = startTime;
                }
                
                // 已清理房间
                if (data.Contains("cleared_rooms"))
                {
                    var clearedArray = (Array)data["cleared_rooms"];
                    CurrentProgress.ClearedRooms = new List<string>();
                    foreach (string roomId in clearedArray)
                    {
                        CurrentProgress.ClearedRooms.Add(roomId);
                    }
                }
                
                // 发现的秘密
                if (data.Contains("discovered_secrets"))
                {
                    var secretsArray = (Array)data["discovered_secrets"];
                    CurrentProgress.DiscoveredSecrets = new List<string>();
                    foreach (string secret in secretsArray)
                    {
                        CurrentProgress.DiscoveredSecrets.Add(secret);
                    }
                }
            }
        }
    }
}
