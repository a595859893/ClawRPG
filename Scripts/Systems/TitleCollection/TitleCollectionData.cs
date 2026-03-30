using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.TitleCollection {
public class TitleCollectionData : Resource
{
    private Godot.Collections.Dictionary _data = new Godot.Collections.Dictionary();
    
    // 已收集的标题
    public Godot.Collections.Dictionary CollectedTitles
    {
        get => GetValue("collected_titles", new Godot.Collections.Dictionary());
        set => SetValue("collected_titles", value);
    }
    
    // 当前显示的标题
    public string CurrentDisplayTitle
    {
        get => GetValue("current_display_title", "");
        set => SetValue("current_display_title", value);
    }
    
    // 标题获取历史
    public Godot.Collections.Array TitleHistory
    {
        get => GetValue("title_history", new Godot.Collections.Array());
        set => SetValue("title_history", value);
    }
    
    // 统计信息
    public int TotalTitlesCollected
    {
        get => GetValue("total_titles_collected", 0);
        set => SetValue("total_titles_collected", value);
    }
    
    public int LegendaryTitles
    {
        get => GetValue("legendary_titles", 0);
        set => SetValue("legendary_titles", value);
    }
    
    public int EpicTitles
    {
        get => GetValue("epic_titles", 0);
        set => SetValue("epic_titles", value);
    }
    
    public int RareTitles
    {
        get => GetValue("rare_titles", 0);
        set => SetValue("rare_titles", value);
    }
    
    public Godot.Collections.Dictionary ToDict()
    {
        return _data;
    }
    
    public void FromDict(Godot.Collections.Dictionary dict)
    {
        _data = dict;
    }
    
    private T GetValue<T>(string key, T defaultValue)
    {
        if (_data.Contains(key))
        {
            return (T)_data[key];
        }
        return defaultValue;
    }
    
    private void SetValue(string key, object value)
    {
        _data[key] = value;
    }
}
}
