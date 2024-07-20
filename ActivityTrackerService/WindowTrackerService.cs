using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
#nullable enable
namespace ActivityTrackerService
{
    public partial class WindowTrackerService : ServiceBase
    {
        private class WindowTimeEntry
        {
            public string WindowName { get; set; }
            public DateTime StartTime { get; set; }
            public TimeSpan Duration { get; set; }
        }
        private List<WindowTimeEntry> _windowTimeEntries;
        private DateTime _lastTime;
        private string _lastWindow;
        public WindowTrackerService()
        {
            _windowTimeEntries = [];
            _lastWindow = "";
        }

        protected override void OnStart(string[] args)
        {

            _lastTime = DateTime.Now;

            Task.Run(() => StartPipeServer());
        }

        protected override void OnStop()
        {
            FileLogger.Log("Service stopped");
        }

        private void StartPipeServer()
        {
            FileLogger.Log("Starting server");
            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.SetAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                PipeAccessRights.FullControl, AccessControlType.Allow));

            while (true)
            {
                using NamedPipeServerStream pipeServer = new NamedPipeServerStream(
                    "WindowPipe", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, pipeSecurity);
                try
                {
                    pipeServer.WaitForConnection();

                    using StreamReader reader = new(pipeServer);
                    string currentWindow = reader.ReadLine();
                    FileLogger.Log($"Window: {currentWindow}");
                    ProcessWindowInfo(currentWindow);
                }
                catch (Exception ex)
                {
                    FileLogger.Log(ex.Message);
                }
            }
        }

        private void ProcessWindowInfo(string currentWindow)
        {
            DateTime currentTime = DateTime.Now;

            if (currentWindow != _lastWindow)
            {
                if (!string.IsNullOrEmpty(_lastWindow))
                {
                    TimeSpan timeSpent = currentTime - _lastTime;
                    _windowTimeEntries.Add(new WindowTimeEntry
                    {
                        WindowName = _lastWindow,
                        StartTime = _lastTime,
                        Duration = timeSpent
                    });
                }

                _lastWindow = currentWindow;
                _lastTime = currentTime;
            }

            SaveWindowTimes();
        }

        private void SaveWindowTimes()
        {
            try
            {
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string serviceDataPath = Path.Combine(localAppDataPath, "MyServiceData");

                if (!Directory.Exists(serviceDataPath))
                {
                    Directory.CreateDirectory(serviceDataPath);
                }

                string json = JsonConvert.SerializeObject(_windowTimeEntries, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(Path.Combine(serviceDataPath, "window_times.json"), json);
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex.Message);
            }
        }
    }
}
public static class FileLogger
{

    public static void Log(string message)
    {
        try
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string serviceDataPath = Path.Combine(localAppDataPath, "MyServiceData");
            string logFilePath = Path.Combine(serviceDataPath, "service.log");
            if (!Directory.Exists(serviceDataPath))
            {
                Directory.CreateDirectory(serviceDataPath);
            }
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions silently or log to Event Log as a fallback
            EventLog.WriteEntry("MyServiceSource", $"Exception in FileLogger.Log: {ex.Message}", EventLogEntryType.Error);
        }
    }
}
