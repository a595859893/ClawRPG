namespace ClawRPG.Scripts.Tools;

/// <summary>
/// 截图工具 - 在 headless 运行时捕获游戏画面
/// 支持指定时机截图、节点路径截图、序列截图
/// </summary>
public partial class ScreenshotTool : BaseSystem
{
    public static ScreenshotTool Instance { get; private set; }

    /// <summary>
    /// 截图输出目录 (相对于 user://)
    /// </summary>
    [Export] private string _outputDirectory = "screenshots";

    /// <summary>
    /// 截图文件名格式
    /// </summary>
    [Export] private string _filenameFormat = "screenshot_{timestamp}_{index}";

    /// <summary>
    /// 是否启用截图 (可通过设置开关)
    /// </summary>
    [Export] private bool _enabled = true;

    /// <summary>
    /// 截图序列索引 (每次截图递增)
    /// </summary>
    private int _sequenceIndex = 0;

    /// <summary>
    /// 截图完成信号
    /// </summary>
    [Signal] public delegate void ScreenshotTakenEventHandler(string filePath);
    [Signal] public delegate void ScreenshotFailedEventHandler(string reason);

    public override void _Ready()
    {
        Instance = this;
        base._Ready();
        EnsureOutputDirectory();
    }

    private void EnsureOutputDirectory()
    {
        var dir = GetScreenshotDirectory();
        if (!DirAccess.Exists(dir))
        {
            DirAccess.MakeDirRecursiveAbsolute(dir);
            GD.Print($"[ScreenshotTool] Created directory: {dir}");
        }
    }

    private string GetScreenshotDirectory()
    {
        return ProjectSettings.GlobalizePath($"user://{_outputDirectory}");
    }

    /// <summary>
    /// 立即截取当前画面
    /// </summary>
    /// <param name="customName">自定义文件名 (不含扩展名)，为空则使用自动命名</param>
    /// <returns>截图文件路径，失败返回空字符串</returns>
    public string Capture(string customName = "")
    {
        if (!_enabled)
        {
            EmitSignal(SignalName.ScreenshotFailed, "ScreenshotTool is disabled");
            return "";
        }

        try
        {
            var filename = string.IsNullOrEmpty(customName)
                ? GenerateFilename()
                : SanitizeFilename(customName) + ".png";

            var dir = GetScreenshotDirectory();
            var fullPath = System.IO.Path.Combine(dir, filename);

            // Godot 4 的截图方式：获取 Viewport 的 texture 并保存
            var viewport = GetTree().Root;
            var image = viewport.GetTexture().GetImage();
            if (image == null)
            {
                // 备选：使用 ViewportTexture 的 CopyDepthTexture 或整体渲染
                EmitSignal(SignalName.ScreenshotFailed, "Failed to get viewport texture");
                return "";
            }

            var err = image.SavePng(fullPath);
            if (err != Error.Ok)
            {
                EmitSignal(SignalName.ScreenshotFailed, $"SavePng failed: {err}");
                return "";
            }

            GD.Print($"[ScreenshotTool] Screenshot saved: {fullPath}");
            EmitSignal(SignalName.ScreenshotTaken, fullPath);
            return fullPath;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[ScreenshotTool] Capture failed: {ex.Message}");
            EmitSignal(SignalName.ScreenshotFailed, ex.Message);
            return "";
        }
    }

    /// <summary>
    /// 延迟截图 (在指定秒数后截取)
    /// </summary>
    public async void CaptureDelayed(float delaySeconds, string customName = "")
    {
        await ToSignal(GetTree().CreateTimer(delaySeconds), Timer.SignalName.Timeout);
        Capture(customName);
    }

    /// <summary>
    /// 序列截图 - 每隔指定秒数截取一张
    /// </summary>
    /// <param name="intervalSeconds">间隔秒数</param>
    /// <param name="count">截图数量，-1 表示无限</param>
    public void CaptureSequence(float intervalSeconds, int count = -1)
    {
        if (!_enabled) return;

        var taken = 0;
        var timer = GetTree().CreateTimer(intervalSeconds, true, true);
        timer.Timeout += () =>
        {
            Capture($"seq_{taken:D4}");
            taken++;
            if (count > 0 && taken >= count)
            {
                timer.Stop();
                timer.QueueFree();
            }
        };
    }

    /// <summary>
    /// 截取指定节点的当前画面
    /// </summary>
    public string CaptureNode(Node node, string customName = "")
    {
        if (!_enabled || node == null)
        {
            EmitSignal(SignalName.ScreenshotFailed, "Disabled or node is null");
            return "";
        }

        try
        {
            // 将节点截图需要先让节点可见地渲染到 Viewport
            // 这里使用简化的实现：截取整个视口
            return Capture(string.IsNullOrEmpty(customName) ? $"node_{node.Name}" : customName);
        }
        catch (System.Exception ex)
        {
            EmitSignal(SignalName.ScreenshotFailed, ex.Message);
            return "";
        }
    }

    /// <summary>
    /// 获取截图目录中的所有截图文件
    /// </summary>
    public string[] GetAllScreenshots()
    {
        var dir = GetScreenshotDirectory();
        if (!DirAccess.Exists(dir)) return new string[0];

        var list = DirAccess.Open(dir);
        if (list == null) return new string[0];

        var files = new System.Collections.Generic.List<string>();
        list.ListBegin();
        string file;
        while ((file = list.ListNext()) != null)
        {
            if (file.EndsWith(".png") || file.EndsWith(".jpg"))
                files.Add(System.IO.Path.Combine(dir, file));
        }
        list.ListEnd();
        return files.ToArray();
    }

    /// <summary>
    /// 清理旧截图
    /// </summary>
    public void CleanupOldScreenshots(int keepCount = 50)
    {
        var files = GetAllScreenshots();
        if (files.Length <= keepCount) return;

        // 按修改时间排序，删除最旧的
        System.Array.Sort(files, (a, b) =>
            System.IO.File.GetLastWriteTime(a).CompareTo(System.IO.File.GetLastWriteTime(b)));

        for (int i = 0; i < files.Length - keepCount; i++)
        {
            System.IO.File.Delete(files[i]);
            GD.Print($"[ScreenshotTool] Deleted old screenshot: {files[i]}");
        }
    }

    private string GenerateFilename()
    {
        var timestamp = Time.GetDatetimeStringFromSystem().Replace(":", "-").Replace("T", "_");
        var index = _sequenceIndex++;
        return $"screenshot_{timestamp}_{index:D4}.png";
    }

    private static string SanitizeFilename(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        foreach (var c in invalid)
            name = name.Replace(c, '_');
        return name;
    }
}
