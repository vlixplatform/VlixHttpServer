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
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;

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

        //public Func<HttpServerConfig,Task<bool>> OnSaveAndApply;
        
        private async void opbSaveApply_Click(object sender, RoutedEventArgs e)
        {

            var httpServerConfigVM = ((HttpServerConfigVM)this.DataContext);
            httpServerConfigVM.IsLoading = true;
            cmHttpServerConfig.ShowMessageProcess("Saving...");
            if (await httpServerConfigVM.OnSaveAndApply?.Invoke(httpServerConfigVM.ToModel()))
            {                
                cmHttpServerConfig.ShowMessageSuccess("Saved!");                
            }
            else cmHttpServerConfig.ShowMessageError("Save Failed!");
            httpServerConfigVM.IsLoading = false;
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
    }
}
