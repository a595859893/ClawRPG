using Godot;
using System;
using System.Threading.Tasks;
using ClawRPG.Scripts.Managers;

/// <summary>
/// 场景管理器 - 负责游戏场景的加载、切换和过渡
/// 使用 EventBusManager 进行事件通信，减少系统耦合
/// </summary>
public class SceneManager : ManagerBase
{
    public static SceneManager Instance { get; private set; }
    
    /// <summary>
    /// 优先级（数值越小越先初始化）
    /// </summary>
    public override int Priority => 3;
    
    /// <summary>
    /// 当前场景路径
    /// </summary>
    public string CurrentScenePath { get; private set; } = "";
    
    /// <summary>
    /// 目标场景路径
    /// </summary>
    public string TargetScenePath { get; private set; } = "";
    
    /// <summary>
    /// 是否正在加载场景
    /// </summary>
    public bool IsLoading { get; private set; } = false;
    
    /// <summary>
    /// 加载进度 (0.0 - 1.0)
    /// </summary>
    public float LoadingProgress { get; private set; } = 0f;
    
    /// <summary>
    /// 场景过渡模式
    /// </summary>
    public enum TransitionMode
    {
        None,
        Fade,
        Slide,
        Zoom
    }
    
    private TransitionMode _transitionMode = TransitionMode.Fade;
    private float _transitionDuration = 0.5f;
    private PackedScene _currentScene;
    private PackedScene _nextScene;
    
    // 事件
    public event Action<string> OnSceneChanging;
    public event Action<string> OnSceneChanged;
    public event Action<float> OnLoadingProgress;
    
    public override void _Ready()
    {
        Instance = this;
        base._Ready();
    }
    
    protected override void Initialize()
    {
        GD.Print("[SceneManager] Initialized");
        
        // 获取当前场景
        var root = GetTree().CurrentScene;
        if (root != null)
        {
            CurrentScenePath = root.Filename;
        }
        
        NotifyInitialized();
    }
    
    /// <summary>
    /// 切换到指定场景
    /// </summary>
    public void ChangeScene(string scenePath)
    {
        if (IsLoading) return;
        
        GD.Print($"[SceneManager] Changing scene to: {scenePath}");
        
        // 触发本地事件
        OnSceneChanging?.Invoke(scenePath);
        
        // 通过事件总线发布全局事件
        if (EventBusManager.Instance != null)
        {
            EventBusManager.Instance.Emit(EventBusManager.Events.SceneLoading, scenePath);
        }
        
        TargetScenePath = scenePath;
        IsLoading = true;
        
        // 使用 Godot 的场景切换
        var error = GetTree().ChangeScene(scenePath);
        if (error != Error.Ok)
        {
            GD.PrintErr($"[SceneManager] Failed to change scene: {error}");
            IsLoading = false;
            return;
        }
        
        // 等待场景切换完成
        _OnSceneLoaded();
    }
    
    /// <summary>
    /// 切换到指定场景（异步）
    /// </summary>
    public async void ChangeSceneAsync(string scenePath)
    {
        if (IsLoading) return;
        
        GD.Print($"[SceneManager] Changing scene async to: {scenePath}");
        OnSceneChanging?.Invoke(scenePath);
        
        TargetScenePath = scenePath;
        IsLoading = true;
        
        // 异步加载场景
        var loadTask = ResourceLoader.LoadThreadedRequest(scenePath);
        
        while (ResourceLoader.LoadThreadGetStatus(loadTask) == ResourceLoader.ThreadLoadStatus.InProgress)
        {
            LoadingProgress = ResourceLoader.LoadThreadGetProgress(loadTask);
            OnLoadingProgress?.Invoke(LoadingProgress);
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        }
        
        if (ResourceLoader.LoadThreadGetStatus(loadTask) == ResourceLoader.ThreadLoadStatus.Loaded)
        {
            var scene = ResourceLoader.LoadThreadGet(loadTask) as PackedScene;
            if (scene != null)
            {
                GetTree().CurrentScene?.QueueFree();
                _currentScene = scene;
                _currentScene.Instance();
                GetTree().CurrentScene = _currentScene.Instance();
                CurrentScenePath = scenePath;
            }
        }
        
        _OnSceneLoaded();
    }
    
    /// <summary>
    /// 切换到指定场景文件
    /// </summary>
    public void ChangeSceneToFile(PackedScene scene)
    {
        if (scene == null || IsLoading) return;
        
        GD.Print("[SceneManager] Changing scene to packed scene");
        GetTree().CurrentScene?.QueueFree();
        
        _currentScene = scene;
        var instance = scene.Instance();
        GetTree().CurrentScene = instance as Node;
        CurrentScenePath = scene.ResourcePath;
        
        _OnSceneLoaded();
    }
    
    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void ReloadCurrentScene()
    {
        if (!string.IsNullOrEmpty(CurrentScenePath))
        {
            ChangeScene(CurrentScenePath);
        }
    }
    
    /// <summary>
    /// 获取当前场景
    /// </summary>
    public Node GetCurrentScene()
    {
        return GetTree().CurrentScene;
    }
    
    /// <summary>
    /// 设置过渡模式
    /// </summary>
    public void SetTransitionMode(TransitionMode mode, float duration = 0.5f)
    {
        _transitionMode = mode;
        _transitionDuration = duration;
    }
    
    /// <summary>
    /// 场景加载完成回调
    /// </summary>
    private void _OnSceneLoaded()
    {
        IsLoading = false;
        LoadingProgress = 1f;
        
        GD.Print($"[SceneManager] Scene loaded: {CurrentScenePath}");
        
        // 触发本地事件
        OnSceneChanged?.Invoke(CurrentScenePath);
        
        // 通过事件总线发布全局事件
        if (EventBusManager.Instance != null)
        {
            EventBusManager.Instance.Emit(EventBusManager.Events.SceneChanged, CurrentScenePath);
        }
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "currentScenePath", CurrentScenePath }
        };
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("currentScenePath"))
            CurrentScenePath = data["currentScenePath"].ToString();
    }
}
