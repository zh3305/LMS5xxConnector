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
            //全局异常拦截
            AppDomain.CurrentDomain.UnhandledException += (sender, e1) =>
            {
                Console.WriteLine(e1.ExceptionObject);
            };
        }
    }
}
