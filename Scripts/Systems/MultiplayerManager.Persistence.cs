using Godot;
using System.Collections.Generic;

public partial class MultiplayerManager
{
    #region 数据持久化
    
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        if (Session != null)
        {
            data["session_room_id"] = Session.CurrentRoomId ?? "";
            data["session_is_host"] = Session.IsHost;
            data["session_needs_password"] = Session.NeedsPassword;
        }
        
        if (NetworkSync != null)
        {
            data["network_is_ready"] = NetworkSync.IsReady;
        }
        
        return data;
    }
    
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.TryGetValue("network_is_ready", out var ready) && ready is bool r)
        {
            if (r) SetReady(true);
        }
        
        GD.Print("[MultiplayerManager] Save data imported");
    }
    
    #endregion
}
