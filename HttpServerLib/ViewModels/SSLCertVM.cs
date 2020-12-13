using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Vlix.HttpServer;
namespace Vlix.HttpServer
{
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

        public SSLCertVM()
        {

        }
        public SSLCertVM(X509Certificate2 x509Certificate, StoreName store, SelectSSLCertVM selectSSLCertVM)
        {
            this.ParentVM = selectSSLCertVM;
            this.Expiry = x509Certificate.NotAfter;
            this.FriendlyName = x509Certificate.FriendlyName;
            this.Subject = x509Certificate.Subject;
            this.IssuedTo = x509Certificate.Subject.ToFirstRegex("(?<=CN=)[^,]*");
            this.IssuedBy = x509Certificate.Issuer.ToFirstRegex("(?<=CN=)[^,]*");
            this.ThumbPrint = x509Certificate.Thumbprint;
            this.StoreName = store;
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
    }

}