using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Vlix.HttpServerConfig
{
    public class UCLogConsoleVM : INotifyPropertyChanged
    {
        #region BOILER PLATE
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
        
        public UCLogConsoleVM()
        {
        }
        

        bool _ConsolePause = false;
        public bool ConsolePause
        {
            get { return _ConsolePause; }
            set { SetField(ref _ConsolePause, value, "ConsolePause"); }
        }
        
        
        public ObservableCollection<ConsoleLogVM> ConsoleLogsCollection { get; set; } = new ObservableCollection<ConsoleLogVM>();
        public ObservableCollection<ConsoleLogVM> ConsoleFilteredLogsCollection { get; set; } = new ObservableCollection<ConsoleLogVM>();
        string _FilterText = ""; public string FilterText { get { return _FilterText; } set { SetField(ref _FilterText, value, "FilterText"); } }        
        bool _FilterApplied = false; public bool FilterApplied { get { return _FilterApplied; } set { SetField(ref _FilterApplied, value, "FilterApplied"); } }
        bool _ShowTimeStamp = true; public bool ShowTimeStamp { get { return _ShowTimeStamp; } set { SetField(ref _ShowTimeStamp, value, "ShowTimeStamp"); } }
        


    }
}
