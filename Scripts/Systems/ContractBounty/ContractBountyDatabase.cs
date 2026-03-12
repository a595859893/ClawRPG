using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ContractBounty
{
    /// <summary>
    /// Contract Bounty Database - 委托赏金配置数据库
    /// </summary>
    
    public class ContractBountyDatabase
    {
        private static ContractBountyDatabase _instance;
        public static ContractBountyDatabase Instance => _instance ??= new ContractBountyDatabase();
        
        // 预定义的委托合同配置
        private List<ContractTemplate> _contractTemplates = new List<ContractTemplate>();
        
        public ContractBountyDatabase()
        {
            InitializeTemplates();
        }
        
        private void InitializeTemplates()
        {
            // Monster Hunt Contracts - 怪物狩猎
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "goblin_scout",
                title = "哥布林哨兵",
                description = "村庄附近的哥布林哨兵骚扰商队，需要将其消灭。",
                clientName = "商队护卫队长",
                type = ContractType.MonsterHunt,
                difficulty = ContractDifficulty.Easy,
                targetName = "哥布林哨兵",
                targetDescription = "绿色的矮小生物，手持简陋武器",
                requiredKills = 3,
                baseLevel = 5,
                goldReward = 100,
                expReward = 50,
                reputationReward = 5,
                timeLimit = 300,
                location = "落叶森林",
                tips = "哥布林害怕火把"
            });
            
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "wolf_pack",
                title = "狼群",
                description = "山区的狼群袭击牧民，需要消灭狼王并驱散狼群。",
                clientName = "牧场主",
                type = ContractType.MonsterHunt,
                difficulty = ContractDifficulty.Medium,
                targetName = "森林狼王",
                targetDescription = "巨大的灰色狼王，带领着狼群",
                requiredKills = 1,
                baseLevel = 15,
                goldReward = 300,
                expReward = 150,
                reputationReward = 15,
                timeLimit = 600,
                location = "灰雾山脉",
                tips = "先消灭小狼，狼王会变得虚弱"
            });
            
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "troll_bridge",
                title = "桥上巨魔",
                description = "桥上的巨魔收取过路费，商人无法通行。",
                clientName = "商人联合会",
                type = ContractType.MonsterHunt,
                difficulty = ContractDifficulty.Hard,
                targetName = "洞穴巨魔",
                targetDescription = "巨大的绿色生物，拥有再生能力",
                requiredKills = 1,
                baseLevel = 25,
                goldReward = 800,
                expReward = 400,
                reputationReward = 30,
                timeLimit = 900,
                location = "石桥",
                tips = "用火可以阻止再生"
            });
            
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "drake_cave",
                title = "龙穴",
                description = "火龙占据村庄附近的山洞，需要讨伐。",
                clientName = "村长",
                type = ContractType.MonsterHunt,
                difficulty = ContractDifficulty.Legendary,
                targetName = "火焰幼龙",
                targetDescription = "喷吐火焰的龙类生物",
                requiredKills = 1,
                baseLevel = 40,
                goldReward = 2000,
                expReward = 1000,
                reputationReward = 100,
                timeLimit = 1800,
                location = "烈焰山",
                tips = "注意躲避火焰吐息"
            });
            
            // Assassination Contracts - 暗杀
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "bandit_leader",
                title = "土匪头目",
                description = "土匪占据了废弃城堡，需要消灭头目。",
                clientName = "本地贵族",
                type = ContractType.Assassination,
                difficulty = ContractDifficulty.Medium,
                targetName = "土匪首领",
                targetDescription = "凶残的武装土匪，手持双刀",
                requiredKills = 1,
                baseLevel = 18,
                goldReward = 500,
                expReward = 250,
                reputationReward = 20,
                timeLimit = 600,
                location = "废弃城堡",
                tips = "潜行进入，避免惊动守卫"
            });
            
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "corrupt_knight",
                title = "堕落骑士",
                description = "曾经的圣殿骑士堕落为黑暗杀手，需要清理门户。",
                clientName = "圣殿骑士团",
                type = ContractType.Assassination,
                difficulty = ContractDifficulty.Hard,
                targetName = "暗影骑士",
                targetDescription = "穿着黑色铠甲的堕落骑士",
                requiredKills = 1,
                baseLevel = 30,
                goldReward = 1000,
                expReward = 500,
                reputationReward = 40,
                timeLimit = 900,
                location = "黑暗教堂",
                tips = "圣光系技能对其有奇效"
            });
            
            // Rescue Contracts - 救援
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "kidnapped_merchant",
                title = "被绑商人",
                description = "商人被地精绑架，需要在时限内救出。",
                clientName = "商人妻子",
                type = ContractType.Rescue,
                difficulty = ContractDifficulty.Easy,
                targetName = "被绑商人",
                targetDescription = "富态的商人，被关押在地精营地",
                requiredKills = 5,
                baseLevel = 8,
                goldReward = 200,
                expReward = 100,
                reputationReward = 10,
                timeLimit = 400,
                location = "地精洞穴",
                tips = "速战速决，时间有限"
            });
            
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "captured_knight",
                title = "被俘骑士",
                description = "骑士被敌人俘虏，需要从敌营救出。",
                clientName = "骑士团团长",
                type = ContractType.Rescue,
                difficulty = ContractDifficulty.Hard,
                targetName = "被俘骑士",
                targetDescription = "受伤的圣殿骑士，被囚禁在敌营",
                requiredKills = 15,
                baseLevel = 22,
                goldReward = 700,
                expReward = 350,
                reputationReward = 25,
                timeLimit = 800,
                location = "敌营",
                tips = "消灭所有守卫"
            });
            
            // Escort Contracts - 护送
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "caravan_protection",
                title = "商队护卫",
                description = "保护商队安全通过危险区域。",
                clientName = "商队老板",
                type = ContractType.Escort,
                difficulty = ContractDifficulty.Medium,
                targetName = "商队",
                targetDescription = "满载货物的商队，需要保护",
                requiredKills = 8,
                baseLevel = 12,
                goldReward = 400,
                expReward = 200,
                reputationReward = 15,
                timeLimit = 500,
                location = "荒野之路",
                tips = "注意埋伏"
            });
            
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "noble_escort",
                title = "贵族护送",
                description = "贵族需要安全送达目的地。",
                clientName = "贵族",
                type = ContractType.Escort,
                difficulty = ContractDifficulty.Hard,
                targetName = "贵族一家",
                targetDescription = "带着家眷和财宝的贵族",
                requiredKills = 12,
                baseLevel = 20,
                goldReward = 900,
                expReward = 450,
                reputationReward = 35,
                timeLimit = 700,
                location = "王座大道",
                tips = "保护贵族是关键"
            });
            
            // Collection Contracts - 收集
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "herb_gathering",
                title = "稀有药草",
                description = "收集特定草药用于制药。",
                clientName = "炼金术师",
                type = ContractType.Collection,
                difficulty = ContractDifficulty.Easy,
                targetName = "月光草",
                targetDescription = "夜晚发光的稀有药草",
                requiredKills = 5,
                baseLevel = 3,
                goldReward = 80,
                expReward = 40,
                reputationReward = 5,
                timeLimit = 300,
                location = "月光森林",
                tips = "夜间更容易找到"
            });
            
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "rare_crystals",
                title = "稀有水晶",
                description = "收集特定水晶用于附魔。",
                clientName = "附魔师",
                type = ContractType.Collection,
                difficulty = ContractDifficulty.Medium,
                targetName = "奥术水晶",
                targetDescription = "蕴含魔法能量的水晶",
                requiredKills = 3,
                baseLevel = 15,
                goldReward = 350,
                expReward = 175,
                reputationReward = 15,
                timeLimit = 600,
                location = "魔法洞窟",
                tips = "水晶有魔法保护"
            });
            
            // Defense Contracts - 防御
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "village_defense",
                title = "村庄防御",
                description = "保护村庄免受怪物袭击。",
                clientName = "村长",
                type = ContractType.Defense,
                difficulty = ContractDifficulty.Medium,
                targetName = "入侵怪物",
                targetDescription = "成群结队的怪物",
                requiredKills = 10,
                baseLevel = 10,
                goldReward = 350,
                expReward = 175,
                reputationReward = 15,
                timeLimit = 500,
                location = "边境村庄",
                tips = "守住入口"
            });
            
            _contractTemplates.Add(new ContractTemplate
            {
                templateId = "castle_defense",
                title = "城堡保卫战",
                description = "抵御大规模进攻，保卫城堡。",
                clientName = "城堡领主",
                type = ContractType.Defense,
                difficulty = ContractDifficulty.Legendary,
                targetName = "敌军",
                targetDescription = "大规模敌军进攻",
                requiredKills = 30,
                baseLevel = 35,
                goldReward = 1500,
                expReward = 750,
                reputationReward = 75,
                timeLimit = 1200,
                location = "王城",
                tips = "利用城墙优势"
            });
        }
        
        public List<ContractTemplate> GetTemplates()
        {
            return new List<ContractTemplate>(_contractTemplates);
        }
        
        public List<ContractTemplate> GetTemplatesByDifficulty(ContractDifficulty difficulty)
        {
            return _contractTemplates.FindAll(t => t.difficulty == difficulty);
        }
        
        public List<ContractTemplate> GetTemplatesByType(ContractType type)
        {
            return _contractTemplates.FindAll(t => t.type == type);
        }
        
        public ContractTemplate GetRandomTemplate(ContractDifficulty? difficulty = null, ContractType? type = null)
        {
            var candidates = _contractTemplates;
            
            if (difficulty.HasValue)
                candidates = candidates.FindAll(t => t.difficulty == difficulty.Value);
            
            if (type.HasValue)
                candidates = candidates.FindAll(t => t.type == type.Value);
            
            if (candidates.Count == 0)
                return null;
            
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }
    }
    
    [System.Serializable]
    public class ContractTemplate
    {
        public string templateId;
        public string title;
        public string description;
        public string clientName;
        public ContractType type;
        public ContractDifficulty difficulty;
        public string targetName;
        public string targetDescription;
        public int requiredKills;
        public int baseLevel;
        public int goldReward;
        public int expReward;
        public int reputationReward;
        public int timeLimit;
        public string location;
        public string tips;
    }
}
