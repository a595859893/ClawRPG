using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 数据库基类，提供通用功能
    /// </summary>
    public abstract class DatabaseBase : IDatabase
    {
        private static Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public abstract object Instance { get; }
        public abstract void Initialize();
        public virtual bool ValidateData() => true;

        /// <summary>
        /// 通用的实例获取模式
        /// </summary>
        protected static T GetOrCreate<T>() where T : new()
        {
            if (!_instances.ContainsKey(typeof(T)))
                _instances[typeof(T)] = new T();
            return (T)_instances[typeof(T)];
        }
    }
}
