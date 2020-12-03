using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Prism.Commands;
using System.Security.Cryptography.X509Certificates;

namespace Vlix.HttpServer
{
    public class HttpServerConfigVM : INotifyPropertyChanged
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
        HttpServerConfig configBeforeChange;
        public HttpServerConfigVM()
        {
            RuleVM sss = new RuleVM(new HttpToHttpsRedirectRule() { Name = "Rule 1" }, this);
            sss.ResponseAction.ActionType = ActionType.AlternativeWWWDirectory;
            this.Rules.Add(sss);
            this.Rules.Add(new RuleVM(new HttpToHttpsRedirectRule() { Name = "Rule 2", Enable=false }, this));
            this.Rules.Add(new RuleVM(new SimpleHostNameRedirectRule("cat","dog") { Name = "Rule 3" }, this));
            this.Rules.Add(new RuleVM(new HttpToHttpsRedirectRule() { Name = "Rule 4", Enable = true }, this));
            this.Rules.Add(new RuleVM(new HttpToHttpsRedirectRule() { Name = "Rule 5", Enable = false }, this));
            this.SelectSSLCertVM = new SelectSSLCertVM(this);

        }

        public async Task LoadVM()
        {
            this.IsLoading = true;
            await Task.Delay(500);
            HttpServerConfig configBeforeChange = await Services.LoadHttpServerConfig();
            this.EnableHTTP = configBeforeChange.EnableHTTP;
            this.HTTPPort = configBeforeChange.HTTPPort;
            this.EnableHTTPS = configBeforeChange.EnableHTTPS;
            this.HTTPSPort = configBeforeChange.HTTPSPort;
            this.EnableCache = configBeforeChange.EnableCache;
            this.OnlyCacheItemsLessThenMB = configBeforeChange.OnlyCacheItemsLessThenMB;
            this.MaximumCacheSizeInMB = configBeforeChange.MaximumCacheSizeInMB;
            this.SSLCertificateStoreName = configBeforeChange.SSLCertificateStoreName;
            this.SSLCertificateSubjectName = configBeforeChange.SSLCertificateSubjectName;
            this.WWWDirectory = configBeforeChange.WWWDirectory;
            this.LogDirectory = configBeforeChange.LogDirectory;
            this.AllowLocalhostConnectionsOnly = configBeforeChange.AllowLocalhostConnectionsOnly;
            this.Rules.Clear();
            foreach (var rule in configBeforeChange.Rules) this.Rules.Add(new RuleVM(rule, this));
            await Task.Delay(500);
            this.IsLoading = false;
        }
        bool _IsLoading = false; public bool IsLoading { get { return _IsLoading; } set { SetField(ref _IsLoading, value, "IsLoading"); } }
        bool _ClientIsLocal = true; public bool ClientIsLocal { get { return _ClientIsLocal; } set { SetField(ref _ClientIsLocal, value, "ClientIsLocal"); } }
        string _SSLCertificateSubjectName = null; public string SSLCertificateSubjectName { get { return _SSLCertificateSubjectName; } set { SetField(ref _SSLCertificateSubjectName, value, "SSLCertificateSubjectName"); } }
        StoreName _SSLCertificateStoreName = StoreName.My; public StoreName SSLCertificateStoreName { get { return _SSLCertificateStoreName; } set { SetField(ref _SSLCertificateStoreName, value, "SSLCertificateStoreName"); } }
        public ObservableCollection<string> SubjectAlternativeNames { get; set; } = new ObservableCollection<string>();
        string _WWWDirectory = Path.Combine("[ProgramDataDirectory]","Vlix","HTTPServer","www"); public string WWWDirectory { get { return _WWWDirectory; } set { SetField(ref _WWWDirectory, value, "WWWDirectory"); } }
        string _LogDirectory = Path.Combine("[ProgramDataDirectory]", "Vlix", "HTTPServer", "Logs"); public string LogDirectory { get { return _LogDirectory; } set { SetField(ref _LogDirectory, value, "LogDirectory"); } }
        bool _EnableHTTP = true; public bool EnableHTTP { get { return _EnableHTTP; } set { SetField(ref _EnableHTTP, value, "EnableHTTP"); } }        
        int _HTTPPort = 80; public int HTTPPort { get { return _HTTPPort; } set { SetField(ref _HTTPPort, value, "HTTPPort"); } }
        bool _EnableHTTPS = true; public bool EnableHTTPS { get { return _EnableHTTPS; } set { SetField(ref _EnableHTTPS, value, "EnableHTTPS"); } }
        int _HTTPSPort = 443; public int HTTPSPort { get { return _HTTPSPort; } set { SetField(ref _HTTPSPort, value, "HTTPSPort"); } }        
        bool _AllowLocalhostConnectionsOnly = false; public bool AllowLocalhostConnectionsOnly { get { return _AllowLocalhostConnectionsOnly; } set { SetField(ref _AllowLocalhostConnectionsOnly, value, "AllowLocalhostConnectionsOnly"); } }
        bool _ShowSelectSSLCertWindow = false; public bool ShowSelectSSLCertWindow { get { return _ShowSelectSSLCertWindow; } set { SetField(ref _ShowSelectSSLCertWindow, value, "ShowSelectSSLCertWindow"); } }
        bool _EnableCache = false; public bool EnableCache { get { return _EnableCache; } set { SetField(ref _EnableCache, value, "EnableCache"); } }
        bool _ShowDisabledRules = true; public bool ShowDisabledRules { get { return _ShowDisabledRules; } set { SetField(ref _ShowDisabledRules, value, "ShowDisabledRules"); } }
        int _OnlyCacheItemsLessThenMB = 10; public int OnlyCacheItemsLessThenMB { get { return _OnlyCacheItemsLessThenMB; } set { SetField(ref _OnlyCacheItemsLessThenMB, value, "OnlyCacheItemsLessThenMB"); } }
        int _MaximumCacheSizeInMB = 250; public int MaximumCacheSizeInMB { get { return _MaximumCacheSizeInMB; } set { SetField(ref _MaximumCacheSizeInMB, value, "MaximumCacheSizeInMB"); } }
        SelectSSLCertVM _SelectSSLCertVM = null; public SelectSSLCertVM SelectSSLCertVM { get { return _SelectSSLCertVM; } set { SetField(ref _SelectSSLCertVM, value, "SelectSSLCertVM"); } }
        public ObservableCollection<RuleVM> Rules { get; set; } = new ObservableCollection<RuleVM>();
        public ICommand RefreshClickCommand { get { return new DelegateCommand<object>(async (p) => await LoadVM(), (p) => true);}}
        public ICommand SelectSSLCertClickCommand { get { return new DelegateCommand<object>((p) => this.ShowSelectSSLCertWindow = true, (p) => true);}}
        public ICommand OpenWWWDirectoryClickCommand { get { return new DelegateCommand<object>((c) => { this.WWWDirectory = Services.LocateDirectory(); }, (c) => true); } }       
        public ICommand SaveAndApplyClickCommand { get { return new DelegateCommand<object>((c) => { Services.SaveServerConfig(this); }, (c) => true); } } 
        public ICommand NewRuleClickCommand { get { return new DelegateCommand<object>((p) => this.Rules.Add(new RuleVM(this)), (p) => true); } }
    }

}