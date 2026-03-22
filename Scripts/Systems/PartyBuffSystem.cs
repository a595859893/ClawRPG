using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

/// <summary>
/// 队伍Buff系统 - 管理队伍增益效果
/// </summary>
public class PartyBuffSystem : BaseSystem
{
    public static PartyBuffSystem Instance { get; private set; }

    // 队伍Buff
    private List<PartyData.PartyBuff> _activeBuffs = new List<PartyData.PartyBuff>();
    private readonly object _buffsLock = new object();

    // 事件信号
    public delegate void BuffAddedEvent(PartyData.PartyBuff buff);
    public delegate void BuffRemovedEvent(PartyData.PartyBuffType buffType);

    public event BuffAddedEvent OnBuffAdded;
    public event BuffRemovedEvent OnBuffRemoved;

    protected override void Initialize()
    {
        Instance = this;
        GD.Print("[PartyBuffSystem] Initialized");
    }

    public override void _Process(float delta)
    {
        UpdateBuffs(delta);
    }

    /// <summary>
    /// 添加队伍Buff
    /// </summary>
    public void AddBuff(PartyData.PartyBuffType type, float value, float duration, int providerId)
    {
        var buff = new PartyData.PartyBuff
        {
            Type = type,
            Value = value,
            Duration = duration,
            RemainingTime = duration,
            ProviderId = providerId
        };
        
        lock (_buffsLock)
        {
            _activeBuffs.RemoveAll(b => b.Type == type);
            _activeBuffs.Add(buff);
        }
        
        OnBuffAdded?.Invoke(buff);
        
        if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected && PartyManager.Instance != null)
        {
            var message = new Dictionary<string, object>
            {
                { "type", "party_add_buff" },
                { "party_id", PartyManager.Instance.PartyId },
                { "buff_type", type.ToString() },
                { "value", value },
                { "duration", duration },
                { "provider_id", providerId }
            };
            NetworkClient.Instance.SendJson(message);
        }
    }

    /// <summary>
    /// 移除队伍Buff
    /// </summary>
    public void RemoveBuff(PartyData.PartyBuffType type)
    {
        lock (_buffsLock)
        {
            _activeBuffs.RemoveAll(b => b.Type == type);
        }
        
        OnBuffRemoved?.Invoke(type);
    }

    /// <summary>
    /// 获取队伍Buff效果
    /// </summary>
    public float GetBuffValue(PartyData.PartyBuffType type)
    {
        lock (_buffsLock)
        {
            foreach (var buff in _activeBuffs)
            {
                if (buff.Type == type)
                {
                    return buff.Value;
                }
            }
        }
        return 0f;
    }

    /// <summary>
    /// 获取所有Buff效果
    /// </summary>
    public Dictionary<PartyData.PartyBuffType, float> GetAllBuffValues()
    {
        var result = new Dictionary<PartyData.PartyBuffType, float>();
        
        lock (_buffsLock)
        {
            foreach (var buff in _activeBuffs)
            {
                if (!result.ContainsKey(buff.Type))
                {
                    result[buff.Type] = 0f;
                }
                result[buff.Type] += buff.Value;
            }
        }
        
        return result;
    }

    /// <summary>
    /// 获取所有活跃Buff
    /// </summary>
    public List<PartyData.PartyBuff> GetActiveBuffs()
    {
        lock (_buffsLock)
        {
            return new List<PartyData.PartyBuff>(_activeBuffs);
        }
    }

    /// <summary>
    /// 更新Buff时间
    /// </summary>
    private void UpdateBuffs(float delta)
    {
        lock (_buffsLock)
        {
            for (int i = _activeBuffs.Count - 1; i >= 0; i--)
            {
                var buff = _activeBuffs[i];
                buff.RemainingTime -= delta;
                
                if (buff.RemainingTime <= 0)
                {
                    _activeBuffs.RemoveAt(i);
                    OnBuffRemoved?.Invoke(buff.Type);
                }
            }
        }
    }

    /// <summary>
    /// 处理服务器消息
    /// </summary>
    public void HandleMessage(Dictionary<string, object> data)
    {
        if (!data.ContainsKey("type")) return;
        
        string msgType = data["type"].ToString();
        
        if (msgType == "party_buff_added")
        {
            var buffType = Enum.Parse<PartyData.PartyBuffType>(data["buff_type"].ToString());
            float buffValue = Convert.ToSingle(data["value"]);
            float buffDuration = Convert.ToSingle(data["duration"]);
            int provider = Convert.ToInt32(data["provider_id"]);
            AddBuff(buffType, buffValue, buffDuration, provider);
        }
    }

    /// <summary>
    /// 清除所有Buff
    /// </summary>
    public void ClearAllBuffs()
    {
        lock (_buffsLock)
        {
            _activeBuffs.Clear();
        }
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        var buffs = new Godot.Collections.Array();
        lock (_buffsLock)
        {
            foreach (var buff in _activeBuffs)
            {
                var b = new Dictionary();
                b["type"] = (int)buff.Type;
                b["value"] = buff.Value;
                b["duration"] = buff.Duration;
                b["remaining_time"] = buff.RemainingTime;
                b["provider_id"] = buff.ProviderId;
                buffs.Add(b);
            }
        }
        data["active_buffs"] = buffs;
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("active_buffs"))
        {
            lock (_buffsLock)
            {
                _activeBuffs.Clear();
                var buffs = (Godot.Collections.Array)data["active_buffs"];
                foreach (Dictionary b in buffs)
                {
                    var buff = new PartyData.PartyBuff
                    {
                        Type = (PartyData.PartyBuffType)(int)b["type"],
                        Value = (float)b["value"],
                        Duration = (float)b["duration"],
                        RemainingTime = (float)b["remaining_time"],
                        ProviderId = (int)b["provider_id"]
                    };
                    _activeBuffs.Add(buff);
                }
            }
        }
    }
}
