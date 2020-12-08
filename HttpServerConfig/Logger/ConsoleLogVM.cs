using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Vlix.HttpServer;

namespace Vlix.HttpServerConfig
{
    public class ConsoleLogVM : INotifyPropertyChanged
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

        //public ConsoleLogVM(ConsoleLog ConsoleLog)
        //{
        //    this.Entry = ConsoleLog.Entry;
        //    this.TSUTC = ConsoleLog.TSUTC;
        //    this.TSLocal = TSUTC.ToLocalTime();
        //    ProcessEntry();
        //}

        public ConsoleLogVM(LogStruct Log)
        {
            this.Entry = Log.LogEntry;
            this.TSUTC = Log.TimeStampInUTC;
            this.TSLocal = TSUTC.ToLocalTime();
            ProcessEntry();
        }

        public ConsoleLogVM()
        {
            this.Entry = "";
            this.TSUTC = DateTime.MinValue;
            this.TSLocal = TSUTC.ToLocalTime();
            ProcessEntry();
        }

        public ConsoleLogVM(DateTime TSUTC, string Entry)
        {
            this.Entry = Entry;
            this.TSUTC = TSUTC;
            this.TSLocal = TSUTC.ToLocalTime();
            ProcessEntry();
        }

        void ProcessEntry()
        {
            if (Entry != null && Entry.Contains("\r\n"))
            {
                _ShowPlusSign = true;
                EntrySingleLine = Entry.Split(new string[] { "\r\n" }, StringSplitOptions.None)[0];
            }
            else
            {
                _ShowPlusSign = false;
                EntrySingleLine = Entry;
            }
        }
        public string Entry { get; } = "";
        public DateTime TSUTC { get; }
        public DateTime TSLocal { get; }
        public string EntrySingleLine { get; private set; } = "";
        bool _ShowPlusSign; public bool ShowPlusSign { get { return _ShowPlusSign; } set { SetField(ref _ShowPlusSign, value, "ShowPlusSign"); } }        
        bool _ShowMinusSign; public bool ShowMinusSign { get { return _ShowMinusSign; } set { SetField(ref _ShowMinusSign, value, "ShowMinusSign"); } }        
    }

}
