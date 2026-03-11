using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Generic object pool for reusing nodes to improve performance
    /// Applies knowledge from 2D game performance optimization
    /// </summary>
    public partial class ObjectPool : Node
    {
        // Pool configuration
        [Export] public int InitialPoolSize = 10;
        [Export] public int MaxPoolSize = 50;
        [Export] public bool AutoExpand = true;
        
        // The scene to pool
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
        /// Return an object to the pool
        /// </summary>
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
    }
}
