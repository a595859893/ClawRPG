using Godot;
using System;
using System.Collections.Generic;

public class CharacterCreationData
{
    // Character Info
    public string CharacterName { get; set; } = "Hero";
    public string SelectedClass { get; set; } = "Warrior";
    
    // Attributes (Base: 10, Points to spend: 20)
    public int Strength { get; set; } = 10;
    public int Agility { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Vitality { get; set; } = 10;
    public int Luck { get; set; } = 10;
    public int AvailablePoints { get; set; } = 20;
    public int UsedPoints { get; set; } = 0;
    
    // Background Story
    public string SelectedBackground { get; set; } = "Commoner";
    
    // Appearance
    public int HairStyle { get; set; } = 0;
    public int SkinColor { get; set; } = 0;
    public int EyeColor { get; set; } = 0;
    
    // Statistics
    public int CharactersCreated { get; set; } = 0;
    public int MostCommonClass { get; set; } = 0; // 0=Warrior, 1=Mage, etc.

    public CharacterCreationData()
    {
    }

    public void LoadData()
    {
        if (File.Exists(GetSavePath()))
        {
            var data = new Godot.File();
            data.Open(GetSavePath(), Godot.File.ModeFlags.Read);
            var json = data.GetAsText();
            data.Close();
            
            var dict = JSON.Parse(json).Result as Dictionary<string, object>;
            if (dict != null)
            {
                CharacterName = dict.Get("CharacterName", "Hero");
                SelectedClass = dict.Get("SelectedClass", "Warrior");
                Strength = (int)dict.Get("Strength", 10);
                Agility = (int)dict.Get("Agility", 10);
                Intelligence = (int)dict.Get("Intelligence", 10);
                Vitality = (int)dict.Get("Vitality", 10);
                Luck = (int)dict.Get("Luck", 10);
                AvailablePoints = (int)dict.Get("AvailablePoints", 20);
                UsedPoints = (int)dict.Get("UsedPoints", 0);
                SelectedBackground = dict.Get("SelectedBackground", "Commoner");
                HairStyle = (int)dict.Get("HairStyle", 0);
                SkinColor = (int)dict.Get("SkinColor", 0);
                EyeColor = (int)dict.Get("EyeColor", 0);
                CharactersCreated = (int)dict.Get("CharactersCreated", 0);
                MostCommonClass = (int)dict.Get("MostCommonClass", 0);
            }
        }
    }
    
    public void SaveData()
    {
        var data = new Godot.File();
        data.Open(GetSavePath(), Godot.File.ModeFlags.Write);
        
        var dict = new Dictionary<string, object>
        {
            { "CharacterName", CharacterName },
            { "SelectedClass", SelectedClass },
            { "Strength", Strength },
            { "Agility", Agility },
            { "Intelligence", Intelligence },
            { "Vitality", Vitality },
            { "Luck", Luck },
            { "AvailablePoints", AvailablePoints },
            { "UsedPoints", UsedPoints },
            { "SelectedBackground", SelectedBackground },
            { "HairStyle", HairStyle },
            { "SkinColor", SkinColor },
            { "EyeColor", EyeColor },
            { "CharactersCreated", CharactersCreated },
            { "MostCommonClass", MostCommonClass }
        };
        
        data.StoreLine(JSON.Print(dict));
        data.Close();
    }
    
    public string GetSavePath()
    {
        return "user://character_creation_data.json";
    }
    
    public Dictionary<string, int> GetAttributes()
    {
        return new Dictionary<string, int>
        {
            { "Strength", Strength },
            { "Agility", Agility },
            { "Intelligence", Intelligence },
            { "Vitality", Vitality },
            { "Luck", Luck }
        };
    }
    
    public int GetTotalAttributes()
    {
        return Strength + Agility + Intelligence + Vitality + Luck;
    }

    public Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // Character Info
        data["character_name"] = CharacterName;
        data["selected_class"] = SelectedClass;
        
        // Attributes
        data["strength"] = Strength;
        data["agility"] = Agility;
        data["intelligence"] = Intelligence;
        data["vitality"] = Vitality;
        data["luck"] = Luck;
        data["available_points"] = AvailablePoints;
        data["used_points"] = UsedPoints;
        
        // Background Story
        data["selected_background"] = SelectedBackground;
        
        // Appearance
        data["hair_style"] = HairStyle;
        data["skin_color"] = SkinColor;
        data["eye_color"] = EyeColor;
        
        // Statistics
        data["characters_created"] = CharactersCreated;
        data["most_common_class"] = MostCommonClass;
        
        return data;
    }
    
    public void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        // Character Info
        CharacterName = (string)data.GetValueOrDefault("character_name", "Hero");
        SelectedClass = (string)data.GetValueOrDefault("selected_class", "Warrior");
        
        // Attributes
        Strength = (int)data.GetValueOrDefault("strength", 10);
        Agility = (int)data.GetValueOrDefault("agility", 10);
        Intelligence = (int)data.GetValueOrDefault("intelligence", 10);
        Vitality = (int)data.GetValueOrDefault("vitality", 10);
        Luck = (int)data.GetValueOrDefault("luck", 10);
        AvailablePoints = (int)data.GetValueOrDefault("available_points", 20);
        UsedPoints = (int)data.GetValueOrDefault("used_points", 0);
        
        // Background Story
        SelectedBackground = (string)data.GetValueOrDefault("selected_background", "Commoner");
        
        // Appearance
        HairStyle = (int)data.GetValueOrDefault("hair_style", 0);
        SkinColor = (int)data.GetValueOrDefault("skin_color", 0);
        EyeColor = (int)data.GetValueOrDefault("eye_color", 0);
        
        // Statistics
        CharactersCreated = (int)data.GetValueOrDefault("characters_created", 0);
        MostCommonClass = (int)data.GetValueOrDefault("most_common_class", 0);
    }
}
