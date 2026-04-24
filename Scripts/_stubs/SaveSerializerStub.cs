using Godot;
using System;

namespace ClawRPG.Scripts.Systems {
    public partial class SaveSerializer : Node {
        public static SaveSerializer Instance { get; private set; }
        public override void _Ready() { Instance = this; }
    }
}
