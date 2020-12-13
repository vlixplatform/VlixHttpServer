using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
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
        string _Host = "http://localhost:33171"; public string Host { get { return _Host; } set { SetField(ref _Host, value, "Host"); } }
        string _Username = "Administrator"; public string Username { get { return _Username; } set { SetField(ref _Username, value, "Username"); } }
        string _Password = null; public string Password { get { return _Password; } set { SetField(ref _Password, value, "Password"); } }
        bool _IsLoading = false; public bool IsLoading { get { return _IsLoading; } set { SetField(ref _IsLoading, value, "IsLoading"); } }
        bool _IsLoggingIn = false; public bool IsLoggingIn { get { return _IsLoggingIn; } set { SetField(ref _IsLoggingIn, value, "IsLoggingIn"); } }
        bool _ConfigEnableHTTPS = false; public bool ConfigEnableHTTPS { get { return _ConfigEnableHTTPS; } set { SetField(ref _ConfigEnableHTTPS, value, "ConfigEnableHTTPS"); } }
        int _ConfigHTTPPort = 33171; public int ConfigHTTPPort { get { return _ConfigHTTPPort; } set { SetField(ref _ConfigHTTPPort, value, "ConfigHTTPPort"); } }
        int _ConfigHTTPSPort = 33170; public int ConfigHTTPSPort { get { return _ConfigHTTPSPort; } set { SetField(ref _ConfigHTTPSPort, value, "ConfigHTTPSPort"); } }
        bool _ConfigLocalhostOnly = false; public bool ConfigLocalhostOnly { get { return _ConfigLocalhostOnly; } set { SetField(ref _ConfigLocalhostOnly, value, "ConfigLocalhostOnly"); } }
        string _ConfigUsername = "Administrator"; public string ConfigUsername { get { return _ConfigUsername; } set { SetField(ref _ConfigUsername, value, "ConfigUsername"); } }        
        string _ConfigPasswordHash = null; public string ConfigPasswordHash { get { return _ConfigPasswordHash; } set { SetField(ref _ConfigPasswordHash, value, "ConfigPasswordHash"); } }        
        
        string _ConfigSSLCertificateSubjectName = null; public string ConfigSSLCertificateSubjectName { get { return _ConfigSSLCertificateSubjectName; } set { SetField(ref _ConfigSSLCertificateSubjectName, value, "ConfigSSLCertificateSubjectName"); } }
        StoreName _ConfigSSLCertificateStoreName = StoreName.My; public StoreName ConfigSSLCertificateStoreName { get { return _ConfigSSLCertificateStoreName; } set { SetField(ref _ConfigSSLCertificateStoreName, value, "ConfigSSLCertificateStoreName"); } }
        public ObservableCollection<string> ConfigSubjectAlternativeNames { get; set; } = new ObservableCollection<string>();
        bool _ShowPasswordSettingsWindow = false; public bool ShowPasswordSettingsWindow { get { return _ShowPasswordSettingsWindow; } set { SetField(ref _ShowPasswordSettingsWindow, value, "ShowPasswordSettingsWindow"); } }
        bool _ShowConfigSelectSSLCertWindow = false; public bool ShowConfigSelectSSLCertWindow { get { return _ShowConfigSelectSSLCertWindow; } set { SetField(ref _ShowConfigSelectSSLCertWindow, value, "ShowConfigSelectSSLCertWindow"); } }
        bool _ShowLoginWindow = true; public bool ShowLoginWindow { get { return _ShowLoginWindow; } set { SetField(ref _ShowLoginWindow, value, "ShowLoginWindow"); } }
        public ICommand ChangePassswordClick { get { return new DelegateCommand<object>((p) => this.ShowPasswordSettingsWindow = true, (p) => true); } }
        public ICommand ConfigSelectSSLCertClick { get { return new DelegateCommand<object>((p) => this.ShowConfigSelectSSLCertWindow = true, (p) => true); } }
        public HttpServerConfigVM HttpServerConfigVM { get; set; } = null;
    }

}