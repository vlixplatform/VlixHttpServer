using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vlix.HttpServer;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Diagnostics;

namespace Vlix.ServerConfigUI
{
    /// <summary>
    /// The Following required to use this User Control
    /// <see cref="HttpServerConfigVM.Initialize(Action{HttpServerConfigVM}, Func{HttpServerConfig})"/> This must be to Initialize the HttpServerConfigUI. This provides a mean to decouple the UI from different Implementations 
    /// <see cref="UCHttpServerConfig.AddLogs(List{LogStruct})"/>
    /// <see cref="HttpServerConfigVM.Refresh"/>
    /// </summary>
    public partial class UCHttpServerConfig : UserControl
    {
        public void AddLogs(List<LogStruct> Logs) { ucLogConsole.AddLogs(Logs); }

        public UCHttpServerConfig()
        {
            InitializeComponent();
            ((SelectSSLCertVM)this.uCSelectSSLCert.DataContext).OnRefresh = (storeName, storeLocation) =>
             {
                 return ((HttpServerConfigVM)this.DataContext).OnSSLCertRefresh?.Invoke(storeName, storeLocation);
             };     
        }
        private async void opfSelectSSLCert_OnShow(object sender, RoutedEventArgs e)
        {
            await ((SelectSSLCertVM)uCSelectSSLCert.DataContext).Refresh();            
        }
        private void opfSelectSSLCert_OnClose(object sender, RoutedEventArgs e)
        {
            ((HttpServerConfigVM)this.DataContext).ShowSelectSSLCertWindow = false;
        }
        private void opfAdvanceSettings_OnClose(object sender, RoutedEventArgs e)
        {
            ((HttpServerConfigVM)this.DataContext).ShowAdvanceSettingsWindow = false;
        }
        private void opbNewRule_Click(object sender, RoutedEventArgs e)
        {
            svRules.ScrollToBottom();
        }
        private void uCSelectSSLCert_OnCertificateSelected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is SSLCertVM sSLCertVM)
            {
                var serverConfigVM = ((HttpServerConfigVM)this.DataContext);
                serverConfigVM.SSLCertificateSubjectName = sSLCertVM.Subject;
                serverConfigVM.SSLCertificateStoreName = sSLCertVM.StoreName;
                serverConfigVM.SubjectAlternativeNames.Clear();
                foreach (var s in sSLCertVM.SubjectAlternativeNames) serverConfigVM.SubjectAlternativeNames.Add(s);
                serverConfigVM.ShowSelectSSLCertWindow = false;
            }
        }
        private void opbSelectWWWDirectory_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(((HttpServerConfigVM)this.DataContext).WWWDirectoryParsed()); }
            catch { cmHttpServerConfig.ShowMessageError("Invalid Directory!"); }
        }
        private void opbOpenAlternativeWWWDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is RuleVM ruleVM)
            {
                try { Process.Start(ruleVM.ResponseAction.AlternativeWWWDirectoryParsed()); } 
                catch { cmHttpServerConfig.ShowMessageError("Invalid Directory!"); }
            }
        }

        //public Func<HttpServerConfig,Task<bool>> OnSaveAndApply;
        
        private async void opbSaveApply_Click(object sender, RoutedEventArgs e)
        {
            var httpServerConfigVM = ((HttpServerConfigVM)this.DataContext);
            try
            {                
                httpServerConfigVM.IsLoading = true;
                cmHttpServerConfig.ShowMessageProcess("Saving...");
                HttpServerConfig model = httpServerConfigVM.ToModel();
                if (await httpServerConfigVM.OnSaveAndApply?.Invoke(model))
                {
                    cmHttpServerConfig.ShowMessageSuccess("Saved!");
                    await Task.Delay(1000);
                }
                else cmHttpServerConfig.ShowMessageError("Save Failed!");
            }
            finally
            {
                httpServerConfigVM.IsLoading = false;
            }
            //this.OnSaveAndApply?.Invoke(((HttpServerConfigVM)this.DataContext).ToModel());
        }


        DateTime LastLogReadUTC = DateTime.MinValue;
        private async void ucLogConsole_Loaded(object sender, RoutedEventArgs e)
        {
            var vM = (HttpServerConfigVM)this.DataContext;
            while (true)
            {
                if (await (vM.CheckConnectionOK?.Invoke() ?? Task.FromResult(false)))
                {
                    List<LogStruct> logs = await vM.OnLogRefresh?.Invoke(LastLogReadUTC);
                    this.AddLogs(logs);
                    if (logs != null && logs.Count > 0) LastLogReadUTC = logs.Last().TimeStampInUTC;
                }
                await Task.Delay(500);
            }
        }


        public bool ShowSaveButtonTop { get { return (bool)GetValue(ShowSaveButtonTopProperty); } set { SetValue(ShowSaveButtonTopProperty, value); } }
        public static readonly DependencyProperty ShowSaveButtonTopProperty = DependencyProperty.Register("ShowSaveButtonTop", typeof(bool), typeof(UCHttpServerConfig), new FrameworkPropertyMetadata(false));
        public bool ShowAdvanceButtonTop { get { return (bool)GetValue(ShowAdvanceButtonTopProperty); } set { SetValue(ShowAdvanceButtonTopProperty, value); } }
        public static readonly DependencyProperty ShowAdvanceButtonTopProperty = DependencyProperty.Register("ShowAdvanceButtonTop", typeof(bool), typeof(UCHttpServerConfig), new FrameworkPropertyMetadata(false));
        public bool ShowSaveButtonBottomStretched { get { return (bool)GetValue(ShowSaveButtonBottomStretchedProperty); } set { SetValue(ShowSaveButtonBottomStretchedProperty, value); } }
        public static readonly DependencyProperty ShowSaveButtonBottomStretchedProperty = DependencyProperty.Register("ShowSaveButtonBottomStretched", typeof(bool), typeof(UCHttpServerConfig), new FrameworkPropertyMetadata(false));
        public bool ShowSaveButtonBottom { get { return (bool)GetValue(ShowSaveButtonBottomProperty); } set { SetValue(ShowSaveButtonBottomProperty, value); } }
        public static readonly DependencyProperty ShowSaveButtonBottomProperty = DependencyProperty.Register("ShowSaveButtonBottom", typeof(bool), typeof(UCHttpServerConfig), new FrameworkPropertyMetadata(true));        
        public bool ShowAdvanceButtonBottom { get { return (bool)GetValue(ShowAdvanceButtonBottomProperty); } set { SetValue(ShowAdvanceButtonBottomProperty, value); } }
        public static readonly DependencyProperty ShowAdvanceButtonBottomProperty = DependencyProperty.Register("ShowAdvanceButtonBottom", typeof(bool), typeof(UCHttpServerConfig), new FrameworkPropertyMetadata(true));

        private void optWWWDir_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is OPTextBox opt) opt.PlaceHolder = System.IO.Path.Combine("[ProgramDataDirectory]", "Vlix", "HTTPServer", "www");
        }

        private void optWWWDir_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is OPTextBox opt) ((HttpServerConfigVM)this.DataContext).WWWDirectory = opt.PlaceHolder;
        }

        private void optHTTPPort_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is OPTextBox opt) ((HttpServerConfigVM)this.DataContext).HTTPPort = opt.PlaceHolder.ToInt();
        }
        private void optHTTPSPort_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is OPTextBox opt) ((HttpServerConfigVM)this.DataContext).HTTPSPort = opt.PlaceHolder.ToInt();
        }

        private void tbRuleName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement fe && fe.DataContext is RuleVM vM)
            {
                vM.NameInEditMode = true;
                vM.NameBeforeChange = vM.Name;
            }            
        }

        private void tbNameEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is RuleVM vM)
            {
                if (e.Key == Key.Enter)
                {
                    vM.NameInEditMode = false;
                    e.Handled = true;
                }
                if (e.Key == Key.Escape)
                {
                    vM.Name = vM.NameBeforeChange;
                    vM.NameInEditMode = false;
                    e.Handled = true;
                }
            }
        }
        private void tbNameEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is RuleVM vM)
            {
                vM.NameInEditMode = false;
                e.Handled = true;
            }
        }
    }
}
