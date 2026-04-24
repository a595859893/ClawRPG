using Godot;
using System;

namespace ClawRPG.Scripts.Systems {
    public partial class CloudSaveSystem : Node {
        public static CloudSaveSystem Instance { get; private set; }
        public override void _Ready() { Instance = this; }
    }
}
