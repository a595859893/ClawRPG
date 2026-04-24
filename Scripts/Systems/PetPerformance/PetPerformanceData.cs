using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 宠物战绩数据 — 记录 pet-assisted vs solo 通关数据
/// </summary>
public partial class PetPerformanceData : BaseSystem
{
    private static PetPerformanceData _instance;
    public static PetPerformanceData Instance => _instance;

    /// <summary>
    /// 通关类型
    /// </summary>
    public enum RunType
    {
        PetAssisted = 0,
        Solo = 1
    }

    /// <summary>
    /// 单次房间通关记录
    /// </summary>
    public class RoomPerformanceRecord
    {
        public string RoomId;
        public RunType RunType;
        public float ClearTimeSeconds;
        public int HpLoss;
        public float DamageDealt;
        public int PetSkillActivations;
        public double Timestamp;
    }

    /// <summary>
    /// 统计数据汇总
    /// </summary>
    public class PerformanceStats
    {
        public int SampleCount;
        public float AvgClearTime;
        public int AvgHpLoss;
        public float AvgDamageDealt;
        public float AvgPetSkillActivations;
    }

    /// <summary>
    /// 对比结果
    /// </summary>
    public class ComparisonResult
    {
        public bool HasEnoughData;
        public float TimeSavedPerRoom;       // 正数 = pet assist 更快
        public int HpSavedPerRoom;            // 正数 = pet assist HP损耗更少
        public float WinRatePetAssisted;
        public float WinRateSolo;
        public int TotalPetAssistedWins;
        public int TotalSoloWins;
        public int MinSamplesForComparison = 5;
    }

    // ===== 运行时数据 =====

    private List<RoomPerformanceRecord> _records = new List<RoomPerformanceRecord>();

    // 当前战斗数据
    private bool _inCombat;
    private float _combatStartTime;
    private int _combatStartHp;
    private int _totalDamageTakenThisCombat;
    private float _totalDamageDealtThisCombat;
    private int _petSkillActivationsThisCombat;

    // ===== 公开 API =====

    public override void _Ready()
    {
        _instance = this;
        base._Ready();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        var bus = EventBusManager.Instance;
        if (bus == null) return;
        bus.Subscribe(EventBusManager.Events.CombatStarted, OnCombatStarted);
        bus.Subscribe(EventBusManager.Events.CombatEnded, OnCombatEnded);
        bus.Subscribe<PlayerHealthChangedEventData>(EventBusManager.Events.PlayerHealthChanged, OnPlayerHealthChanged);

        // Pet synergy attacks
        if (PetCombatCompanionSystem.Instance != null)
        {
            PetCombatCompanionSystem.Instance.SynergyAttackTriggered += OnPetSynergyAttack;
        }
    }

    private void OnCombatStarted()
    {
        _inCombat = true;
        _combatStartTime = Time.GetUnixTimeFromSystem();
        _totalDamageTakenThisCombat = 0;
        _totalDamageDealtThisCombat = 0;
        _petSkillActivationsThisCombat = 0;
        _combatStartHp = 0; // Will be set from first PlayerHealthChanged event
    }

    private void OnCombatEnded()
    {
        if (!_inCombat) return;
        _inCombat = false;

        float clearTime = (float)(Time.GetUnixTimeFromSystem() - _combatStartTime);
        int hpLoss = _totalDamageTakenThisCombat; // Already accumulated via OnPlayerHealthChanged
        RunType runType = DetermineRunType();

        var record = new RoomPerformanceRecord
        {
            RoomId = GetCurrentRoomId(),
            RunType = runType,
            ClearTimeSeconds = clearTime,
            HpLoss = hpLoss,
            DamageDealt = _totalDamageDealtThisCombat,
            PetSkillActivations = _petSkillActivationsThisCombat,
            Timestamp = Time.GetUnixTimeFromSystem()
        };

        _records.Add(record);

        // 限制记录数量（保留最近100条）
        if (_records.Count > 100)
        {
            _records.RemoveAt(0);
        }
    }

    private void OnPlayerHealthChanged(PlayerHealthChangedEventData data)
    {
        if (!_inCombat) return;

        // Set baseline HP on first health change event after combat starts
        if (_combatStartHp == 0 && data.MaxHealth > 0)
        {
            _combatStartHp = data.NewHealth - data.Delta;
            if (_combatStartHp <= 0) _combatStartHp = data.NewHealth;
        }

        if (data.Delta < 0)
        {
            _totalDamageTakenThisCombat += Mathf.Abs(data.Delta);
        }
    }

    private void OnPetSynergyAttack(string petId, string attackType, float syncLevel)
    {
        if (!_inCombat) return;
        _petSkillActivationsThisCombat++;
    }

    private RunType DetermineRunType()
    {
        // PetAssisted: an active pet is summoned and available
        // Solo: no pet is summoned
        if (PetCombatCompanionSystem.Instance == null)
            return RunType.Solo;

        string activePetId = PetCombatCompanionSystem.Instance.GetActivePetId();
        if (!string.IsNullOrEmpty(activePetId))
            return RunType.PetAssisted;

        return RunType.Solo;
    }

    private string GetCurrentRoomId()
    {
        if (ProceduralDungeonSystem.Instance != null)
        {
            var dungeon = ProceduralDungeonSystem.Instance.CurrentDungeon;
            if (dungeon?.CurrentRoom != null)
                return dungeon.CurrentRoom.RoomId;
        }
        return "unknown";
    }

    public List<RoomPerformanceRecord> GetRecords() => new List<RoomPerformanceRecord>(_records);

    public PerformanceStats GetStats(RunType type)
    {
        var filtered = _records.FindAll(r => r.RunType == type);
        if (filtered.Count == 0)
            return new PerformanceStats { SampleCount = 0 };

        return new PerformanceStats
        {
            SampleCount = filtered.Count,
            AvgClearTime = SumField(filtered, r => r.ClearTimeSeconds) / filtered.Count,
            AvgHpLoss = (int)(SumField(filtered, r => (float)r.HpLoss) / filtered.Count),
            AvgDamageDealt = SumField(filtered, r => r.DamageDealt) / filtered.Count,
            AvgPetSkillActivations = SumField(filtered, r => (float)r.PetSkillActivations) / filtered.Count
        };
    }

    public ComparisonResult GetComparison()
    {
        var petStats = GetStats(RunType.PetAssisted);
        var soloStats = GetStats(RunType.Solo);

        var result = new ComparisonResult();

        if (petStats.SampleCount < 5 || soloStats.SampleCount < 5)
        {
            result.HasEnoughData = false;
            return result;
        }

        result.HasEnoughData = true;
        result.TimeSavedPerRoom = soloStats.AvgClearTime - petStats.AvgClearTime;
        result.HpSavedPerRoom = soloStats.AvgHpLoss - petStats.AvgHpLoss;

        // Simplified win rate: lower HP loss = win for that type in a room
        int petAssistedWins = 0, soloWins = 0;
        foreach (var r in _records)
        {
            if (r.RunType == RunType.PetAssisted && r.HpLoss <= soloStats.AvgHpLoss)
                petAssistedWins++;
            else if (r.RunType == RunType.Solo)
                soloWins++;
        }

        int totalPetRooms = _records.FindAll(r => r.RunType == RunType.PetAssisted).Count;
        int totalSoloRooms = _records.FindAll(r => r.RunType == RunType.Solo).Count;

        result.WinRatePetAssisted = totalPetRooms > 0 ? (float)petAssistedWins / totalPetRooms : 0;
        result.WinRateSolo = totalSoloRooms > 0 ? (float)soloWins / totalSoloRooms : 0;

        return result;
    }

    public int GetPetAssistedCount() => _records.FindAll(r => r.RunType == RunType.PetAssisted).Count;
    public int GetSoloCount() => _records.FindAll(r => r.RunType == RunType.Solo).Count;

    private float SumField(List<RoomPerformanceRecord> list, Func<RoomPerformanceRecord, float> field)
    {
        float sum = 0;
        foreach (var r in list) sum += field(r);
        return sum;
    }

    // ===== SaveData =====

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        var recordsList = new List<Dictionary>();
        foreach (var r in _records)
        {
            recordsList.Add(new Dictionary
            {
                ["room_id"] = r.RoomId,
                ["run_type"] = (int)r.RunType,
                ["clear_time"] = r.ClearTimeSeconds,
                ["hp_loss"] = r.HpLoss,
                ["damage_dealt"] = r.DamageDealt,
                ["pet_skill_activations"] = r.PetSkillActivations,
                ["timestamp"] = r.Timestamp
            });
        }
        data["records"] = recordsList;
        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        _records.Clear();
        if (!data.ContainsKey("records")) return;
        var recordsList = (Godot.Collections.Array)data["records"];
        foreach (Godot.Collections.Dictionary rd in recordsList)
        {
            _records.Add(new RoomPerformanceRecord
            {
                RoomId = rd.ContainsKey("room_id") ? rd["room_id"].ToString() : "",
                RunType = rd.ContainsKey("run_type") ? (RunType)(int)rd["run_type"] : RunType.Solo,
                ClearTimeSeconds = rd.ContainsKey("clear_time") ? (float)rd["clear_time"] : 0,
                HpLoss = rd.ContainsKey("hp_loss") ? (int)rd["hp_loss"] : 0,
                DamageDealt = rd.ContainsKey("damage_dealt") ? (float)rd["damage_dealt"] : 0,
                PetSkillActivations = rd.ContainsKey("pet_skill_activations") ? (int)rd["pet_skill_activations"] : 0,
                Timestamp = rd.ContainsKey("timestamp") ? (double)rd["timestamp"] : 0
            });
        }
    }
}
