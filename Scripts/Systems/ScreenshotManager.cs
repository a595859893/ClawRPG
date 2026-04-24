using Godot;
using System;

namespace ClawRPG.Systems
{
/// <summary>
/// 截图管理器 - 负责游戏画面截图功能
/// </summary>
public partial class ScreenshotManager : BaseSystem
{
    public static ScreenshotManager Instance { get; private set; }

    // 配置
    [Export] public bool AutoSaveScreenshots = true;
    [Export] public string ScreenshotFolder = "user://screenshots/";
    
    // 信号
    public delegate void ScreenshotTakenEvent(string filePath);
    public event ScreenshotTakenEvent OnScreenshotTaken;

    // 截图数据
    private Viewport _mainViewport;
    private Image _lastScreenshot;

    public override void _Ready()
    {
        Instance = this;
        EnsureScreenshotDirectory();
    }

    /// <summary>
    /// 截取游戏画面
    /// </summary>
    public string TakeScreenshot(string customFileName = "")
    {
        var root = GetTree().Root;
        _mainViewport = root;
        
        if (_mainViewport == null)
        {
            GD.PrintErr("[ScreenshotManager] Failed to get main viewport");
            return "";
        }

        // 创建图像
        var image = _mainViewport.GetTexture().GetData();
        if (image == null)
        {
            GD.PrintErr("[ScreenshotManager] Failed to get image from viewport");
            return "";
        }
        image.FlipY();
        
        _lastScreenshot = image;
        
        // 生成文件名
        string fileName = string.IsNullOrEmpty(customFileName) 
            ? $"screenshot_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.png" 
            : customFileName;
        
        string fullPath = ScreenshotFolder + fileName;
        
        // 保存图片
        Error error = image.SavePng(fullPath);
        
        if (error == Error.Ok)
        {
            GD.Print($"[ScreenshotManager] Screenshot saved: {fullPath}");
            OnScreenshotTaken?.Invoke(fullPath);
            return fullPath;
        }
        else
        {
            GD.PrintErr($"[ScreenshotManager] Failed to save screenshot: {error}");
            return "";
        }
    }

    /// <summary>
    /// 截取UI区域
    /// </summary>
    public string TakeUIScreenshot(TextureRect targetUI, string fileName = "")
    {
        if (targetUI == null)
        {
            return TakeScreenshot(fileName);
        }

        var image = targetUI.GetTexture().GetData();
        if (image == null)
        {
            GD.PrintErr("[ScreenshotManager] Failed to get image from UI texture");
            return "";
        }
        image.FlipY();
        
        string name = string.IsNullOrEmpty(fileName) 
            ? $"ui_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.png" 
            : fileName;
        
        string fullPath = ScreenshotFolder + name;
        
        if (image.SavePng(fullPath) == Error.Ok)
        {
            OnScreenshotTaken?.Invoke(fullPath);
            return fullPath;
        }
        
        return "";
    }

    /// <summary>
    /// 获取最后一张截图
    /// </summary>
    public Image GetLastScreenshot()
    {
        return _lastScreenshot;
    }

    /// <summary>
    /// 确保截图目录存在
    /// </summary>
    private void EnsureScreenshotDirectory()
    {
        var dir = new Directory();
        if (!dir.DirExists(ScreenshotFolder))
        {
            dir.MakeDirRecursive(ScreenshotFolder);
            GD.Print($"[ScreenshotManager] Created screenshot directory: {ScreenshotFolder}");
        }
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "screenshot_folder", ScreenshotFolder },
            { "auto_save", AutoSaveScreenshots }
        };
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("screenshot_folder"))
            ScreenshotFolder = data["screenshot_folder"].ToString();
        if (data.ContainsKey("auto_save"))
            AutoSaveScreenshots = (bool)data["auto_save"];
    }
}
}
