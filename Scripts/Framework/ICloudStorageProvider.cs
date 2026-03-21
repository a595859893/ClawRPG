using System;
using System.Collections.Generic;

namespace ClawRPG.Framework
{
    /// <summary>
    /// 云存储提供商接口，支持 Steam Cloud、Local File 等多种实现
    /// </summary>
    public interface ICloudStorageProvider
    {
        /// <summary>
        /// 上传存档槽到云端
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <param name="jsonData">存档的 JSON 数据</param>
        /// <returns>上传是否成功</returns>
        bool UploadSlot(int slot, string jsonData);

        /// <summary>
        /// 从云端下载存档槽
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <returns>存档的 JSON 数据，云端无此槽返回 null</returns>
        string DownloadSlot(int slot);

        /// <summary>
        /// 删除云端存档槽
        /// </summary>
        /// <param name="slot">存档槽位编号</param>
        /// <returns>删除是否成功</returns>
        bool DeleteSlot(int slot);

        /// <summary>
        /// 列出云端所有存档槽信息
        /// </summary>
        /// <returns>存档槽信息列表</returns>
        List<SaveSlotInfo> ListSlots();

        /// <summary>
        /// 获取云存储使用量（字节）
        /// </summary>
        /// <returns>当前已使用的存储字节数</returns>
        long GetStorageUsageBytes();
    }

    /// <summary>
    /// 存档槽信息结构
    /// </summary>
    public struct SaveSlotInfo
    {
        /// <summary>
        /// 存档槽位编号
        /// </summary>
        public int Slot;

        /// <summary>
        /// 存档名称
        /// </summary>
        public string SaveName;

        /// <summary>
        /// 存档时间
        /// </summary>
        public DateTime SaveTime;

        /// <summary>
        /// 游玩时长（秒）
        /// </summary>
        public int PlayTime;

        /// <summary>
        /// 位置名称
        /// </summary>
        public string LocationName;

        /// <summary>
        /// 当前等级
        /// </summary>
        public int Level;
    }
}
