// Truly missing type stubs — 36 types not defined anywhere in the codebase
// These are minimal definitions to unblock compilation
// Each stub should be replaced with proper implementation

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
}

namespace ClawRPG.Scripts.UI
{
    // Godot UI element stubs
    public class ComboBox : Godot.Control { }
}

namespace Godot
{
    // Type alias for Godot dictionary (must be before class declarations)
    using GodotDictionary = Godot.Collections.Dictionary;

    // Godot types that may not be resolving correctly
    public class DynamicFont : Resource { }
    public class TextureProgress : Control { }
}
