using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 宠物友谊系统 - 管理宠物之间的友谊关系和互动
/// </summary>
public class PetFriendshipSystem : BaseSystem
{
    public static PetFriendshipSystem Instance { get; private set; }

    private Dictionary<int, Dictionary<int, PetFriendshipData>> friendships = new Dictionary<int, Dictionary<int, PetFriendshipData>>();
    private Dictionary<int, string> equippedSkills = new Dictionary<int, string>();

    public int TotalBonds { get; private set; }
    public int MaxLevelBonds { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void AddFriendship(int petId1, int petId2)
    {
        if (petId1 == petId2) return;
        
        int smallerId = Math.Min(petId1, petId2);
        int largerId = Math.Max(petId1, petId2);

        if (!friendships.ContainsKey(smallerId))
            friendships[smallerId] = new Dictionary<int, PetFriendshipData>();

        if (!friendships[smallerId].ContainsKey(largerId))
        {
            var newFriendship = new PetFriendshipData
            {
                PetId = smallerId,
                FriendPetId = largerId,
                FriendshipLevel = 1,
                Experience = 0,
                LastInteraction = DateTime.Now,
                IsBondsOfWar = false
            };
            friendships[smallerId][largerId] = newFriendship;
            TotalBonds++;
            GD.Print($"[PetFriendship] New friendship formed between pet {smallerId} and pet {largerId}");
        }
    }

    public void RemoveFriendship(int petId1, int petId2)
    {
        int smallerId = Math.Min(petId1, petId2);
        int largerId = Math.Max(petId1, petId2);

        if (friendships.ContainsKey(smallerId) && friendships[smallerId].ContainsKey(largerId))
        {
            friendships[smallerId].Remove(largerId);
            TotalBonds--;
            GD.Print($"[PetFriendship] Friendship removed between pet {smallerId} and pet {largerId}");
        }
    }

    public PetFriendshipData GetFriendship(int petId1, int petId2)
    {
        int smallerId = Math.Min(petId1, petId2);
        int largerId = Math.Max(petId1, petId2);

        if (friendships.ContainsKey(smallerId) && friendships[smallerId].ContainsKey(largerId))
            return friendships[smallerId][largerId];
        return null;
    }

    public bool AreFriends(int petId1, int petId2)
    {
        return GetFriendship(petId1, petId2) != null;
    }

    public void AddExperience(int petId1, int petId2, int exp)
    {
        var friendship = GetFriendship(petId1, petId2);
        if (friendship == null) return;

        friendship.Experience += exp;
        
        int expNeeded = PetFriendshipDatabase.GetExpForLevel(friendship.FriendshipLevel + 1);
        while (friendship.Experience >= expNeeded && friendship.FriendshipLevel < 20)
        {
            friendship.Experience -= expNeeded;
            friendship.FriendshipLevel++;
            expNeeded = PetFriendshipDatabase.GetExpForLevel(friendship.FriendshipLevel + 1);
            
            if (friendship.FriendshipLevel >= 20)
                MaxLevelBonds++;
            
            GD.Print($"[PetFriendship] Friendship level up! Pet {petId1} & {petId2} are now level {friendship.FriendshipLevel}");
        }

        friendship.LastInteraction = DateTime.Now;
    }

    public float GetCombatBonus(int petId1, int petId2)
    {
        var friendship = GetFriendship(petId1, petId2);
        if (friendship == null) return 1.0f;

        float baseBonus = PetFriendshipDatabase.GetBonusMultiplier(friendship.FriendshipLevel);
        
        if (friendship.IsBondsOfWar)
            baseBonus *= 1.25f;

        TimeSpan timeSinceInteraction = DateTime.Now - friendship.LastInteraction;
        if (timeSinceInteraction.TotalHours > 24)
            baseBonus *= 0.9f;

        return baseBonus;
    }

    public void SetBondsOfWar(int petId1, int petId2, bool value)
    {
        var friendship = GetFriendship(petId1, petId2);
        if (friendship != null)
        {
            friendship.IsBondsOfWar = value;
            GD.Print($"[PetFriendship] Bonds of War set to {value} for pet {petId1} and {petId2}");
        }
    }

    public void EquipSkill(int petId1, int petId2, string skill)
    {
        int smallerId = Math.Min(petId1, petId2);
        int key = smallerId * 10000 + Math.Max(petId1, petId2);
        
        var friendship = GetFriendship(petId1, petId2);
        if (friendship != null && friendship.FriendshipLevel >= 5)
        {
            equippedSkills[key] = skill;
            GD.Print($"[PetFriendship] Skill {skill} equipped for pet {petId1} and {petId2}");
        }
    }

    public string GetEquippedSkill(int petId1, int petId2)
    {
        int smallerId = Math.Min(petId1, petId2);
        int key = smallerId * 10000 + Math.Max(petId1, petId2);
        
        if (equippedSkills.ContainsKey(key))
            return equippedSkills[key];
        return "";
    }

    public Dictionary<int, Dictionary<int, PetFriendshipData>> GetAllFriendships()
    {
        return friendships;
    }

    public int GetFriendshipCount()
    {
        return TotalBonds;
    }

    public Dictionary<int, List<int>> GetFriendsForPet(int petId)
    {
        var result = new Dictionary<int, List<int>>();
        
        foreach (var outer in friendships)
        {
            foreach (var kvp in outer.Value)
            {
                if (kvp.Value.PetId == petId || kvp.Value.FriendPetId == petId)
                {
                    int friendId = kvp.Value.PetId == petId ? kvp.Value.FriendPetId : kvp.Value.PetId;
                    if (!result.ContainsKey(petId))
                        result[petId] = new List<int>();
                    result[petId].Add(friendId);
                }
            }
        }
        
        return result;
    }

    public Dictionary<string, int> GetStatistics()
    {
        var stats = new Dictionary<string, int>
        {
            { "total_bonds", TotalBonds },
            { "max_level_bonds", MaxLevelBonds },
            { "stranger_bonds", 0 },
            { "acquaintance_bonds", 0 },
            { "friend_bonds", 0 },
            { "close_friend_bonds", 0 },
            { "best_friend_bonds", 0 },
            { "soulmate_bonds", 0 }
        };

        foreach (var outer in friendships)
        {
            foreach (var kvp in outer.Value)
            {
                string tier = PetFriendshipDatabase.GetFriendshipTier(kvp.Value.FriendshipLevel);
                switch (tier)
                {
                    case "Stranger": stats["stranger_bonds"]++; break;
                    case "Acquaintance": stats["acquaintance_bonds"]++; break;
                    case "Friend": stats["friend_bonds"]++; break;
                    case "CloseFriend": stats["close_friend_bonds"]++; break;
                    case "BestFriend": stats["best_friend_bonds"]++; break;
                    case "Soulmate": stats["soulmate_bonds"]++; break;
                }
            }
        }

        return stats;
    }

    public void SaveData()
    {
        var saveData = new Dictionary<string, object>();
        
        var friendshipList = new List<Dictionary<string, object>>();
        foreach (var outer in friendships)
        {
            foreach (var kvp in outer.Value)
            {
                friendshipList.Add(new Dictionary<string, object>
                {
                    { "pet_id_1", kvp.Value.PetId },
                    { "pet_id_2", kvp.Value.FriendPetId },
                    { "level", kvp.Value.FriendshipLevel },
                    { "exp", kvp.Value.Experience },
                    { "last_interaction", kvp.Value.LastInteraction.ToString("o") },
                    { "bonds_of_war", kvp.Value.IsBondsOfWar }
                });
            }
        }
        
        saveData["friendships"] = friendshipList;
        saveData["equipped_skills"] = equippedSkills;
        
        SaveSystem.Instance.SaveCustomData("pet_friendship", saveData);
    }

    public void LoadData()
    {
        var data = SaveSystem.Instance.LoadCustomData("pet_friendship");
        if (data == null) return;

        friendships.Clear();
        equippedSkills.Clear();

        if (data.ContainsKey("friendships"))
        {
            var friendshipList = (Godot.Collections.Array)data["friendships"];
            foreach (Godot.Collections.Dictionary friendshipDict in friendshipList)
            {
                int petId1 = Convert.ToInt32(friendshipDict["pet_id_1"]);
                int petId2 = Convert.ToInt32(friendshipDict["pet_id_2"]);
                
                int smallerId = Math.Min(petId1, petId2);
                int largerId = Math.Max(petId1, petId2);
                
                if (!friendships.ContainsKey(smallerId))
                    friendships[smallerId] = new Dictionary<int, PetFriendshipData>();
                
                var friendship = new PetFriendshipData
                {
                    PetId = petId1,
                    FriendPetId = petId2,
                    FriendshipLevel = Convert.ToInt32(friendshipDict["level"]),
                    Experience = Convert.ToInt32(friendshipDict["exp"]),
                    LastInteraction = DateTime.Parse((string)friendshipDict["last_interaction"]),
                    IsBondsOfWar = Convert.ToBoolean(friendshipDict["bonds_of_war"])
                };
                
                friendships[smallerId][largerId] = friendship;
                TotalBonds++;
                
                if (friendship.FriendshipLevel >= 20)
                    MaxLevelBonds++;
            }
        }
    }

        // ===== 持久化 =====
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // 保存友谊数据
        var friendshipsData = new Dictionary();
        foreach (var outerKvp in friendships)
        {
            var innerDict = new Dictionary();
            foreach (var innerKvp in outerKvp.Value)
            {
                var friendship = new Dictionary();
                friendship["pet_id"] = innerKvp.Value.PetId;
                friendship["friend_pet_id"] = innerKvp.Value.FriendPetId;
                friendship["level"] = innerKvp.Value.FriendshipLevel;
                friendship["exp"] = innerKvp.Value.Experience;
                friendship["last_interaction"] = innerKvp.Value.LastInteraction.ToString("o");
                friendship["bonds_of_war"] = innerKvp.Value.IsBondsOfWar;
                innerDict[innerKvp.Key.ToString()] = friendship;
            }
            friendshipsData[outerKvp.Key.ToString()] = innerDict;
        }
        data["friendships"] = friendshipsData;
        
        // 保存已装备技能
        var skillsData = new Dictionary();
        foreach (var kvp in equippedSkills)
        {
            skillsData[kvp.Key.ToString()] = kvp.Value;
        }
        data["equipped_skills"] = skillsData;
        
        // 保存统计数据
        data["total_bonds"] = TotalBonds;
        data["max_level_bonds"] = MaxLevelBonds;
        
        return data;
    }
    
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        // 恢复友谊数据
        friendships.Clear();
        if (data.ContainsKey("friendships"))
        {
            var friendshipsData = (Dictionary)data["friendships"];
            foreach (var outerKvp in friendshipsData)
            {
                int smallerId = Convert.ToInt32(outerKvp.Key);
                friendships[smallerId] = new Dictionary<int, PetFriendshipData>();
                
                var innerDict = (Dictionary)outerKvp.Value;
                foreach (var innerKvp in innerDict)
                {
                    int largerId = Convert.ToInt32(innerKvp.Key);
                    var friendshipDict = (Dictionary)innerKvp.Value;
                    
                    var friendship = new PetFriendshipData
                    {
                        PetId = Convert.ToInt32(friendshipDict["pet_id"]),
                        FriendPetId = Convert.ToInt32(friendshipDict["friend_pet_id"]),
                        FriendshipLevel = Convert.ToInt32(friendshipDict["level"]),
                        Experience = Convert.ToInt32(friendshipDict["exp"]),
                        LastInteraction = DateTime.Parse(friendshipDict["last_interaction"].ToString()),
                        IsBondsOfWar = Convert.ToBoolean(friendshipDict["bonds_of_war"])
                    };
                    
                    friendships[smallerId][largerId] = friendship;
                }
            }
        }
        
        // 恢复已装备技能
        if (data.ContainsKey("equipped_skills"))
        {
            equippedSkills.Clear();
            var skillsData = (Dictionary)data["equipped_skills"];
            foreach (var kvp in skillsData)
            {
                equippedSkills[Convert.ToInt32(kvp.Key)] = kvp.Value.ToString();
            }
        }
        
        // 恢复统计数据
        if (data.ContainsKey("total_bonds"))
            TotalBonds = Convert.ToInt32(data["total_bonds"]);
        if (data.ContainsKey("max_level_bonds"))
            MaxLevelBonds = Convert.ToInt32(data["max_level_bonds"]);
    }
}
