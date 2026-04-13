using Godot;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Systems;

/// <summary>
/// Integrates the deposit system with the run lifecycle.
/// - On new run: generates deposit cards and injects them into starting hand
/// - On game over: applies decay to deposit slots
/// </summary>
public partial class DepositRunIntegration : Node
{
    public static DepositRunIntegration Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        SubscribeToEvents();
        GD.Print("[DepositRunIntegration] Initialized");
    }

    private void SubscribeToEvents()
    {
        var bus = EventBusManager.Instance;
        if (bus == null) return;

        // Game state changes - check for run start
        bus.Subscribe<GameOverEventData>(EventBusManager.Events.GameOver, OnGameOver);

        // Also hook into scene changed to catch new run start
        bus.Subscribe<string>(EventBusManager.Events.SceneChanged, OnSceneChanged);
    }

    private void OnSceneChanged(string scenePath)
    {
        // Detect new run start by scene path
        // Common dungeon/fight scenes indicate a run is active
        if (IsDungeonScene(scenePath) && !_runActive)
        {
            StartRunIntegration();
        }
        else if (!IsDungeonScene(scenePath) && _runActive)
        {
            // Left dungeon - run ended
            _runActive = false;
        }
    }

    private bool _runActive = false;

    private void StartRunIntegration()
    {
        _runActive = true;

        // Generate and inject deposit cards
        var generator = DepositCardGenerator.Instance;
        if (generator == null) return;

        var depositCards = generator.GenerateDepositCards();
        if (depositCards.Count == 0) return;

        // Inject into deck building system
        var deckSystem = GetNodeOrNull<DeckBuildingSystem>("/root/DeckBuildingSystem");
        if (deckSystem != null)
        {
            foreach (var cardId in depositCards)
            {
                deckSystem.AddCardToHand(cardId);
            }
            GD.Print($"[DepositRunIntegration] Injected {depositCards.Count} deposit cards into starting hand");
        }

        // Show deposit UI
        var depositUI = GetNodeOrNull<DepositSlotUI>("/root/DepositSlotUI");
        depositUI?.ShowDepositUI();
    }

    private void OnGameOver(GameOverEventData data)
    {
        _runActive = false;

        // Apply deposit decay
        DepositData.Instance?.ApplyGlobalDecay();

        // Hide deposit UI
        var depositUI = GetNodeOrNull<DepositSlotUI>("/root/DepositSlotUI");
        depositUI?.HideDepositUI();
    }

    private bool IsDungeonScene(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        string lower = path.ToLower();
        return lower.Contains("dungeon") || lower.Contains("combat") ||
               lower.Contains("battle") || lower.Contains("arena") ||
               lower.Contains("floor");
    }
}
