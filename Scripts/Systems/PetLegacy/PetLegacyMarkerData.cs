using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.PetLegacy
{
    /// <summary>
    /// 宠物遗产标记类型
    /// </summary>
    public enum LegacyType
    {
        /// <summary>墓碑 — 通用类型</summary>
        Tombstone = 0,
        /// <summary>灵魂光球 — 高友谊等级宠物死亡后显示</summary>
        Soul = 1,
        /// <summary>战旗 — 大型宠物死亡后显示</summary>
        Banner = 2
    }

    /// <summary>
    /// 单个宠物遗产标记数据
    /// </summary>
    [System.Serializable]
    public class PetLegacyMarkerData
    {
        /// <summary>宠物ID</summary>
        public int PetId;
        /// <summary>宠物名称</summary>
        public string PetName;
        /// <summary>宠物颜色（用于灵魂光球着色）</summary>
        public string PetColor;
        /// <summary>死亡时所在战斗的BattleId</summary>
        public string DeathBattleId;
        /// <summary>死亡时间戳</summary>
        public float DeathTimestamp;
        /// <summary>遗产标记类型</summary>
        public LegacyType MarkerType;
        /// <summary>死亡时友谊等级</summary>
        public int FriendshipLevel;
        /// <summary>累计共同战斗次数</summary>
        public int TotalBattles;
        /// <summary>是否处于休眠状态（超过3个标记时）</summary>
        public bool IsDormant;

        public PetLegacyMarkerData()
        {
            PetId = 0;
            PetName = "";
            PetColor = "#FFFFFF";
            DeathBattleId = "";
            DeathTimestamp = 0f;
            MarkerType = LegacyType.Tombstone;
            FriendshipLevel = 0;
            TotalBattles = 0;
            IsDormant = false;
        }

        public PetLegacyMarkerData(int petId, string petName, string petColor, string deathBattleId,
            float deathTimestamp, LegacyType markerType, int friendshipLevel, int totalBattles)
        {
            PetId = petId;
            PetName = petName;
            PetColor = petColor;
            DeathBattleId = deathBattleId;
            DeathTimestamp = deathTimestamp;
            MarkerType = markerType;
            FriendshipLevel = friendshipLevel;
            TotalBattles = totalBattles;
            IsDormant = false;
        }
    }

    /// <summary>
    /// 宠物遗产数据库 — 持有所有已收集的遗产标记
    /// </summary>
    [System.Serializable]
    public class PetLegacyDatabase
    {
        /// <summary>所有已收集的遗产标记（按死亡时间排序）</summary>
        public List<PetLegacyMarkerData> Markers = new List<PetLegacyMarkerData>();
        /// <summary>当前激活的标记petId列表（最多3个，FIFO）</summary>
        public List<int> ActiveMarkerIds = new List<int>();
        /// <summary>已休眠的标记petId列表</summary>
        public List<int> DormantMarkerIds = new List<int>();

        /// <summary>
        /// 添加新标记，自动管理激活/休眠状态
        /// </summary>
        public void AddMarker(PetLegacyMarkerData marker)
        {
            Markers.Add(marker);

            // 激活状态管理：最多3个激活
            if (ActiveMarkerIds.Count < 3)
            {
                ActiveMarkerIds.Add(marker.PetId);
                marker.IsDormant = false;
            }
            else
            {
                // 将最旧的激活标记转为休眠
                int oldestId = ActiveMarkerIds[0];
                var oldest = Markers.Find(m => m.PetId == oldestId);
                if (oldest != null)
                {
                    oldest.IsDormant = true;
                }
                DormantMarkerIds.Add(oldestId);
                ActiveMarkerIds.RemoveAt(0);

                // 新标记加入激活
                ActiveMarkerIds.Add(marker.PetId);
                marker.IsDormant = false;
            }
        }

        /// <summary>
        /// 获取当前所有激活标记
        /// </summary>
        public List<PetLegacyMarkerData> GetActiveMarkers()
        {
            var result = new List<PetLegacyMarkerData>();
            foreach (var id in ActiveMarkerIds)
            {
                var marker = Markers.Find(m => m.PetId == id);
                if (marker != null)
                    result.Add(marker);
            }
            return result;
        }

        /// <summary>
        /// 获取指定宠物ID的标记
        /// </summary>
        public PetLegacyMarkerData GetMarkerByPetId(int petId)
        {
            return Markers.Find(m => m.PetId == petId);
        }

        /// <summary>
        /// 获取遗产类型对应的视觉效果配置
        /// </summary>
        public static string GetMarkerScenePath(LegacyType type)
        {
            return type switch
            {
                LegacyType.Soul => "res://Scenes/Systems/PetLegacy/PetLegacySoul.tscn",
                LegacyType.Banner => "res://Scenes/Systems/PetLegacy/PetLegacyBanner.tscn",
                _ => "res://Scenes/Systems/PetLegacy/PetLegacyTombstone.tscn"
            };
        }
    }

    /// <summary>
    /// 宠物遗产系统保存数据
    /// </summary>
    [System.Serializable]
    public class PetLegacySaveData
    {
        /// <summary>所有标记数据</summary>
        public List<PetLegacyMarkerData> Markers = new List<PetLegacyMarkerData>();
        /// <summary>激活标记的宠物ID列表</summary>
        public List<int> ActiveMarkerIds = new List<int>();
        /// <summary>休眠标记的宠物ID列表</summary>
        public List<int> DormantMarkerIds = new List<int>();
    }
}
