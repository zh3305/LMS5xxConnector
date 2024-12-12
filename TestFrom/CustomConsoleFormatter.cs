using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace DistanceSensorAppDemo;

public class CustomConsoleFormatter : ConsoleFormatter
{
    private readonly ConsoleFormatterOptions _options;

    public CustomConsoleFormatter(IOptions<ConsoleFormatterOptions> options)
        : base("CustomFormatter")
    {
        _options = options.Value;
    }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        // 获取时间戳
        string timestamp = DateTime.Now.ToString(_options.TimestampFormat);

        // 获取日志级别对应的颜色和前缀
        (ConsoleColor color, string prefix) = GetLogLevelInfo(logEntry.LogLevel);

        // 保存当前控制台颜色
        var originalColor = Console.ForegroundColor;

        // 设置新的颜色
        Console.ForegroundColor = color;

        // 构建并写入日志消息
        string message = logEntry.Formatter!(logEntry.State, logEntry.Exception);
        textWriter.WriteLine($"[{timestamp}] {prefix} {message}");

        // 如果有异常，写入异常信息
        if (logEntry.Exception != null)
        {
            textWriter.WriteLine($"      异常: {logEntry.Exception.Message}");
            textWriter.WriteLine($"      堆栈: {logEntry.Exception.StackTrace}");
        }

        // 恢复原始颜色
        Console.ForegroundColor = originalColor;
    }

    private static (ConsoleColor color, string prefix) GetLogLevelInfo(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => (ConsoleColor.Gray, "[TRACE]"),
            LogLevel.Debug => (ConsoleColor.DarkGray, "[DEBUG]"),
            LogLevel.Information => (ConsoleColor.Green, "[INFO ]"),
            LogLevel.Warning => (ConsoleColor.Yellow, "[WARN ]"),
            LogLevel.Error => (ConsoleColor.Red, "[ERROR]"),
            LogLevel.Critical => (ConsoleColor.DarkRed, "[FATAL]"),
            _ => (ConsoleColor.White, "[     ]")
        };
    }
}