using MaterialIcons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Vlix.HttpServer;

namespace Vlix.HttpServerConfig
{
    public partial class UCLogConsole : UserControl
    {
        public UCLogConsoleVM ucLogConsoleVM;
        public ScrollViewer sv = new ScrollViewer();
        readonly object PauseResumeLogLock = new object();

        public UCLogConsole()
        {
            InitializeComponent();
            InitializeUILogUpdate();
            ucLogConsoleVM = (UCLogConsoleVM)this.DataContext;

            
        }
        private void InUCLogConsole_Loaded(object sender, RoutedEventArgs e)
        {
            btnFilterButton.Content = this.FilterButtonApplyText;
        }

        #region DEPENDENCY PROPERTIES

        public bool ShowFilter { get { return (bool)GetValue(ShowFilterProperty); } set { SetValue(ShowFilterProperty, value); } }
        public static readonly DependencyProperty ShowFilterProperty = DependencyProperty.Register("ShowFilter", typeof(bool), typeof(UCLogConsole), new FrameworkPropertyMetadata(true));

        public bool ShowPause { get { return (bool)GetValue(ShowPauseProperty); } set { SetValue(ShowPauseProperty, value); } }
        public static readonly DependencyProperty ShowPauseProperty = DependencyProperty.Register("ShowPause", typeof(bool), typeof(UCLogConsole), new FrameworkPropertyMetadata(true));

        public string ClearButtonText { get { return (string)GetValue(ClearButtonTextProperty); } set { SetValue(ClearButtonTextProperty, value); } }
        public static readonly DependencyProperty ClearButtonTextProperty = DependencyProperty.Register("ClearButtonText", typeof(string), typeof(UCLogConsole), new FrameworkPropertyMetadata("Clear"));

        public string FilterButtonApplyText { get { return (string)GetValue(FilterButtonApplyTextProperty); } set { SetValue(FilterButtonApplyTextProperty, value); } }
        public static readonly DependencyProperty FilterButtonApplyTextProperty = DependencyProperty.Register("FilterButtonApplyText", typeof(string), typeof(UCLogConsole), new FrameworkPropertyMetadata("Apply"));

        public string FilterButtonClearText { get { return (string)GetValue(FilterButtonClearTextProperty); } set { SetValue(FilterButtonClearTextProperty, value); } }
        public static readonly DependencyProperty FilterButtonClearTextProperty = DependencyProperty.Register("FilterButtonClearText", typeof(string), typeof(UCLogConsole), new FrameworkPropertyMetadata("Reset"));

        public static readonly RoutedEvent OnClearLogsEvent = EventManager.RegisterRoutedEvent("OnClearLogs", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(UCLogConsole));
        public event RoutedEventHandler OnClearLogs
        {
            add { AddHandler(OnClearLogsEvent, value); }
            remove { RemoveHandler(OnClearLogsEvent, value); }
        }
        void RaiseOnClearLogsEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(UCLogConsole.OnClearLogsEvent);
            RaiseEvent(newEventArgs);
        }

        #endregion

        private void svConsole_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sV = (ScrollViewer)sender;
            this.sv = sV;

            if (e.ExtentHeightChange == 0) 
            {
                if (sv.VerticalOffset == sv.ScrollableHeight) ResumeLogger(); else PauseLogger(); // User scroll event : set or unset auto-scroll mode
            }

            if (!ucLogConsoleVM.ConsolePause && e.ExtentHeightChange != 0) // Content scroll event : auto-scroll when new logs added
            {
                sv.ScrollToBottom();
            }
        }


        private void Button_Click_PauseScrolling(object sender, RoutedEventArgs e)
        {
            //RESUME SCROLLING
            if (ucLogConsoleVM.ConsolePause)
            {
                ResumeLogger();
                if (ucLogConsoleVM.ConsoleLogsCollection.Count > 0) sv?.ScrollToBottom();
            }           
            else PauseLogger();
        }

        void ResumeLogger()
        {
            lock (PauseResumeLogLock)
            {
                //foreach (ConsoleLogVM CLVM in TempLogsDuringPause)
                //{
                //    this.ucLogConsoleVM.ConsoleLogsCollection.Add(CLVM);
                //    if (WildCardFilter!=null && WildCardFilter.IsMatch(CLVM.Entry)) this.ucLogConsoleVM.ConsoleFilteredLogsCollection.Add(CLVM);
                //}
                //TempLogsDuringPause.Clear();
                ucLogConsoleVM.ConsolePause = false;
                this.btnPauseResume.Content = "Pause";
                this.btnPauseResume.Icon = MaterialIconType.ic_pause;
            }
        }
        void PauseLogger()
        {
            lock (PauseResumeLogLock)
            {
                this.btnPauseResume.Content = "Resume";
                this.btnPauseResume.Icon = MaterialIconType.ic_play_arrow;
                ucLogConsoleVM.ConsolePause = true;
            }
        }


        public void ResetGUI(bool RemoveFilterIfApplied = true)
        {
            this.ucLogConsoleVM.ConsoleFilteredLogsCollection.Clear();
            this.ucLogConsoleVM.ConsoleLogsCollection.Clear();
            if (RemoveFilterIfApplied && ucLogConsoleVM.FilterApplied) Button_Filter(null, null);
            if (ucLogConsoleVM.ConsolePause)
            {
                //TempLogsDuringPause.Clear();
                this.UILogQueue.Clear();
                ucLogConsoleVM.ConsolePause = false;
                this.btnPauseResume.Content = "Pause";
                this.btnPauseResume.Icon = MaterialIconType.ic_pause;
                if (ucLogConsoleVM.ConsoleLogsCollection.Count > 0) sv?.ScrollToBottom();
            }
        }

        



        private void Button_Filter(object sender, RoutedEventArgs e)
        {
            lock (PauseResumeLogLock)
            {
                ucLogConsoleVM.FilterApplied = !ucLogConsoleVM.FilterApplied;
                if (ucLogConsoleVM.FilterApplied) //Show filtered logs
                {
                    tbFilter.IsEnabled = false;
                    btnFilterButton.Content = this.FilterButtonClearText; // "Clear Filter";
                    WildCardFilter = new Wildcard("*" + ucLogConsoleVM.FilterText + "*", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                    this.ucLogConsoleVM.ConsoleFilteredLogsCollection = new ObservableCollection<ConsoleLogVM>();
                    foreach (var log in this.ucLogConsoleVM.ConsoleLogsCollection) if (WildCardFilter.IsMatch(log.Entry)) this.ucLogConsoleVM.ConsoleFilteredLogsCollection.Add(log);
                    icLogViewer.ItemsSource = this.ucLogConsoleVM.ConsoleFilteredLogsCollection;
                }
                else // Show default logs
                {
                    tbFilter.IsEnabled = true;
                    btnFilterButton.Content = this.FilterButtonApplyText;
                    WildCardFilter = null;
                    ucLogConsoleVM.FilterText = "";
                    icLogViewer.ItemsSource = ucLogConsoleVM.ConsoleLogsCollection;
                }
            }
        }

        public void ClearLogConsole()
        {
            Button_Click_ClearConsole(null, null);
        }

        private void Button_Click_ClearConsole(object sender, RoutedEventArgs e)
        {
            ucLogConsoleVM.ConsoleLogsCollection.Clear();
            ucLogConsoleVM.ConsoleFilteredLogsCollection.Clear();
            this.RaiseOnClearLogsEvent();
        }

        private void ShowFull_MaterialIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is MaterialIcon MI && MI.DataContext is ConsoleLogVM CLVM)
            {
                CLVM.ShowPlusSign = false;
                CLVM.ShowMinusSign = true;
            }
        }

        private void HideFull_MaterialIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is MaterialIcon MI && MI.DataContext is ConsoleLogVM CLVM)
            {
                CLVM.ShowPlusSign = true;
                CLVM.ShowMinusSign = false;
            }
        }

    }
}
