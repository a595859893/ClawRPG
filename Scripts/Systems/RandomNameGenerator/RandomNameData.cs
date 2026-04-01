using Godot;
using System;
using System.Collections.Generic;

public partial class RandomNameData : BaseSystem
{
    // Name history for uniqueness tracking
    public List<string> GeneratedNames = new List<string>();
    
    // Statistics
    public int TotalGenerated = 0;
    public Dictionary<string, int> FirstNameCount = new Dictionary<string, int>();
    public Dictionary<string, int> LastNameCount = new Dictionary<string, int>();
    public Dictionary<string, int> CultureCount = new Dictionary<string, int>();
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 生成的名字历史
        data["generated_names"] = new Array(GeneratedNames);
        
        // 统计数据
        data["total_generated"] = TotalGenerated;
        data["first_name_count"] = new Dictionary(FirstNameCount);
        data["last_name_count"] = new Dictionary(LastNameCount);
        data["culture_count"] = new Dictionary(CultureCount);
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 生成的名字历史
        if (data.Contains("generated_names"))
        {
            var namesArray = (Array)data["generated_names"];
            GeneratedNames = new List<string>();
            foreach (string name in namesArray)
            {
                GeneratedNames.Add(name);
            }
        }
        
        // 统计数据
        TotalGenerated = (int)data.GetValueOrDefault("total_generated", 0);
        
        if (data.Contains("first_name_count"))
        {
            FirstNameCount = new Dictionary<string, int>();
            var dict = (Dictionary)data["first_name_count"];
            foreach (var kvp in dict)
            {
                FirstNameCount[kvp.Key] = (int)kvp.Value;
            }
        }
        
        if (data.Contains("last_name_count"))
        {
            LastNameCount = new Dictionary<string, int>();
            var dict = (Dictionary)data["last_name_count"];
            foreach (var kvp in dict)
            {
                LastNameCount[kvp.Key] = (int)kvp.Value;
            }
        }
        
        if (data.Contains("culture_count"))
        {
            CultureCount = new Dictionary<string, int>();
            var dict = (Dictionary)data["culture_count"];
            foreach (var kvp in dict)
            {
                CultureCount[kvp.Key] = (int)kvp.Value;
            }
        }
    }
}
