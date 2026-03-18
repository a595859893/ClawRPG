using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 数据库基类，提供通用功能
    /// </summary>
    public abstract class DatabaseBase : IDatabase
    {
        private const string SAVE_VERSION = "1.0";
        private const string KEY_SAVE_VERSION = "_saveVersion";

        /// <summary>
        /// 子类实例的静态引用
        /// </summary>
        public abstract object Instance { get; }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 验证数据完整性
        /// </summary>
        public virtual bool ValidateData() => true;

        /// <summary>
        /// 通用的数据存储字典（子类可复用）
        /// </summary>
        protected Dictionary<string, object> _dataStore = new Dictionary<string, object>();

        /// <summary>
        /// 通过 key 获取数据（IDatabase 实现）
        /// </summary>
        public virtual T GetData<T>(string key) where T : class
        {
            if (_dataStore.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return null;
        }

        /// <summary>
        /// 获取所有数据的 key（IDatabase 实现）
        /// </summary>
        public virtual IEnumerable<string> GetAllKeys()
        {
            return _dataStore.Keys;
        }

        /// <summary>
        /// 获取数据总数（IDatabase 实现）
        /// </summary>
        public virtual int GetDataCount()
        {
            return _dataStore.Count;
        }

        /// <summary>
        /// 导出存档数据（返回 Godot Dictionary 兼容 BaseSystem）
        /// </summary>
        public virtual Godot.Collections.Dictionary ExportSaveData()
        {
            var saveData = new Godot.Collections.Dictionary
            {
                [KEY_SAVE_VERSION] = SAVE_VERSION
            };
            OnExportSaveData(saveData);
            return saveData;
        }

        /// <summary>
        /// 导入存档数据
        /// </summary>
        public virtual void ImportSaveData(Godot.Collections.Dictionary saveData)
        {
            if (saveData == null || saveData.Count == 0)
                return;
            OnImportSaveData(saveData);
        }

        /// <summary>
        /// 子类重写：导出子类特定数据
        /// </summary>
        protected virtual void OnExportSaveData(Godot.Collections.Dictionary saveData)
        {
        }

        /// <summary>
        /// 子类重写：导入子类特定数据
        /// </summary>
        protected virtual void OnImportSaveData(Godot.Collections.Dictionary saveData)
        {
        }
    }
}
