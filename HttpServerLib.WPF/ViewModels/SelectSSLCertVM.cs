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
using System.Security.Cryptography;

namespace Vlix.HttpServer
{
    public class SelectSSLCertVM : INotifyPropertyChanged
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
        public SelectSSLCertVM()
        {
        }
        public SelectSSLCertVM(HttpServerConfigVM parentVM)
        {
            this.ParentVM = parentVM;
        }

        public async Task LoadVM()
        {
            this.IsLoading = true;
            var certs = await Services.GetSSLCertificates(this.StoreName);
            this.SSLCerts.Clear();
            foreach (var c in certs) if (c.HasPrivateKey) this.SSLCerts.Add(new SSLCertVM(c, this));
            this.IsLoading = false;
        }

        public HttpServerConfigVM ParentVM;
        bool _EnableCache = false; public bool EnableCache { get { return _EnableCache; } set { SetField(ref _EnableCache, value, "EnableCache"); } }
        StoreName _StoreName = StoreName.My; public StoreName StoreName { get { return _StoreName; } set { SetField(ref _StoreName, value, "StoreName"); } }
        bool _IsLoading = false; public bool IsLoading { get { return _IsLoading; } set { SetField(ref _IsLoading, value, "IsLoading"); } }
        SSLCertVM _SelectedSSLCert = null; public SSLCertVM SelectedSSLCert { get { return _SelectedSSLCert; } set { SetField(ref _SelectedSSLCert, value, "SelectedSSLCert"); } }
        public ObservableCollection<SSLCertVM> SSLCerts { get; set; } = new ObservableCollection<SSLCertVM>();


        public ICommand StoreRefreshClickCommand
        {
            get
            {
                return new DelegateCommand<object>(async (c) => await LoadVM(), (c) => true);
            }
        }

        //TODO
        //Import Nuget Package CERTES
        // https://github.com/fszlin/certes
        //public ICommand NewSSLCertClickCommand { get { return new DelegateCommand<object>(async (c) => 
        //{
        //    var k = await Services.GetLetsEncryptCertificate("azrinsani@gmail.com"," *.vlixplatform.com");
        //}, (c) => true); } }
    }

    public class SSLCertVM : INotifyPropertyChanged
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

        public SSLCertVM(X509Certificate2 x509Certificate, SelectSSLCertVM selectSSLCertVM)
        {
            this.ParentVM = selectSSLCertVM;
            this.Expiry = x509Certificate.NotAfter;
            this.FriendlyName = x509Certificate.FriendlyName;
            this.Subject = x509Certificate.Subject;
            this.IssuedTo = x509Certificate.Subject.ToFirstRegex("(?<=CN=)[^,]*");
            this.IssuedBy = x509Certificate.Issuer.ToFirstRegex("(?<=CN=)[^,]*");
            this.ThumbPrint = x509Certificate.Thumbprint;
            this.StoreName = selectSSLCertVM.StoreName;
            this.SubjectAlternativeNames.Clear();
            foreach (X509Extension extension in x509Certificate.Extensions)
            {
                if (extension.Oid.FriendlyName == "Subject Alternative Name")
                {
                    AsnEncodedData data = new AsnEncodedData(extension.Oid, extension.RawData);
                    string[] sANs = data.Format(true).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var sAN in sANs)
                    {
                        var sANOnly = sAN.ToFirstRegex("(?<=Name=)[^,]*");
                        if (!string.IsNullOrWhiteSpace(sANOnly)) this.SubjectAlternativeNames.Add(sANOnly);
                    }
                    break;
                }              
            }

            //this.SubjectAlternativeNames = 
        }

        public SelectSSLCertVM ParentVM;
        string _Subject = null; public string Subject { get { return _Subject; } set { SetField(ref _Subject, value, "Subject"); } }
        string _IssuedTo = null; public string IssuedTo { get { return _IssuedTo; } set { SetField(ref _IssuedTo, value, "IssuedTo"); } }
        string _IssuedBy = null; public string IssuedBy { get { return _IssuedBy; } set { SetField(ref _IssuedBy, value, "IssuedBy"); } }
        DateTime? _Expiry = null; public DateTime? Expiry { get { return _Expiry; } set { SetField(ref _Expiry, value, "Expiry"); } }
        string _FriendlyName = null; public string FriendlyName { get { return _FriendlyName; } set { SetField(ref _FriendlyName, value, "FriendlyName"); } }
        string _ThumbPrint = null; public string ThumbPrint { get { return _ThumbPrint; } set { SetField(ref _ThumbPrint, value, "ThumbPrint"); } }
        StoreName _StoreName = StoreName.My; public StoreName StoreName { get { return _StoreName; } set { SetField(ref _StoreName, value, "StoreName"); } }
        public ObservableCollection<string> SubjectAlternativeNames { get; set; } = new ObservableCollection<string>();
        public ICommand SelectSSLCertClickCommand { get { return new DelegateCommand<object>((c) => 
        { 
            this.ParentVM.SelectedSSLCert = this;
            this.ParentVM.ParentVM.ShowSelectSSLCertWindow = false;
            this.ParentVM.ParentVM.SSLCertificateSubjectName = this.Subject;
            this.ParentVM.ParentVM.SSLCertificateStoreName = this.StoreName;
            this.ParentVM.ParentVM.SubjectAlternativeNames.Clear();
            foreach (var s in this.SubjectAlternativeNames) this.ParentVM.ParentVM.SubjectAlternativeNames.Add(s);

        }, (c) => true); } }

        
    }

}