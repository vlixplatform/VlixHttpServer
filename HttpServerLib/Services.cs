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
using System.Configuration;
using Newtonsoft.Json;
//using Certes;
//using Certes.Acme;

namespace  Vlix.HttpServer
{
    
    public class Services
    {
        static string _PasswordField = null;
        public static string PasswordField { get { if (_PasswordField == null) _PasswordField = Guid.NewGuid().ToString().Substring(0, 12); return _PasswordField; } }
        public static OPHttpClient OPHttpClient = new OPHttpClient();
        public static object Config; //Place holder for Configuration Settings
        public static void SaveConfigFile<T>(string configFileName, T configObject)
        {
            string appDirectory = ConfigurationManager.AppSettings.Get("AppDirectory");
            if (appDirectory == null) appDirectory = Environment.CurrentDirectory;
            else appDirectory = appDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            Directory.CreateDirectory(appDirectory);
            string configFilePath = Path.Combine(appDirectory, configFileName);
            string jsonString = JsonConvert.SerializeObject(configObject, Formatting.Indented, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            File.WriteAllText(configFilePath, jsonString);
        }

        public static T LoadConfigFile<T>(string configFileName) where T: new()
        {
            T config = new T();
            string appDirectory = ConfigurationManager.AppSettings.Get("AppDirectory");
            if (appDirectory == null) appDirectory = Environment.CurrentDirectory;
            else appDirectory = appDirectory.Replace("[ProgramDataDirectory]", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            Directory.CreateDirectory(appDirectory);
            string configFilePath = Path.Combine(appDirectory, configFileName);
            if (File.Exists(configFilePath))
            {
                string configJSONStr = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<T>(configJSONStr, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            }
            else
            {
                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, jsonString);
            }
            return config;
        }

        //static HttpServerConfig config;
        //public static Task<HttpServerConfig> LoadHttpServerConfig()
        //{
        //    config = new HttpServerConfig();
        //    config.LoadConfigFile();
        //    return Task.FromResult(config);
        //}



        //public static void SaveServerConfig(HttpServerConfig httpServerConfig)
        //{
        //    httpServerConfig.SaveConfigFile();
        //}


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