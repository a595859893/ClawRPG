// Missing type stubs — unblock compilation only
// Replace with proper implementations when available

namespace ClawRPG.Scripts.Systems
{
    // ── Pet Systems ──────────────────────────────────────────────────
    public class PetSystem { }
    public class PetData { }
    public class PetBreedingResult { }
    public class PetLifeCycleEntry { }
    public class LifeStage { }
    public class LifeCycleHistoryEntry { }

    // ── Player / Save ───────────────────────────────────────────────
    public class PlayerData { }
    public class PlayerPetTalentData { }

    // ── World ──────────────────────────────────────────────────────
    public class RealmStats { }
    public class WorldEvent { }
    public class EnemyManager { }

    // ── Item ───────────────────────────────────────────────────────
    public class EquipmentData { }
    public class EnchantmentAttribute { }
    public class EnchantmentData { }

    // ── Boss ──────────────────────────────────────────────────────
    public class BossSkillData { }

    // ── Arena ──────────────────────────────────────────────────────
    public class ArenaTournamentData
    {
        public class Tournament
        {
            public int Id;
            public string Name = "";
            public int PlayerCount;
            public bool IsActive;
        }
    }

    // ── Artifact ───────────────────────────────────────────────────
    public class ArtifactData { }

    // ── Monster Taming ─────────────────────────────────────────────
    public class TameableMonster { }
    public enum TamingMethod { Normal, Advanced, Special }
    public enum MonsterRarity { Common, Uncommon, Rare, Epic, Legendary }

    // ── Pet Battle ────────────────────────────────────────────────
    public enum ArenaType { Normal, Ranked, Tournament }

    // ── Multiplayer Vote ──────────────────────────────────────────
    public enum VoteStatus { Pending, Accepted, Rejected, Expired }

    // ── Combat ──────────────────────────────────────────────────
    public class Player { }

    // ── Signal types ───────────────────────────────────────────────
    public class SignalPurchaseCompleted { }
    public class SignalShopRefreshed { }
    public class SignalItemSold { }
    public class SignalContainer<T> { }

    // ── Event Args ────────────────────────────────────────────────
    public class MessageEventArgs { }
    public class ErrorEventArgs { }
    public class CloseEventArgs { }

    // ── Network ───────────────────────────────────────────────────
    public class ChannelSettings { }
    public class NodeId { }
    public class DataNode { }
    public class WebSocket { }

    // ── Tutorial ──────────────────────────────────────────────────
    public class TutorialTrigger { }

    // ── Misc ──────────────────────────────────────────────────────
    public class GetRandomPetSynthesisData { }

    // ── Title System ──────────────────────────────────────────────
    public class Title { }
    public enum TitleType { Level, Combat, Quest, Exploration, Social, Special }
}

namespace ClawRPG.Scripts.UI
{
    public partial class ComboBox : Godot.Control { }
}
