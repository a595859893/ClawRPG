namespace ClawRPG.Scripts.Tools;

/// <summary>
/// Godot Headless 运行器 - 封装 headless 执行逻辑
/// 用于 AI Agent 自动化测试：运行指定帧数、捕获日志、分析结果
/// </summary>
public partial class GodotRunner : BaseSystem
{
    public static GodotRunner Instance { get; private set; }

    /// <summary>
    /// Godot 可执行文件路径 (默认从 PATH 或已知位置查找)
    /// </summary>
    [Export] private string _godotPath = "godot";

    /// <summary>
    /// 最近一次运行的退出码
    /// </summary>
    public int LastExitCode { get; private set; }

    /// <summary>
    /// 最近一次运行的 stdout
    /// </summary>
    public string LastStdout { get; private set; } = string.Empty;

    /// <summary>
    /// 最近一次运行的 stderr
    /// </summary>
    public string LastStderr { get; private set; } = string.Empty;

    /// <summary>
    /// 最近一次运行的持续时间 (秒)
    /// </summary>
    public double LastRunDuration { get; private set; }

    /// <summary>
    /// 运行结果信号
    /// </summary>
    [Signal] public delegate void RunCompletedEventHandler(int exitCode, string stdout, string stderr, double duration);
    [Signal] public delegate void RunFailedEventHandler(string reason);

    public override void _Ready()
    {
        Instance = this;
        base._Ready();
        AutoDetectGodotPath();
    }

    /// <summary>
    /// 尝试自动查找 Godot 可执行文件
    /// </summary>
    private void AutoDetectGodotPath()
    {
        string[] candidates = { "godot", "godot4", "/usr/bin/godot", "/usr/local/bin/godot",
                                  "$HOME/go/bin/godot", "$HOME/.local/bin/godot" };
        foreach (var candidate in candidates)
        {
            // 简单检查命令是否存在 (非阻塞)
            if (candidate.Contains("/"))
            {
                if (System.IO.File.Exists(candidate))
                {
                    _godotPath = candidate;
                    GD.Print($"[GodotRunner] Found Godot at: {_godotPath}");
                    return;
                }
            }
        }
        GD.Print($"[GodotRunner] Using Godot from PATH: {_godotPath}");
    }

    /// <summary>
    /// Headless 运行项目 (指定帧数后退出)
    /// </summary>
    /// <param name="scenePath">场景路径 (空则用 project.godot 默认场景)</param>
    /// <param name="frames">运行帧数，-1 表示不限制</param>
    /// <param name="quitAfter">多少秒后退出 (超时保护)</param>
    /// <returns>运行是否成功</returns>
    public async Task<bool> RunHeadless(string scenePath = "", int frames = -1, int quitAfter = 60)
    {
        var args = BuildHeadlessArgs(scenePath, frames);
        return await RunAsync(args, quitAfter);
    }

    /// <summary>
    /// 运行脚本 (--script 模式)
    /// </summary>
    public async Task<bool> RunScript(string scriptPath, string scenePath = "", int quitAfter = 60)
    {
        var args = $"--headless --script {scriptPath}";
        if (!string.IsNullOrEmpty(scenePath))
            args += $" --path {scenePath}";
        return await RunAsync(args, quitAfter);
    }

    /// <summary>
    /// 截图并保存到文件
    /// </summary>
    public async Task<bool> CaptureScreenshot(string outputPath, string scenePath = "", int quitAfter = 10)
    {
        var args = $"--headless --rendering{GetRenderingArgs()} --quit-after 2";
        if (!string.IsNullOrEmpty(scenePath))
            args += $" --path {scenePath}";
        // 截图通过 --script 脚本实现，这里只触发执行
        return await RunAsync(args, quitAfter);
    }

    private string BuildHeadlessArgs(string scenePath, int frames)
    {
        var args = "--headless";
        if (!string.IsNullOrEmpty(scenePath))
            args += $" --path {scenePath}";
        if (frames > 0)
            args += $" --quit-after {frames}";
        else if (frames == 0)
            args += " --quit"; // 立即退出，仅验证启动
        return args;
    }

    private string GetRenderingArgs()
    {
        // 输出到文件而非窗口
        return " --rendering-dt 0.016";
    }

    private async System.Threading.Tasks.Task<bool> RunAsync(string args, int timeoutSeconds)
    {
        var startTime = Time.GetTicksMsec() / 1000.0;
        LastExitCode = -1;
        LastStdout = string.Empty;
        LastStderr = string.Empty;

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = _godotPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = psi };
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) outputBuilder.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var completed = process.WaitForExit(timeoutSeconds * 1000);
            if (!completed)
            {
                try { process.Kill(); } catch { }
                LastStderr = errorBuilder.ToString() + $"\n[GodotRunner] Process timed out after {timeoutSeconds}s";
                EmitSignal(SignalName.RunFailed, LastStderr);
                LastRunDuration = Time.GetTicksMsec() / 1000.0 - startTime;
                return false;
            }

            LastExitCode = process.ExitCode;
            LastStdout = outputBuilder.ToString();
            LastStderr = errorBuilder.ToString();
            LastRunDuration = Time.GetTicksMsec() / 1000.0 - startTime;

            var success = LastExitCode == 0;
            EmitSignal(SignalName.RunCompleted, LastExitCode, LastStdout, LastStderr, LastRunDuration);

            if (!success)
            {
                GD.PrintErr($"[GodotRunner] Run failed: exit code {LastExitCode}");
                GD.PrintErr($"[GodotRunner] stderr: {LastStderr}");
            }
            else
            {
                GD.Print($"[GodotRunner] Run succeeded in {LastRunDuration:F2}s");
            }

            return success;
        }
        catch (System.Exception ex)
        {
            LastStderr = ex.Message;
            LastRunDuration = Time.GetTicksMsec() / 1000.0 - startTime;
            EmitSignal(SignalName.RunFailed, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 检查 Godot 是否可用
    /// </summary>
    public bool IsGodotAvailable()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = _godotPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = System.Diagnostics.Process.Start(psi);
            if (process == null) return false;
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取 Godot 版本字符串
    /// </summary>
    public string GetGodotVersion()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = _godotPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = System.Diagnostics.Process.Start(psi);
            if (process == null) return "unknown";
            process.WaitForExit(5000);
            return process.StandardOutput.ReadToEnd().Trim();
        }
        catch
        {
            return "unavailable";
        }
    }
}
