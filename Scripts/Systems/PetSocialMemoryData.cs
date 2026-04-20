using System;

/// <summary>
/// 宠物社交记忆记录 - 跨游戏局次持久化
/// 记录两只宠物在所有历史游戏局次中的最高友谊和战斗统计
/// </summary>
[System.Serializable]
public class PetSocialMemoryRecord
{
    /// <summary>
    /// 宠物A的ID（较小ID）
    /// </summary>
    public int PetIdA;

    /// <summary>
    /// 宠物B的ID（较大ID）
    /// </summary>
    public int PetIdB;

    /// <summary>
    /// 历史最高友谊等级
    /// </summary>
    public int MaxFriendship;

    /// <summary>
    /// 共同战斗总次数
    /// </summary>
    public int TotalBattles;

    /// <summary>
    /// 上次共同战斗时间（ISO 8601格式）
    /// </summary>
    public string LastBattleTime;

    /// <summary>
    /// 是否为"战友情谊"（Bonds of War）
    /// </summary>
    public bool IsBondsOfWar;

    public PetSocialMemoryRecord() { }

    public PetSocialMemoryRecord(int petIdA, int petIdB, int maxFriendship, int totalBattles, DateTime lastBattle, bool isBondsOfWar = false)
    {
        int smallerId = Math.Min(petIdA, petIdB);
        int largerId = Math.Max(petIdA, petIdB);
        PetIdA = smallerId;
        PetIdB = largerId;
        MaxFriendship = maxFriendship;
        TotalBattles = totalBattles;
        LastBattleTime = lastBattle.ToString("o");
        IsBondsOfWar = isBondsOfWar;
    }
}

/// <summary>
/// 宠物社交记忆数据库 - 管理跨游戏局次的宠物友谊历史
/// </summary>
public class PetSocialMemoryDatabase
{
    private static PetSocialMemoryDatabase _instance;
    public static PetSocialMemoryDatabase Instance => _instance ??= new PetSocialMemoryDatabase();

    /// <summary>
    /// 所有历史友谊记录（以 "petIdA_petIdB" 为键）
    /// </summary>
    private System.Collections.Generic.Dictionary<string, PetSocialMemoryRecord> _memoryRecords =
        new System.Collections.Generic.Dictionary<string, PetSocialMemoryRecord>();

    private static readonly string SAVE_KEY = "pet_social_memory";

    private PetSocialMemoryDatabase() { }

    /// <summary>
    /// 获取两只宠物的社交记忆（若不存在返回null）
    /// </summary>
    public PetSocialMemoryRecord GetMemory(int petId1, int petId2)
    {
        string key = MakeKey(petId1, petId2);
        return _memoryRecords.TryGetValue(key, out var record) ? record : null;
    }

    /// <summary>
    /// 更新或创建社交记忆记录
    /// </summary>
    public void UpdateMemory(int petId1, int petId2, int currentFriendship, int battleCount, DateTime lastBattle, bool isBondsOfWar)
    {
        string key = MakeKey(petId1, petId2);

        if (_memoryRecords.TryGetValue(key, out var existing))
        {
            // 取历史最高友谊和战斗次数的最大值
            existing.MaxFriendship = Math.Max(existing.MaxFriendship, currentFriendship);
            existing.TotalBattles = Math.Max(existing.TotalBattles, battleCount);
            existing.LastBattleTime = lastBattle.ToString("o");
            existing.IsBondsOfWar = existing.IsBondsOfWar || isBondsOfWar;
        }
        else
        {
            _memoryRecords[key] = new PetSocialMemoryRecord(petId1, petId2, currentFriendship, battleCount, lastBattle, isBondsOfWar);
        }
    }

    /// <summary>
    /// 从当前游戏局次的友谊数据构建社交记忆
    /// 在游戏结束时调用（SaveData 时）
    /// </summary>
    public void SyncFromCurrentSession(System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, PetFriendshipData>> friendships)
    {
        foreach (var outerKvp in friendships)
        {
            foreach (var innerKvp in outerKvp.Value)
            {
                var fd = innerKvp.Value;
                UpdateMemory(
                    fd.PetId,
                    fd.FriendPetId,
                    fd.FriendshipLevel,
                    fd.TotalBattles,
                    fd.LastInteraction,
                    fd.IsBondsOfWar
                );
            }
        }
    }

    /// <summary>
    /// 获取所有社交记忆记录
    /// </summary>
    public System.Collections.Generic.List<PetSocialMemoryRecord> GetAllMemories()
    {
        return new System.Collections.Generic.List<PetSocialMemoryRecord>(_memoryRecords.Values);
    }

    /// <summary>
    /// 是否有历史记忆（用于判断是否为"重逢"）
    /// </summary>
    public bool HasHistory(int petId1, int petId2)
    {
        var mem = GetMemory(petId1, petId2);
        return mem != null && mem.TotalBattles > 0;
    }

    /// <summary>
    /// 清除所有社交记忆（"新游戏"时调用）
    /// </summary>
    public void ClearAllMemories()
    {
        _memoryRecords.Clear();
    }

    /// <summary>
    /// 导出存档数据
    /// </summary>
    public Godot.Collections.Dictionary<string, Godot.Variant> ExportSaveData()
    {
        var records = new Godot.Collections.Array();
        foreach (var kvp in _memoryRecords)
        {
            var d = new Godot.Collections.Dictionary<string, Godot.Variant>
            {
                { "key", kvp.Key },
                { "pet_id_a", kvp.Value.PetIdA },
                { "pet_id_b", kvp.Value.PetIdB },
                { "max_friendship", kvp.Value.MaxFriendship },
                { "total_battles", kvp.Value.TotalBattles },
                { "last_battle_time", kvp.Value.LastBattleTime },
                { "is_bonds_of_war", kvp.Value.IsBondsOfWar }
            };
            records.Add(d);
        }

        return new Godot.Collections.Dictionary<string, Godot.Variant>
        {
            { "records", records }
        };
    }

    /// <summary>
    /// 导入存档数据
    /// </summary>
    public void ImportSaveData(Godot.Collections.Dictionary<string, Godot.Variant> data)
    {
        _memoryRecords.Clear();
        if (data == null || !data.ContainsKey("records")) return;

        var records = (Godot.Collections.Array)data["records"];
        foreach (Godot.Collections.Dictionary<string, Godot.Variant> rd in records)
        {
            var record = new PetSocialMemoryRecord
            {
                PetIdA = Convert.ToInt32(rd["pet_id_a"]),
                PetIdB = Convert.ToInt32(rd["pet_id_b"]),
                MaxFriendship = Convert.ToInt32(rd["max_friendship"]),
                TotalBattles = Convert.ToInt32(rd["total_battles"]),
                LastBattleTime = rd["last_battle_time"].ToString(),
                IsBondsOfWar = Convert.ToBoolean(rd["is_bonds_of_war"])
            };
            _memoryRecords[rd["key"].ToString()] = record;
        }
    }

    /// <summary>
    /// 获取指定宠物最近一次共同战斗的时间（跨所有同伴）
    /// 用于 REQ-178 社交记忆可视化
    /// </summary>
    /// <returns>最近战斗时间，若无记录返回 null</returns>
    public DateTime? GetLastBattleTimeForPet(int petId)
    {
        DateTime? latest = null;
        foreach (var kvp in _memoryRecords)
        {
            var record = kvp.Value;
            if (record.PetIdA == petId || record.PetIdB == petId)
            {
                if (DateTime.TryParse(record.LastBattleTime, out var battleTime))
                {
                    if (latest == null || battleTime > latest.Value)
                        latest = battleTime;
                }
            }
        }
        return latest;
    }

    /// <summary>
    /// 获取指定宠物所有社交记忆记录
    /// </summary>
    public System.Collections.Generic.List<PetSocialMemoryRecord> GetMemoriesForPet(int petId)
    {
        var result = new System.Collections.Generic.List<PetSocialMemoryRecord>();
        foreach (var kvp in _memoryRecords)
        {
            var record = kvp.Value;
            if (record.PetIdA == petId || record.PetIdB == petId)
                result.Add(record);
        }
        return result;
    }

    private static string MakeKey(int petId1, int petId2)
    {
        int a = Math.Min(petId1, petId2);
        int b = Math.Max(petId1, petId2);
        return $"{a}_{b}";
    }
}
