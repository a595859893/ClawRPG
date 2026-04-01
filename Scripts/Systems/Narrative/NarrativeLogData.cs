using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Narrative
{
    /// <summary>
    /// 叙事碎片 - 预写的故事片段，按房间类型+楼层+循环分配
    /// </summary>
    [System.Serializable]
    public class NarrativeFragment
    {
        /// <summary>唯一ID，如 "library_burn_01"</summary>
        public string FragmentId;
        /// <summary>房间类型，如 "Library", "BossRoom", "Merchant"</summary>
        public string RoomType;
        /// <summary>楼层范围："1-5", "6-10", "11+"</summary>
        public string FloorRange;
        /// <summary>循环次数（已完成局次），0=第1局</summary>
        public int Loop;
        /// <summary>叙事文本内容</summary>
        public string NarrativeText;
        /// <summary>主题标签，帮助玩家关联碎片，如 "betrayal", "sacrifice"</summary>
        public string Theme;
    }

    /// <summary>
    /// 叙事碎片保存数据
    /// </summary>
    [System.Serializable]
    public class NarrativeLogSaveData
    {
        public List<string> CollectedFragmentIds = new List<string>();
        public int TotalFragments = 0;
        public int UniqueRoomsVisited = 0;
    }

    /// <summary>
    /// 房间访问记录，用于去重（同一局游戏中每个房间碎片只出现一次）
    /// </summary>
    [System.Serializable]
    public class RoomVisitRecord
    {
        public string RoomId;        // "Floor3_Library_A"
        public string FragmentId;     // 已分配的碎片ID
        public bool WasCollected;    // 玩家是否收集了该碎片
    }
}
