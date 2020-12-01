﻿using System;
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

            this.Rules.Add(new RuleVM(new HttpToHttpsRedirectRule()));
            this.Rules.Add(new RuleVM(new HttpToHttpsRedirectRule()));
            this.Rules.Add(new RuleVM(new HttpToHttpsRedirectRule()));
            this.SelectSSLCertVM = new SelectSSLCertVM(this);

        }

        public async Task LoadVM()
        {
            this.IsLoading = true;
            await Task.Delay(500);
            HttpServerConfig configBeforeChange = await Services.LoadHttpServerConfig();
            this.AllowLocalhostConnectionsOnly = configBeforeChange.AllowLocalhostConnectionsOnly;
            this.WWWDirectory = configBeforeChange.WWWDirectory;
            this.EnableHTTP = configBeforeChange.EnableHTTP;
            this.HTTPPort = configBeforeChange.HTTPPort;
            this.EnableHTTPS = configBeforeChange.EnableHTTPS;
            this.HTTPSPort = configBeforeChange.HTTPSPort;
            this.EnableCache = configBeforeChange.EnableCache;
            this.OnlyCacheItemsLessThenMB = configBeforeChange.OnlyCacheItemsLessThenMB;
            this.MaximumCacheSizeInMB = configBeforeChange.MaximumCacheSizeInMB;
            await Task.Delay(500);
            this.IsLoading = false;
        }
        bool _IsLoading = false; public bool IsLoading { get { return _IsLoading; } set { SetField(ref _IsLoading, value, "IsLoading"); } }
        bool _ClientIsLocal = true; public bool ClientIsLocal { get { return _ClientIsLocal; } set { SetField(ref _ClientIsLocal, value, "ClientIsLocal"); } }
        string _SSLCertSubjectName = null; public string SSLCertSubjectName { get { return _SSLCertSubjectName; } set { SetField(ref _SSLCertSubjectName, value, "SSLCertSubjectName"); } }
        StoreName _SSLCertStoreName = StoreName.My; public StoreName SSLCertStoreName { get { return _SSLCertStoreName; } set { SetField(ref _SSLCertStoreName, value, "SSLCertStoreName"); } }
        public ObservableCollection<string> SubjectAlternativeNames { get; set; } = new ObservableCollection<string>();
        string _WWWDirectory = Path.Combine("[ProgramDataDirectory]","Vlix","HTTPServer","www"); public string WWWDirectory { get { return _WWWDirectory; } set { SetField(ref _WWWDirectory, value, "WWWDirectory"); } }
        bool _EnableHTTP = true; public bool EnableHTTP { get { return _EnableHTTP; } set { SetField(ref _EnableHTTP, value, "EnableHTTP"); } }        
        int _HTTPPort = 80; public int HTTPPort { get { return _HTTPPort; } set { SetField(ref _HTTPPort, value, "HTTPPort"); } }
        bool _EnableHTTPS = true; public bool EnableHTTPS { get { return _EnableHTTPS; } set { SetField(ref _EnableHTTPS, value, "EnableHTTPS"); } }
        int _HTTPSPort = 443; public int HTTPSPort { get { return _HTTPSPort; } set { SetField(ref _HTTPSPort, value, "HTTPSPort"); } }        
        bool _AllowLocalhostConnectionsOnly = false; public bool AllowLocalhostConnectionsOnly { get { return _AllowLocalhostConnectionsOnly; } set { SetField(ref _AllowLocalhostConnectionsOnly, value, "AllowLocalhostConnectionsOnly"); } }
        bool _ShowAdvance = true; public bool ShowAdvance { get { return _ShowAdvance; } set { SetField(ref _ShowAdvance, value, "ShowAdvance"); } }
        bool _ShowSelectSSLCertWindow = false; public bool ShowSelectSSLCertWindow { get { return _ShowSelectSSLCertWindow; } set { SetField(ref _ShowSelectSSLCertWindow, value, "ShowSelectSSLCertWindow"); } }
        bool _EnableCache = false; public bool EnableCache { get { return _EnableCache; } set { SetField(ref _EnableCache, value, "EnableCache"); } }
        int _OnlyCacheItemsLessThenMB = 10; public int OnlyCacheItemsLessThenMB { get { return _OnlyCacheItemsLessThenMB; } set { SetField(ref _OnlyCacheItemsLessThenMB, value, "OnlyCacheItemsLessThenMB"); } }
        int _MaximumCacheSizeInMB = 250; public int MaximumCacheSizeInMB { get { return _MaximumCacheSizeInMB; } set { SetField(ref _MaximumCacheSizeInMB, value, "MaximumCacheSizeInMB"); } }
        SelectSSLCertVM _SelectSSLCertVM = null; public SelectSSLCertVM SelectSSLCertVM { get { return _SelectSSLCertVM; } set { SetField(ref _SelectSSLCertVM, value, "SelectSSLCertVM"); } }
        public ObservableCollection<RuleVM> Rules { get; set; } = new ObservableCollection<RuleVM>();
        public ICommand ShowAdvanceClickCommand { get { return new DelegateCommand<object>((p) => this.ShowAdvance = true, (p) => true);}}
        public ICommand HideAdvanceClickCommand { get { return new DelegateCommand<object>((p) => this.ShowAdvance = false, (p) => true);}}
        public ICommand RefreshClickCommand { get { return new DelegateCommand<object>(async (p) => await LoadVM(), (p) => true);}}
        public ICommand SelectSSLCertClickCommand { get { return new DelegateCommand<object>((p) => this.ShowSelectSSLCertWindow = true, (p) => true);}}
        public ICommand OpenWWWDirectoryClickCommand
        {
            get
            {
                return new DelegateCommand<object>((p) =>
                {
                    var k = 1;

                }, (p) => true);
            }
        }
        public ICommand SaveAndApplyClickCommand
        {
            get { return new DelegateCommand<object>((p)=> 
            {
                var k = 1;
            
            }, (p)=> true); }
        }
        public ICommand AddRedirectCommand
        {
            get
            {
                return new DelegateCommand<object>((c) => this.Rules.Add(new RuleVM()), (c) => true);
            }
        }
    }


    public class RuleVM : INotifyPropertyChanged
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
        public RuleVM() { }
        public RuleVM(Rule rule) 
        {
            this.Enable = rule.Enable;
            this.RequestMatch.AnyHostName = rule.RequestMatch.AnyHostName;
            this.RequestMatch.HostNameWildCard = rule.RequestMatch.HostNameMatch;
            this.RequestMatch.HostNameMatchType = rule.RequestMatch.HostNameMatchType;
            this.RequestMatch.AnyPort = rule.RequestMatch.AnyPort;
            this.RequestMatch.Port = rule.RequestMatch.Port;
            this.RequestMatch.AnyPath = rule.RequestMatch.AnyPath;
            this.RequestMatch.PathWildCard = rule.RequestMatch.PathMatch;
            this.RequestMatch.PathMatchType = rule.RequestMatch.PathMatchType;
            this.ResponseAction.ActionName = rule.ResponseAction.ActionName;
            if (rule.ResponseAction is RedirectAction redirectAction)
            {
                this.ResponseAction.Scheme = redirectAction.Scheme;
                this.ResponseAction.HostName = redirectAction.HostName;
                this.ResponseAction.Port = redirectAction.Port;
                this.ResponseAction.Path = redirectAction.Path;
            }
            if (rule.ResponseAction is AlternativeWWWDirectoryAction alternativeWWWDirectoryAction)
            {
                this.ResponseAction.AlternativeWWWDirectory = alternativeWWWDirectoryAction.AlternativeWWWDirectory;
            }            
        }
        bool _Enable = false; public bool Enable { get { return _Enable; } set { SetField(ref _Enable, value, "Enable"); } }
        RequestMatchVM _RequestMatchVM = new RequestMatchVM(); public RequestMatchVM RequestMatch { get { return _RequestMatchVM; } set { SetField(ref _RequestMatchVM, value, "RequestMatch"); } }
        ResponseActionVM _ResponseActionVM = new ResponseActionVM(); public ResponseActionVM ResponseAction { get { return _ResponseActionVM; } set { SetField(ref _ResponseActionVM, value, "ResponseAction"); } }
    }

    public class RequestMatchVM : INotifyPropertyChanged
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
        bool _AnyHostName = false; public bool AnyHostName { get { return _AnyHostName; } set { SetField(ref _AnyHostName, value, "AnyHostName"); } }
        MatchType _HostNameMatchType = MatchType.Wildcard; public MatchType HostNameMatchType { get { return _HostNameMatchType; } set { SetField(ref _HostNameMatchType, value, "HostNameMatchType"); } }
        string _HostNameWildCard = null; public string HostNameWildCard { get { return _HostNameWildCard; } set { SetField(ref _HostNameWildCard, value, "HostNameWildCard"); } }
        bool _AnyPort = false; public bool AnyPort { get { return _AnyPort; } set { SetField(ref _AnyPort, value, "AnyPort"); } }
        int? _Port = null; public int? Port { get { return _Port; } set { SetField(ref _Port, value, "Port"); } }
        bool _AnyPath = false; public bool AnyPath { get { return _AnyPath; } set { SetField(ref _AnyPath, value, "AnyPath"); } }
        MatchType _PathMatchType = MatchType.Wildcard; public MatchType PathMatchType { get { return _PathMatchType; } set { SetField(ref _PathMatchType, value, "PathMatchType"); } }
        string _PathWildCard = null; public string PathWildCard { get { return _PathWildCard; } set { SetField(ref _PathWildCard, value, "PathWildCard"); } }

        Wildcard hostWildCard = null;
        public Wildcard GetHostWildCard()
        {
            if (hostWildCard == null) hostWildCard = new Wildcard(HostNameWildCard);
            return hostWildCard;
        }
        Wildcard uRLWilCard = null;
        public Wildcard GetURLWildCard()
        {
            if (uRLWilCard == null) uRLWilCard = new Wildcard(PathWildCard);
            return uRLWilCard;
        }
    }


    public class ResponseActionVM: INotifyPropertyChanged
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
        
        string _ActionName = null; public string ActionName { get { return _ActionName; } set { SetField(ref _ActionName, value, "ActionName"); } }
        Scheme? _Scheme = null; public Scheme? Scheme { get { return _Scheme; } set { SetField(ref _Scheme, value, "Scheme"); } }
        string _HostName = null; public string HostName { get { return _HostName; } set { SetField(ref _HostName, value, "HostName"); } }
        int? _Port = null; public int? Port { get { return _Port; } set { SetField(ref _Port, value, "Port"); } }
        string _Path = null; public string Path { get { return _Path; } set { SetField(ref _Path, value, "Path"); } }
        string _AlternativeWWWDirectory = null; public string AlternativeWWWDirectory { get { return _AlternativeWWWDirectory; } set { SetField(ref _AlternativeWWWDirectory, value, "AlternativeWWWDirectory"); } }
    }
}