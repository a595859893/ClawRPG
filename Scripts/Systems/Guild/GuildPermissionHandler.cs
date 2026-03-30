using System;
using ClawRPG.Scripts.Systems.Guild;

/// <summary>
/// 公会权限处理器
/// </summary>
public static class GuildPermissionHandler
{
    /// <summary>
    /// 检查是否拥有指定权限
    /// </summary>
    /// <param name="playerPermissions">玩家权限位掩码</param>
    /// <param name="permission">要检查的权限</param>
    /// <returns>是否拥有该权限</returns>
    public static bool HasPermission(GuildPermission playerPermissions, GuildPermission permission)
    {
        return (playerPermissions & permission) == permission;
    }
}
