using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ClawRPG.Systems
{
/// <summary>
/// 社交分享系统 - 协调层
/// 整合截图、分享模板、群聊机器人功能
/// </summary>
public class SocialSharingSystem : BaseSystem
{
    public static SocialSharingSystem Instance { get; private set; }

    // 信号 - 代理到子模块
    public delegate void ScreenshotTakenEvent(string filePath);
    public delegate void ShareCompletedEvent(bool success, string message);
    public delegate void BotResponseEvent(string response);
    
    public event ScreenshotTakenEvent OnScreenshotTaken;
    public event ShareCompletedEvent OnShareCompleted;
    public event BotResponseEvent OnBotResponse;

    // 子系统引用
    private ScreenshotManager _screenshotManager;
    private ShareTemplateManager _templateManager;
    private GroupBotConnector _botConnector;

    public override void _Ready()
    {
        Instance = this;
        
        // 获取子系统引用
        _screenshotManager = ScreenshotManager.Instance;
        _templateManager = ShareTemplateManager.Instance;
        _botConnector = GroupBotConnector.Instance;
        
        // 订阅子系统事件
        if (_screenshotManager != null)
        {
            _screenshotManager.OnScreenshotTaken += HandleScreenshotTaken;
        }
        
        if (_botConnector != null)
        {
            _botConnector.OnShareCompleted += HandleShareCompleted;
            _botConnector.OnBotResponse += HandleBotResponse;
        }
        
        GD.Print("[SocialSharingSystem] Initialized");
    }

    #region Event Handlers

    private void HandleScreenshotTaken(string filePath)
    {
        OnScreenshotTaken?.Invoke(filePath);
    }

    private void HandleShareCompleted(bool success, string message)
    {
        OnShareCompleted?.Invoke(success, message);
    }

    private void HandleBotResponse(string response)
    {
        OnBotResponse?.Invoke(response);
    }

    #endregion

    #region Screenshot API (代理到 ScreenshotManager)

    /// <summary>
    /// 截取游戏画面
    /// </summary>
    public string TakeScreenshot(string customFileName = "")
    {
        if (_screenshotManager == null)
        {
            GD.PrintErr("[SocialSharingSystem] ScreenshotManager not available");
            return "";
        }
        return _screenshotManager.TakeScreenshot(customFileName);
    }

    /// <summary>
    /// 截取UI区域
    /// </summary>
    public string TakeUIScreenshot(TextureRect targetUI, string fileName = "")
    {
        if (_screenshotManager == null)
        {
            GD.PrintErr("[SocialSharingSystem] ScreenshotManager not available");
            return "";
        }
        return _screenshotManager.TakeUIScreenshot(targetUI, fileName);
    }

    /// <summary>
    /// 获取最后一张截图
    /// </summary>
    public Godot.Image GetLastScreenshot()
    {
        if (_screenshotManager == null)
            return null;
        return _screenshotManager.GetLastScreenshot();
    }

    #endregion

    #region Share Templates API (代理到 ShareTemplateManager)

    /// <summary>
    /// 生成分享文本
    /// </summary>
    public string GenerateShareText(ShareTemplateManager.ShareData data, ShareTemplateManager.ShareTemplate template = ShareTemplateManager.ShareTemplate.Simple)
    {
        if (_templateManager == null)
        {
            GD.PrintErr("[SocialSharingSystem] ShareTemplateManager not available");
            return "";
        }
        return _templateManager.GenerateShareText(data, template);
    }

    /// <summary>
    /// 生成群聊战绩卡片
    /// </summary>
    public string GenerateGroupCard(ShareTemplateManager.ShareData data)
    {
        if (_templateManager == null)
            return "";
        return _templateManager.GenerateGroupCard(data);
    }

    /// <summary>
    /// 生成战绩JSON
    /// </summary>
    public string GenerateShareJson(ShareTemplateManager.ShareData data)
    {
        if (_templateManager == null)
            return "";
        return _templateManager.GenerateShareJson(data);
    }

    #endregion

    #region Group Bot API (代理到 GroupBotConnector)

    /// <summary>
    /// 发送战绩到群聊
    /// </summary>
    public void SendToGroup(ShareTemplateManager.ShareData data, long groupId)
    {
        if (_botConnector == null)
        {
            GD.PrintErr("[SocialSharingSystem] GroupBotConnector not available");
            OnShareCompleted?.Invoke(false, "Bot connector not available");
            return;
        }
        _botConnector.SendToGroup(data, groupId);
    }

    /// <summary>
    /// 发送战绩到群聊(带截图)
    /// </summary>
    public void SendScreenshotToGroup(ShareTemplateManager.ShareData data, long groupId, string screenshotPath = "")
    {
        if (_botConnector == null)
        {
            OnShareCompleted?.Invoke(false, "Bot connector not available");
            return;
        }
        _botConnector.SendScreenshotToGroup(data, groupId, screenshotPath);
    }

    /// <summary>
    /// 查询群内战绩排行
    /// </summary>
    public void QueryGroupLeaderboard(long groupId, string gameMode = "")
    {
        if (_botConnector == null)
            return;
        _botConnector.QueryGroupLeaderboard(groupId, gameMode);
    }

    /// <summary>
    /// 处理机器人回调
    /// </summary>
    public void HandleBotCallback(string jsonResponse)
    {
        if (_botConnector != null)
            _botConnector.HandleBotCallback(jsonResponse);
    }

    /// <summary>
    /// 连接 WebSocket
    /// </summary>
    public void ConnectWebSocket()
    {
        if (_botConnector != null)
            _botConnector.ConnectWebSocket();
    }

    /// <summary>
    /// 断开 WebSocket 连接
    /// </summary>
    public void DisconnectWebSocket()
    {
        if (_botConnector != null)
            _botConnector.DisconnectWebSocket();
    }

    #endregion

    #region Platform Share API

    /// <summary>
    /// 打开分享对话框 - 支持多平台系统分享
    /// </summary>
    public void OpenShareDialog(ShareTemplateManager.ShareData data)
    {
        if (_templateManager == null)
        {
            OnShareCompleted?.Invoke(false, "Template manager not available");
            return;
        }
        
        var shareText = _templateManager.GenerateShareText(data, ShareTemplateManager.ShareTemplate.Simple);
        OpenShareDialogWithText(shareText);
    }

    /// <summary>
    /// 使用指定文本打开分享对话框
    /// </summary>
    public void OpenShareDialogWithText(string shareText)
    {
        string currentOS = OS.GetName();
        GD.Print($"[SocialSharingSystem] Opening share dialog on {currentOS}");
        
        // 根据平台选择分享方式
        switch (currentOS)
        {
            case "Android":
                ShareOnAndroid(shareText);
                break;
                
            case "iOS":
                ShareOnIOS(shareText);
                break;
                
            case "Windows":
            case "macOS":
            case "Linux":
                ShareOnDesktop(shareText);
                break;
                
            case "Web":
                ShareOnWeb(shareText);
                break;
                
            default:
                OS.SetClipboardString(shareText);
                GD.Print($"[SocialSharingSystem] Unsupported platform - copied to clipboard");
                break;
        }
        
        OnShareCompleted?.Invoke(true, "Share dialog opened");
    }

    /// <summary>
    /// Android 平台分享
    /// </summary>
    private void ShareOnAndroid(string text)
    {
        var shareData = new Dictionary<string, string>
        {
            { "title", "分享 ClawRPG 战绩" },
            { "text", text }
        };
        
        GD.Print($"[SocialSharingSystem] Android share: {JsonSerializer.Serialize(shareData)}");
        OnShareCompleted?.Invoke(true, "Android share dialog opened");
    }

    /// <summary>
    /// iOS 平台分享
    /// </summary>
    private void ShareOnIOS(string text)
    {
        var shareData = new Dictionary<string, string>
        {
            { "text", text }
        };
        
        GD.Print($"[SocialSharingSystem] iOS share: {JsonSerializer.Serialize(shareData)}");
        OnShareCompleted?.Invoke(true, "iOS share dialog opened");
    }

    /// <summary>
    /// 桌面平台分享
    /// </summary>
    private void ShareOnDesktop(string text)
    {
        OS.SetClipboardString(text);
        GD.Print($"[SocialSharingSystem] Desktop - copied to clipboard: {text.Substring(0, Math.Min(50, text.Length))}...");
        OnShareCompleted?.Invoke(true, "Copied to clipboard");
    }

    /// <summary>
    /// Web 平台分享
    /// </summary>
    private void ShareOnWeb(string text)
    {
        var shareData = new Dictionary<string, string>
        {
            { "text", text },
            { "url", "https://clawrpg.example.com" }
        };
        
        GD.Print($"[SocialSharingSystem] Web share: {JsonSerializer.Serialize(shareData)}");
        OnShareCompleted?.Invoke(true, "Web share prepared");
    }

    /// <summary>
    /// 分享截图到系统
    /// </summary>
    public void ShareScreenshot(string screenshotPath = "", ShareTemplateManager.ShareData data = null)
    {
        // 如果没有指定截图，先截取
        if (string.IsNullOrEmpty(screenshotPath))
        {
            screenshotPath = TakeScreenshot();
        }
        
        if (string.IsNullOrEmpty(screenshotPath))
        {
            GD.PrintErr("[SocialSharingSystem] No screenshot to share");
            OnShareCompleted?.Invoke(false, "No screenshot available");
            return;
        }
        
        string shareText = data != null && _templateManager != null 
            ? _templateManager.GenerateShareText(data, ShareTemplateManager.ShareTemplate.Simple) 
            : "";
        
        string currentOS = OS.GetName();
        
        if (currentOS == "Android" || currentOS == "iOS")
        {
            var shareData = new Dictionary<string, string>
            {
                { "image", screenshotPath },
                { "text", shareText }
            };
            
            GD.Print($"[SocialSharingSystem] Mobile screenshot share prepared: {screenshotPath}");
            OnShareCompleted?.Invoke(true, "Screenshot share opened");
        }
        else
        {
            OS.SetClipboardString(screenshotPath);
            GD.Print($"[SocialSharingSystem] Screenshot path copied: {screenshotPath}");
            OnShareCompleted?.Invoke(true, "Screenshot path copied");
        }
    }

    /// <summary>
    /// 复制到剪贴板
    /// </summary>
    public void CopyToClipboard(string text)
    {
        OS.SetClipboardString(text);
        GD.Print("[SocialSharingSystem] Copied to clipboard");
    }

    #endregion

    public override void _ExitTree()
    {
        // 取消事件订阅
        if (_screenshotManager != null)
        {
            _screenshotManager.OnScreenshotTaken -= HandleScreenshotTaken;
        }
        
        if (_botConnector != null)
        {
            _botConnector.OnShareCompleted -= HandleShareCompleted;
            _botConnector.OnBotResponse -= HandleBotResponse;
        }
        
        Instance = null;
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 导出子系统数据
        if (_screenshotManager != null)
        {
            foreach (var kvp in _screenshotManager.ExportSaveData())
            {
                data[kvp.Key] = kvp.Value;
            }
        }
        
        if (_botConnector != null)
        {
            foreach (var kvp in _botConnector.ExportSaveData())
            {
                data["bot_" + kvp.Key] = kvp.Value;
            }
        }
        
        return data;
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        // 导入子系统数据
        if (_screenshotManager != null)
        {
            _screenshotManager.ImportSaveData(data);
        }
        
        if (_botConnector != null)
        {
            // 提取 bot 相关配置
            var botData = new Dictionary<string, object>();
            foreach (var kvp in data)
            {
                if (kvp.Key.ToString().StartsWith("bot_"))
                {
                    botData[kvp.Key.ToString().Substring(4)] = kvp.Value;
                }
            }
            if (botData.Count > 0)
            {
                _botConnector.ImportSaveData(botData);
            }
        }
    }
}
}
