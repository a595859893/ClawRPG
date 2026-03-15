using Godot;
using System;
using System.Collections.Generic;

public class RandomNameDatabase : BaseSystem
{
    // Culture-based first names
    public static readonly Dictionary<string, string[]> FirstNames = new Dictionary<string, string[]>
    {
        ["Western"] = new string[] {
            "James", "William", "Oliver", "Henry", "Lucas", "Alexander", "Benjamin", "Sebastian", "Jack", "Aiden",
            "Emma", "Charlotte", "Amelia", "Harper", "Evelyn", "Abigail", "Emily", "Elizabeth", "Sofia", "Avery",
            "Arthur", "Edward", "George", "Charles", "Frederick", "Louis", "Albert", "Victor", "Hugo", "Felix",
            "Rose", "Claire", "Grace", "Victoria", "Catherine", "Eleanor", "Margaret", "Dorothy", "Helen", "Ruby"
        },
        ["Nordic"] = new string[] {
            "Erik", "Bjorn", "Leif", "Ragnar", "Harald", "Gunnar", "Sven", "Magnus", "Thorin", "Knut",
            "Astrid", "Freya", "Ingrid", "Sigrid", "Helga", "Thyra", "Ragnhild", "Solveig", "Gudrun", "Eira",
            "Ulf", "Sten", "Lars", "Jarl", "Trym", "Arne", "Per", "Olav", "Ivar", "Sigurd",
            "Eira", "Saga", "Frida", "Nanna", "Liv", "Nova", "Hilda", "Alva", "Mildred", "Thylda"
        },
        ["Eastern"] = new string[] {
            "Wei", "Ming", "Jun", "Chen", "Hao", "Yang", "Feng", "Kai", "Yuan", "Zhen",
            "Mei", "Lin", "Yan", "Jing", "Hui", "Ling", "Xia", "Yue", "Fei", "Lan",
            "Takeshi", "Kenji", "Hiroshi", "Yuki", "Ryu", "Kazu", "Haru", "Sora", "Ren", "Akira",
            "Sakura", "Yuki", "Hana", "Aiko", "Emi", "Naomi", "Rin", "Mai", "Yui", "Kaori"
        },
        ["Fantasy"] = new string[] {
            "Aldric", "Theron", "Kael", "Zephyr", "Draven", "Caelum", "Elysian", "Seraph", "Orion", "Cyrus",
            "Lyra", "Aria", "Elowen", "Isolde", "Nyx", "Seren", "Lyria", "Astrid", "Freya", "Morgana",
            "Zarathos", "Morthos", "Vex", "Keth", "Sylas", "Riven", "Zane", "Axel", "Jett", "Flint",
            "Celeste", "Iridia", "Vesper", "Nyx", "Ravenna", "Storm", "Winter", "Ember", "Onyx", "Lux"
        },
        ["Ancient"] = new string[] {
            "Marcus", "Gaius", "Titus", "Julius", "Augustus", "Flavius", "Quintus", "Decimus", "Cornelius", "Valerius",
            "Livia", "Claudia", "Julia", "Augusta", "Cornelia", "Helena", "Vibia", "Poppaea", "Agrippina", "Domitia",
            "Khufu", "Thutmose", "Akhenaten", "Ramesses", "Hatshepsut", "Imhotep", "Seti", "Nefertiti", "Cleopatra", "Tutankhamun",
            "Isis", "Osiris", "Horus", "Anubis", "Ra", "Thoth", "Bastet", "Sekhmet", "Set", "Nekhbet"
        }
    };

    // Last names
    public static readonly string[] LastNames = new string[] {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
        "Anderson", "Taylor", "Thomas", "Moore", "Jackson", "Martin", "Lee", "Thompson", "White", "Harris",
        "von Stroheim", "van der Berg", "de la Cruz", "di Angelo", "O'Brien", "MacDonald", "McCarthy", "Fitzgerald", "Nakamura", "Tanaka",
        "Ironforge", "Shadowmere", "Stormwind", "Darkwood", "Silverleaf", "Goldmane", "Swiftwind", "Blackthorn", "Redfang", "Whitestripe",
        "Zheng", "Wang", "Li", "Zhang", "Liu", "Chen", "Yang", "Huang", "Zhao", "Wu",
        "Petrov", "Ivanov", "Smirnov", "Kuznetsov", "Popov", "Vasiliev", "Sokolov", "Mikhailov", "Fedorov", "Morozov"
    };

    // Name prefixes for fantasy names
    public static readonly string[] NamePrefixes = new string[] {
        "Shadow", "Storm", "Fire", "Ice", "Thunder", "Dark", "Light", "Moon", "Sun", "Star",
        "Iron", "Steel", "Golden", "Silver", "Bronze", "Crystal", "Void", "Eternal", "Ancient", "Mystic"
    };

    // Name suffixes for fantasy names
    public static readonly string[] NameSuffixes = new string[] {
        "blade", "shield", "walker", "runner", "slayer", "hunter", "keeper", "weaver", "singer", "dancer",
        "heart", "soul", "spirit", "mind", "eye", "hand", "foot", "wing", "horn", "tail"
    };

    // Gender types
    public enum NameGender
    {
        Any,
        Male,
        Female,
        Neutral
    }

    // Name style
    public enum NameStyle
    {
        Random,
        Western,
        Nordic,
        Eastern,
        Fantasy,
        Ancient
    }

    // Get first names by gender and culture
    public static string[] GetFirstNames(NameStyle style, NameGender gender)
    {
        string cultureKey = style.ToString();
        if (!FirstNames.ContainsKey(cultureKey))
            cultureKey = "Western";
        
        var names = new List<string>(FirstNames[cultureKey]);
        
        // Filter by gender if needed
        if (gender == NameGender.Male)
        {
            // For simplicity, first half considered male in each culture
            int half = names.Count / 2;
            return names.GetRange(0, half).ToArray();
        }
        else if (gender == NameGender.Female)
        {
            int half = names.Count / 2;
            return names.GetRange(half, names.Count - half).ToArray();
        }
        
        return names.ToArray();
    }

    // Get color for culture
    public static Color GetCultureColor(string culture)
    {
        switch (culture)
        {
            case "Western": return new Color(0.8f, 0.6f, 0.4f);
            case "Nordic": return new Color(0.6f, 0.8f, 0.9f);
            case "Eastern": return new Color(0.9f, 0.5f, 0.4f);
            case "Fantasy": return new Color(0.6f, 0.4f, 0.9f);
            case "Ancient": return new Color(0.9f, 0.8f, 0.5f);
            default: return new Color(1f, 1f, 1f);
        }
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        // RandomNameDatabase 是静态配置数据，不需要持久化
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        // RandomNameDatabase 是静态配置数据，不需要持久化
    }
}
