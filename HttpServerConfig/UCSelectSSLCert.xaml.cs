using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    public partial class UCSelectSSLCert : UserControl
    {
        SelectSSLCertVM selectSSLCertVM = null;

        public UCSelectSSLCert()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            selectSSLCertVM = (SelectSSLCertVM)this.DataContext;
            await selectSSLCertVM.LoadVM();
        }

        private async void sslStore_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectSSLCertVM = (SelectSSLCertVM)this.DataContext;
            await selectSSLCertVM.LoadVM();
        }



        //public void CertificateSelected()
        //{

        //    RaiseOnCertificateSelected();
        //    //this.StoryboardClose.Completed -= StoryboardClose_Completed;
        //    //this.StoryboardClose.Completed += StoryboardClose_Completed;
        //}


        public static readonly RoutedEvent OnCertificateSelectedEvent = EventManager.RegisterRoutedEvent("OnCertificateSelected", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(OPFrame));
        public event RoutedEventHandler OnCertificateSelected
        {
            add { AddHandler(OnCertificateSelectedEvent, value); }
            remove { RemoveHandler(OnCertificateSelectedEvent, value); }
        }
        //void RaiseOnCertificateSelected()
        //{
        //    RoutedEventArgs newEventArgs = new RoutedEventArgs(UCSelectSSLCert.OnCertificateSelectedEvent,);
        //    RaiseEvent(newEventArgs);
        //}

        private void SSLCertSelect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is SSLCertVM sSLCertVM)
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(UCSelectSSLCert.OnCertificateSelectedEvent, sSLCertVM);
                RaiseEvent(newEventArgs);
            }
        }
    }

    //public class SelectedSSLCertificate
    //{
    //    public SelectedSSLCertificate(SSLCertVM sSLCertVM)
    //    {
    //        this.SubjectName = sSLCertVM.Subject;
    //        this.StoreName = sSLCertVM.StoreName;
    //        foreach (var s in sSLCertVM.SubjectAlternativeNames) this.SubjectAlternativeNames.Add(s);
    //    }

    //    public string SubjectName { get; private set; } = null;
    //    public StoreName StoreName { get; private set; } = StoreName.My;
    //    public List<string> SubjectAlternativeNames { get; private set; } = new List<string>();
            
    //}

}
