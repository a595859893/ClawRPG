using System;
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
}
