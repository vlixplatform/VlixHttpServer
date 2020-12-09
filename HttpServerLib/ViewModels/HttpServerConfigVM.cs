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
        public HttpServerConfigVM()
        {
            //this.Rules.Add(new RuleVM(new SimpleReverseProxyRule("somehost", "wildard", 5000), this));
            //this.Rules.Add(new RuleVM(new SimplePathDenyRule("NonAllowedPath"), this));
            //this.Rules.Add(new RuleVM(new HttpToHttpsRedirectRule("somehost") { Name = "Rule 2", Enable=false }, this));
            //this.Rules.Add(new RuleVM(new SimpleHostNameRedirectRule("cat","dog") { Name = "Rule 3" }, this));
            //this.Rules.Add(new RuleVM(new HttpToHttpsRedirectRule("somehost") { Name = "Rule 4", Enable = true }, this));
            //this.Rules.Add(new RuleVM(new HttpToHttpsRedirectRule("somehost") { Name = "Rule 5", Enable = false }, this));
            //this.Rules.Add(new RuleVM(new HttpToHttpsRedirectRule("somehost") { Name = "Rule 5", Enable = false }, this));
        }

        public HttpServerConfig ToModel()
        {
            HttpServerConfig httpServerConfig = new HttpServerConfig()
            {
                SSLCertificateStoreName = this.SSLCertificateStoreName,
                SSLCertificateSubjectName = this.SSLCertificateSubjectName,
                EnableHTTPS = this.EnableHTTPS,
                HTTPSPort = this.HTTPSPort,
                EnableHTTP = this.EnableHTTP,
                HTTPPort = this.HTTPPort,
                AllowLocalhostConnectionsOnlyForHttp = this.AllowLocalhostConnectionsOnlyForHttp,
                EnableCache = this.EnableCache,
                LogDirectory = this.LogDirectory,
                MaximumCacheSizeInMB = this.MaximumCacheSizeInMB,
                OnlyCacheItemsLessThenMB = this.OnlyCacheItemsLessThenMB,
                WWWDirectory = this.WWWDirectory,
            };
            foreach (var ruleVM in this.Rules)
            {
                Rule r = new Rule()
                {
                    Name = ruleVM.Name,
                    Enable = ruleVM.Enable,
                    RequestMatch = new RequestMatch()
                    {
                        CheckHostName = ruleVM.RequestMatch.CheckHostName,
                        CheckPath = ruleVM.RequestMatch.CheckPath,
                        CheckPort = ruleVM.RequestMatch.CheckPort,
                        HostNameMatch = ruleVM.RequestMatch.HostNameMatch,
                        HostNameMatchType = ruleVM.RequestMatch.HostNameMatchType,
                        PathMatch = ruleVM.RequestMatch.PathMatch,
                        PathMatchType = ruleVM.RequestMatch.PathMatchType,
                        Port = ruleVM.RequestMatch.Port
                    }
                };
                switch (ruleVM.ResponseAction.ActionType)
                {
                    case ActionType.AlternativeWWWDirectory:
                        r.ResponseAction = new AlternativeWWWDirectoryAction() { AlternativeWWWDirectory = ruleVM.ResponseAction.AlternativeWWWDirectory };
                        break;
                    case ActionType.Deny:
                        r.ResponseAction = new DenyAction();
                        break;
                    case ActionType.Redirect:
                        r.ResponseAction = new RedirectAction()
                        {
                            Scheme = ruleVM.ResponseAction.RedirectScheme,
                            SetScheme = ruleVM.ResponseAction.SetRedirectScheme,
                            HostName = ruleVM.ResponseAction.RedirectHostName,
                            SetHostName = ruleVM.ResponseAction.SetRedirectHostName,
                            Path = ruleVM.ResponseAction.RedirectPath,
                            SetPath = ruleVM.ResponseAction.SetRedirectPath,
                            Port = ruleVM.ResponseAction.RedirectPort,
                            SetPort = ruleVM.ResponseAction.SetRedirectPort
                        };
                        break;
                    case ActionType.ReverseProxy:
                        r.ResponseAction = new ReverseProxyAction()
                        {
                            SetScheme = ruleVM.ResponseAction.SetReverseProxyScheme,
                            Scheme = ruleVM.ResponseAction.ReverseProxyScheme,
                            SetHostName = ruleVM.ResponseAction.SetReverseProxyHostName,
                            HostName = ruleVM.ResponseAction.ReverseProxyHostName,
                            SetPort = ruleVM.ResponseAction.SetReverseProxyPort,
                            Port = ruleVM.ResponseAction.ReverseProxyPort,
                            UsePathVariable = ruleVM.ResponseAction.ReverseProxyUsePathVariable,
                            SetPath = ruleVM.ResponseAction.SetReverseProxyPath,
                            Path = ruleVM.ResponseAction.ReverseProxyPath
                        };
                        break;
                }
                httpServerConfig.Rules.Add(r);
            }
            return httpServerConfig;
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
        bool _AllowLocalhostConnectionsOnlyForHttp = false; public bool AllowLocalhostConnectionsOnlyForHttp { get { return _AllowLocalhostConnectionsOnlyForHttp; } set { SetField(ref _AllowLocalhostConnectionsOnlyForHttp, value, "AllowLocalhostConnectionsOnlyForHttp"); } }
        bool _ShowSelectSSLCertWindow = false; public bool ShowSelectSSLCertWindow { get { return _ShowSelectSSLCertWindow; } set { SetField(ref _ShowSelectSSLCertWindow, value, "ShowSelectSSLCertWindow"); } }
        bool _ShowAdvanceSettingsWindow = false; public bool ShowAdvanceSettingsWindow { get { return _ShowAdvanceSettingsWindow; } set { SetField(ref _ShowAdvanceSettingsWindow, value, "ShowAdvanceSettingsWindow"); } }
        bool _EnableCache = false; public bool EnableCache { get { return _EnableCache; } set { SetField(ref _EnableCache, value, "EnableCache"); } }
        bool _ShowDisabledRules = true; public bool ShowDisabledRules { get { return _ShowDisabledRules; } set { SetField(ref _ShowDisabledRules, value, "ShowDisabledRules"); } }
        int _OnlyCacheItemsLessThenMB = 10; public int OnlyCacheItemsLessThenMB { get { return _OnlyCacheItemsLessThenMB; } set { SetField(ref _OnlyCacheItemsLessThenMB, value, "OnlyCacheItemsLessThenMB"); } }
        int _MaximumCacheSizeInMB = 250; public int MaximumCacheSizeInMB { get { return _MaximumCacheSizeInMB; } set { SetField(ref _MaximumCacheSizeInMB, value, "MaximumCacheSizeInMB"); } }
        public ObservableCollection<RuleVM> Rules { get; set; } = new ObservableCollection<RuleVM>();
        //public ICommand RefreshClickCommand { get { return new DelegateCommand<object>(async (p) => await LoadVM(), (p) => true);}}
        public ICommand SelectSSLCertClickCommand { get { return new DelegateCommand<object>((p) => this.ShowSelectSSLCertWindow = true, (p) => true);}}
        public ICommand ShowAdvanceSettingsClickCommand { get { return new DelegateCommand<object>((p) => this.ShowAdvanceSettingsWindow = true, (p) => true);}}  
        //public ICommand SaveAndApplyClickCommand { get { return new DelegateCommand<object>((c) => { Services.SaveServerConfig(this.ToModel()); }, (c) => true); } } 
        public ICommand NewRuleClickCommand { get { return new DelegateCommand<object>((p) =>
        {
            this.Rules.Add(new RuleVM(this));
        }, (p) => true); } }
    }

}