namespace ClawRPG.Scripts.Data {
    public enum BountyType { Kill, Collect, Survive, Escort }
    public enum BountyDifficulty { Easy, Medium, Hard, Legendary }
    public class Bounty { public string Id; public BountyType Type; public BountyDifficulty Difficulty; }
    public class BountyManager { public static BountyManager Instance; }
}
