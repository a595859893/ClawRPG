using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Items {
    /// <summary>
    /// Equipment set database - contains all equipment sets
    /// </summary>
    public class EquipmentSetDatabase
    {
        private static EquipmentSetDatabase _instance;
        public static EquipmentSetDatabase Instance => _instance ??= new EquipmentSetDatabase();
        
        private Dictionary<int, EquipmentSet> _sets = new();
        
        public EquipmentSetDatabase()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            // === 套装1: 战士之力 (Warrior's Might) ===
            var warriorSet = new EquipmentSet
            {
                SetId = 1,
                SetName = "Warrior's Might",
                SetNameCN = "战士之力",
                Description = "古老战士的遗留下的套装，蕴含着强大的战斗之力",
                EquipmentIds = new List<int> { 1001, 1002, 1003, 1004, 1005 }
            };
            warriorSet.Bonuses.Add(new SetBonusEffect(2, "战士之力", "攻击+10%"));
            warriorSet.Bonuses.Add(new SetBonusEffect(3, "愤怒打击", "暴击率+10%"));
            warriorSet.Bonuses.Add(new SetBonusEffect(5, "无尽战斗", "攻击+25%, 暴击伤害+20%"));
            _sets[1] = warriorSet;
            
            // === 套装2: 法师长袍 (Mage's Robes) ===
            var mageSet = new EquipmentSet
            {
                SetId = 2,
                SetName = "Mage's Robes",
                SetNameCN = "法师长袍",
                Description = "蕴含着奥术能量的法师套装",
                EquipmentIds = new List<int> { 1011, 1012, 1013, 1014, 1015 }
            };
            mageSet.Bonuses.Add(new SetBonusEffect(2, "法力洪流", "法力上限+100"));
            mageSet.Bonuses.Add(new SetBonusEffect(3, "奥术强化", "魔法伤害+15%"));
            mageSet.Bonuses.Add(new SetBonusEffect(5, "元素之主", "全元素抗性+20%"));
            _sets[2] = mageSet;
            
            // === 套装3: 刺客阴影 (Assassin's Shadow) ===
            var assassinSet = new EquipmentSet
            {
                SetId = 3,
                SetName = "Assassin's Shadow",
                SetNameCN = "刺客阴影",
                Description = "暗影刺客的专属套装",
                EquipmentIds = new List<int> { 1021, 1022, 1023, 1024, 1025 }
            };
            assassinSet.Bonuses.Add(new SetBonusEffect(2, "暗影步伐", "移动速度+10%"));
            assassinSet.Bonuses.Add(new SetBonusEffect(3, "致命一击", "暴击率+15%"));
            assassinSet.Bonuses.Add(new SetBonusEffect(5, "无声杀手", "暴击伤害+40%, 攻击速度+20%"));
            _sets[3] = assassinSet;
            
            // === 套装4: 龙鳞护甲 (Dragon Scale Armor) ===
            var dragonSet = new EquipmentSet
            {
                SetId = 4,
                SetName = "Dragon Scale Armor",
                SetNameCN = "龙鳞护甲",
                Description = "用巨龙鳞片打造的顶级防御套装",
                EquipmentIds = new List<int> { 1031, 1032, 1033, 1034, 1035 }
            };
            dragonSet.Bonuses.Add(new SetBonusEffect(2, "龙鳞护体", "防御+20%"));
            dragonSet.Bonuses.Add(new SetBonusEffect(3, "龙血沸腾", "生命上限+15%"));
            dragonSet.Bonuses.Add(new SetBonusEffect(5, "巨龙之力", "防御+40%, 生命+30%, 全抗性+15%"));
            _sets[4] = dragonSet;
            
            // === 套装5: 神圣之光 (Holy Light) ===
            var holySet = new EquipmentSet
            {
                SetId = 5,
                SetName = "Holy Light",
                SetNameCN = "神圣之光",
                Description = "神圣殿堂祭司的神圣套装",
                EquipmentIds = new List<int> { 1041, 1042, 1043, 1044, 1045 }
            };
            holySet.Bonuses.Add(new SetBonusEffect(2, "圣光护体", "神圣抗性+25%"));
            holySet.Bonuses.Add(new SetBonusEffect(3, "治疗之光", "治疗效果+20%"));
            holySet.Bonuses.Add(new SetBonusEffect(5, "天使降临", "全抗性+20%, 生命恢复+5/秒"));
            _sets[5] = holySet;
            
            // === 套装6: 元素大师 (Elemental Master) ===
            var elementalSet = new EquipmentSet
            {
                SetId = 6,
                SetName = "Elemental Master",
                SetNameCN = "元素大师",
                Description = "掌控四大元素的法师套装",
                EquipmentIds = new List<int> { 1051, 1052, 1053, 1054, 1055 }
            };
            elementalSet.Bonuses.Add(new SetBonusEffect(2, "元素亲和", "全元素伤害+10%"));
            elementalSet.Bonuses.Add(new SetBonusEffect(3, "元素爆发", "元素伤害+20%"));
            elementalSet.Bonuses.Add(new SetBonusEffect(5, "元素之主", "全元素伤害+40%, 暴击率+10%"));
            _sets[6] = elementalSet;
            
            // === 套装7: 暗影王者 (Shadow Lord) ===
            var shadowSet = new EquipmentSet
            {
                SetId = 7,
                SetName = "Shadow Lord",
                SetNameCN = "暗影王者",
                Description = "暗影领主的黑暗套装",
                EquipmentIds = new List<int> { 1061, 1062, 1063, 1064, 1065 }
            };
            shadowSet.Bonuses.Add(new SetBonusEffect(2, "暗影侵蚀", "暗影伤害+15%"));
            shadowSet.Bonuses.Add(new SetBonusEffect(3, "暗影之矛", "攻击+15%"));
            shadowSet.Bonuses.Add(new SetBonusEffect(5, "暗影君主", "暗影伤害+35%, 攻击+25%, 暴击伤害+25%"));
            _sets[7] = shadowSet;
            
            // === 套装8: 火焰领主 (Fire Lord) ===
            var fireSet = new EquipmentSet
            {
                SetId = 8,
                SetName = "Fire Lord",
                SetNameCN = "火焰领主",
                Description = "掌控火焰之力的毁灭套装",
                EquipmentIds = new List<int> { 1071, 1072, 1073, 1074, 1075 }
            };
            fireSet.Bonuses.Add(new SetBonusEffect(2, "火焰亲和", "火焰伤害+15%"));
            fireSet.Bonuses.Add(new SetBonusEffect(3, "灼热攻击", "攻击+10%"));
            fireSet.Bonuses.Add(new SetBonusEffect(5, "火焰君主", "火焰伤害+35%, 攻击+20%, 燃烧几率+20%"));
            _sets[8] = fireSet;
            
            // === 套装9: 冰霜之心 (Frost Heart) ===
            var frostSet = new EquipmentSet
            {
                SetId = 9,
                SetName = "Frost Heart",
                SetNameCN = "冰霜之心",
                Description = "极寒之地的冰霜套装",
                EquipmentIds = new List<int> { 1081, 1082, 1083, 1084, 1085 }
            };
            frostSet.Bonuses.Add(new SetBonusEffect(2, "冰霜护体", "冰霜抗性+25%"));
            frostSet.Bonuses.Add(new SetBonusEffect(3, "寒冰之力", "冰霜伤害+15%"));
            frostSet.Bonuses.Add(new SetBonusEffect(5, "冰霜君主", "冰霜伤害+35%, 减速效果+15%, 暴击率+10%"));
            _sets[9] = frostSet;
            
            // === 套装10: 闪电使者 (Lightning Messenger) ===
            var lightningSet = new EquipmentSet
            {
                SetId = 10,
                SetName = "Lightning Messenger",
                SetNameCN = "闪电使者",
                Description = "雷神之力附体的雷电套装",
                EquipmentIds = new List<int> { 1091, 1092, 1093, 1094, 1095 }
            };
            lightningSet.Bonuses.Add(new SetBonusEffect(2, "闪电亲和", "雷电伤害+15%"));
            lightningSet.Bonuses.Add(new SetBonusEffect(3, "迅捷如雷", "攻击速度+15%, 移动速度+10%"));
            lightningSet.Bonuses.Add(new SetBonusEffect(5, "雷神降世", "雷电伤害+40%, 攻击速度+25%, 暴击率+15%"));
            _sets[10] = lightningSet;
            
            // === 套装11: 精灵套装 (Elven Grace) ===
            var elvenSet = new EquipmentSet
            {
                SetId = 11,
                SetName = "Elven Grace",
                SetNameCN = "精灵套装",
                Description = "精灵族传承的优雅套装",
                EquipmentIds = new List<int> { 1101, 1102, 1103, 1104, 1105 }
            };
            elvenSet.Bonuses.Add(new SetBonusEffect(2, "精灵祝福", "生命上限+10%"));
            elvenSet.Bonuses.Add(new SetBonusEffect(3, "自然亲和", "全抗性+10%"));
            elvenSet.Bonuses.Add(new SetBonusEffect(5, "精灵之王", "生命+25%, 防御+20%, 移动速度+15%"));
            _sets[11] = elvenSet;
            
            // === 套装12: 泰坦之力 (Titan's Power) ===
            var titanSet = new EquipmentSet
            {
                SetId = 12,
                SetName = "Titan's Power",
                SetNameCN = "泰坦之力",
                Description = "远古泰坦的力量套装",
                EquipmentIds = new List<int> { 1111, 1112, 1113, 1114, 1115 }
            };
            titanSet.Bonuses.Add(new SetBonusEffect(2, "泰坦之力", "力量+20%"));
            titanSet.Bonuses.Add(new SetBonusEffect(3, "泰坦之怒", "攻击+20%"));
            titanSet.Bonuses.Add(new SetBonusEffect(5, "泰坦降世", "攻击+45%, 防御+30%, 生命上限+25%"));
            _sets[12] = titanSet;
        }
        
        public EquipmentSet GetSet(int setId)
        {
            return _sets.ContainsKey(setId) ? _sets[setId] : null;
        }
        
        public EquipmentSet GetSetByEquipmentId(int equipmentId)
        {
            foreach (var set in _sets.Values)
            {
                if (set.EquipmentIds.Contains(equipmentId))
                {
                    return set;
                }
            }
            return null;
        }
        
        public List<EquipmentSet> GetAllSets()
        {
            return new List<EquipmentSet>(_sets.Values);
        }
        public List<EquipmentSet> GetSetsByType(Game.EquipmentSetData.SetType type)
        {
            var result = new List<EquipmentSet>();
            foreach (var set in _sets.Values)
                if (set.SetType == type) result.Add(set);
            return result;
        }

        
        public int GetSetCount()
        {
            return _sets.Count;
        }
    }
}
