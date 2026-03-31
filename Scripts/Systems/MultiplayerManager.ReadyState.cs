public partial class MultiplayerManager
{
    #region 准备状态管理 (委托给 NetworkSync)
    
    /// <summary>
    /// 设置准备状态
    /// </summary>
    public void SetReady(bool ready)
    {
        if (NetworkSync != null)
        {
            NetworkSync.SetLocalPlayerReady(ready);
            Lobby?.SetPlayerReady(LocalPlayerId, ready);
        }
    }
    
    /// <summary>
    /// 切换准备状态
    /// </summary>
    public void ToggleReady()
    {
        if (NetworkSync != null)
        {
            bool currentReady = NetworkSync.IsReady;
            SetReady(!currentReady);
        }
    }
    
    /// <summary>
    /// 检查所有玩家是否准备
    /// </summary>
    public bool AreAllPlayersReady()
    {
        return NetworkSync?.AreAllPlayersReady() ?? false;
    }
    
    #endregion
}
