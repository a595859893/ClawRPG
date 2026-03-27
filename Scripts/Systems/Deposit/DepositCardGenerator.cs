using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Generates deposit cards from active deposit slots at the start of a new run.
/// Reads deposit levels and converts them into playable deposit cards.
/// </summary>
public class DepositCardGenerator : Node
{
    public static DepositCardGenerator Instance { get; private set; }

    // ── Card ID prefix for deposit cards ───────────────────────────────────
    public const string DEPOSIT_CARD_PREFIX = "deposit_";

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[DepositCardGenerator] Initialized");
    }

    /// <summary>
    /// Generate deposit cards from current deposit slots.
    /// Returns a list of card IDs to add to the player's starting hand.
    /// </summary>
    public List<string> GenerateDepositCards()
    {
        var cards = new List<string>();
        var depositData = DepositData.Instance;
        if (depositData == null) return cards;

        foreach (DepositData.DepositType type in Enum.GetValues(typeof(DepositData.DepositType)))
        {
            var slot = depositData.GetSlot(type);
            if (slot == null || slot.Level <= 0) continue;

            string cardId = GetCardIdForType(type, slot.Level);
            cards.Add(cardId);

            // Emit for UI feedback
            EmitSignal(nameof(DepositCardGenerated), type, slot.Level, cardId);
            GD.Print($"[DepositCardGenerator] Generated deposit card: {cardId} (level {slot.Level})");
        }

        return cards;
    }

    /// <summary>
    /// Get the card ID for a given deposit type and level.
    /// Cards scale in power with level.
    /// </summary>
    public string GetCardIdForType(DepositData.DepositType type, int level)
    {
        return $"{DEPOSIT_CARD_PREFIX}{type.ToString().ToLower()}_lv{level}";
    }

    /// <summary>
    /// Get the display name for a deposit card.
    /// </summary>
    public static string GetCardDisplayName(string cardId)
    {
        if (!cardId.StartsWith(DEPOSIT_CARD_PREFIX)) return cardId;

        var parts = cardId.Replace(DEPOSIT_CARD_PREFIX, "").Split('_');
        if (parts.Length < 2) return cardId;

        string type = parts[0];
        string levelPart = parts[1]; // e.g. "lv3"
        int level = levelPart.StartsWith("lv") ? levelPart.Replace("lv", "").ToInt() : 1;

        string typeName = type switch
        {
            "ember" => "余烬",
            "sediment" => "沉积",
            "echo" => "残影",
            "debt" => "血债",
            "synergy" => "协同",
            _ => type
        };

        string levelName = level switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            _ => $"L{level}"
        };

        return $"{typeName} {levelName}";
    }

    /// <summary>
    /// Get the card description based on type and level.
    /// </summary>
    public static string GetCardDescription(string cardId)
    {
        if (!cardId.StartsWith(DEPOSIT_CARD_PREFIX)) return "";
        if (!cardId.Contains("_lv")) return "";

        var parts = cardId.Replace(DEPOSIT_CARD_PREFIX, "").Split('_');
        if (parts.Length < 2) return "";
        string type = parts[0];
        string levelPart = parts[1];
        int level = levelPart.StartsWith("lv") ? levelPart.Replace("lv", "").ToInt() : 1;

        float intensity = level / 5f; // 0.2 to 1.0

        return type switch
        {
            "ember" => $"战斗开始时对所有敌人造成 {2 + level * 2} 点灼烧伤害。",
            "sediment" => $"获得 {5 + level * 3} 点临时护盾，每受到伤害有 {10 + level * 5}% 概率碎裂反击。",
            "echo" => $"重复上一个使用的技能，费用-{Mathf.CeilToInt(level / 2f)}（最低0）。",
            "debt" => $"失去 {level} 点生命，造成额外 {3 + level * 2} 点伤害。",
            "synergy" => $"宠物攻击附带你 {10 + level * 8}% 的攻击力。",
            _ => $"沉积卡 Lv{level}"
        };
    }

    /// <summary>
    /// Check if a card ID is a deposit card.
    /// </summary>
    public static bool IsDepositCard(string cardId)
    {
        return cardId.StartsWith(DEPOSIT_CARD_PREFIX);
    }

    // ── Signals ────────────────────────────────────────────────────────────
    [Signal]
    public delegate void DepositCardGenerated(DepositData.DepositType type, int level, string cardId);
}
