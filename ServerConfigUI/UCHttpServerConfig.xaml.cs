using Microsoft.WindowsAPICodePack.Dialogs;
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

namespace Vlix.ServerConfigUI
{
    /// <summary>
    /// The Following required to use this User Control
    /// <see cref="UCHttpServerConfig.Initialize(Action{HttpServerConfigVM}, Func{HttpServerConfig})"/> This must be to Initialize the HttpServerConfigUI. This provides a mean to decouple the UI from different Implementations 
    /// <see cref="UCHttpServerConfig.AddLogs(List{LogStruct})"/>
    /// <see cref="UCHttpServerConfig.Refresh"/>
    /// </summary>
    public partial class UCHttpServerConfig : UserControl
    {
        public async Task Initialize(Action<HttpServerConfigVM> onSaveAndApply, Func<HttpServerConfig> onRefresh)
        {
            this.OnSaveAndApply = onSaveAndApply;
            this.OnRefresh = onRefresh;
            await this.Refresh();
        }
        public void AddLogs(List<LogStruct> Logs) { ucLogConsole.AddLogs(Logs); }
        public async Task Refresh()
        {
            HttpServerConfigVM vM = (HttpServerConfigVM)this.DataContext;
            vM.IsLoading = true;
            await Task.Delay(500);
            HttpServerConfig configBeforeChange = this.OnRefresh();
            vM.EnableHTTP = configBeforeChange.EnableHTTP;
            vM.HTTPPort = configBeforeChange.HTTPPort;
            vM.EnableHTTPS = configBeforeChange.EnableHTTPS;
            vM.HTTPSPort = configBeforeChange.HTTPSPort;
            vM.EnableCache = configBeforeChange.EnableCache;
            vM.OnlyCacheItemsLessThenMB = configBeforeChange.OnlyCacheItemsLessThenMB;
            vM.MaximumCacheSizeInMB = configBeforeChange.MaximumCacheSizeInMB;
            vM.SSLCertificateStoreName = configBeforeChange.SSLCertificateStoreName;
            vM.SSLCertificateSubjectName = configBeforeChange.SSLCertificateSubjectName;
            vM.WWWDirectory = configBeforeChange.WWWDirectory;
            vM.LogDirectory = configBeforeChange.LogDirectory;
            vM.AllowLocalhostConnectionsOnlyForHttp = configBeforeChange.AllowLocalhostConnectionsOnlyForHttp;
            vM.Rules.Clear();
            foreach (var rule in configBeforeChange.Rules) vM.Rules.Add(new RuleVM(rule, vM));
            await Task.Delay(500);
            vM.IsLoading = false;
        }
        public UCHttpServerConfig()
        {
            InitializeComponent();
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
        private void UCSelectSSLCert_OnCertificateSelected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is SSLCertVM sSLCertVM)
            {
                HttpServerConfigVM httpServerConfigVM = ((HttpServerConfigVM)this.DataContext);
                httpServerConfigVM.SSLCertificateSubjectName = sSLCertVM.Subject;
                httpServerConfigVM.SSLCertificateStoreName = sSLCertVM.StoreName;
                httpServerConfigVM.SubjectAlternativeNames.Clear();
                foreach (var s in sSLCertVM.SubjectAlternativeNames) httpServerConfigVM.SubjectAlternativeNames.Add(s);
                httpServerConfigVM.ShowSelectSSLCertWindow = false;
            }
        }
        private void opbSelectWWWDirectory_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) ((HttpServerConfigVM)this.DataContext).WWWDirectory = dialog.FileName; else ((HttpServerConfigVM)this.DataContext).WWWDirectory = null;
        }
        private void opbOpenAlternativeWWWDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is RuleVM ruleVM)
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = "C:\\Users";
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok) ruleVM.ResponseAction.AlternativeWWWDirectory = dialog.FileName; else ruleVM.ResponseAction.AlternativeWWWDirectory = null;
            }
        }
        public Action<HttpServerConfigVM> OnSaveAndApply;
        public Func<HttpServerConfig> OnRefresh;
        private void opbSaveApply_Click(object sender, RoutedEventArgs e)
        {
            this.OnSaveAndApply.Invoke(((HttpServerConfigVM)this.DataContext));
        }

    }
}
