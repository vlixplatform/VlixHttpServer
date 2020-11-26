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
//using Certes;
//using Certes.Acme;

namespace  Vlix.HttpServer
{
    public class Services
    {
        public static Task<HttpServerConfig> LoadHttpServerConfig()
        {
            HttpServerConfig config = new HttpServerConfig();
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