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


        public Func<StoreName, StoreLocation, Task<List<SSLCertVM>>> OnRefresh;
        public async Task Refresh()
        {
            List<SSLCertVM> certs;
            try
            {
                if (this.OnRefresh == null) return;
                this.IsLoading = true;
                certs = await this.OnRefresh?.Invoke(this.StoreName, StoreLocation.LocalMachine);
                if (certs != null)
                {
                    this.SSLCerts.Clear();
                    foreach (var c in certs) { this.SSLCerts.Add(c); c.ParentVM = this; }
                }
            }
            catch (Exception ex)
            {
                certs = null;
            }
            finally
            {
                this.IsLoading = false;
            }           
        }
        bool _EnableCache = false; public bool EnableCache { get { return _EnableCache; } set { SetField(ref _EnableCache, value, "EnableCache"); } }
        StoreName _StoreName = StoreName.My; public StoreName StoreName { get { return _StoreName; } set { SetField(ref _StoreName, value, "StoreName"); } }
        bool _IsLoading = false; public bool IsLoading { get { return _IsLoading; } set { SetField(ref _IsLoading, value, "IsLoading"); } }
        SSLCertVM _SelectedSSLCert = null; public SSLCertVM SelectedSSLCert { get { return _SelectedSSLCert; } set { SetField(ref _SelectedSSLCert, value, "SelectedSSLCert"); } }
        public ObservableCollection<SSLCertVM> SSLCerts { get; set; } = new ObservableCollection<SSLCertVM>();


        public ICommand StoreRefreshClickCommand { get { return new DelegateCommand<object>(async (c) => await Refresh(), (c) => true); }}
        //TODO
        //Import Nuget Package CERTES
        // https://github.com/fszlin/certes
        //public ICommand NewSSLCertClickCommand { get { return new DelegateCommand<object>(async (c) => 
        //{
        //    var k = await Services.GetLetsEncryptCertificate("azrinsani@gmail.com"," *.vlixplatform.com");
        //}, (c) => true); } }
    }

}