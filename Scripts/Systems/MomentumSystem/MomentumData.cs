using System;
using System.Collections.Generic;

public class MomentumData
{
    public enum MomentumState
    {
        Neutral,
        Building,
        Charged,
        Overcharged,
        Fading
    }
    
    public enum MomentumType
    {
        Attack,
        Defense,
        Speed,
        Luck,
        Critical
    }
    
    public class MomentumInstance
    {
        public MomentumType Type { get; set; }
        public MomentumState State { get; set; }
        public int Level { get; set; }
        public float Charge { get; set; }
        public float MaxCharge { get; set; }
        public float DecayRate { get; set; }
        public float Multiplier { get; set; }
        public int ConsecutiveKills { get; set; }
        public DateTime LastKillTime { get; set; }
    }
    
    public class PlayerMomentumData
    {
        public Dictionary<MomentumType, MomentumInstance> ActiveMomenta { get; set; }
        public int TotalMomentumGained { get; set; }
        public int MaxMomentumReached { get; set; }
        public int OverchargeCount { get; set; }
        public int MomentumLostToDecay { get; set; }
        
        public PlayerMomentumData()
        {
            ActiveMomenta = new Dictionary<MomentumType, MomentumInstance>();
        }
    }
    
    public class MomentumConfig
    {
        public MomentumType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float MaxCharge { get; set; }
        public float DecayRate { get; set; }
        public float ChargePerKill { get; set; }
        public float ChargePerHit { get; set; }
        public float ChargePerSecond { get; set; }
        public int MaxLevel { get; set; }
        public Dictionary<MomentumState, Dictionary<string, float>> StateMultipliers { get; set; }
        public Dictionary<string, float> AttributeBonuses { get; set; }
        
        public MomentumConfig()
        {
            StateMultipliers = new Dictionary<MomentumState, Dictionary<string, float>>();
            AttributeBonuses = new Dictionary<string, float>();
        }
    }
}
