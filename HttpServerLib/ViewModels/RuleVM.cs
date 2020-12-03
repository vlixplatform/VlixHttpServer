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
                this.ResponseAction.UseRedirectScheme = redirectAction.RedirectScheme;
                this.ResponseAction.RedirectScheme = redirectAction.Scheme;
                this.ResponseAction.UseRedirectHostName = redirectAction.RedirectHostName;
                this.ResponseAction.RedirectHostName = redirectAction.HostName;
                this.ResponseAction.UseRedirectPort = redirectAction.RedirectPort;
                this.ResponseAction.RedirectPort = redirectAction.Port;
                this.ResponseAction.UseRedirectPath = redirectAction.RedirectPath;
                this.ResponseAction.RedirectPath = redirectAction.Path;
            }
            if (rule.ResponseAction is AlternativeWWWDirectoryAction alternativeWWWDirectoryAction)
            {
                this.ResponseAction.AlternativeWWWDirectory = alternativeWWWDirectoryAction.AlternativeWWWDirectory;
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
        MatchType _HostNameMatchType = MatchType.Wildcard; public MatchType HostNameMatchType { get { return _HostNameMatchType; } set { SetField(ref _HostNameMatchType, value, "HostNameMatchType"); } }
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
        ActionType _ActionType = ActionType.Redirect; public ActionType ActionType { get { return _ActionType; } set { SetField(ref _ActionType, value, "ActionType"); } }
        
        //Alternative WWW Drectory 
        string _AlternativeWWWDirectory = null; public string AlternativeWWWDirectory { get { return _AlternativeWWWDirectory; } set { SetField(ref _AlternativeWWWDirectory, value, "AlternativeWWWDirectory"); } }
        public ICommand OpenAlternativeWWWDirectoryClickCommand { get { return new DelegateCommand<object>((c) => { this.AlternativeWWWDirectory = Services.LocateDirectory(); }, (c) => true); } }

        //Redirect
        public IEnumerable<Scheme> RedirectSchemeTypes { get { return Enum.GetValues(typeof(Scheme)).Cast<Scheme>(); } }
        bool _UseRedirectScheme = false; public bool UseRedirectScheme { get { return _UseRedirectScheme; } set { SetField(ref _UseRedirectScheme, value, "UseRedirectScheme"); } }
        Scheme? _RedirectScheme = null; public Scheme? RedirectScheme { get { return _RedirectScheme; } set { SetField(ref _RedirectScheme, value, "RedirectScheme"); } }
        bool _UseRedirectHostName = false; public bool UseRedirectHostName { get { return _UseRedirectHostName; } set { SetField(ref _UseRedirectHostName, value, "UseRedirectHostName"); } }
        string _RedirectHostName = null; public string RedirectHostName { get { return _RedirectHostName; } set { SetField(ref _RedirectHostName, value, "HostName"); } }
        bool _UseRedirectPort = false; public bool UseRedirectPort { get { return _UseRedirectPort; } set { SetField(ref _UseRedirectPort, value, "UseRedirectPort"); } }
        int? _RedirectPort = null; public int? RedirectPort { get { return _RedirectPort; } set { SetField(ref _RedirectPort, value, "RedirectPort"); } }
        bool _UseRedirectPath = false; public bool UseRedirectPath { get { return _UseRedirectPath; } set { SetField(ref _UseRedirectPath, value, "UseRedirectPath"); } }
        string _RedirectPath = null; public string RedirectPath { get { return _RedirectPath; } set { SetField(ref _RedirectPath, value, "RedirectPath"); } }
        

        

    }

}