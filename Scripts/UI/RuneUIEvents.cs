using System;
using System.Collections.Generic;
using Godot;

public partial class RuneUI
{
    private void ConnectSignals()
    {
        // Connect rune system signals
        // Note: In actual implementation, connect to RuneSystem signals
    }
    
    private void OnFilterTypeChanged(long index)
    {
        RefreshRuneList();
    }
    
    private void OnFilterRarityChanged(long index)
    {
        RefreshRuneList();
    }
    
    private void OnRuneSelected(RuneData rune)
    {
        _selectedRune = rune;
        ShowRuneDetails(rune);
    }
    
    private void OnEquippedSlotPressed(RuneSlotType slotType)
    {
        // Show unequip option or show available runes for this slot
        var equippedRune = _runeSystem.GetEquippedRune(slotType);
        if (equippedRune != null)
        {
            ShowRuneDetails(equippedRune);
        }
    }
    
    private void OnUnequipPressed()
    {
        if (_selectedRune == null) return;
        
        // Find which slot this rune is in and unequip
        foreach (var kvp in _runeSystem.GetAllEquippedRunes())
        {
            if (kvp.Value == _selectedRune)
            {
                _runeSystem.UnequipRune(kvp.Key);
                RefreshEquippedRunes();
                RefreshStats();
                RefreshRuneList();
                _detailsPanel.Visible = false;
                break;
            }
        }
    }
    
    private void OnEquipPressed()
    {
        if (_selectedRune == null) return;
        
        // Try to equip to appropriate slot
        RuneSlotType targetSlot = _selectedRune.SlotType;
        
        // If Any slot, ask user which slot to equip to
        if (targetSlot == RuneSlotType.Any)
        {
            // For now, just try to find an empty slot
            foreach (RuneSlotType slot in Enum.GetValues(typeof(RuneSlotType)))
            {
                if (slot == RuneSlotType.Any) continue;
                if (_runeSystem.GetEquippedRune(slot) == null)
                {
                    targetSlot = slot;
                    break;
                }
            }
        }
        
        if (_runeSystem.EquipRune(_selectedRune, targetSlot))
        {
            RefreshEquippedRunes();
            RefreshStats();
            RefreshRuneList();
        }
    }
}
