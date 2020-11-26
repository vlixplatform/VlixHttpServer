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

            this.Redirects.Add(new RedirectVM(new HttpToHttpsRedirect()));
            this.Redirects.Add(new RedirectVM(new HttpToHttpsRedirect()));
            this.Redirects.Add(new RedirectVM(new HttpToHttpsRedirect()));
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
        string _SSLCertName = null; public string SSLCertName { get { return _SSLCertName; } set { SetField(ref _SSLCertName, value, "SSLCertName"); } }
        string _SSLCertThumbPrint = null; public string SSLCertThumbPrint { get { return _SSLCertThumbPrint; } set { SetField(ref _SSLCertThumbPrint, value, "SSLCertThumbPrint"); } }
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
        public ObservableCollection<RedirectVM> Redirects { get; set; } = new ObservableCollection<RedirectVM>();
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

               
                return new DelegateCommand<object>((c) => this.Redirects.Add(new RedirectVM()), (c) => true);
            }
        }
    }


    public class RedirectVM : INotifyPropertyChanged
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
        public RedirectVM() { }
        public RedirectVM(Redirect redirect) 
        {
            this.Enable = redirect.Enable;
            this.From.AnyHostName = redirect.From.AnyHostName;
            this.From.HostNameWildCard = redirect.From.HostNameWildCard;
            this.From.AnyPort = redirect.From.AnyPort;
            this.From.Port = redirect.From.Port;
            this.From.AnyPath = redirect.From.AnyPath;
            this.From.PathWildCard = redirect.From.PathWildCard;
            this.To.Scheme = redirect.To.Scheme;
            this.To.HostName = redirect.To.HostName;
            this.To.Port = redirect.To.Port;
            this.To.Path = redirect.To.Path;            
        }
        bool _Enable = false; public bool Enable { get { return _Enable; } set { SetField(ref _Enable, value, "Enable"); } }
        RedirectFrom _From = new RedirectFrom(); public RedirectFrom From { get { return _From; } set { SetField(ref _From, value, "From"); } }
        RedirectTo _To = new RedirectTo(); public RedirectTo To { get { return _To; } set { SetField(ref _To, value, "To"); } }
    }

    public class RedirectFromVM : INotifyPropertyChanged
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
        string _HostNameWildCard = null; public string HostNameWildCard { get { return _HostNameWildCard; } set { SetField(ref _HostNameWildCard, value, "HostNameWildCard"); } }
        bool _AnyPort = false; public bool AnyPort { get { return _AnyPort; } set { SetField(ref _AnyPort, value, "AnyPort"); } }
        int? _Port = null; public int? Port { get { return _Port; } set { SetField(ref _Port, value, "Port"); } }
        bool _AnyPath = false; public bool AnyPath { get { return _AnyPath; } set { SetField(ref _AnyPath, value, "AnyPath"); } }
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


    public class RedirectToVM: INotifyPropertyChanged
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
        Scheme? _Scheme = null; public Scheme? Scheme { get { return _Scheme; } set { SetField(ref _Scheme, value, "Scheme"); } }
        string _HostName = null; public string HostName { get { return _HostName; } set { SetField(ref _HostName, value, "HostName"); } }
        int? _Port = null; public int? Port { get { return _Port; } set { SetField(ref _Port, value, "Port"); } }
        string _Path = null; public string Path { get { return _Path; } set { SetField(ref _Path, value, "Path"); } }
    }
}