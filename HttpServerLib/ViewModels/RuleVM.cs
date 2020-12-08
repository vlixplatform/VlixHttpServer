using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Vlix.HttpServer
{
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
        public RuleVM(HttpServerConfigVM parentVM) { this.ParentVM = parentVM; }
        public RuleVM(Rule rule, HttpServerConfigVM parentVM) 
        {
            this.Enable = rule.Enable;
            this.Name = rule.Name;
            this.RequestMatch.CheckHostName = rule.RequestMatch.CheckHostName;
            this.RequestMatch.HostNameMatch = rule.RequestMatch.HostNameMatch;
            this.RequestMatch.HostNameMatchType = rule.RequestMatch.HostNameMatchType;
            this.RequestMatch.CheckPort = rule.RequestMatch.CheckPort;
            this.RequestMatch.Port = rule.RequestMatch.Port;
            this.RequestMatch.CheckPath = rule.RequestMatch.CheckPath;
            this.RequestMatch.PathMatch = rule.RequestMatch.PathMatch;
            this.RequestMatch.PathMatchType = rule.RequestMatch.PathMatchType;

            if (rule.ResponseAction is RedirectAction redirectAction)
            {
                this.ResponseAction.ActionType = ActionType.Redirect;
                this.ResponseAction.SetRedirectScheme = redirectAction.SetScheme;
                this.ResponseAction.RedirectScheme = redirectAction.Scheme;
                this.ResponseAction.SetRedirectHostName = redirectAction.SetHostName;
                this.ResponseAction.RedirectHostName = redirectAction.HostName;
                this.ResponseAction.SetRedirectPort = redirectAction.SetPort;
                this.ResponseAction.RedirectPort = redirectAction.Port;
                this.ResponseAction.SetRedirectPath = redirectAction.SetPath;
                this.ResponseAction.RedirectPath = redirectAction.Path;
            }
            if (rule.ResponseAction is AlternativeWWWDirectoryAction alternativeWWWDirectoryAction)
            {
                this.ResponseAction.ActionType = ActionType.AlternativeWWWDirectory;
                this.ResponseAction.AlternativeWWWDirectory = alternativeWWWDirectoryAction.AlternativeWWWDirectory;
            }
            if (rule.ResponseAction is DenyAction denyAction)
            {
                this.ResponseAction.ActionType = ActionType.Deny;
            }
            if (rule.ResponseAction is ReverseProxyAction reverseProxyAction)
            {
                this.ResponseAction.ActionType = ActionType.ReverseProxy;
                this.ResponseAction.SetReverseProxyScheme = reverseProxyAction.SetScheme;
                this.ResponseAction.ReverseProxyScheme = reverseProxyAction.Scheme;
                this.ResponseAction.SetReverseProxyHostName = reverseProxyAction.SetHostName;
                this.ResponseAction.ReverseProxyHostName = reverseProxyAction.HostName;
                this.ResponseAction.SetReverseProxyPort = reverseProxyAction.SetPort;
                this.ResponseAction.ReverseProxyPort = reverseProxyAction.Port;
                this.ResponseAction.ReverseProxyUsePathVariable = reverseProxyAction.UsePathVariable;
                this.ResponseAction.SetReverseProxyPath = reverseProxyAction.SetPath;
                this.ResponseAction.ReverseProxyPath = reverseProxyAction.Path;
            }
            this.ParentVM = parentVM;
        }
        HttpServerConfigVM _ParentVM = null; public HttpServerConfigVM ParentVM { get { return _ParentVM; } set { SetField(ref _ParentVM, value, "ParentVM"); } }
        bool _Enable = true; public bool Enable { get { return _Enable; } set { SetField(ref _Enable, value, "Enable"); } }
        string _Name = "New Rule"; public string Name { get { return _Name; } set { SetField(ref _Name, value, "Name"); } }
        RequestMatchVM _RequestMatchVM = new RequestMatchVM(); public RequestMatchVM RequestMatch { get { return _RequestMatchVM; } set { SetField(ref _RequestMatchVM, value, "RequestMatch"); } }
        ResponseActionVM _ResponseActionVM = new ResponseActionVM(); public ResponseActionVM ResponseAction { get { return _ResponseActionVM; } set { SetField(ref _ResponseActionVM, value, "ResponseAction"); } }

        bool _ShowConfirmDelete = false; public bool ShowConfirmDelete { get { return _ShowConfirmDelete; } set { SetField(ref _ShowConfirmDelete, value, "ShowConfirmDelete"); } }
        public ICommand DeleteClickCommand { get { return new DelegateCommand<object>((p) => ShowConfirmDelete = true, (p) => true); } }
        public ICommand DeleteConfirmYesClickCommand { get { return new DelegateCommand<object>((p) => this.ParentVM.Rules.Remove(this), (p) => true); } }
        public ICommand DeleteConfirmCancelClickCommand { get { return new DelegateCommand<object>((p) => this.ShowConfirmDelete = false, (p) => true); } }

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

        public RequestMatchVM()
        {
        }
        public IEnumerable<MatchType> ComboBoxMatchTypes { get { return Enum.GetValues(typeof(MatchType)).Cast<MatchType>(); }}
        bool _CheckHostName = false; public bool CheckHostName { get { return _CheckHostName; } set { SetField(ref _CheckHostName, value, "CheckHostName"); } }
        MatchType _HostNameMatchType = MatchType.Exact; public MatchType HostNameMatchType { get { return _HostNameMatchType; } set { SetField(ref _HostNameMatchType, value, "HostNameMatchType"); } }
        string _HostNameMatch = null; public string HostNameMatch { get { return _HostNameMatch; } set { SetField(ref _HostNameMatch, value, "HostNameMatch"); } }
        bool _CheckPort = false; public bool CheckPort { get { return _CheckPort; } set { SetField(ref _CheckPort, value, "CheckPort"); } }
        int? _Port = null; public int? Port { get { return _Port; } set { SetField(ref _Port, value, "Port"); } }
        bool _CheckPath = false; public bool CheckPath { get { return _CheckPath; } set { SetField(ref _CheckPath, value, "CheckPath"); } }
        MatchType _PathMatchType = MatchType.Wildcard; public MatchType PathMatchType { get { return _PathMatchType; } set { SetField(ref _PathMatchType, value, "PathMatchType"); } }
        string _PathMatch = null; public string PathMatch { get { return _PathMatch; } set { SetField(ref _PathMatch, value, "PathMatch"); } }

        Wildcard hostWildCard = null;
        public Wildcard GetHostWildCard()
        {
            if (hostWildCard == null) hostWildCard = new Wildcard(HostNameMatch);
            return hostWildCard;
        }
        Wildcard uRLWilCard = null;
        public Wildcard GetURLWildCard()
        {
            if (uRLWilCard == null) uRLWilCard = new Wildcard(PathMatch);
            return uRLWilCard;
        }
    }


    public class ResponseActionVM : INotifyPropertyChanged
    {
        #region BOILER PLATE
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName, string p2=null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            if (p2!=null) OnPropertyChanged(p2);
            return true;
        }
        #endregion        
        public IEnumerable<ActionType> ActionTypes { get { return Enum.GetValues(typeof(ActionType)).Cast<ActionType>(); } }
        public IEnumerable<Scheme> SchemeTypes { get { return Enum.GetValues(typeof(Scheme)).Cast<Scheme>(); } }

        ActionType _ActionType = ActionType.Redirect; public ActionType ActionType { get { return _ActionType; } set { SetField(ref _ActionType, value, "ActionType"); } }
        
        //Alternative WWW Drectory 
        string _AlternativeWWWDirectory = null; public string AlternativeWWWDirectory { get { return _AlternativeWWWDirectory; } set { SetField(ref _AlternativeWWWDirectory, value, "AlternativeWWWDirectory"); } }

        //Redirect
        bool _SetRedirectScheme = false; public bool SetRedirectScheme { get { return _SetRedirectScheme; } set { SetField(ref _SetRedirectScheme, value, "SetRedirectScheme"); } }
        Scheme? _RedirectScheme = null; public Scheme? RedirectScheme { get { return _RedirectScheme; } set { SetField(ref _RedirectScheme, value, "RedirectScheme"); } }
        bool _SetRedirectHostName = false; public bool SetRedirectHostName { get { return _SetRedirectHostName; } set { SetField(ref _SetRedirectHostName, value, "SetRedirectHostName"); } }
        string _RedirectHostName = null; public string RedirectHostName { get { return _RedirectHostName; } set { SetField(ref _RedirectHostName, value, "RedirectHostName"); } }
        bool _SetRedirectPort = false; public bool SetRedirectPort { get { return _SetRedirectPort; } set { SetField(ref _SetRedirectPort, value, "SetRedirectPort"); } }
        int? _RedirectPort = null; public int? RedirectPort { get { return _RedirectPort; } set { SetField(ref _RedirectPort, value, "RedirectPort"); } }
        bool _SetRedirectPath = false; public bool SetRedirectPath { get { return _SetRedirectPath; } set { SetField(ref _SetRedirectPath, value, "SetRedirectPath"); } }
        string _RedirectPath = null; public string RedirectPath { get { return _RedirectPath; } set { SetField(ref _RedirectPath, value, "RedirectPath"); } }

        //Reverse Proxy
        bool _SetReverseProxyScheme = true; public bool SetReverseProxyScheme { get { return _SetReverseProxyScheme; } set { SetField(ref _SetReverseProxyScheme, value, "SetReverseProxyScheme"); } }
        Scheme? _ReverseProxyScheme = Scheme.http; public Scheme? ReverseProxyScheme { get { return _ReverseProxyScheme; } set { SetField(ref _ReverseProxyScheme, value, "ReverseProxyScheme"); } }
        bool _SetReverseProxyHostName = true; public bool SetReverseProxyHostName { get { return _SetReverseProxyHostName; } set { SetField(ref _SetReverseProxyHostName, value, "SetReverseProxyHostName"); } }
        string _ReverseProxyHostName = "localhost"; public string ReverseProxyHostName { get { return _ReverseProxyHostName; } set { SetField(ref _ReverseProxyHostName, value, "ReverseProxyHostName"); } }
        bool _SetReverseProxyPort = false; public bool SetReverseProxyPort { get { return _SetReverseProxyPort; } set { SetField(ref _SetReverseProxyPort, value, "SetReverseProxyPort"); } }
        int? _ReverseProxyPort = null; public int? ReverseProxyPort { get { return _ReverseProxyPort; } set { SetField(ref _ReverseProxyPort, value, "ReverseProxyPort"); } }
        bool _SetReverseProxyPath = false; public bool SetReverseProxyPath { get { return _SetReverseProxyPath; } set { SetField(ref _SetReverseProxyPath, value, "SetReverseProxyPath"); } }
        bool _ReverseProxyUsePathVariable = false; public bool ReverseProxyUsePathVariable { get { return _ReverseProxyUsePathVariable; } set { SetField(ref _ReverseProxyUsePathVariable, value, "ReverseProxyUsePathVariable"); } }
        string _ReverseProxyPath = null; public string ReverseProxyPath { get { return _ReverseProxyPath; } set { SetField(ref _ReverseProxyPath, value, "ReverseProxyPath"); } }




    }

}