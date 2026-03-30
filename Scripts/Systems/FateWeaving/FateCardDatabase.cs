using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Data.FateWeaving;
using ClawRPG.Scripts.Database.Loaders;

namespace ClawRPG.Scripts.Systems.FateWeaving
{

    /// <summary>
    /// 子系统: 卡牌数据存储
    /// 负责存储和管理所有卡牌定义、路径定义、选择定义的数据
    /// </summary>
    public class FateCardDatabase : BaseSystem
    {

        private static FateCardDatabase _instance;
        public static new FateCardDatabase Instance
        {
            get
            {
                if (_instance == null) _instance = new FateCardDatabase();
                return _instance;
            }
        }

        /// <summary>
        /// 所有命运路径的定义数据
        /// </summary>
        public List<FatePathData> Paths { get; private set; }

        /// <summary>
        /// 所有选择的定义数据
        /// </summary>
        public List<FateChoice> Choices { get; private set; }

        public override void _Ready()
        {
            Instance = this;
            base._Ready();
        }

        protected override void Initialize()
        {
            LoadFromConfig();
            base.Initialize();
        }

        /// <summary>
        /// 从配置文件加载数据
        /// </summary>
        private void LoadFromConfig()
        {
            var loader = FateCardConfigLoader.Instance;

            // 尝试从Resources目录加载配置
            string configPath = "res://Resources/Config/fate_cards_config.json";

            if (!loader.Load(configPath))
            {
                GD.PrintErr($"[FateCardDatabase] 加载配置文件失败: {loader.LastError}");
                // 使用空数据
                Paths = new List<FatePathData>();
                Choices = new List<FateChoice>();
                return;
            }

            Paths = loader.GetAllPaths();
            Choices = loader.GetAllChoices();

            GD.Print($"[FateCardDatabase] 已加载 {Paths.Count} 个路径和 {Choices.Count} 个选择");
        }

        /// <summary>
        /// 根据路径类型获取路径数据
        /// </summary>
        public FatePathData GetPath(FatePathType type)
        {
            foreach (var path in Paths)
            {
                if (path.Type == type) return path;
            }
            return null;
        }

        /// <summary>
        /// 获取指定层级的所有可用选择
        /// </summary>
        public List<FateChoice> GetChoicesByTier(int tier)
        {
            var result = new List<FateChoice>();
            foreach (var choice in Choices)
            {
                if (choice.TierRequired <= tier)
                {
                    result.Add(choice);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取指定类型的所有选择
        /// </summary>
        public List<FateChoice> GetChoicesByType(FateChoiceType type)
        {
            var result = new List<FateChoice>();
            foreach (var choice in Choices)
            {
                if (choice.ChoiceType == type)
                {
                    result.Add(choice);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据 ID 获取选择
        /// </summary>
        public FateChoice GetChoiceById(string id)
        {
            foreach (var choice in Choices)
            {
                if (choice.Id == id) return choice;
            }
            return null;
        }

        /// <summary>
        /// 重置数据（用于新游戏）
        /// </summary>
        public override void Reset()
        {
            LoadFromConfig();
            base.Reset();
        }

        #region BaseSystem Persistence

        public override Dictionary<string, object> ExportSaveData() => new Dictionary<string, object>();

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
        }

        #endregion
    }
}
