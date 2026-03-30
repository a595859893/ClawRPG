using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 宠物故事系统管理器
    /// </summary>
    public class PetStorySystem : BaseSystem {
        public static PetStorySystem Instance { get; private set; }
public delegate void StoryUnlocked(int petId, PetStory story);
public delegate void StoryRead(int petId, int storyId);
        
        private PetStoryDatabase database;
        private Dictionary<int, PlayerPetStoryData> playerPetStories = new Dictionary<int, PlayerPetStoryData>();
        
        public override void _Ready() {
            Instance = this;
            database = new PetStoryDatabase();
        }
        
        /// <summary>
        /// 初始化宠物故事数据
        /// </summary>
        public void InitializePetStory(int petId, int petTypeId) {
            if (!playerPetStories.ContainsKey(petId)) {
                var data = new PlayerPetStoryData {
                    PetId = petId
                };
                
                // 解锁初始故事
                var stories = database.GetStoriesForPet(petTypeId);
                foreach (var story in stories) {
                    if (story.UnlockCondition.Type == PetStoryUnlockType.Default) {
                        data.UnlockedStoryIds.Add(story.StoryId);
                        data.StoryReadStatus[story.StoryId] = false;
                    }
                }
                
                playerPetStories[petId] = data;
            }
        }
        
        /// <summary>
        /// 获取宠物故事列表
        /// </summary>
        public List<PetStory> GetPetStories(int petId, int petTypeId) {
            var result = new List<PetStory>();
            var stories = database.GetStoriesForPet(petTypeId);
            
            if (playerPetStories.ContainsKey(petId)) {
                var data = playerPetStories[petId];
                foreach (var story in stories) {
                    if (data.UnlockedStoryIds.Contains(story.StoryId)) {
                        story.IsUnlocked = true;
                        result.Add(story);
                    }
                }
            } else {
                // 返回默认解锁的故事
                foreach (var story in stories) {
                    if (story.UnlockCondition.Type == PetStoryUnlockType.Default) {
                        story.IsUnlocked = true;
                        result.Add(story);
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 检查并解锁故事
        /// </summary>
        public void CheckAndUnlockStories(int petId, int petTypeId, int affectionLevel, int evolutionStage, int battleCount, int expeditionSuccess, int breedingCount) {
            if (!playerPetStories.ContainsKey(petId)) {
                InitializePetStory(petId, petTypeId);
            }
            
            var data = playerPetStories[petId];
            var stories = database.GetStoriesForPet(petTypeId);
            
            foreach (var story in stories) {
                if (data.UnlockedStoryIds.Contains(story.StoryId)) continue;
                
                bool canUnlock = false;
                
                switch (story.UnlockCondition.Type) {
                    case PetStoryUnlockType.AffectionLevel:
                        canUnlock = affectionLevel >= story.UnlockCondition.RequiredValue;
                        break;
                    case PetStoryUnlockType.EvolutionStage:
                        canUnlock = evolutionStage >= story.UnlockCondition.RequiredValue;
                        break;
                    case PetStoryUnlockType.BattleCount:
                        canUnlock = battleCount >= story.UnlockCondition.RequiredValue;
                        break;
                    case PetStoryUnlockType.ExpeditionSuccess:
                        canUnlock = expeditionSuccess >= story.UnlockCondition.RequiredValue;
                        break;
                    case PetStoryUnlockType.BreedingCount:
                        canUnlock = breedingCount >= story.UnlockCondition.RequiredValue;
                        break;
                }
                
                if (canUnlock) {
                    data.UnlockedStoryIds.Add(story.StoryId);
                    data.StoryReadStatus[story.StoryId] = false;
                    EmitSignal(nameof(StoryUnlocked), petId, story);
                    GD.Print($"[PetStory] Story unlocked: {story.Title} for pet {petId}");
                }
            }
        }
        
        /// <summary>
        /// 标记故事为已读
        /// </summary>
        public void MarkStoryAsRead(int petId, int storyId) {
            if (playerPetStories.ContainsKey(petId)) {
                var data = playerPetStories[petId];
                if (data.StoryReadStatus.ContainsKey(storyId)) {
                    data.StoryReadStatus[storyId] = true;
                    EmitSignal(nameof(StoryRead), petId, storyId);
                }
            }
        }
        
        /// <summary>
        /// 检查是否有未读故事
        /// </summary>
        public bool HasUnreadStories(int petId) {
            if (playerPetStories.ContainsKey(petId)) {
                var data = playerPetStories[petId];
                foreach (var status in data.StoryReadStatus) {
                    if (!status.Value) return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 获取未读故事数量
        /// </summary>
        public int GetUnreadStoryCount(int petId) {
            int count = 0;
            if (playerPetStories.ContainsKey(petId)) {
                var data = playerPetStories[petId];
                foreach (var status in data.StoryReadStatus) {
                    if (!status.Value) count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// 获取解锁的故事数量
        /// </summary>
        public int GetUnlockedStoryCount(int petId) {
            if (playerPetStories.ContainsKey(petId)) {
                return playerPetStories[petId].UnlockedStoryIds.Count;
            }
            return 0;
        }
        
        /// <summary>
        /// 保存数据
        /// </summary>
        public Dictionary<string, object> Serialize() {
            var data = new Dictionary<string, object>();
            var petDataList = new List<Dictionary<string, object>>();
            
            foreach (var kvp in playerPetStories) {
                var petData = new Dictionary<string, object>();
                petData["petId"] = kvp.Key;
                petData["unlockedStoryIds"] = kvp.Value.UnlockedStoryIds;
                
                var readStatusList = new List<Dictionary<string, int>>();
                foreach (var status in kvp.Value.StoryReadStatus) {
                    readStatusList.Add(new Dictionary<string, int> {
                        ["storyId"] = status.Key,
                        ["read"] = status.Value ? 1 : 0
                    });
                }
                petData["readStatus"] = readStatusList;
                
                petDataList.Add(petData);
            }
            
            data["petStories"] = petDataList;
            return data;
        }
        
        /// <summary>
        /// 加载数据
        /// </summary>
        public void Deserialize(Dictionary<string, object> data) {
            if (!data.ContainsKey("petStories")) return;
            
            playerPetStories.Clear();
            var petDataList = (List<object>)data["petStories"];
            
            foreach (var petDataObj in petDataList) {
                var petDataDict = (Dictionary<string, object>)petDataObj;
                int petId = (int)petDataDict["petId"];
                
                var petData = new PlayerPetStoryData {
                    PetId = petId
                };
                
                var unlockedIds = (List<object>)petDataDict["unlockedStoryIds"];
                foreach (var id in unlockedIds) {
                    petData.UnlockedStoryIds.Add((int)id);
                }
                
                if (petDataDict.ContainsKey("readStatus")) {
                    var readStatusList = (List<object>)petDataDict["readStatus"];
                    foreach (var statusObj in readStatusList) {
                        var statusDict = (Dictionary<string, object>)statusObj;
                        int storyId = (int)statusDict["storyId"];
                        bool read = (int)statusDict["read"] == 1;
                        petData.StoryReadStatus[storyId] = read;
                    }
                }
                
                playerPetStories[petId] = petData;
            }
        }
        
        /// <summary>
        /// 清除宠物故事数据
        /// </summary>
        public void ClearPetStory(int petId) {
            if (playerPetStories.ContainsKey(petId)) {
                playerPetStories.Remove(petId);
            }
        }

        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return Serialize();
        }

        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            Deserialize(data);
        }
    }
}
