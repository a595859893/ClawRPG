using Godot;
using System;
using System.Collections.Generic;

public enum WeatherType
{
    Clear,
    Cloudy,
    Rain,
    Snow,
    Thunderstorm,
    Fog,
    Sandstorm,
    Hail,
    Blizzard,
    Storm

    public override Dictionary ExportSaveData() => new();
    public override void ImportSaveData(Dictionary data) { }

}

public enum WeatherIntensity
{
    Light,
    Moderate,
    Heavy,
    Severe
}

public class WeatherEffect
{
    public string effect_id;
    public string display_name;
    public string description;
    public float duration;
    public WeatherIntensity intensity;
    public Dictionary<string, float> stat_modifiers;
    
    public WeatherEffect(string id, string name, string desc, float dur, WeatherIntensity inty)
    {
        effect_id = id;
        display_name = name;
        description = desc;
        duration = dur;
        intensity = inty;
        stat_modifiers = new Dictionary<string, float>();
    }
}

public class WeatherInstance
{
    public WeatherType type;
    public WeatherIntensity intensity;
    public float remaining_time;
    public float total_duration;
    public bool is_permanent;
    public Vector2 position;
    public float radius;
    
    public WeatherInstance(WeatherType t, WeatherIntensity i, float dur, bool permanent = false)
    {
        type = t;
        intensity = i;
        remaining_time = dur;
        total_duration = dur;
        is_permanent = permanent;
        position = Vector2.Zero;
        radius = 0;
    }
    
    public float GetProgress()
    {
        if (total_duration <= 0) return 0;
        return 1.0f - (remaining_time / total_duration);
    }
    
    public float GetIntensityMultiplier()
    {
        switch (intensity)
        {
            case WeatherIntensity.Light: return 0.3f;
            case WeatherIntensity.Moderate: return 0.6f;
            case WeatherIntensity.Heavy: return 1.0f;
            case WeatherIntensity.Severe: return 1.5f;
            default: return 0.5f;
        }
    }
}

public class PlayerWeatherData
{
    public List<string> unlocked_weather_types;
    public Dictionary<WeatherType, int> weather_encounters;
    public Dictionary<WeatherType, float> time_spent;
    public int total_weather_changes;
    public float favorite_weather_time;
    public WeatherType favorite_weather;
    
    public PlayerWeatherData()
    {
        unlocked_weather_types = new List<string>();
        weather_encounters = new Dictionary<WeatherType, int>();
        time_spent = new Dictionary<WeatherType, float>();
        
        foreach (WeatherType wt in Enum.GetValues(typeof(WeatherType)))
        {
            weather_encounters[wt] = 0;
            time_spent[wt] = 0;
        }
        
        favorite_weather = WeatherType.Clear;
        favorite_weather_time = 0;
    }
}

public class WeatherSystem : BaseSystem
{
    public static WeatherSystem Instance { get; private set; }
    
    private WeatherInstance current_weather;
    private WeatherInstance upcoming_weather;
    private float weather_change_timer;
    private float global_weather_timer;
    private bool is_weather_enabled = true;
    private bool is_transitioning = false;
    private float transition_progress = 0;
    private float transition_duration = 5.0f;
    
    // Weather effects
    private WeatherType last_weather_type = WeatherType.Clear;
    private float visibility_reduction = 1.0f;
    private float movement_speed_modifier = 1.0f;
    private float attack_speed_modifier = 1.0f;
    private float defense_modifier = 1.0f;
    private float drop_rate_modifier = 1.0f;
    
    // Player data
    private PlayerWeatherData player_data;
    
    // Signals
    [Signal] public delegate void WeatherChanged(WeatherType old_type, WeatherType new_type);
    [Signal] public delegate void WeatherIntensityChanged(WeatherIntensity intensity);
    [Signal] public delegate void WeatherTransitionStarted(WeatherType from, WeatherType to);
    [Signal] public delegate void WeatherTransitionEnded(WeatherType new_weather);
    
    public override void _Ready()
    {
        Instance = this;
        player_data = new PlayerWeatherData();
        InitializeWeather();
    }
    
    private void InitializeWeather()
    {
        // Start with clear weather
        SetWeather(WeatherType.Clear, WeatherIntensity.Light, 300, true);
        global_weather_timer = 0;
    }
    
    public override void _Process(float delta)
    {
        if (!is_weather_enabled) return;
        
        global_weather_timer += delta;
        
        // Update current weather timer
        if (current_weather != null && !current_weather.is_permanent)
        {
            current_weather.remaining_time -= delta;
            
            if (current_weather.remaining_time <= 0)
            {
                // Weather expired, transition to new weather
                TransitionToRandomWeather();
            }
        }
        
        // Update transition
        if (is_transitioning)
        {
            transition_progress += delta / transition_duration;
            if (transition_progress >= 1.0f)
            {
                transition_progress = 1.0f;
                is_transitioning = false;
                EmitSignal(nameof(WeatherTransitionEnded), current_weather.type);
            }
            
            UpdateWeatherEffects();
        }
        
        // Update player weather time
        if (current_weather != null)
        {
            float time_in_weather = delta;
            WeatherType current_type = current_weather.type;
            
            if (player_data.time_spent.ContainsKey(current_type))
            {
                player_data.time_spent[current_type] += time_in_weather;
                
                // Update favorite weather
                if (player_data.time_spent[current_type] > player_data.favorite_weather_time)
                {
                    player_data.favorite_weather_time = player_data.time_spent[current_type];
                    player_data.favorite_weather = current_type;
                }
            }
        }
    }
    
    private void TransitionToRandomWeather()
    {
        WeatherType[] weather_types = (WeatherType[])Enum.GetValues(typeof(WeatherType));
        WeatherType new_type = weather_types[GD.Randi() % weather_types.Length];
        
        WeatherIntensity[] intensities = (WeatherIntensity[])Enum.GetValues(typeof(WeatherIntensity));
        WeatherIntensity new_intensity = intensities[GD.Randi() % intensities.Length];
        
        float duration = GetRandomWeatherDuration(new_type);
        
        EmitSignal(nameof(WeatherTransitionStarted), current_weather.type, new_type);
        
        SetWeather(new_type, new_intensity, duration, false);
        
        player_data.total_weather_changes++;
    }
    
    private float GetRandomWeatherDuration(WeatherType type)
    {
        switch (type)
        {
            case WeatherType.Clear: return 180 + GD.Randf() * 120;
            case WeatherType.Cloudy: return 120 + GD.Randf() * 60;
            case WeatherType.Rain: return 60 + GD.Randf() * 60;
            case WeatherType.Snow: return 90 + GD.Randf() * 90;
            case WeatherType.Thunderstorm: return 30 + GD.Randf() * 30;
            case WeatherType.Fog: return 120 + GD.Randf() * 60;
            case WeatherType.Sandstorm: return 45 + GD.Randf() * 45;
            case WeatherType.Hail: return 30 + GD.Randf() * 30;
            case WeatherType.Blizzard: return 60 + GD.Randf() * 60;
            case WeatherType.Storm: return 45 + GD.Randf() * 45;
            default: return 120;
        }
    }
    
    public void SetWeather(WeatherType type, WeatherIntensity intensity, float duration, bool permanent = false)
    {
        WeatherType old_type = current_weather != null ? current_weather.type : WeatherType.Clear;
        
        current_weather = new WeatherInstance(type, intensity, duration, permanent);
        
        // Update encounter count
        if (player_data.weather_encounters.ContainsKey(type))
        {
            player_data.weather_encounters[type]++;
        }
        
        // Unlock weather type
        string weather_key = type.ToString();
        if (!player_data.unlocked_weather_types.Contains(weather_key))
        {
            player_data.unlocked_weather_types.Add(weather_key);
        }
        
        last_weather_type = old_type;
        EmitSignal(nameof(WeatherChanged), old_type, type);
        EmitSignal(nameof(WeatherIntensityChanged), intensity);
        
        UpdateWeatherEffects();
    }
    
    public void ForceWeatherChange(WeatherType type, WeatherIntensity intensity, float duration)
    {
        if (current_weather != null)
        {
            EmitSignal(nameof(WeatherTransitionStarted), current_weather.type, type);
        }
        
        SetWeather(type, intensity, duration, false);
    }
    
    public void SetWeatherEnabled(bool enabled)
    {
        is_weather_enabled = enabled;
    }
    
    private void UpdateWeatherEffects()
    {
        if (current_weather == null) return;
        
        float intensity_mult = current_weather.GetIntensityMultiplier();
        
        switch (current_weather.type)
        {
            case WeatherType.Clear:
                visibility_reduction = 1.0f;
                movement_speed_modifier = 1.0f + 0.1f * intensity_mult;
                attack_speed_modifier = 1.0f;
                defense_modifier = 1.0f;
                drop_rate_modifier = 1.0f + 0.1f * intensity_mult;
                break;
                
            case WeatherType.Cloudy:
                visibility_reduction = 0.9f - 0.1f * intensity_mult;
                movement_speed_modifier = 1.0f;
                attack_speed_modifier = 0.95f;
                defense_modifier = 1.0f;
                drop_rate_modifier = 1.0f + 0.15f * intensity_mult;
                break;
                
            case WeatherType.Rain:
                visibility_reduction = 0.85f - 0.15f * intensity_mult;
                movement_speed_modifier = 0.95f;
                attack_speed_modifier = 0.9f;
                defense_modifier = 0.95f;
                drop_rate_modifier = 1.2f + 0.2f * intensity_mult;
                break;
                
            case WeatherType.Snow:
                visibility_reduction = 0.8f - 0.2f * intensity_mult;
                movement_speed_modifier = 0.85f - 0.1f * intensity_mult;
                attack_speed_modifier = 0.9f;
                defense_modifier = 1.1f;
                drop_rate_modifier = 1.3f + 0.2f * intensity_mult;
                break;
                
            case WeatherType.Thunderstorm:
                visibility_reduction = 0.75f - 0.15f * intensity_mult;
                movement_speed_modifier = 0.9f;
                attack_speed_modifier = 1.1f + 0.1f * intensity_mult;
                defense_modifier = 0.85f;
                drop_rate_modifier = 1.5f + 0.3f * intensity_mult;
                break;
                
            case WeatherType.Fog:
                visibility_reduction = 0.6f - 0.2f * intensity_mult;
                movement_speed_modifier = 0.95f;
                attack_speed_modifier = 0.85f;
                defense_modifier = 1.0f;
                drop_rate_modifier = 1.0f;
                break;
                
            case WeatherType.Sandstorm:
                visibility_reduction = 0.7f - 0.2f * intensity_mult;
                movement_speed_modifier = 0.8f - 0.1f * intensity_mult;
                attack_speed_modifier = 0.85f;
                defense_modifier = 0.9f;
                drop_rate_modifier = 1.4f + 0.2f * intensity_mult;
                break;
                
            case WeatherType.Hail:
                visibility_reduction = 0.8f - 0.15f * intensity_mult;
                movement_speed_modifier = 0.9f;
                attack_speed_modifier = 0.95f;
                defense_modifier = 0.85f - 0.1f * intensity_mult;
                drop_rate_modifier = 1.3f + 0.2f * intensity_mult;
                break;
                
            case WeatherType.Blizzard:
                visibility_reduction = 0.5f - 0.2f * intensity_mult;
                movement_speed_modifier = 0.7f - 0.15f * intensity_mult;
                attack_speed_modifier = 0.8f;
                defense_modifier = 1.2f;
                drop_rate_modifier = 1.6f + 0.3f * intensity_mult;
                break;
                
            case WeatherType.Storm:
                visibility_reduction = 0.7f - 0.2f * intensity_mult;
                movement_speed_modifier = 0.85f;
                attack_speed_modifier = 1.05f;
                defense_modifier = 0.9f;
                drop_rate_modifier = 1.4f + 0.3f * intensity_mult;
                break;
        }
    }
    
    // Getters
    public WeatherType GetCurrentWeatherType() => current_weather?.type ?? WeatherType.Clear;
    public WeatherIntensity GetCurrentIntensity() => current_weather?.intensity ?? WeatherIntensity.Light;
    public float GetVisibilityReduction() => visibility_reduction;
    public float GetMovementSpeedModifier() => movement_speed_modifier;
    public float GetAttackSpeedModifier() => attack_speed_modifier;
    public float GetDefenseModifier() => defense_modifier;
    public float GetDropRateModifier() => drop_rate_modifier;
    public float GetRemainingTime() => current_weather?.remaining_time ?? 0;
    public bool IsTransitioning() => is_transitioning;
    public float GetTransitionProgress() => transition_progress;
    public PlayerWeatherData GetPlayerData() => player_data;
    public bool IsWeatherEnabled() => is_weather_enabled;
    
    public Dictionary<string, object> GetStatistics()
    {
        var stats = new Dictionary<string, object>();
        stats["total_changes"] = player_data.total_weather_changes;
        stats["unlocked_types"] = player_data.unlocked_weather_types.Count;
        stats["favorite_weather"] = player_data.favorite_weather.ToString();
        stats["favorite_time"] = player_data.favorite_weather_time;
        
        var encounter_dict = new Dictionary<string, int>();
        foreach (var kvp in player_data.weather_encounters)
        {
            encounter_dict[kvp.Key.ToString()] = kvp.Value;
        }
        stats["encounters"] = encounter_dict;
        
        return stats;
    }
    
    // Save/Load
    public Dictionary<string, object> Save()
    {
        var data = new Dictionary<string, object>();
        
        if (current_weather != null)
        {
            data["current_type"] = (int)current_weather.type;
            data["current_intensity"] = (int)current_weather.intensity;
            data["remaining_time"] = current_weather.remaining_time;
            data["is_permanent"] = current_weather.is_permanent;
        }
        
        data["enabled"] = is_weather_enabled;
        data["global_timer"] = global_weather_timer;
        
        // Save player data
        var pd = new Dictionary<string, object>();
        pd["unlocked"] = player_data.unlocked_weather_types;
        pd["total_changes"] = player_data.total_weather_changes;
        
        var encounters = new Dictionary<string, int>();
        foreach (var kvp in player_data.weather_encounters)
        {
            encounters[kvp.Key.ToString()] = kvp.Value;
        }
        pd["encounters"] = encounters;
        
        var time_spent = new Dictionary<string, float>();
        foreach (var kvp in player_data.time_spent)
        {
            time_spent[kvp.Key.ToString()] = kvp.Value;
        }
        pd["time_spent"] = time_spent;
        
        pd["favorite_weather"] = (int)player_data.favorite_weather;
        pd["favorite_time"] = player_data.favorite_weather_time;
        
        data["player_data"] = pd;
        
        return data;
    }
    
    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("current_type"))
        {
            WeatherType type = (WeatherType)(int)data["current_type"];
            WeatherIntensity intensity = (WeatherIntensity)(int)data["current_intensity"];
            float remaining = (float)data["remaining_time"];
            bool permanent = (bool)data["is_permanent"];
            
            current_weather = new WeatherInstance(type, intensity, remaining, permanent);
        }
        
        if (data.ContainsKey("enabled"))
        {
            is_weather_enabled = (bool)data["enabled"];
        }
        
        if (data.ContainsKey("global_timer"))
        {
            global_weather_timer = (float)data["global_timer"];
        }
        
        if (data.ContainsKey("player_data"))
        {
            var pd = (Dictionary<string, object>)data["player_data"];
            
            if (pd.ContainsKey("unlocked"))
            {
                player_data.unlocked_weather_types = (List<string>)pd["unlocked"];
            }
            
            if (pd.ContainsKey("total_changes"))
            {
                player_data.total_weather_changes = (int)pd["total_changes"];
            }
            
            if (pd.ContainsKey("encounters"))
            {
                var encounters = (Dictionary<string, object>)pd["encounters"];
                foreach (var kvp in encounters)
                {
                    WeatherType wt;
                    if (Enum.TryParse<WeatherType>(kvp.Key, out wt))
                    {
                        player_data.weather_encounters[wt] = Convert.ToInt32(kvp.Value);
                    }
                }
            }
            
            if (pd.ContainsKey("time_spent"))
            {
                var time_spent = (Dictionary<string, object>)pd["time_spent"];
                foreach (var kvp in time_spent)
                {
                    WeatherType wt;
                    if (Enum.TryParse<WeatherType>(kvp.Key, out wt))
                    {
                        player_data.time_spent[wt] = Convert.ToSingle(kvp.Value);
                    }
                }
            }
            
            if (pd.ContainsKey("favorite_weather"))
            {
                player_data.favorite_weather = (WeatherType)(int)pd["favorite_weather"];
            }
            
            if (pd.ContainsKey("favorite_time"))
            {
                player_data.favorite_weather_time = (float)pd["favorite_time"];
            }
        }
        
        UpdateWeatherEffects();
    }

    public override Dictionary ExportSaveData() => new();
    public override void ImportSaveData(Dictionary data) { }
}
