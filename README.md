
# LMS5xxConnector

## 简介

`LMS5xxConnector` 是一个用于连接和操作 SICK LMS5xx 系列激光雷达的 .NET 库。它提供了简单易用的 API 来进行设备连接、数据获取和命令发送等功能。

## 安装

### NuGet 包管理器

您可以使用 NuGet 包管理器来安装 `LMS5xxConnector`：

```sh
Install-Package LMS5xxConnector
```

### .NET CLI

您也可以使用 .NET CLI 来安装：

```sh
dotnet add package LMS5xxConnector
```

### PackageReference

如果您使用的是 .NET Core 或 .NET 5+，可以在您的项目文件中添加以下引用：

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="LMS5xxConnector" Version="1.0.0" />
  </ItemGroup>

</Project>
```

## 使用说明

### 基本用法

以下是一个简单的示例，展示了如何使用 `LMS5xxConnector` 连接到激光雷达并获取设备信息：

```csharp
using System;
using System.Threading.Tasks;
using LMS5xxConnector;

class Program
{
    static async Task Main(string[] args)
    {
        // 创建 LMS5xxConnector 实例
        var connector = new Lms5XxConnector();

        try
        {
            // 连接到激光雷达
            await connector.ConnectAsync("192.168.1.231:2111");

            // 获取设备信息
            var deviceInfo = await connector.GetDeviceInfo();
            Console.WriteLine($"Device Name: {deviceInfo.Name}");
            Console.WriteLine($"Device Version: {deviceInfo.Version}");

            // 开始永久扫描
            await connector.StartScanData();

            // 停止永久扫描
            await connector.StopScanData();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // 断开连接
            connector.Disconnect();
        }
    }
}
```

### 高级用法

#### 数据接收处理

您可以注册数据接收处理回调，以便在接收到扫描数据时进行处理：

```csharp
connector.ReceivedHandles.AddOrUpdate(
    (CommandTypes.Ssn, Commands.LMDscandata),
    (key) => (content) => HandleScanData(content),
    (key, existing) => (content) => HandleScanData(content)
);

private void HandleScanData(TelegramContent content)
{
    if (content.Payload.CommandConnent is LMDscandataModeCommand scanData)
    {
        // 处理扫描数据
        Console.WriteLine($"Received scan data with {scanData.OutputChannelList[0].DistDatas.Count} points");
    }
}
```

#### 异常处理

在实际应用中，建议添加异常处理以确保程序的健壮性：

```csharp
try
{
    // 连接到激光雷达
    await connector.ConnectAsync("192.168.1.231:2111");
}
catch (FormatException ex)
{
    Console.WriteLine($"Invalid IP address format: {ex.Message}");
}
catch (SocketException ex)
{
    Console.WriteLine($"Network connection error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
finally
{
    // 断开连接
    connector.Disconnect();
}
```

## 示例项目

该项目包含一个示例 WPF 应用程序 `DistanceSensorApp`，展示了如何使用 `LMS5xxConnector` 进行设备连接、数据获取和命令发送。您可以参考 `TestFrom` 目录下的 `MainWindow.xaml` 和 `MainViewModel.cs` 文件了解详细实现。

## 贡献

欢迎贡献代码和提出改进建议！如果您有任何问题或建议，请通过 GitHub Issues 提交。

## 许可

本项目采用 MIT 许可证。详情请参见 [LICENSE](LICENSE) 文件。

## 联系我们
