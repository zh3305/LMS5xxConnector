using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DistanceSensorAppDemo;

namespace TestFrom
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                LogException("UI线程异常", e.Exception);
                MessageBox.Show($"发生了一个错误：{e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                LogException("异常处理器异常", ex);
                MessageBox.Show($"致命错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                LogException("非UI线程异常", ex);
                MessageBox.Show($"发生了一个未处理的错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                LogException("异常处理器异常", ex);
                MessageBox.Show($"致命错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                LogException("Task异常", e.Exception);
                MessageBox.Show($"Task中发生了一个错误：{e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                e.SetObserved();
            }
            catch (Exception ex)
            {
                LogException("异常处理器异常", ex);
                MessageBox.Show($"致命错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogException(string type, Exception ex)
        {
            Console.WriteLine($"\n=== {type} - {DateTime.Now} ===");
            Console.WriteLine($"异常类型: {ex.GetType().FullName}");
            Console.WriteLine($"异常消息: {ex.Message}");
            Console.WriteLine($"异常源: {ex.Source}");
            Console.WriteLine("堆栈跟踪:");
            Console.WriteLine(ex.StackTrace);

            // 输出内部异常信息（如果存在）
            if (ex.InnerException != null)
            {
                Console.WriteLine("\n内部异常:");
                Console.WriteLine($"类型: {ex.InnerException.GetType().FullName}");
                Console.WriteLine($"消息: {ex.InnerException.Message}");
                Console.WriteLine("堆栈跟踪:");
                Console.WriteLine(ex.InnerException.StackTrace);
            }

            Console.WriteLine("==========================================\n");
        }
    }
}
