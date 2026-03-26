using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI;

/// <summary>
/// 调试控制台 UI
/// 提供游戏内命令输入和控制台输出
/// </summary>
public partial class DebugConsoleUI : Control
{
    public static DebugConsoleUI Instance { get; private set; }

    // 命令历史
    private List<string> _commandHistory = new List<string>();
    private int _historyIndex = -1;

    // UI 组件
    private LineEdit _commandInput;
    private RichTextLabel _outputLog;
    private PanelContainer _consolePanel;
    private VBoxContainer _contentContainer;
    private ScrollContainer _scrollContainer;

    // 样式
    private Color _bgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    private Color _inputBgColor = new Color(0.15f, 0.15f, 0.2f, 1.0f);
    private Color _textColor = new Color(0.85f, 0.85f, 0.9f, 1.0f);
    private Color _promptColor = new Color(0.3f, 0.8f, 0.3f, 1.0f);
    private Color _errorColor = new Color(1.0f, 0.4f, 0.4f, 1.0f);
    private Color _infoColor = new Color(0.4f, 0.7f, 1.0f, 1.0f);

    public override void _Ready()
    {
        Instance = this;
        SetupConsole();
        Hide();
    }

    private void SetupConsole()
    {
        // 主面板
        _consolePanel = new PanelContainer
        {
            Name = "ConsolePanel",
            ZIndex = 1000
        };
        _consolePanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_consolePanel);

        StyleBoxFlat bgStyle = new StyleBoxFlat
        {
            BgColor = _bgColor,
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            BorderColor = new Color(0.3f, 0.3f, 0.4f, 0.8f)
        };
        _consolePanel.AddThemeStyleboxOverride("panel", bgStyle);

        // 内容容器
        _contentContainer = new VBoxContainer
        {
            Name = "ContentContainer"
        };
        _contentContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _contentContainer.AddThemeConstantOverride("separation", 4);
        _consolePanel.AddChild(_contentContainer);

        // 输出日志（可滚动）
        _scrollContainer = new ScrollContainer
        {
            Name = "OutputScroll",
            SizeFlagsVertical = Control.SizeFlags.Expand
        };
        _contentContainer.AddChild(_scrollContainer);

        _outputLog = new RichTextLabel
        {
            Name = "OutputLog",
            SizeFlagsHorizontal = Control.SizeFlags.Expand,
            SizeFlagsVertical = Control.SizeFlags.Expand,
            BbcodeEnabled = true,
            ScrollFollowing = true,
            SelectionEnabled = false
        };
        _outputLog.AddThemeColorOverride("default_color", _textColor);
        _outputLog.AddThemeFontSizeOverride("normal_font_size", 14);
        _scrollContainer.AddChild(_outputLog);

        // 输入框容器（水平布局）
        HBoxContainer inputRow = new HBoxContainer
        {
            Name = "InputRow"
        };
        _contentContainer.AddChild(inputRow);

        // 命令提示符
        Label prompt = new Label
        {
            Text = "> ",
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin
        };
        prompt.AddThemeColorOverride("font_color", _promptColor);
        prompt.AddThemeFontSizeOverride("font_size", 16);
        inputRow.AddChild(prompt);

        // 命令输入框
        _commandInput = new LineEdit
        {
            Name = "CommandInput",
            PlaceholderText = "输入命令...",
            SizeFlagsHorizontal = Control.SizeFlags.Expand,
            ExpandToTextLength = true
        };
        _commandInput.AddThemeColorOverride("font_color", _textColor);
        _commandInput.AddThemeColorOverride("placeholder_color", new Color(0.5f, 0.5f, 0.55f, 1.0f));
        _commandInput.TextSubmitted += OnCommandSubmitted;
        _commandInput.PrependInputEvent += OnInputKeyPressed;
        inputRow.AddChild(_commandInput);

        // 初始提示
        PrintSystemLine("=== ClawRPG Debug Console ===");
        PrintSystemLine("按 /help 查看可用命令");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // ~ 键打开/关闭控制台
            if (keyEvent.Keycode == Key.BackQuote || keyEvent.Keycode == Key.Grave)
            {
                ToggleConsole();
                GetTree().SetInputAsHandled();
            }
        }
    }

    private void ToggleConsole()
    {
        if (Visible)
        {
            Hide();
            _commandInput.Text = "";
        }
        else
        {
            Show();
            _commandInput.GrabFocus();
            _commandInput.Text = "";
            _historyIndex = -1;
        }
    }

    private void OnCommandSubmitted(string text)
    {
        string cmd = text.Trim();
        if (string.IsNullOrEmpty(cmd))
            return;

        // 添加到历史
        _commandHistory.Add(cmd);
        _historyIndex = -1;

        // 显示命令
        PrintCommandLine(cmd);

        // 执行命令
        ExecuteCommand(cmd);

        _commandInput.Text = "";
    }

    private void OnInputKeyPressed(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // 上键：上一条历史
            if (keyEvent.Keycode == Key.Up)
            {
                NavigateHistory(-1);
                GetTree().SetInputAsHandled();
            }
            // 下键：下一条历史
            else if (keyEvent.Keycode == Key.Down)
            {
                NavigateHistory(1);
                GetTree().SetInputAsHandled();
            }
        }
    }

    private void NavigateHistory(int direction)
    {
        if (_commandHistory.Count == 0)
            return;

        _historyIndex += direction;

        if (_historyIndex >= _commandHistory.Count)
        {
            _historyIndex = _commandHistory.Count - 1;
        }
        else if (_historyIndex < -1)
        {
            _historyIndex = -1;
        }

        if (_historyIndex == -1)
        {
            _commandInput.Text = "";
        }
        else
        {
            _commandInput.Text = _commandHistory[_commandHistory.Count - 1 - _historyIndex];
            _commandInput.CaretColumn = _commandInput.Text.Length;
        }
    }

    private void ExecuteCommand(string input)
    {
        if (!input.StartsWith("/"))
        {
            PrintErrorLine("命令必须以 / 开头");
            return;
        }

        string[] parts = input.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        string cmdName = parts[0];
        string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

        var cmd = CommandRegistry.Instance.Get(cmdName);
        if (cmd != null)
        {
            try
            {
                cmd.Action.Invoke(args);
            }
            catch (Exception ex)
            {
                PrintErrorLine($"命令执行错误: {ex.Message}");
                MainDebug.ErrorPrint($"Console command '{cmdName}' failed: {ex}");
            }
        }
        else
        {
            PrintErrorLine($"未知命令: /{cmdName}，输入 /help 查看可用命令");
        }
    }

    /// <summary>
    /// 打印普通文本
    /// </summary>
    public void PrintLine(string text)
    {
        _outputLog.AppendText(text + "\n");
    }

    /// <summary>
    /// 打印带颜色的文本
    /// </summary>
    public void PrintLine(string text, Color color)
    {
        _outputLog.AppendText($"[color=#{color.ToHtml()}]{text}[/color]\n");
    }

    private void PrintCommandLine(string cmd)
    {
        _outputLog.AppendText($"[color=#{_promptColor.ToHtml()}]> {cmd}[/color]\n");
    }

    private void PrintSystemLine(string text)
    {
        _outputLog.AppendText($"[color=#{_infoColor.ToHtml()}]{text}[/color]\n");
    }

    private void PrintErrorLine(string text)
    {
        _outputLog.AppendText($"[color=#{_errorColor.ToHtml()}]{text}[/color]\n");
    }

    /// <summary>
    /// 清空控制台输出
    /// </summary>
    public void ClearOutput()
    {
        _outputLog.Clear();
    }
}
