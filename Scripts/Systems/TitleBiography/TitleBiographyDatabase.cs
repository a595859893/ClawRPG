using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.TitleBiography
{
    /// <summary>
    /// 称号 → 传记模板数据库
    /// 每个模板含动态插值字段，运行时用玩家数据填充
    /// </summary>
    public class TitleBiographyDatabase
    {
        private static TitleBiographyDatabase _instance;
        public static TitleBiographyDatabase Instance => _instance ??= new TitleBiographyDatabase();

        /// <summary>
        /// 传记模板 entry
        /// bioTemplate: 动态传记正文，支持 {placeholder}
        /// dataField: 填充 bioTemplate 所需的玩家数据字段名（对应 StyleMasterySystem / CombatStatusSystem 等的API）
        /// milestoneTemplate: 未解锁时的里程碑提示
        /// </summary>
        public class BiographyTemplate
        {
            public string TitleId { get; set; }
            public string BioTemplate { get; set; }
            public string DataField { get; set; }
            public string MilestoneTemplate { get; set; }

            public BiographyTemplate(string titleId, string bio, string dataField, string milestone)
            {
                TitleId = titleId;
                BioTemplate = bio;
                DataField = dataField;
                MilestoneTemplate = milestone;
            }
        }

        private readonly Dictionary<string, BiographyTemplate> _templates = new Dictionary<string, BiographyTemplate>();

        public TitleBiographyDatabase()
        {
            InitializeTemplates();
        }

        private void InitializeTemplates()
        {
            // ===== Combat Titles — Kill-based =====
            Add("killer_novice",    "你在这片幻想大陆猎杀了 {value} 个敌人，\n他们的第一滴血是你划开的。",             "total_kills",       "再击杀 {remaining} 个敌人即可解锁称号");
            Add("killer_expert",    "500 个敌人倒在你手下。\n他们的同伴开始互相警告：小心那个拿着剑的家伙。",           "total_kills",       "再击杀 {remaining} 个敌人即可解锁称号");
            Add("killer_master",    "1000 个敌人的亡魂环绕着你。\n他们说，死亡还不是终点——只是开始。",                 "total_kills",       "再击杀 {remaining} 个敌人即可解锁称号");
            Add("killer_legend",   "5000 个敌人化为尘土。\n远方传来低语：战场上有一个不可阻挡的传说。",               "total_kills",       "再击杀 {remaining} 个敌人即可解锁称号");
            Add("killer_god",       "10000 个敌人。\n你已经不是在猎杀——你是在收割。整片大陆都在你的剑锋下颤抖。",      "total_kills",       "再击杀 {remaining} 个敌人即可解锁称号");

            // ===== Boss Titles =====
            Add("boss_slayer_novice",  "10 位 Boss 在你手中陨落。\n他们曾是不可一世的强者，如今只是你荣誉簿上的注脚。", "boss_kills",        "再击败 {remaining} 位Boss即可解锁称号");
            Add("boss_slayer_expert",  "50 位 Boss 的传奇在你脚下崩塌。\n每场胜利都是一首埋葬强者的安魂曲。",             "boss_kills",        "再击败 {remaining} 位Boss即可解锁称号");
            Add("boss_slayer_legend",  "100 位 Boss——他们曾是大陆的噩梦，如今他们的名字只存在于你的传说里。",           "boss_kills",        "再击败 {remaining} 位Boss即可解锁称号");

            // ===== Gold Titles =====
            Add("rich_novice",     "10000 金币流过你的手。\n那是你与这个世界做生意的起点。",                          "total_gold",        "再积累 {remaining} 金币即可解锁称号");
            Add("rich_merchant",  "100000 金币。\n商人见到你都会鞠躬——不是因为你有钱，而是因为你买得起任何东西。", "total_gold",        "再积累 {remaining} 金币即可解锁称号");
            Add("rich_king",      "1000000 金币。\n你已经超越了大商人的想象。你就是金币本身。",                     "total_gold",        "再积累 {remaining} 金币即可解锁称号");
            Add("rich_god",       "10000000 金币。\n金币的流动就是你的呼吸。这片大陆的每一笔交易都有你的影子。",    "total_gold",        "再积累 {remaining} 金币即可解锁称号");

            // ===== Level Titles =====
            Add("level_10",   "10 级。你终于站稳了脚跟。\n这个世界开始认真对待你了。",                               "player_level",      "再升 {remaining} 级即可解锁称号");
            Add("level_25",  "25 级。你的名字开始被人提起。\n不是所有人喜欢你，但那不重要。",                        "player_level",      "再升 {remaining} 级即可解锁称号");
            Add("level_50",  "50 级。你已是精英中的精英。\n传说开始有了你的轮廓。",                                  "player_level",      "再升 {remaining} 级即可解锁称号");
            Add("level_75",  "75 级。你的剑上沾满了星辰的碎片。\n每一步都是传奇。",                                   "player_level",      "再升 {remaining} 级即可解锁称号");
            Add("level_100", "100 级。你站在了凡人的顶点。\n从这一刻起，你的故事由你自己书写。",                     "player_level",      "再升 {remaining} 级即可解锁称号");

            // ===== Exploration Titles =====
            Add("explorer_novice",  "10 个地点留下了你的足迹。\n地图上开始有了你自己的标记。",                          "locations_discovered", "再探索 {remaining} 个地点即可解锁称号");
            Add("explorer_expert", "50 个地点。你见证了这片大陆的每一个角落。\n每条路你都走过两次——去程和回程。",          "locations_discovered", "再探索 {remaining} 个地点即可解锁称号");
            Add("explorer_master", "100 个地点。你是活着的地图。\n没有人比你更了解这片土地的秘密。",                    "locations_discovered", "再探索 {remaining} 个地点即可解锁称号");
            Add("explorer_legend", "所有已知地点。你踏遍了大陆的每一寸土地。\n那些无人涉足的禁区，也为你敞开了大门。",       "locations_discovered", "再探索 {remaining} 个地点即可解锁称号");

            // ===== Pet Titles =====
            Add("pet_collector_novice",  "5 只宠物与你并肩作战。\n它们记住了你的气味，你的脚步声，和你挥剑的节奏。",        "pet_count",         "再获得 {remaining} 只宠物即可解锁称号");
            Add("pet_collector_expert",   "15 只宠物。它们不只是伙伴——是你的军团。\n每只都有名字，每只都有一段故事。",      "pet_count",         "再获得 {remaining} 只宠物即可解锁称号");
            Add("pet_collector_legend",   "30 只宠物。这支队伍足以让一个王国颤抖。\n你站在它们中间，如同领袖中的领袖。",       "pet_count",         "再获得 {remaining} 只宠物即可解锁称号");

            // ===== Mount Titles =====
            Add("mount_rider_novice",  "3 只坐骑。你终于学会了一边骑马一边战斗。\n这比你想象的要难得多。",               "mount_count",       "再获得 {remaining} 只坐骑即可解锁称号");
            Add("mount_rider_expert",  "10 只坐骑。每一只都有独特的步伐。\n你已经能用缰绳说出完整的话。",                "mount_count",       "再获得 {remaining} 只坐骑即可解锁称号");

            // ===== Guild Titles =====
            Add("guild_founder",  "你创建了一个公会。从这一刻起，\n你有了一群愿意为你赴汤蹈火的人。",                   "guild_founded",     "创建公会后解锁称号");
            Add("guild_leader",   "10 名成员。你不只是会长——是精神领袖。\n他们追随你，因为相信你。",                     "guild_member_count","再招募 {remaining} 名成员即可解锁称号");

            // ===== PvP Titles =====
            Add("pvp_novice",  "10 场 PvP 胜利。\n你终于知道，敌人不只有怪物一种。",                               "pvp_wins",          "再赢得 {remaining} 场PvP即可解锁称号");
            Add("pvp_expert",  "50 场 PvP 胜利。\n其他玩家见到你的名字就会紧张——这是一种荣誉。",                 "pvp_wins",          "再赢得 {remaining} 场PvP即可解锁称号");
            Add("pvp_legend",  "100 场 PvP 胜利。你是战场上的梦魇。\n每次你出现在竞技场，对手就开始寻找逃跑的路线。",   "pvp_wins",          "再赢得 {remaining} 场PvP即可解锁称号");

            // ===== Crafting Titles =====
            Add("crafter_novice",  "50 件物品出自你手。\n它们不完美，但每一件都带着你的体温。",                      "items_crafted",     "再制作 {remaining} 件物品即可解锁称号");
            Add("crafter_expert",  "200 件物品。你的工坊有了名气。\n人们会特意找来，只为得到你亲手制作的东西。",        "items_crafted",     "再制作 {remaining} 件物品即可解锁称号");
            Add("crafter_master",  "500 件物品。你是真正的工匠。\n每一件作品都是你对这片世界的理解。",                  "items_crafted",     "再制作 {remaining} 件物品即可解锁称号");
            Add("crafter_legend",   "1000 件物品。传说级工匠。\n你的名字本身就是一种质量保证。",                       "items_crafted",     "再制作 {remaining} 件物品即可解锁称号");

            // ===== Special Titles =====
            Add("first_blood",   "你的第一场胜利。\n那是你在这片幻想大陆写下的第一个字。",                             "battles_won",       "赢得第一场战斗后解锁称号");
            Add("survivor",      "100 场战斗存活。你活下来了——\n不是因为运气，而是因为实力。",                        "battles_survived",  "再存活 {remaining} 场战斗即可解锁称号");
            Add("dedicated",     "100 小时游戏时间。你在这里付出了大量的时间。\n这不是沉迷，这是热爱。",                     "hours_played",      "再游玩 {remaining} 小时即可解锁称号");
            Add("veteran",       "500 小时。你已经是这片幻想大陆的居民了。\n这片世界的空气里，有你的汗水和回忆。",            "hours_played",      "再游玩 {remaining} 小时即可解锁称号");

            // ===== Achievement Titles =====
            Add("achiever_novice",  "10 个成就。你是解锁之道的初学者。\n每一步都有奖励在等着你。",                        "achievements_count","再解锁 {remaining} 个成就即可解锁称号");
            Add("achiever_expert",  "25 个成就。\n你开始理解这片世界的深层逻辑。",                                   "achievements_count","再解锁 {remaining} 个成就即可解锁称号");
            Add("achiever_master",  "50 个成就。成就猎人。\n每一枚徽章都是你征服这片世界的见证。",                      "achievements_count","再解锁 {remaining} 个成就即可解锁称号");
            Add("achiever_legend",  "100 个成就。全部成就。\n这片幻想大陆的每一寸土地你都丈量过了。完美。",               "achievements_count","再解锁 {remaining} 个成就即可解锁称号");

            // ===== Seasonal Titles =====
            Add("season_champion", "赢得了一次季节赛冠军。\n在那段时间里，你是整片大陆的焦点。",                        "seasonal_wins",     "再赢得 {remaining} 场季节赛即可解锁称号");
            Add("season_legend",   "3 次季节赛冠军。你是常胜将军。\n每次新的赛季开始，别人都在研究怎么击败你。",           "seasonal_wins",     "再赢得 {remaining} 场季节赛即可解锁称号");
        }

        private void Add(string titleId, string bio, string dataField, string milestone)
        {
            _templates[titleId] = new BiographyTemplate(titleId, bio, dataField, milestone);
        }

        /// <summary>
        /// 获取指定称号的传记模板
        /// </summary>
        public BiographyTemplate GetTemplate(string titleId)
        {
            return _templates.TryGetValue(titleId, out var t) ? t : null;
        }

        /// <summary>
        /// 用玩家数据填充传记模板
        /// </summary>
        /// <param name="titleId">称号ID</param>
        /// <param name="playerData">key-value 玩家数据字典</param>
        /// <param name="requiredValue">目标值（用于计算 remaining）</param>
        /// <returns>填充后的传记文本，或 null（如果模板不存在）</returns>
        public string GenerateBiography(string titleId, Dictionary<string, object> playerData, int requiredValue = 0)
        {
            var tpl = GetTemplate(titleId);
            if (tpl == null) return null;

            string bio = tpl.BioTemplate;

            // 替换动态字段
            if (playerData != null && playerData.TryGetValue(tpl.DataField, out var val))
            {
                bio = bio.Replace("{value}", val.ToString());
                bio = bio.Replace("{" + tpl.DataField + "}", val.ToString());
            }

            // 计算 remaining（如果模板中有此占位符）
            if (requiredValue > 0 && playerData != null && playerData.TryGetValue(tpl.DataField, out var current))
            {
                int cur = 0;
                if (current is int ci) cur = ci;
                else if (current is long cl) cur = (int)cl;
                else if (current is double cd) cur = (int)cd;

                int remaining = Mathf.Max(0, requiredValue - cur);
                bio = bio.Replace("{remaining}", remaining.ToString());
            }

            // 清理未替换的占位符
            bio = bio.Replace("{remaining}", "???");
            bio = System.Text.RegularExpressions.Regex.Replace(bio, @"\{[^{}]+\}", "???");

            return bio;
        }

        /// <summary>
        /// 生成里程碑提示文本（用于未解锁称号）
        /// </summary>
        public string GenerateMilestone(string titleId, int current, int required)
        {
            var tpl = GetTemplate(titleId);
            if (tpl == null) return $"解锁进度：{current}/{required}";

            int remaining = Mathf.Max(0, required - current);
            return tpl.MilestoneTemplate.Replace("{remaining}", remaining.ToString());
        }

        /// <summary>
        /// 获取所有已注册模板的称号ID列表
        /// </summary>
        public List<string> GetAllTemplateIds()
        {
            return new List<string>(_templates.Keys);
        }
    }
}
