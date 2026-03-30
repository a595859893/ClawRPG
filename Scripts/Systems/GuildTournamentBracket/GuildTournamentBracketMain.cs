using Godot;

namespace ClawRPG.Scripts.Systems.GuildTournamentBracket {
    /// <summary>
    /// 公会锦标赛赛程系统主入口
    /// </summary>
    public class GuildTournamentBracketMain : BaseSystem {
        private GuildTournamentBracketSystem _bracketSystem;
        private GuildTournamentBracketUI _bracketUI;
        
        public override void _Ready() {
            // 初始化系统
            _bracketSystem = GuildTournamentBracketSystem.Instance;
            
            // 初始化 UI
            _bracketUI = new GuildTournamentBracketUI();
            
            GD.Print("[GuildTournamentBracket] Guild Tournament Bracket System initialized");
        }
        
        /// <summary>
        /// 切换 UI 显示
        /// </summary>
        public static void ToggleUI() {
            GuildTournamentBracketUI.Toggle();
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            // GuildTournamentBracketMain 是入口类，数据在_bracketSystem中
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            // GuildTournamentBracketMain 是入口类，数据在_bracketSystem中
        }
    }
}
