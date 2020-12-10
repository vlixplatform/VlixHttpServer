using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Prism.Commands;

namespace Vlix.HttpServer
{
    public class ServerConfigVM : INotifyPropertyChanged
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
        bool _ConfigRemoteEnable = false; public bool ConfigRemoteEnable { get { return _ConfigRemoteEnable; } set { SetField(ref _ConfigRemoteEnable, value, "ConfigRemoteEnable"); } }
        string _ConfigRemoteUsername = null; public string ConfigRemoteUsername { get { return _ConfigRemoteUsername; } set { SetField(ref _ConfigRemoteUsername, value, "ConfigRemoteUsername"); } }
        public string ConfigRemotePassword { get; set; } = null;
        bool _ShowRemoteSettingsWindow = false; public bool ShowRemoteSettingsWindow { get { return _ShowRemoteSettingsWindow; } set { SetField(ref _ShowRemoteSettingsWindow, value, "ShowRemoteSettingsWindow"); } }
        bool _ShowLoginWindow = false; public bool ShowLoginWindow { get { return _ShowLoginWindow; } set { SetField(ref _ShowLoginWindow, value, "ShowLoginWindow"); } }
        public ICommand RemoteClickCommand { get { return new DelegateCommand<object>((p) => this.ShowRemoteSettingsWindow = true, (p) => true); } }
    }

}