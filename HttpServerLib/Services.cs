using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
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

   

        public static void SaveServerConfig(HttpServerConfig httpServerConfig)
        {
            httpServerConfig.SaveConfigFile();
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