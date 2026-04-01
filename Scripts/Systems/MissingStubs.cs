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
    public class PetAIContext { }

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

    // ── Boss ──────────────────────────────────────────────────────
    public class BossSkillData { }

    // ── Arena ──────────────────────────────────────────────────────
    public class ArenaTournamentData { }

    // ── Artifact ───────────────────────────────────────────────────
    public class ArtifactData { }

    // ── Monster Taming ─────────────────────────────────────────────
    public class TameableMonster { }
    public enum TamingMethod { Normal, Advanced, Special }
    public enum MonsterRarity { Common, Uncommon, Rare, Epic, Legendary }

    // ── Pet Battle ────────────────────────────────────────────────
    public enum ArenaType { Normal, Ranked, Tournament }

    // ── Pet (type already exists in Pets namespace; enum stubs here) ──
    public enum PetRarityEnum { Common, Uncommon, Rare, Epic, Legendary }

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

    // ── Title System ───────────────────────────────────────────────
    public class Title { }
    public enum TitleType { Combat, Exploration, Social, Special }
}

namespace ClawRPG.Scripts.UI
{
    public class ComboBox : Godot.Control { }
}

namespace ClawRPG.Scripts
{
    public class Boss { }
}
