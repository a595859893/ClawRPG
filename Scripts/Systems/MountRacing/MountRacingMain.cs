using Godot;
using System;

namespace ClawRPG.Systems {
    public partial class MountRacingMain : BaseSystem {
        private MountRacingSystem _mountRacingSystem;
        private MountRacingDatabase _mountRacingDatabase;
        private MountRacingData _mountRacingData;
        private MountRacingUI _mountRacingUI;
        
        public override void _Ready() {
            InitializeSystems();
        }
        
        private void InitializeSystems() {
            // Initialize database
            _mountRacingDatabase = new MountRacingDatabase();
            
            // Initialize data (load from save in production)
            _mountRacingData = new MountRacingData();
            
            // Initialize system
            _mountRacingSystem = new MountRacingSystem();
            _mountRacingSystem.Initialize(_mountRacingData, _mountRacingDatabase);
            
            // Initialize UI
            _mountRacingUI = new MountRacingUI();
            _mountRacingUI.Initialize(_mountRacingSystem, _mountRacingDatabase, _mountRacingData);
            _mountRacingUI.Visible = false;
            AddChild(_mountRacingUI);
        }
        
        public void ToggleMountRacingUI() {
            _mountRacingUI.Visible = !_mountRacingUI.Visible;
        }
        
        public override void _UnhandledInput(InputEvent e) {
            if (e is InputEventKey key && key.Pressed) {
                // R key for Mount Racing
                if (key.Keycode == Key.R) {
                    ToggleMountRacingUI();
                }
            }
        }
    }
}
