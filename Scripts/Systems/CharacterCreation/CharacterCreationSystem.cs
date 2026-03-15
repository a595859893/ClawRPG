using Godot;
using System;
using System.Collections.Generic;

public class CharacterCreationSystem : BaseSystem
{
    private CharacterCreationData _data;
    private CharacterCreationDatabase _database;
    
    // Signals
    public signal void CharacterCreated(string name, string characterClass, Dictionary<string, int> attributes);
    public signal void AttributeChanged(string attribute, int value);
    public signal void ClassChanged(string characterClass);
    public signal void BackgroundChanged(string background);
    
    public override void _Ready()
    {
        _data = GetNode<CharacterCreationData>("/root/CharacterCreationData");
        _database = GetNode<CharacterCreationDatabase>("/root/CharacterCreationDatabase");
        
        if (_data == null)
        {
            _data = new CharacterCreationData();
            _data.Name = "CharacterCreationData";
            GetTree().Root.AddChild(_data);
        }
        
        if (_database == null)
        {
            _database = new CharacterCreationDatabase();
            _database.Name = "CharacterCreationDatabase";
            GetTree().Root.AddChild(_database);
        }
        
        GD.Print("[CharacterCreationSystem] System initialized");
    }
    
    // Character Name
    public void SetCharacterName(string name)
    {
        _data.CharacterName = name;
    }
    
    public string GetCharacterName()
    {
        return _data.CharacterName;
    }
    
    // Class Selection
    public void SetCharacterClass(string characterClass)
    {
        if (_database.Classes.ContainsKey(characterClass))
        {
            _data.SelectedClass = characterClass;
            ClassChanged(characterClass);
            GD.Print("[CharacterCreationSystem] Class changed to: " + characterClass);
        }
    }
    
    public string GetCharacterClass()
    {
        return _data.SelectedClass;
    }
    
    public Dictionary<string, object> GetClassData()
    {
        return _database.GetClassData(_data.SelectedClass);
    }
    
    // Attribute Management
    public bool SetAttribute(string attribute, int value)
    {
        int pointsUsed = _data.UsedPoints;
        
        // Check if we have points available
        if (value > GetAttribute(attribute))
        {
            // Adding points
            int pointsNeeded = value - GetAttribute(attribute);
            if (pointsNeeded > _data.AvailablePoints - pointsUsed)
            {
                GD.Print("[CharacterCreationSystem] Not enough attribute points");
                return false;
            }
            _data.UsedPoints += pointsNeeded;
        }
        else
        {
            // Removing points
            int pointsReturned = GetAttribute(attribute) - value;
            if (GetAttribute(attribute) - value < 5) // Minimum 5 per attribute
            {
                GD.Print("[CharacterCreationSystem] Attribute cannot be less than 5");
                return false;
            }
            _data.UsedPoints -= pointsReturned;
        }
        
        switch (attribute)
        {
            case "Strength": _data.Strength = value; break;
            case "Agility": _data.Agility = value; break;
            case "Intelligence": _data.Intelligence = value; break;
            case "Vitality": _data.Vitality = value; break;
            case "Luck": _data.Luck = value; break;
        }
        
        AttributeChanged(attribute, value);
        return true;
    }
    
    public int GetAttribute(string attribute)
    {
        switch (attribute)
        {
            case "Strength": return _data.Strength;
            case "Agility": return _data.Agility;
            case "Intelligence": return _data.Intelligence;
            case "Vitality": return _data.Vitality;
            case "Luck": return _data.Luck;
            default: return 0;
        }
    }
    
    public Dictionary<string, int> GetAllAttributes()
    {
        return _data.GetAttributes();
    }
    
    public int GetAvailablePoints()
    {
        return _data.AvailablePoints - _data.UsedPoints;
    }
    
    public int GetUsedPoints()
    {
        return _data.UsedPoints;
    }
    
    // Background Selection
    public void SetBackground(string background)
    {
        if (_database.Backgrounds.ContainsKey(background))
        {
            _data.SelectedBackground = background;
            BackgroundChanged(background);
            GD.Print("[CharacterCreationSystem] Background changed to: " + background);
        }
    }
    
    public string GetBackground()
    {
        return _data.SelectedBackground;
    }
    
    public Dictionary<string, object> GetBackgroundData()
    {
        return _database.GetBackgroundData(_data.SelectedBackground);
    }
    
    // Appearance
    public void SetHairStyle(int styleIndex)
    {
        if (styleIndex >= 0 && styleIndex < _database.HairStyles.Length)
        {
            _data.HairStyle = styleIndex;
        }
    }
    
    public int GetHairStyle()
    {
        return _data.HairStyle;
    }
    
    public string GetHairStyleName()
    {
        return _database.HairStyles[_data.HairStyle];
    }
    
    public void SetSkinColor(int colorIndex)
    {
        if (colorIndex >= 0 && colorIndex < _database.SkinColors.Length)
        {
            _data.SkinColor = colorIndex;
        }
    }
    
    public int GetSkinColor()
    {
        return _data.SkinColor;
    }
    
    public string GetSkinColorName()
    {
        return _database.SkinColors[_data.SkinColor];
    }
    
    public void SetEyeColor(int colorIndex)
    {
        if (colorIndex >= 0 && colorIndex < _database.EyeColors.Length)
        {
            _data.EyeColor = colorIndex;
        }
    }
    
    public int GetEyeColor()
    {
        return _data.EyeColor;
    }
    
    public string GetEyeColorName()
    {
        return _database.EyeColors[_data.EyeColor];
    }
    
    // Character Creation
    public bool CanCreateCharacter()
    {
        return _data.CharacterName.Length >= 2 && 
               GetAvailablePoints() == 0 && // All points must be spent
               _data.SelectedClass != null &&
               _data.SelectedBackground != null;
    }
    
    public Dictionary<string, object> CreateCharacter()
    {
        if (!CanCreateCharacter())
        {
            GD.Print("[CharacterCreationSystem] Cannot create character - requirements not met");
            return null;
        }
        
        // Get class data
        var classData = GetClassData();
        var backgroundData = GetBackgroundData();
        
        // Calculate final stats with class bonuses
        int hp = (int)classData["base_hp"] + (_data.Vitality - 10) * 10;
        int attack = (int)classData["base_attack"] + (_data.Strength - 10) * 2;
        int defense = (int)classData["base_defense"] + (_data.Vitality - 10) * 1 + (_data.Agility - 10) * 1;
        int magic = (int)classData["base_magic"] + (_data.Intelligence - 10) * 2;
        int speed = (int)classData["base_speed"] + (_data.Agility - 10) * 1 + (_data.Luck - 10) / 2;
        
        var character = new Dictionary<string, object>
        {
            { "name", _data.CharacterName },
            { "class", _data.SelectedClass },
            { "background", _data.SelectedBackground },
            { "attributes", GetAllAttributes() },
            { "stats", new Dictionary<string, int>
                {
                    { "hp", hp },
                    { "attack", attack },
                    { "defense", defense },
                    { "magic", magic },
                    { "speed", speed }
                }
            },
            { "appearance", new Dictionary<string, int>
                {
                    { "hair_style", _data.HairStyle },
                    { "skin_color", _data.SkinColor },
                    { "eye_color", _data.EyeColor }
                }
            },
            { "starting_items", backgroundData["starting_items"] }
        };
        
        // Update statistics
        _data.CharactersCreated++;
        
        // Update most common class
        var classIndex = Array.IndexOf(_database.GetClassNames(), _data.SelectedClass);
        if (classIndex >= 0)
        {
            _data.MostCommonClass = classIndex;
        }
        
        // Save data
        _data.SaveData();
        
        // Emit signal
        CharacterCreated(_data.CharacterName, _data.SelectedClass, GetAllAttributes());
        
        GD.Print("[CharacterCreationSystem] Character created: " + _data.CharacterName + " (" + _data.SelectedClass + ")");
        
        return character;
    }
    
    // Reset
    public void ResetCharacter()
    {
        _data.CharacterName = "Hero";
        _data.SelectedClass = "Warrior";
        _data.Strength = 10;
        _data.Agility = 10;
        _data.Intelligence = 10;
        _data.Vitality = 10;
        _data.Luck = 10;
        _data.UsedPoints = 0;
        _data.SelectedBackground = "Commoner";
        _data.HairStyle = 0;
        _data.SkinColor = 0;
        _data.EyeColor = 0;
        
        GD.Print("[CharacterCreationSystem] Character reset to defaults");
    }
    
    // Statistics
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "characters_created", _data.CharactersCreated },
            { "most_common_class", _data.MostCommonClass }
        };
    }
    
    // Get database references
    public string[] GetAvailableClasses()
    {
        return _database.GetClassNames();
    }
    
    public string[] GetAvailableBackgrounds()
    {
        return _database.GetBackgroundNames();
    }
    
    public string[] GetAvailableHairStyles()
    {
        return _database.HairStyles;
    }
    
    public string[] GetAvailableSkinColors()
    {
        return _database.SkinColors;
    }
    
    public string[] GetAvailableEyeColors()
    {
        return _database.EyeColors;
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        data["character_name"] = _data.CharacterName;
        data["selected_class"] = _data.SelectedClass;
        data["selected_background"] = _data.SelectedBackground;
        data["strength"] = _data.Strength;
        data["agility"] = _data.Agility;
        data["intelligence"] = _data.Intelligence;
        data["vitality"] = _data.Vitality;
        data["luck"] = _data.Luck;
        data["used_points"] = _data.UsedPoints;
        data["hair_style"] = _data.HairStyle;
        data["skin_color"] = _data.SkinColor;
        data["eye_color"] = _data.EyeColor;
        data["characters_created"] = _data.CharactersCreated;
        data["most_common_class"] = _data.MostCommonClass;
        return data;
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("character_name")) _data.CharacterName = (string)data["character_name"];
        if (data.Contains("selected_class")) _data.SelectedClass = (string)data["selected_class"];
        if (data.Contains("selected_background")) _data.SelectedBackground = (string)data["selected_background"];
        if (data.Contains("strength")) _data.Strength = (int)data["strength"];
        if (data.Contains("agility")) _data.Agility = (int)data["agility"];
        if (data.Contains("intelligence")) _data.Intelligence = (int)data["intelligence"];
        if (data.Contains("vitality")) _data.Vitality = (int)data["vitality"];
        if (data.Contains("luck")) _data.Luck = (int)data["luck"];
        if (data.Contains("used_points")) _data.UsedPoints = (int)data["used_points"];
        if (data.Contains("hair_style")) _data.HairStyle = (int)data["hair_style"];
        if (data.Contains("skin_color")) _data.SkinColor = (int)data["skin_color"];
        if (data.Contains("eye_color")) _data.EyeColor = (int)data["eye_color"];
        if (data.Contains("characters_created")) _data.CharactersCreated = (int)data["characters_created"];
        if (data.Contains("most_common_class")) _data.MostCommonClass = (int)data["most_common_class"];
    }
}
