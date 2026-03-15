using Godot;
using System;
using System.Collections.Generic;

public class CharacterCreationMain : BaseSystem
{
    private CharacterCreationSystem _system;
    private CharacterCreationUI _ui;
    private bool _uiAdded = false;
    
    public override void _Ready()
    {
        // Initialize system
        _system = GetNode<CharacterCreationSystem>("/root/CharacterCreationSystem");
        if (_system == null)
        {
            _system = new CharacterCreationSystem();
            _system.Name = "CharacterCreationSystem";
            GetTree().Root.AddChild(_system);
        }
        
        GD.Print("[CharacterCreationMain] System initialized");
    }
    
    public override void _Process(float delta)
    {
        // Handle toggle input
        if (Input.IsKeyPressed(Key.C) && Input.IsKeyPressed(Key.Control) && Input.IsKeyPressed(Key.Shift))
        {
            ToggleUI();
        }
    }
    
    private void ToggleUI()
    {
        if (!_uiAdded)
        {
            AddUI();
        }
        
        if (_ui != null)
        {
            _ui.Toggle();
        }
    }
    
    private void AddUI()
    {
        var canvasLayer = GetTree().Root.GetNodeOrNull<CanvasLayer>("CanvasLayer");
        if (canvasLayer == null)
        {
            canvasLayer = new CanvasLayer();
            canvasLayer.Name = "CanvasLayer";
            GetTree().Root.AddChild(canvasLayer);
        }
        
        _ui = new CharacterCreationUI();
        _ui.Name = "CharacterCreationUI";
        canvasLayer.AddChild(_ui);
        _uiAdded = true;
        
        GD.Print("[CharacterCreationMain] UI added");
    }
}
