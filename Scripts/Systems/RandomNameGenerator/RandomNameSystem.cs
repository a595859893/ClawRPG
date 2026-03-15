using Godot;
using System;
using System.Collections.Generic;

public class RandomNameSystem : BaseSystem
{
    private RandomNameData _data;
    private Random _random = new Random();
    
    public override void _Ready()
    {
        _data = new RandomNameData();
    }

    // Generate a random name
    public string GenerateName(RandomNameDatabase.NameStyle style = RandomNameDatabase.NameStyle.Random, 
                                RandomNameDatabase.NameGender gender = RandomNameDatabase.NameGender.Any)
    {
        // Random style if specified
        if (style == RandomNameDatabase.NameStyle.Random)
        {
            var styles = Enum.GetValues(typeof(RandomNameDatabase.NameStyle));
            style = (RandomNameDatabase.NameStyle)styles.GetValue(_random.Next(styles.Length));
        }
        
        // Get first name
        string[] firstNames = RandomNameDatabase.GetFirstNames(style, gender);
        string firstName = firstNames[_random.Next(firstNames.Length)];
        
        // Get last name (50% chance)
        string fullName = firstName;
        if (_random.Next(100) < 50)
        {
            string lastName = RandomNameDatabase.LastNames[_random.Next(RandomNameDatabase.LastNames.Length)];
            fullName = firstName + " " + lastName;
        }
        
        // Track statistics
        _data.TotalGenerated++;
        
        if (!_data.FirstNameCount.ContainsKey(firstName))
            _data.FirstNameCount[firstName] = 0;
        _data.FirstNameCount[firstName]++;
        
        string culture = style.ToString();
        if (!_data.CultureCount.ContainsKey(culture))
            _data.CultureCount[culture] = 0;
        _data.CultureCount[culture]++;
        
        _data.GeneratedNames.Add(fullName);
        
        // Keep history manageable
        if (_data.GeneratedNames.Count > 100)
            _data.GeneratedNames.RemoveAt(0);
        
        return fullName;
    }

    // Generate fantasy-style name with prefix/suffix
    public string GenerateFantasyName()
    {
        string prefix = RandomNameDatabase.NamePrefixes[_random.Next(RandomNameDatabase.NamePrefixes.Length)];
        string suffix = RandomNameDatabase.NameSuffixes[_random.Next(RandomNameDatabase.NameSuffixes.Length)];
        
        string name = prefix + suffix;
        
        // Track
        _data.TotalGenerated++;
        string culture = "Fantasy";
        if (!_data.CultureCount.ContainsKey(culture))
            _data.CultureCount[culture] = 0;
        _data.CultureCount[culture]++;
        
        _data.GeneratedNames.Add(name);
        if (_data.GeneratedNames.Count > 100)
            _data.GeneratedNames.RemoveAt(0);
        
        return name;
    }

    // Generate multiple names at once
    public string[] GenerateMultipleNames(int count, RandomNameDatabase.NameStyle style = RandomNameDatabase.NameStyle.Random)
    {
        var names = new string[count];
        for (int i = 0; i < count; i++)
        {
            names[i] = GenerateName(style);
        }
        return names;
    }

    // Get statistics
    public Dictionary<string, int> GetStatistics()
    {
        var stats = new Dictionary<string, int>();
        stats["TotalGenerated"] = _data.TotalGenerated;
        
        foreach (var kvp in _data.CultureCount)
        {
            stats["Culture_" + kvp.Key] = kvp.Value;
        }
        
        return stats;
    }

    // Get recent names
    public string[] GetRecentNames(int count = 10)
    {
        int start = Math.Max(0, _data.GeneratedNames.Count - count);
        int length = Math.Min(count, _data.GeneratedNames.Count - start);
        
        if (length <= 0)
            return new string[0];
        
        return _data.GeneratedNames.GetRange(start, length).ToArray();
    }

    // Get most popular first names
    public Dictionary<string, int> GetMostPopularFirstNames(int count = 5)
    {
        var sorted = new List<KeyValuePair<string, int>>(_data.FirstNameCount);
        sorted.Sort((a, b) => b.Value.CompareTo(a.Value));
        
        var result = new Dictionary<string, int>();
        for (int i = 0; i < Math.Min(count, sorted.Count); i++)
        {
            result[sorted[i].Key] = sorted[i].Value;
        }
        
        return result;
    }

    // Save data
    public Dictionary<string, object> SaveData()
    {
        var saveData = new Dictionary<string, object>();
        saveData["total_generated"] = _data.TotalGenerated;
        saveData["generated_names"] = _data.GeneratedNames;
        saveData["first_name_count"] = _data.FirstNameCount;
        saveData["last_name_count"] = _data.LastNameCount;
        saveData["culture_count"] = _data.CultureCount;
        return saveData;
    }

    // Load data
    public void LoadData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("total_generated"))
            _data.TotalGenerated = (int)data["total_generated"];
        
        if (data.ContainsKey("generated_names"))
            _data.GeneratedNames = new List<string>((List<string>)data["generated_names"]);
        
        if (data.ContainsKey("culture_count"))
            _data.CultureCount = new Dictionary<string, int>((Dictionary<string, int>)data["culture_count"]);
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // 委托给数据层
        if (_data != null)
        {
            var dataData = _data.ExportSaveData();
            foreach (var kvp in dataData)
            {
                data[kvp.Key] = kvp.Value;
            }
        }
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        // 委托给数据层
        if (_data != null)
        {
            _data.ImportSaveData(data);
        }
    }
}
