using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 对象池系统 - 用于复用节点以提升性能
    /// 应用2D游戏性能优化知识
    /// </summary>
    public partial class ObjectPool : BaseSystem
    {
        // Pool configuration
        
        /// <summary>
        /// 初始池大小
        /// </summary>
        [Export] public int InitialPoolSize = 10;
        
        /// <summary>
        /// 最大池大小
        /// </summary>
        [Export] public int MaxPoolSize = 50;
        
        /// <summary>
        /// 是否自动扩展
        /// </summary>
        [Export] public bool AutoExpand = true;
        
        // The scene to pool
        
        /// <summary>
        /// 要池化的场景
        /// </summary>
        [Export] public PackedScene PooledScene;
        
        // Pool storage
        private Queue<Node2D> _availableObjects = new();
        private HashSet<Node2D> _activeObjects = new();
        
        // Parent node for pooled objects
        private Node _pooledObjectsParent;
        
        public override void _Ready()
        {
            // Create parent for pooled objects
            _pooledObjectsParent = new Node2D();
            _pooledObjectsParent.Name = "ObjectPool_" + Name;
            GetTree().CurrentScene.AddChild(_pooledObjectsParent);
            
            // Pre-instantiate objects
            for (int i = 0; i < InitialPoolSize; i++)
            {
                CreateNewObject();
            }
            
            GD.Print("[ObjectPool] Initialized with " + InitialPoolSize + " objects");
        }
        
        /// <summary>
        /// Get an object from the pool, creating new if needed
        /// </summary>
        /// <returns>池化的节点对象，如果池已满且不允许自动扩展则返回null</returns>
        public Node2D GetObject()
        {
            Node2D obj;
            
            if (_availableObjects.Count > 0)
            {
                obj = _availableObjects.Dequeue();
            }
            else if (AutoExpand && _activeObjects.Count + _availableObjects.Count < MaxPoolSize)
            {
                obj = CreateNewObject();
            }
            else
            {
                GD.Warning("[ObjectPool] Pool exhausted and auto-expand disabled");
                return null;
            }
            
            _activeObjects.Add(obj);
            obj.Visible = true;
            return obj;
        }
        
        /// <summary>
        /// 返还对象到池中
        /// </summary>
        /// <param name="obj">要返还的节点</param>
        public void ReturnObject(Node2D obj)
        {
            if (obj == null) return;
            
            if (_activeObjects.Contains(obj))
            {
                _activeObjects.Remove(obj);
                obj.Visible = false; 
                _availableObjects.Enqueue(obj);
            }
        }
        
        /// <summary>
        /// Create a new object for the pool
        /// </summary>
        private Node2D CreateNewObject()
        {
            if (PooledScene == null)
            {
                GD.Warning("[ObjectPool] No scene assigned");
                return null;
            }
            
            var obj = PooledScene.Instantiate<Node2D>();
            if (obj != null)
            {
                obj.Visible = false; 
                _pooledObjectsParent.AddChild(obj);
                _availableObjects.Enqueue(obj);
            }
            return obj;
        }
        
        /// <summary>
        /// Get active object count
        /// </summary>
        public int GetActiveCount() => _activeObjects.Count;
        
        /// <summary>
        /// Get available object count
        /// </summary>
        public int GetAvailableCount() => _availableObjects.Count;
        
        /// <summary>
        /// Clear all objects from pool
        /// </summary>
        public void ClearPool()
        {
            foreach (var obj in _activeObjects)
            {
                obj.QueueFree();
            }
            _activeObjects.Clear();
            
            while (_availableObjects.Count > 0)
            {
                var obj = _availableObjects.Dequeue();
                obj.QueueFree();
            }
        }

        #region Persistence

        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // No persistent data needed
        }

        #endregion
    }
}
