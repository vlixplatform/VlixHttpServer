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

namespace Vlix.HttpServerConfig
{
    /// <summary>
    /// Interaction logic for UCHttpServerConfig.xaml
    /// </summary>
    public partial class UCHttpServerConfig : UserControl
    {
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





        public static readonly RoutedEvent OnSaveApplyClickEvent = EventManager.RegisterRoutedEvent("OnSaveApplyClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(UCHttpServerConfig));
        public event RoutedEventHandler OnSaveApplyClick
        {
            add { AddHandler(OnSaveApplyClickEvent, value); }
            remove { RemoveHandler(OnSaveApplyClickEvent, value); }
        }
        private void opbSaveApply_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is HttpServerConfigVM vM)
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(UCHttpServerConfig.OnSaveApplyClickEvent, vM);
                RaiseEvent(newEventArgs);
            }
        }





        public static readonly RoutedEvent OnRefreshClickEvent = EventManager.RegisterRoutedEvent("OnRefreshClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(UCHttpServerConfig));
        public event RoutedEventHandler OnRefreshClick
        {
            add { AddHandler(OnRefreshClickEvent, value); }
            remove { RemoveHandler(OnRefreshClickEvent, value); }
        }
        private void opbRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is HttpServerConfigVM vM)
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(UCHttpServerConfig.OnRefreshClickEvent, vM);
                RaiseEvent(newEventArgs);
            }
        }



        public void AddLogs(List<LogStruct> Logs) { ucLogConsole.AddLogs(Logs); }
    }
}
