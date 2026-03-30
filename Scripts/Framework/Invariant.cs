namespace ClawRPG.Scripts.Framework
{

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

/// <summary>
/// 不变量断言系统
/// 在 DEBUG 编译时强制检测运行时违规
/// RELEASE 编译时自动剔除所有断言代码
/// </summary>
public static class Invariant
{
    /// <summary>
    /// 断言系统全局开关（默认跟随 DEBUG 编译符号）
    /// </summary>
    public static bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }
    private static bool _enabled =
#if DEBUG
        true;
#else
        false;
#endif

    /// <summary>
    /// 是否在断言失败时抛出异常（默认 false，不打断游戏）
    /// </summary>
    public static bool ThrowOnViolation { get; set; } = false;

    private static readonly List<Violation> _violations = new();
    private static readonly object _lock = new();

    /// <summary>
    /// 当前 Violation 数量
    /// </summary>
    public static int ViolationCount
    {
        get
        {
            lock (_lock) return _violations.Count;
        }
    }

    /// <summary>
    /// 获取所有 Violation 的只读快照
    /// </summary>
    public static IReadOnlyList<Violation> GetViolations()
    {
        lock (_lock) return _violations.ToArray();
    }

    /// <summary>
    /// 清除所有 Violation 记录
    /// </summary>
    public static void ClearViolations()
    {
        lock (_lock) _violations.Clear();
    }

    /// <summary>
    /// 核心断言方法
    /// </summary>
    /// <param name="condition">必须为 true，否则记录 violation</param>
    /// <param name="message">违规描述，支持格式化参数</param>
    /// <param name="args">格式化参数</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Assert(bool condition, string message, params object[] args)
    {
#if DEBUG
        if (!condition && Enabled)
        {
            string fullMessage = args.Length > 0
                ? string.Format(message, args)
                : message;

            var violation = new Violation(fullMessage, CreateCallSite());
            RecordViolation(violation);

            if (ThrowOnViolation)
            {
                throw new InvariantViolationException(violation);
            }
        }
#endif
    }

    /// <summary>
    /// 范围断言 — value 必须在 [min, max] 范围内
    /// </summary>
    public static void AssertRange<T>(T value, T min, T max, string name)
        where T : IComparable<T>
    {
#if DEBUG
        if (!Enabled) return;

        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            string msg = $"{name} out of range: {value} not in [{min}, {max}]";
            var violation = new Violation(msg, CreateCallSite());
            RecordViolation(violation);

            if (ThrowOnViolation)
            {
                throw new InvariantViolationException(violation);
            }
        }
#endif
    }

    /// <summary>
    /// 断言两个值相等（用于检测预期外的状态漂移）
    /// </summary>
    public static void AssertEqual<T>(T expected, T actual, string name)
    {
#if DEBUG
        if (!Enabled) return;

        bool equal = EqualityComparer<T>.Default.Equals(expected, actual);
        if (!equal)
        {
            string msg = $"{name} mismatch: expected {expected}, got {actual}";
            var violation = new Violation(msg, CreateCallSite());
            RecordViolation(violation);

            if (ThrowOnViolation)
            {
                throw new InvariantViolationException(violation);
            }
        }
#endif
    }

    /// <summary>
    /// 断言对象非空
    /// </summary>
    public static void AssertNotNull(object obj, string name)
    {
#if DEBUG
        if (!Enabled) return;

        if (obj == null)
        {
            string msg = $"{name} is null — unexpected null reference";
            var violation = new Violation(msg, CreateCallSite());
            RecordViolation(violation);

            if (ThrowOnViolation)
            {
                throw new InvariantViolationException(violation);
            }
        }
#endif
    }

    // --- 私有工具方法 ---

    private static CallSite CreateCallSite()
    {
        var stackTrace = new StackTrace(2, true); // skip 2 frames
        var frames = stackTrace.GetFrames();

        string filePath = "unknown";
        int lineNumber = 0;
        string methodName = "unknown";

        if (frames != null && frames.Length > 0)
        {
            var frame = frames[0];
            var method = frame.GetMethod();
            if (method != null)
            {
                methodName = method.Name;
                var declaringType = method.DeclaringType;
                if (declaringType != null)
                {
                    methodName = $"{declaringType.Name}.{methodName}";
                }
            }
            filePath = frame.GetFileName() ?? "unknown";
            lineNumber = frame.GetFileLineNumber();
        }

        return new CallSite(filePath, lineNumber, methodName, (int)Godot.Engine.GetProcessFrames());
    }

    private static void RecordViolation(Violation violation)
    {
        string logMsg = $"[INVARIANT VIOLATION] {violation.Message} @ {violation.CallSite.FileName}:{violation.CallSite.LineNumber} ({violation.CallSite.MethodName}, frame={violation.CallSite.GameFrame})";
        GD.PushWarning(logMsg);

        lock (_lock)
        {
            _violations.Add(violation);
        }
    }
}

/// <summary>
/// 记录一次不变量违规
/// </summary>
public readonly struct Violation
{
    public string Message { get; }
    public CallSite CallSite { get; }
    public long Timestamp { get; }
    public Dictionary<string, string> Context { get; }

    public Violation(string message, CallSite callSite)
    {
        Message = message;
        CallSite = callSite;
        Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Context = new Dictionary<string, string>();
    }

    /// <summary>
    /// 添加上下文键值对（链式调用）
    /// </summary>
    public Violation WithContext(string key, string value)
    {
        var copy = this;
        copy.Context[key] = value;
        return copy;
    }
}

/// <summary>
/// 断言失败抛出的异常类型
/// </summary>
public class InvariantViolationException : Exception
{
    public Violation Violation { get; }

    public InvariantViolationException(Violation violation) : base(violation.Message)
    {
        Violation = violation;
    }
}

/// <summary>
/// 调用位置信息
/// </summary>
public readonly struct CallSite
{
    public string FileName { get; }
    public int LineNumber { get; }
    public string MethodName { get; }
    public int GameFrame { get; }

    public CallSite(string fileName, int lineNumber, string methodName, int gameFrame)
    {
        FileName = fileName;
        LineNumber = lineNumber;
        MethodName = methodName;
        GameFrame = gameFrame;
    }

    public override string ToString() => $"{FileName}:{LineNumber} ({MethodName})";
}

}
