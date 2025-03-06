using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LMS5xxConnector.Telegram;

namespace LMS5xxConnector
{
    /// <summary>
    /// LMS5xx 雷达设备连接器接口，定义了与雷达设备通信的基本操作
    /// </summary>
    public interface ILms5XxConnector : IDisposable
    {
        /// <summary>
        /// 获取连接超时时间（毫秒）
        /// </summary>
        int ConnectTimeout { get; set; }

        /// <summary>
        /// 获取读取超时时间（毫秒）
        /// </summary>
        int ReadTimeout { get; set; }

        /// <summary>
        /// 获取写入超时时间（毫秒）
        /// </summary>
        int WriteTimeout { get; set; }

        /// <summary>
        /// 获取是否启用调试模式
        /// </summary>
        bool IsDebug { get; set; }

        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 连接到本地回环地址的默认端口（2111）
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        Task ConnectAsync();

        /// <summary>
        /// 连接到指定的远程终端点（字符串格式）
        /// </summary>
        /// <param name="remoteEndpoint">远程终端点字符串，格式：IP:端口，例如："192.168.0.1:2111"</param>
        /// <returns>表示异步操作的任务</returns>
        Task ConnectAsync(string remoteEndpoint);

        /// <summary>
        /// 连接到指定的远程终端点
        /// </summary>
        /// <param name="remoteEndpoint">远程设备的IP地址和端口</param>
        /// <returns>表示异步操作的任务</returns>
        Task ConnectAsync(IPEndPoint remoteEndpoint);

        /// <summary>
        /// 断开与设备的连接
        /// </summary>
        void Disconnect();

        // /// <summary>
        // /// 获取设备信息
        // /// </summary>
        // /// <returns>设备标识信息，如果获取失败则返回null</returns>
        // Task<object?> GetDeviceInfo();

        /// <summary>
        /// 当接收到数据时触发的事件
        /// </summary>
        event EventHandler<DataReceivedEventArgs>? DataReceived;
    }
}