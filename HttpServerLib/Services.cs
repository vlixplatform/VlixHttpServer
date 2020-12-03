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
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
//using Certes;
//using Certes.Acme;

namespace  Vlix.HttpServer
{
    public class Services
    {


        static HttpServerConfig config;
        public static Task<HttpServerConfig> LoadHttpServerConfig()
        {
            config = new HttpServerConfig();
            config.LoadConfigFile();
            return Task.FromResult(config);
        }

        public static Task<List<X509Certificate2>> GetSSLCertificates(StoreName storeName)
        {
            X509Store store = new X509Store(storeName, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            List<X509Certificate2> output = new List<X509Certificate2>();
            foreach (X509Certificate2 certificate in store.Certificates) output.Add(certificate);
            return Task.FromResult(output);
        }


        public static string LocateDirectory()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            else return "";
        }

        public static void SaveServerConfig(HttpServerConfigVM httpServerConfigVM)
        {
            HttpServerConfig httpServerConfig = new HttpServerConfig()
            {
                SSLCertificateStoreName = httpServerConfigVM.SSLCertificateStoreName,
                SSLCertificateSubjectName = httpServerConfigVM.SSLCertificateSubjectName,
                EnableHTTPS = httpServerConfigVM.EnableHTTPS,
                HTTPSPort = httpServerConfigVM.HTTPSPort,
                EnableHTTP = httpServerConfigVM.EnableHTTP,
                HTTPPort = httpServerConfigVM.HTTPPort,
                AllowLocalhostConnectionsOnly = httpServerConfigVM.AllowLocalhostConnectionsOnly,
                EnableCache = httpServerConfigVM.EnableCache,
                LogDirectory = httpServerConfigVM.LogDirectory,
                MaximumCacheSizeInMB = httpServerConfigVM.MaximumCacheSizeInMB,
                OnlyCacheItemsLessThenMB = httpServerConfigVM.OnlyCacheItemsLessThenMB,
                WWWDirectory = httpServerConfigVM.WWWDirectory,
            };
            foreach (var ruleVM in httpServerConfigVM.Rules)
            {
                Rule r = new Rule()
                {
                    Name = ruleVM.Name,
                    Enable = ruleVM.Enable,
                    RequestMatch = new RequestMatch()
                    {
                        CheckHostName = ruleVM.RequestMatch.CheckHostName,
                        CheckPath = ruleVM.RequestMatch.CheckPath,
                        CheckPort = ruleVM.RequestMatch.CheckPort,
                        HostNameMatch = ruleVM.RequestMatch.HostNameMatch,
                        HostNameMatchType = ruleVM.RequestMatch.HostNameMatchType,
                        PathMatch = ruleVM.RequestMatch.PathMatch,
                        PathMatchType = ruleVM.RequestMatch.PathMatchType,
                        Port = ruleVM.RequestMatch.Port
                    }
                };
                switch (ruleVM.ResponseAction.ActionType)
                {
                    case ActionType.AlternativeWWWDirectory:
                        r.ResponseAction = new AlternativeWWWDirectoryAction() { AlternativeWWWDirectory = ruleVM.ResponseAction.AlternativeWWWDirectory };
                        break;
                    case ActionType.Deny:
                        r.ResponseAction = new DenyAction();
                        break;
                    case ActionType.Redirect:
                        r.ResponseAction = new RedirectAction()
                        {
                            Scheme = ruleVM.ResponseAction.RedirectScheme,
                            RedirectScheme = ruleVM.ResponseAction.UseRedirectScheme,
                            HostName = ruleVM.ResponseAction.RedirectHostName,
                            RedirectHostName = ruleVM.ResponseAction.UseRedirectHostName,
                            Path = ruleVM.ResponseAction.RedirectPath,
                            RedirectPath = ruleVM.ResponseAction.UseRedirectPath,
                            Port = ruleVM.ResponseAction.RedirectPort,
                            RedirectPort = ruleVM.ResponseAction.UseRedirectPort
                        };
                        break;
                    case ActionType.ReverseProxy:
                        r.ResponseAction = new ReverseProxyAction()
                        {

                        };
                        break;
                }
                httpServerConfig.Rules.Add(r);
            }
            httpServerConfig.SaveConfigFile();
        }


        public static void RemoveSSLCertFromPort(string IP, int PortNumber, Action<string> OnInfoLog = null)
        {
            //OnInfoLog?.Invoke("Checking if any SSL Cert has been binded to " + IP + ":" + PortNumber + "...");
            var SSLCertCheckBindingCmdRes = ExecuteCommand("netsh http show sslcert " + IP +":" + PortNumber);
            if (SSLCertCheckBindingCmdRes.IndexOf("application", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string SSLCertDeleteCmd = "netsh http delete sslcert ipport=" + IP + ":" + PortNumber;
                OnInfoLog?.Invoke("Deleting SSL Cert binded Port via Windows Command: " + SSLCertDeleteCmd);
                string BindResultText = ExecuteCommand(SSLCertDeleteCmd).RemoveAllNewLines().Trim(' ');
                OnInfoLog?.Invoke(BindResultText);
            }
        }
        
        public async static Task<bool> TryBindSSLCertToPort(int PortNumber, string subject, StoreName storeName = StoreName.My, Action<string> OnInfoLog = null, Action<string> OnErrorLog = null, bool RemoveAnyPreviousBinding = true, string IP = "0.0.0.0")
        {
            //List<X509Certificate2> x509Certificates = new List<X509Certificate2>(); 
            var certs = await Services.GetSSLCertificates(storeName);
            X509Certificate2 x509Certificate = certs.Where(c => c.HasPrivateKey && c.Subject ==subject).OrderByDescending(c=>c.NotAfter).FirstOrDefault();
            if (x509Certificate==null) OnErrorLog.Invoke("Unable to find SSL Certificate with subject '" + subject + "' in certificte store '" + storeName.ToString() + "'");
            
            string applicationId = null;
            try
            {
                applicationId = ((GuidAttribute)Assembly.GetEntryAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;
            }
            catch
            { }
            if (applicationId == null) applicationId = Guid.NewGuid().ToString();


            //Remove any Previously Binded SSL Sert at PORT
            if (RemoveAnyPreviousBinding) RemoveSSLCertFromPort(IP,PortNumber, (log) => OnInfoLog.Invoke(log));

            string BindCommand = "netsh http add sslcert ipport=" + IP + ":" + PortNumber + " certhash=" + x509Certificate.Thumbprint + " appid={" + applicationId + "}";
            OnInfoLog?.Invoke("Binding SSL Certificate '" + subject + "' to " + IP + ":" + PortNumber); // + " via Command=" + BindCommand);
            try
            {
                string BindResultText = ExecuteCommand(BindCommand).RemoveAllNewLines().Trim(' ');
                OnInfoLog?.Invoke(BindResultText);
            }
            catch (Exception ex)
            {
                OnErrorLog.Invoke("Unable to bind generate SSL Certificate to Port " + PortNumber + "\r\n" + ex.ToString());
                return false;
            }
            return true;
        }



        public static string ExecuteCommand(string action)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (Process process = new Process
            {
                StartInfo = new ProcessStartInfo { WindowStyle = ProcessWindowStyle.Normal, FileName = "cmd.exe", UseShellExecute = false, RedirectStandardOutput = true, Arguments = "/c " + action }
            })
            {
                process.Start();
                while (!process.StandardOutput.EndOfStream) stringBuilder.AppendLine(process.StandardOutput.ReadLine());
                process.Close();
            }
            return stringBuilder.ToString();
        }
        //TODO
        //Import Nuget Package CERTES
        // https://github.com/fszlin/certes
        //public async static Task<X509Certificate2> GetLetsEncryptCertificate(string email, string certificateName)
        //{
        //    var acme = new AcmeContext(WellKnownServers.LetsEncryptStagingV2);
        //    var account = await acme.NewAccount(email, true);


        //    var order = await acme.NewOrder(new[] { certificateName });
        //    var authz = (await order.Authorizations()).First();
        //    var httpChallenge = await authz.Http();
        //    var keyAuthz = httpChallenge.KeyAuthz;

        //    return new X509Certificate2();
        //}
    }
}