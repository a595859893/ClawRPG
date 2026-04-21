using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Data;

/// <summary>
/// MountTrainingUI 桥接器 - REQ-075 解耦示例
/// 
/// 职责：将 MountTrainingUI 的事件连接到 MountTrainingSystem 的方法。
/// 这样 UI 不再直接持有 System 引用，而是通过事件广播操作意图，
/// 由 Bridge 负责将事件转换为 System 调用并更新 UI 展示数据。
/// 
/// 这是 REQ-075 UI/业务逻辑解耦的 pilot 实现。
/// </summary>
public partial class MountTrainingUI
{
    private class UIBridge
    {
        private readonly MountTrainingUI _ui;
        private readonly MountTrainingSystem _system;
        private readonly MountTrainingDatabase _database;
        private string _currentMountId = "";
        private string _selectedProjectId = "";
        
        public UIBridge(MountTrainingUI ui)
        {
            _ui = ui;
            _system = MountTrainingSystem.Instance;
            _database = _system != null ? GetDatabase() : null;
            
            // 订阅 UI 事件
            _ui.OnMountSelected += HandleMountSelected;
            _ui.OnProjectSelected += HandleProjectSelected;
            _ui.OnTrainingStarted += HandleTrainingStarted;
            _ui.OnRefreshRequested += HandleRefreshRequested;
        }
        
        private MountTrainingDatabase GetDatabase()
        {
            // 通过反射或已知接口获取 Database 引用
            // 由于 Database 是 private 字段，这里需要 System 提供访问方法
            // 暂时通过 System 的已知方法间接访问
            return null;
        }
        
        private void HandleMountSelected(string mountId)
        {
            if (_system == null) return;
            _currentMountId = mountId;
            
            // 更新坐骑显示
            var data = _system.GetOrCreateTrainingData(mountId);
            int expProgress = _system.GetExperienceProgress(mountId);
            int bondProgress = _system.GetBondProgress(mountId);
            var skills = _system.GetUnlockedSkills(mountId);
            
            _ui.UpdateMountDisplay(
                data.Level,
                data.BondLevel,
                expProgress,
                bondProgress,
                skills.ToArray()
            );
            
            // 更新训练列表
            RefreshTrainingLists();
        }
        
        private void HandleProjectSelected(string mountId, string projectId)
        {
            if (_system == null || _database == null) return;
            _selectedProjectId = projectId;
            
            // 解析 projectId 获取类别和索引 (格式: "Category_Index")
            // 这里需要 projectId 解析逻辑
            // 暂时跳过详细信息更新
            _ui.UpdateProjectDetails(null, 0);
        }
        
        private void HandleTrainingStarted(string mountId, string projectId)
        {
            if (_system == null) return;
            
            bool success = _system.StartTraining(mountId, projectId);
            _ui.ShowTrainingResult(success);
            
            if (success)
            {
                // 刷新显示
                HandleMountSelected(mountId);
            }
        }
        
        private void HandleRefreshRequested()
        {
            if (_system == null) return;
            
            // 更新坐骑列表
            var mountIds = new List<string>();
            var trainingData = _system.GetOrCreateTrainingData(_currentMountId);
            mountIds.Add(_currentMountId);
            _ui.UpdateMountList(mountIds.ToArray());
            
            // 更新当前坐骑显示
            HandleMountSelected(_currentMountId);
            
            // 更新统计
            var stats = _system.GetStatistics();
            _ui.UpdateStatistics(
                stats.ContainsKey("TotalTrainingSessions") ? Convert.ToInt32(stats["TotalTrainingSessions"]) : 0,
                stats.ContainsKey("TotalExperienceGained") ? Convert.ToInt32(stats["TotalExperienceGained"]) : 0,
                stats.ContainsKey("AverageLevel") ? Convert.ToInt32(stats["AverageLevel"]) : 0,
                stats.ContainsKey("AverageBondLevel") ? Convert.ToInt32(stats["AverageBondLevel"]) : 0
            );
        }
        
        private void RefreshTrainingLists()
        {
            if (_system == null || string.IsNullOrEmpty(_currentMountId)) return;
            
            var projects = new TrainingProject[6][];
            for (int i = 0; i < 6; i++)
            {
                var cat = (TrainingCategory)i;
                var catProjects = _system.GetProjectsByCategory(_currentMountId, cat);
                projects[i] = catProjects.ToArray();
            }
            
            _ui.UpdateTrainingLists(projects);
        }
    }
}
