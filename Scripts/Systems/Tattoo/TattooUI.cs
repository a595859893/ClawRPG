using Godot;
using System;
using System.Collections.Generic;

public partial class TattooUI : Control
{
    private TattooSystem _tattooSystem;
    private TattooDatabase _database;
    
    private Label _titleLabel;
    private Label _goldLabel;
    private Label _bonusLabel;
    private ItemList _tattooList;
    private ItemList _slotList;
    private Button _applyButton;
    private Button _removeButton;
    private Button _closeButton;
    private RichTextLabel _infoLabel;
    
    private string _selectedTattoo = null;
    private string _selectedSlot = null;
    private int _playerGold = 10000; // Default for testing
    
    public void Initialize(TattooSystem system, TattooDatabase database)
    {
        _tattooSystem = system;
        _database = database;
        
        SetupUI();
        RefreshTattooList();
        RefreshSlotList();
        UpdateBonuses();
    }
    
    private void SetupUI()
    {
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "Tattoo System";
        _titleLabel.RectPosition = new Vector2(20, 20);
        _titleLabel.RectSize = new Vector2(200, 30);
        AddChild(_titleLabel);
        
        // Gold label
        _goldLabel = new Label();
        _goldLabel.Text = "Gold: 10000";
        _goldLabel.RectPosition = new Vector2(250, 20);
        _goldLabel.RectSize = new Vector2(200, 30);
        AddChild(_goldLabel);
        
        // Tattoo list
        _tattooList = new ItemList();
        _tattooList.RectPosition = new Vector2(20, 60);
        _tattooList.RectSize = new Vector2(300, 250);
        _tattooList.ItemSelected += _on_tattoo_selected;
        AddChild(_tattooList);
        
        // Slot list
        _slotList = new ItemList();
        _slotList.RectPosition = new Vector2(340, 60);
        _slotList.RectSize = new Vector2(150, 250);
        _slotList.ItemSelected += _on_slot_selected;
        AddChild(_slotList);
        
        // Info label
        _infoLabel = new RichTextLabel();
        _infoLabel.RectPosition = new Vector2(510, 60);
        _infoLabel.RectSize = new Vector2(250, 200);
        _infoLabel.BbcodeEnabled = true;
        AddChild(_infoLabel);
        
        // Apply button
        _applyButton = new Button();
        _applyButton.Text = "Apply Tattoo";
        _applyButton.RectPosition = new Vector2(20, 330);
        _applyButton.RectSize = new Vector2(150, 40);
        _applyButton.Pressed += _on_apply_pressed;
        AddChild(_applyButton);
        
        // Remove button
        _removeButton = new Button();
        _removeButton.Text = "Remove";
        _removeButton.RectPosition = new Vector2(180, 330);
        _removeButton.RectSize = new Vector2(150, 40);
        _removeButton.Pressed += _on_remove_pressed;
        AddChild(_removeButton);
        
        // Bonus label
        _bonusLabel = new Label();
        _bonusLabel.Text = "Current Bonuses:\nATK: 0\nDEF: 0\nHP: 0\nSPD: 0\nCRIT: 0\nEVA: 0";
        _bonusLabel.RectPosition = new Vector2(510, 280);
        _bonusLabel.RectSize = new Vector2(250, 120);
        AddChild(_bonusLabel);
        
        // Close button
        _closeButton = new Button();
        _closeButton.Text = "Close";
        _closeButton.RectPosition = new Vector2(650, 330);
        _closeButton.RectSize = new Vector2(100, 40);
        _closeButton.Pressed += _on_close_pressed;
        AddChild(_closeButton);
    }
    
    private void RefreshTattooList()
    {
        _tattooList.Clear();
        
        var unlockedTattoos = _tattooSystem.GetUnlockedTattoos();
        
        foreach (var tattooId in unlockedTattoos)
        {
            var tattoo = _database.GetTattoo(tattooId);
            if (tattoo != null)
            {
                string displayName = $"[{tattoo.Rarity}] {tattoo.Name} - {tattoo.Cost}g";
                _tattooList.AddItem(displayName);
            }
        }
        
        // Add locked tattoos that can be purchased
        foreach (var kvp in _database.Tattoos)
        {
            if (!_tattooSystem.IsTattooUnlocked(kvp.Key))
            {
                var tattoo = kvp.Value;
                string displayName = $"[LOCKED] {tattoo.Name} - {tattoo.Cost}g";
                _tattooList.AddItem(displayName);
            }
        }
    }
    
    private void RefreshSlotList()
    {
        _slotList.Clear();
        
        var slots = _tattooSystem.GetAvailableSlots();
        foreach (var slot in slots)
        {
            string display = slot;
            var appliedTattoo = _tattooSystem.GetAppliedTattoo(slot);
            if (appliedTattoo != null)
            {
                var tattoo = _database.GetTattoo(appliedTattoo);
                if (tattoo != null)
                    display = $"{slot}: {tattoo.Name}";
            }
            _slotList.AddItem(display);
        }
    }
    
    private void UpdateBonuses()
    {
        var bonuses = _tattooSystem.CalculateTotalBonuses();
        _bonusLabel.Text = $"Current Bonuses:\n" +
            $"ATK: {bonuses["attack"]}\n" +
            $"DEF: {bonuses["defense"]}\n" +
            $"HP: {bonuses["health"]}\n" +
            $"SPD: {bonuses["speed"]}\n" +
            $"CRIT: {bonuses["critical"]}%\n" +
            $"EVA: {bonuses["evasion"]}%";
    }
    
    private void _on_tattoo_selected(int index)
    {
        // Get tattoo ID from list
        var unlockedTattoos = _tattooSystem.GetUnlockedTattoos();
        
        if (index < unlockedTattoos.Count)
        {
            _selectedTattoo = unlockedTattoos[index];
        }
        else
        {
            int lockedIndex = index - unlockedTattoos.Count;
            int count = 0;
            foreach (var kvp in _database.Tattoos)
            {
                if (!_tattooSystem.IsTattooUnlocked(kvp.Key))
                {
                    if (count == lockedIndex)
                    {
                        _selectedTattoo = kvp.Key;
                        break;
                    }
                    count++;
                }
            }
        }
        
        UpdateInfoLabel();
    }
    
    private void _on_slot_selected(int index)
    {
        var slots = _tattooSystem.GetAvailableSlots();
        if (index >= 0 && index < slots.Count)
        {
            _selectedSlot = slots[index];
            UpdateInfoLabel();
        }
    }
    
    private void UpdateInfoLabel()
    {
        if (_selectedTattoo == null)
        {
            _infoLabel.Text = "Select a tattoo to view details";
            return;
        }
        
        var tattoo = _database.GetTattoo(_selectedTattoo);
        if (tattoo == null)
        {
            _infoLabel.Text = "Invalid tattoo";
            return;
        }
        
        bool isUnlocked = _tattooSystem.IsTattooUnlocked(_selectedTattoo);
        
        string info = $"[b]{tattoo.Name}[/b]\n\n";
        info += $"{tattoo.Description}\n\n";
        info += $"Category: {tattoo.Category}\n";
        info += $"Rarity: {tattoo.Rarity}\n";
        info += $"Cost: {tattoo.Cost}g\n\n";
        
        if (tattoo.AttackBonus > 0)
            info += $"ATK: +{tattoo.AttackBonus}\n";
        if (tattoo.DefenseBonus > 0)
            info += $"DEF: +{tattoo.DefenseBonus}\n";
        if (tattoo.HealthBonus > 0)
            info += $"HP: +{tattoo.HealthBonus}\n";
        if (tattoo.SpeedBonus > 0)
            info += $"SPD: +{tattoo.SpeedBonus}\n";
        if (tattoo.CriticalBonus > 0)
            info += $"CRIT: +{tattoo.CriticalBonus}%\n";
        if (tattoo.EvasionBonus > 0)
            info += $"EVA: +{tattoo.EvasionBonus}%\n";
        
        if (!isUnlocked)
        {
            info += $"\n[color=yellow]Status: LOCKED (Purchase to unlock)[/color]";
        }
        
        _infoLabel.Text = info;
    }
    
    private void _on_apply_pressed()
    {
        if (_selectedTattoo == null || _selectedSlot == null)
            return;
        
        if (!_tattooSystem.IsTattooUnlocked(_selectedTattoo))
        {
            // Try to purchase
            if (_tattooSystem.UnlockTattoo(_selectedTattoo, _playerGold))
            {
                _playerGold -= _database.GetTattoo(_selectedTattoo).Cost;
                _goldLabel.Text = $"Gold: {_playerGold}";
                RefreshTattooList();
            }
            else
            {
                GD.Print("Not enough gold or invalid tattoo");
                return;
            }
        }
        
        if (_tattooSystem.ApplyTattoo(_selectedTattoo, _selectedSlot))
        {
            RefreshSlotList();
            UpdateBonuses();
        }
    }
    
    private void _on_remove_pressed()
    {
        if (_selectedSlot != null)
        {
            _tattooSystem.RemoveTattoo(_selectedSlot);
            RefreshSlotList();
            UpdateBonuses();
        }
    }
    
    private void _on_close_pressed()
    {
        Hide();
    }
    
    public void SetPlayerGold(int gold)
    {
        _playerGold = gold;
        _goldLabel.Text = $"Gold: {gold}";
    }
}
