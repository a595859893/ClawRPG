using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.UI
{
    /// <summary>
    /// UI 管理器 - 协调多个UI组件和面板
    /// </summary>
    public partial class UIManager : BaseSystem
    {
        private UIPanelController _panelController;
        private Node _mainNode;
        
        // UI 状态追踪
        private Dictionary<string, bool> _uiStates = new Dictionary<string, bool>();
        
        public override void _Ready()
        {
            base._Ready();
            _panelController = new UIPanelController();
        }
        
        /// <summary>
        /// 初始化 UI 管理器
        /// </summary>
        public void Initialize(Node mainNode)
        {
            _mainNode = mainNode;
            _panelController.SetMainNode(mainNode);
            GD.Print("[UIManager] Initialized");
        }
        
        /// <summary>
        /// 切换 UI 显示
        /// </summary>
        public void ToggleUI(string uiName)
        {
            _panelController.TogglePanel(uiName);
            _uiStates[uiName] = _panelController.IsPanelVisible(uiName);
        }
        
        /// <summary>
        /// 显示 UI
        /// </summary>
        public void ShowUI(string uiName)
        {
            _panelController.ShowPanel(uiName);
            _uiStates[uiName] = true;
        }
        
        /// <summary>
        /// 隐藏 UI
        /// </summary>
        public void HideUI(string uiName)
        {
            _panelController.HidePanel(uiName);
            _uiStates[uiName] = false;
        }
        
        /// <summary>
        /// 检查 UI 是否可见
        /// </summary>
        public bool IsUIVisible(string uiName)
        {
            if (_uiStates.ContainsKey(uiName))
            {
                return _uiStates[uiName];
            }
            return _panelController.IsPanelVisible(uiName);
        }
        
        /// <summary>
        /// 隐藏所有 UI
        /// </summary>
        public void HideAllUI()
        {
            _panelController.HideAllPanels();
            foreach (var key in _uiStates.Keys)
            {
                _uiStates[key] = false;
            }
        }
        
        /// <summary>
        /// 注册新 UI
        /// </summary>
        public void RegisterUI(string name, string path, string displayName = "")
        {
            _panelController.RegisterPanel(name, path, displayName);
        }
        
        /// <summary>
        /// 获取所有可见的 UI
        /// </summary>
        public List<string> GetVisibleUIs()
        {
            var result = new List<string>();
            foreach (var kvp in _uiStates)
            {
                if (kvp.Value)
                {
                    result.Add(kvp.Key);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 显示消息提示
        /// </summary>
        public void ShowMessage(string message, float duration = 2.0f)
        {
            GD.Print($"[UIManager] Message: {message}");
            // 可以扩展为显示 toast 或通知
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 导出 UI 状态
            var states = new Dictionary<string, object>();
            foreach (var kvp in _uiStates)
            {
                states[kvp.Key] = kvp.Value;
            }
            data["ui_states"] = states;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 导入 UI 状态
            if (data.Contains("ui_states"))
            {
                var states = data["ui_states"] as Dictionary;
                if (states != null)
                {
                    foreach (var key in states.Keys)
                    {
                        _uiStates[key.ToString()] = (bool)states[key];
                    }
                }
            }
        }
    }
}
