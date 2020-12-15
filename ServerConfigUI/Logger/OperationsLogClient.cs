using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Vlix.HttpServer;

namespace Vlix.ServerConfigUI
{
    public partial class UCLogConsole : UserControl
    {
        Wildcard WildCardFilter;
        //This sub adds the logs to the UI in bulk to prevent the UI from over procissing
        public ConcurrentQueue<ConsoleLogVM> UILogQueue;

        public void Refresh()
        {
            //if (_IsRefreshing) return;
            //_IsRefreshing = true;
            using (this.Dispatcher.DisableProcessing())
            {
                lock (PauseResumeLogLock)
                {
                    if (!ucLogConsoleVM.ConsolePause)
                    {
                        if (this.ucLogConsoleVM.FilterApplied)
                        {
                            while (UILogQueue.Count > 0)
                            {
                                if (UILogQueue.TryDequeue(out ConsoleLogVM log))
                                {
                                    this.ucLogConsoleVM.ConsoleLogsCollection.Add(log);
                                    if (WildCardFilter.IsMatch(log.Entry)) this.ucLogConsoleVM.ConsoleFilteredLogsCollection?.Add(log);
                                }
                            }
                        }
                        else while (UILogQueue.Count > 0) 
                                if (UILogQueue.TryDequeue(out ConsoleLogVM log)) 
                                    this.ucLogConsoleVM.ConsoleLogsCollection.Add(log);
                    }
                }
                //_IsRefreshing = false;
            }
        }

        private bool _IsRefreshing = false;
        public void InitializeUILogUpdate()
        {
            UILogQueue = new ConcurrentQueue<ConsoleLogVM>();
            new Thread(() => 
            {
                AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                while (true)
                {
                    if (!_IsRefreshing)
                    {
                        _IsRefreshing = true;
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            using (this.Dispatcher.DisableProcessing())
                            {
                                lock (PauseResumeLogLock)
                                {
                                    if (!ucLogConsoleVM.ConsolePause)
                                    {
                                        if (this.ucLogConsoleVM.FilterApplied)
                                        {
                                            while (UILogQueue.Count > 0)
                                            {
                                                if (UILogQueue.TryDequeue(out ConsoleLogVM log))
                                                {
                                                    this.ucLogConsoleVM.ConsoleLogsCollection.Add(log);
                                                    if (WildCardFilter.IsMatch(log.Entry)) this.ucLogConsoleVM.ConsoleFilteredLogsCollection?.Add(log);
                                                }
                                            }
                                        }
                                        else while (UILogQueue.Count > 0) if (UILogQueue.TryDequeue(out ConsoleLogVM log)) this.ucLogConsoleVM.ConsoleLogsCollection.Add(log);
                                    }
                                    autoResetEvent.Set();
                                }
                            }
                        }));
                        autoResetEvent.WaitOne();
                        _IsRefreshing = false;
                    }
                    Thread.Sleep(500);
                }
            }).Start();
            
            
        }

        public void ClearLogs() {
            Button_Click_ClearConsole(null, null);
        }
        

        //***********************************
        //Add Logs
        //***********************************        
        public void AddLogs(IEnumerable<ConsoleLogVM> Logs)
        {
            foreach (ConsoleLogVM C in Logs) this.UILogQueue.Enqueue(C);
        }
        //public void AddLogs(List<ConsoleLog> Logs)
        //{
        //    List<ConsoleLogVM> ConsoleLogVMs = new List<ConsoleLogVM>();
        //    foreach (var log in Logs) ConsoleLogVMs.Add(new ConsoleLogVM(log.TSUTC, log.Entry));
        //    this.AddLogs(ConsoleLogVMs);
        //}
        public void AddLogs(List<LogStruct> Logs)
        {
            if (Logs == null) return;
            List<ConsoleLogVM> ConsoleLogVMs = new List<ConsoleLogVM>();
            foreach (var log in Logs) ConsoleLogVMs.Add(new ConsoleLogVM(log));
            this.AddLogs(ConsoleLogVMs);
        }
        public void AddLog(ConsoleLogVM Log) { AddLogs(new List<ConsoleLogVM>() { Log }); }
        //public void AddLog(ConsoleLog Log) { this.AddLogs(new List<ConsoleLog>() { Log }); }
        //public void AddLog(LogStruct Log) { this.AddLogs(new List<LogStruct>() { Log }); }

    }
}