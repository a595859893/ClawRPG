using System.Collections.Generic;

public partial class MultiplayerManager
{
    #region 玩家数据访问
    
    /// <summary>
    /// 获取所有网络玩家
    /// </summary>
    public List<NetworkSyncSystem.NetworkPlayer> GetNetworkPlayers()
    {
        return NetworkSync?.GetAllPlayers() ?? new List<NetworkSyncSystem.NetworkPlayer>();
    }
    
    /// <summary>
    /// 获取指定玩家
    /// </summary>
    public NetworkSyncSystem.NetworkPlayer GetNetworkPlayer(int playerId)
    {
        return NetworkSync?.GetPlayer(playerId) ?? default;
    }
    
    #endregion
}
