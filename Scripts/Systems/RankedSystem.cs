using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    public enum RankTier { Bronze, Silver, Gold, Diamond, Master, GrandMaster }
    public enum RankDivision { IV, III, II, I }
    
    public partial class RankedSystem : Node {
        public static RankedSystem Instance { get; private set; }
        
        public override void _Ready() {
            Instance = this;
        }
    }
}
