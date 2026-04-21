using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace ClawRPG.Framework
{
    /// <summary>
    /// 本地文件云存储提供器
    /// 使用本地文件系统模拟云存储，将存档以 JSON 格式存储到 user://cloud_saves/ 目录
    /// </summary>
    public partial class LocalCloudStorageProvider : BaseSystem, ICloudStorageProvider
    {
        /// <summary>
        /// 云存储根目录（相对于 user://）
        /// </summary>
        private const string CloudSaveDirectory = "cloud_saves";

        /// <summary>
        /// 获取存档槽文件路径
        /// </summary>
        private string GetSlotPath(int slot)
        {
            return CloudSaveDirectory + "/slot_" + slot.ToString() + ".json";
        }

        /// <summary>
        /// 确保云存储目录存在
        /// </summary>
        private bool EnsureDirectoryExists()
        {
            var dir = DirAccess.Open("user://");
            if (dir == null)
            {
                GD.PrintErr("[LocalCloudStorageProvider] Failed to open user:// directory");
                return false;
            }

            if (!dir.DirExists(CloudSaveDirectory))
            {
                var error = dir.MakeDir(CloudSaveDirectory);
                if (error != Error.Ok)
                {
                    GD.PrintErr("[LocalCloudStorageProvider] Failed to create directory: " + CloudSaveDirectory);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 从 JSON 文件中解析存档槽信息
        /// </summary>
        private SaveSlotInfo? ParseSlotInfoFromJson(int slot, string jsonData)
        {
            try
            {
                var json = new Json();
                var parseResult = json.Parse(jsonData);
                if (parseResult != Error.Ok)
                {
                    return null;
                }

                var data = json.Data.AsGodotDictionary();
                if (data == null)
                {
                    return null;
                }

                var info = new SaveSlotInfo();
                info.Slot = slot;

                if (data.Contains("save_name") && data["save_name"] != null)
                {
                    info.SaveName = data["save_name"].ToString();
                }
                else
                {
                    info.SaveName = "Slot " + slot;
                }

                if (data.Contains("save_time") && data["save_time"] != null)
                {
                    if (long.TryParse(data["save_time"].ToString(), out var timestamp))
                    {
                        info.SaveTime = DateTime.FromBinary(timestamp);
                    }
                }
                else
                {
                    info.SaveTime = DateTime.Now;
                }

                if (data.Contains("play_time") && data["play_time"] != null)
                {
                    if (int.TryParse(data["play_time"].ToString(), out var playTime))
                    {
                        info.PlayTime = playTime;
                    }
                }
                else
                {
                    info.PlayTime = 0;
                }

                if (data.Contains("location_name") && data["location_name"] != null)
                {
                    info.LocationName = data["location_name"].ToString();
                }
                else
                {
                    info.LocationName = "Unknown";
                }

                if (data.Contains("level") && data["level"] != null)
                {
                    if (int.TryParse(data["level"].ToString(), out var level))
                    {
                        info.Level = level;
                    }
                }
                else
                {
                    info.Level = 1;
                }

                return info;
            }
            catch (Exception e)
            {
                GD.PrintWarn("[LocalCloudStorageProvider] Failed to parse slot info for slot " + slot + ": " + e.Message);
                return null;
            }
        }

        public bool UploadSlot(int slot, string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                GD.PrintWarn("[LocalCloudStorageProvider] UploadSlot called with null or empty data for slot " + slot);
                return false;
            }

            if (!EnsureDirectoryExists())
            {
                return false;
            }

            var path = "user://" + GetSlotPath(slot);
            using (var file = Godot.FileAccess.Open(path, FileAccess.ModeFlags.Write))
            {
                if (file == null)
                {
                    GD.PrintErr("[LocalCloudStorageProvider] Failed to open file for writing: " + path);
                    return false;
                }

                file.StoreString(jsonData);
                file.Flush();
            }

            GD.Print("[LocalCloudStorageProvider] Uploaded slot " + slot + " to " + path);
            return true;
        }

        public string DownloadSlot(int slot)
        {
            var path = "user://" + GetSlotPath(slot);

            if (!Godot.FileAccess.FileExists(path))
            {
                GD.Print("[LocalCloudStorageProvider] Slot " + slot + " does not exist in cloud storage");
                return null;
            }

            using (var file = Godot.FileAccess.Open(path, FileAccess.ModeFlags.Read))
            {
                if (file == null)
                {
                    GD.PrintErr("[LocalCloudStorageProvider] Failed to open file for reading: " + path);
                    return null;
                }

                var jsonData = file.GetAsText();
                GD.Print("[LocalCloudStorageProvider] Downloaded slot " + slot + " from cloud storage");
                return jsonData;
            }
        }

        public bool DeleteSlot(int slot)
        {
            var path = "user://" + GetSlotPath(slot);

            if (!Godot.FileAccess.FileExists(path))
            {
                GD.Print("[LocalCloudStorageProvider] Slot " + slot + " does not exist, nothing to delete");
                return true;
            }

            var dir = DirAccess.Open("user://");
            if (dir == null)
            {
                GD.PrintErr("[LocalCloudStorageProvider] Failed to open user:// directory");
                return false;
            }

            var error = dir.Remove(GetSlotPath(slot));
            if (error != Error.Ok)
            {
                GD.PrintErr("[LocalCloudStorageProvider] Failed to delete slot " + slot + ": " + error.ToString());
                return false;
            }

            GD.Print("[LocalCloudStorageProvider] Deleted slot " + slot + " from cloud storage");
            return true;
        }

        public List<SaveSlotInfo> ListSlots()
        {
            var result = new List<SaveSlotInfo>();

            if (!EnsureDirectoryExists())
            {
                return result;
            }

            var dir = DirAccess.Open("user://" + CloudSaveDirectory);
            if (dir == null)
            {
                GD.PrintErr("[LocalCloudStorageProvider] Failed to open cloud saves directory");
                return result;
            }

            dir.ListDirBegin();
            var fileName = dir.GetNext();

            while (!string.IsNullOrEmpty(fileName))
            {
                if (fileName.StartsWith("slot_") && fileName.EndsWith(".json"))
                {
                    var slotStr = fileName.Substring(5, fileName.Length - 10);
                    if (int.TryParse(slotStr, out var slot))
                    {
                        var path = "user://" + CloudSaveDirectory + "/" + fileName;
                        using (var file = Godot.FileAccess.Open(path, FileAccess.ModeFlags.Read))
                        {
                            if (file != null)
                            {
                                var jsonData = file.GetAsText();
                                var info = ParseSlotInfoFromJson(slot, jsonData);
                                if (info.HasValue)
                                {
                                    result.Add(info.Value);
                                }
                            }
                        }
                    }
                }
                fileName = dir.GetNext();
            }

            dir.ListDirEnd();
            result.Sort((a, b) => a.Slot.CompareTo(b.Slot));

            GD.Print("[LocalCloudStorageProvider] Listed " + result.Count + " cloud slots");
            return result;
        }

        public long GetStorageUsageBytes()
        {
            long totalBytes = 0;

            if (!EnsureDirectoryExists())
            {
                return 0;
            }

            var dir = DirAccess.Open("user://" + CloudSaveDirectory);
            if (dir == null)
            {
                GD.PrintErr("[LocalCloudStorageProvider] Failed to open cloud saves directory for size calculation");
                return 0;
            }

            dir.ListDirBegin();
            var fileName = dir.GetNext();

            while (!string.IsNullOrEmpty(fileName))
            {
                if (!fileName.StartsWith(".") && !fileName.StartsWith(".."))
                {
                    var path = "user://" + CloudSaveDirectory + "/" + fileName;
                    if (Godot.FileAccess.FileExists(path))
                    {
                        using (var file = Godot.FileAccess.Open(path, FileAccess.ModeFlags.Read))
                        {
                            if (file != null)
                            {
                                var content = file.GetAsText();
                                if (content != null)
                                {
                                    totalBytes += System.Text.Encoding.UTF8.GetByteCount(content);
                                }
                            }
                        }
                    }
                }
                fileName = dir.GetNext();
            }

            dir.ListDirEnd();

            GD.Print("[LocalCloudStorageProvider] Total storage usage: " + totalBytes + " bytes");
            return totalBytes;
        }

        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>();
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
        }
    }
}
