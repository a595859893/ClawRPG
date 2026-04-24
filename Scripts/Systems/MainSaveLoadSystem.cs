using Godot;
using System;

namespace ClawRPG.Scripts.Systems {
    public class MainSaveLoadSystem : Node {
        public static MainSaveLoadSystem Instance { get; private set; }
        
        public override void _Ready() {
            Instance = this;
        }
    }
}
