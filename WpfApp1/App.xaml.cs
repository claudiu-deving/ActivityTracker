using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ProcessStartInfo activityTrackerProcess = new ProcessStartInfo()
            {
                FileName = @"C:\Users\CCS\source\repos\Tools\Activity Tracker\ActivityTracker\bin\Release\net8.0\ActivityTracker.exe",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden

            };
            Process.Start(activityTrackerProcess);

            ProcessStartInfo extensions = new ProcessStartInfo()
            {
                FileName = @"C:\Users\CCS\source\Tools\DEV\Sandbox\bin\Debug\net481\Sandbox.exe",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden

            };
            Process.Start(extensions);

            App.Current.Shutdown();
        }
    }
}
