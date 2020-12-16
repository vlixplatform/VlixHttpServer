using System;
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
        public void Update(WebServerConfig webServerConfig)
        {
            this.ConfigUsername = webServerConfig.UtilityConfig.ConfigUsername;
            this.ConfigPasswordHash = webServerConfig.UtilityConfig.ConfigPasswordHash;
            this.ConfigHTTPPort = webServerConfig.UtilityConfig.HTTPPort;
            this.ConfigLocalhostOnly = webServerConfig.UtilityConfig.AllowLocalhostConnectionsOnlyForHttp;
            this.ConfigEnableHTTPS = webServerConfig.UtilityConfig.EnableHTTPS;
            this.ConfigHTTPSPort = webServerConfig.UtilityConfig.HTTPSPort;
            this.ConfigSSLCertificateSubjectName = webServerConfig.UtilityConfig.SSLCertificateSubjectName;
            this.ConfigSSLCertificateStoreName = webServerConfig.UtilityConfig.SSLCertificateStoreName;

            //this.HttpServerConfigVM.Update(webServerConfig);
        }

        //For Login Window
        string _Host = "http://localhost:33171"; public string Host { get { return _Host; } set { SetField(ref _Host, value, "Host"); } }
        string _Username = "Administrator"; public string Username { get { return _Username; } set { SetField(ref _Username, value, "Username"); } }
        string _Password = null; public string Password { get { return _Password; } set { SetField(ref _Password, value, "Password"); } }
        bool _IsLoading = false; public bool IsLoading { get { return _IsLoading; } set { SetField(ref _IsLoading, value, "IsLoading"); } }
        bool _IsLoggingIn = false; public bool IsLoggingIn { get { return _IsLoggingIn; } set { SetField(ref _IsLoggingIn, value, "IsLoggingIn"); } }



        // For Config Setup Window
        public UtilityConfig ToUtilityConfig(bool changePassword,string newPasswordHash)
        {
            return new UtilityConfig()
            {
                AllowLocalhostConnectionsOnlyForHttp = NewConfigLocalhostOnly,
                ConfigPasswordHash = changePassword? newPasswordHash : ConfigPasswordHash,
                ConfigUsername = NewConfigUsername,
                SSLCertificateStoreName = NewConfigSSLCertificateStoreName,
                SSLCertificateSubjectName = NewConfigSSLCertificateSubjectName,
                EnableHTTPS = NewConfigEnableHTTPS,
                HTTPSPort = NewConfigHTTPSPort,
                HTTPPort = NewConfigHTTPPort
            };
        }
        string _NewConfigUsername = "Administrator"; public string NewConfigUsername { get { return _NewConfigUsername; } set { SetField(ref _NewConfigUsername, value, "NewConfigUsername"); } }
        int _NewConfigHTTPPort = 33171; public int NewConfigHTTPPort { get { return _NewConfigHTTPPort; } set { SetField(ref _NewConfigHTTPPort, value, "NewConfigHTTPPort"); } }
        bool _NewConfigLocalhostOnly = false; public bool NewConfigLocalhostOnly { get { return _NewConfigLocalhostOnly; } set { SetField(ref _NewConfigLocalhostOnly, value, "NewConfigLocalhostOnly"); } }
        bool _NewConfigEnableHTTPS = false; public bool NewConfigEnableHTTPS { get { return _NewConfigEnableHTTPS; } set { SetField(ref _NewConfigEnableHTTPS, value, "NewConfigEnableHTTPS"); } }
        int _NewConfigHTTPSPort = 33170; public int NewConfigHTTPSPort { get { return _NewConfigHTTPSPort; } set { SetField(ref _NewConfigHTTPSPort, value, "NewConfigHTTPSPort"); } }
        string _NewConfigSSLCertificateSubjectName = null; public string NewConfigSSLCertificateSubjectName { get { return _NewConfigSSLCertificateSubjectName; } set { SetField(ref _NewConfigSSLCertificateSubjectName, value, "NewConfigSSLCertificateSubjectName"); } }
        StoreName _NewConfigSSLCertificateStoreName = StoreName.My; public StoreName NewConfigSSLCertificateStoreName { get { return _NewConfigSSLCertificateStoreName; } set { SetField(ref _NewConfigSSLCertificateStoreName, value, "NewConfigSSLCertificateStoreName"); } }
        public ObservableCollection<string> NewConfigSubjectAlternativeNames { get; set; } = new ObservableCollection<string>();


        //Current Settings
        public string ConfigUsername { get; set; } = "Administrator";
        public string ConfigPasswordHash { get; set; } = null;
        public int ConfigHTTPPort { get; set; } = 33171;
        public bool ConfigLocalhostOnly { get; set; } = false;
        public bool ConfigEnableHTTPS { get; set; } = false;
        public int ConfigHTTPSPort { get; set; } = 33170;
        public string ConfigSSLCertificateSubjectName { get; set; } = null;
        public StoreName ConfigSSLCertificateStoreName { get; set; } = StoreName.My;
        public ObservableCollection<string> ConfigSubjectAlternativeNames { get; set; } = new ObservableCollection<string>();




        //Other Flow Controls
        bool _ShowConfigSettingsWindow = false; public bool ShowConfigSettingsWindow { get { return _ShowConfigSettingsWindow; } set { SetField(ref _ShowConfigSettingsWindow, value, "ShowConfigSettingsWindow"); } }
        bool _ShowConfigSelectSSLCertWindow = false; public bool ShowConfigSelectSSLCertWindow { get { return _ShowConfigSelectSSLCertWindow; } set { SetField(ref _ShowConfigSelectSSLCertWindow, value, "ShowConfigSelectSSLCertWindow"); } }
        bool _ShowLoginWindow = true; public bool ShowLoginWindow { get { return _ShowLoginWindow; } set { SetField(ref _ShowLoginWindow, value, "ShowLoginWindow"); } }
        bool _LoggedIn = false; public bool LoggedIn { get { return _LoggedIn; } set { SetField(ref _LoggedIn, value, "LoggedIn"); } }
        bool _IsSaving = false; public bool IsSaving { get { return _IsSaving; } set { SetField(ref _IsSaving, value, "IsSaving"); } }
        public ICommand SettingsLaunchClick { get { return new DelegateCommand<object>((p) => this.ShowConfigSettingsWindow = true, (p) => true); } }
        public ICommand ConfigSelectSSLCertClick { get { return new DelegateCommand<object>((p) => this.ShowConfigSelectSSLCertWindow = true, (p) => true); } }
        public ICommand ExitClick { get { return new DelegateCommand<object>((p) => Environment.Exit(0), (p) => true); } }
        public ICommand DisconnectClick { get { return new DelegateCommand<object>((p) => { this.LoggedIn = false; this.ShowLoginWindow = true; this.ShowConfigSelectSSLCertWindow = false; this.ShowConfigSettingsWindow = false; }, (p) => true) ; } }

    }

}