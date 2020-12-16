using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Vlix.HttpServer;
using Vlix;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Vlix.HttpServer.Tests
{
    public class UnitTests
    {
        string tempWWWPath, altTempWWWPath;
        private readonly ITestOutputHelper output;
        public UnitTests(ITestOutputHelper output)
        {
            this.output = output;
            tempWWWPath = Path.Combine(Path.GetTempPath(), "VlixWebServer", "General", "www");
            altTempWWWPath = Path.Combine(Path.GetTempPath(), "VlixWebServer", "Alternative", "www");
        }


        //[Theory(Skip ="No Need")]        
        [Theory]
        [InlineData("/questions/9122708/unit-sharp.html", "C:\\www\\questions\\9122708\\unit-sharp.html", "C:\\www\\questions\\9122708", true)]
        [InlineData("/questions/aaa/test.html", "C:\\www\\questions\\aaa\\test.html", "C:\\www\\questions\\aaa", true)]
        [InlineData("/questions/aaa/unit", "", "C:\\www\\questions\\aaa\\unit", true)]
        [InlineData("/questions/aaa/", "", "C:\\www\\questions\\aaa", true)]
        [InlineData("/questions/aaa.html/", "", "C:\\www\\questions\\aaa.html", true)]
        [InlineData("/questions/aaa.html/sss", "", "C:\\www\\questions\\aaa.html\\sss", true)]
        [InlineData("/questions/aaa.html/rt.htm", "C:\\www\\questions\\aaa.html\\rt.htm", "C:\\www\\questions\\aaa.html", true)]
        [InlineData("", "", "C:\\www", true)]
        [InlineData("  ", "", "C:\\www", true)]
        [InlineData("  /my folder /", "", "C:\\www\\my folder", true)]
        [InlineData("  /my folder /test.htm", "C:\\www\\my folder\\test.htm", "C:\\www\\my folder", true)]
        [InlineData("/", "", "C:\\www", true)]
        [InlineData("/  ", "", "C:\\www", true)]
        [InlineData("/ss", "", "C:\\www\\ss", true)]
        [InlineData("aa/bb", "", "C:\\www\\aa\\bb", true)]
        [InlineData("/ aa/bb", "", "C:\\www\\ aa\\bb", true)]
        [InlineData("/ aa/bb/c.html", "C:\\www\\ aa\\bb\\c.html", "C:\\www\\ aa\\bb", true)]
        [InlineData(" aa/bb/c.html", "C:\\www\\ aa\\bb\\c.html", "C:\\www\\ aa\\bb", true)]
        [InlineData("/ aa /bb/c.html", "C:\\www\\ aa\\bb\\c.html", "C:\\www\\ aa\\bb", true)]
        [InlineData("/ aa / bb/ c.html", "C:\\www\\ aa\\ bb\\ c.html", "C:\\www\\ aa\\ bb", true)]
        [InlineData("/ aa / bb/ c .html", "C:\\www\\ aa\\ bb\\ c .html", "C:\\www\\ aa\\ bb", true)]
        [InlineData("/ aa / bb/ c   .html", "C:\\www\\ aa\\ bb\\ c   .html", "C:\\www\\ aa\\ bb", true)]
        [InlineData("/ aa / bb/ c.html ", "C:\\www\\ aa\\ bb\\ c.html", "C:\\www\\ aa\\ bb", true)]
        [InlineData("/ aa /bb", "", "C:\\www\\ aa\\bb", true)]
        [InlineData(" aa /bb", "", "C:\\www\\ aa\\bb", true)]
        [InlineData("/ aa / bb", "", "C:\\www\\ aa\\ bb", true)]
        [InlineData("/ aa / bb ", "", "C:\\www\\ aa\\ bb", true)]
        [InlineData(" aa / bb ", "", "C:\\www\\ aa\\ bb", true)]
        [InlineData("    / aa / bb ", "", "C:\\www\\ aa\\ bb", true)]
        [InlineData("    / aa / bb /c.html ", "C:\\www\\ aa\\ bb\\c.html", "C:\\www\\ aa\\ bb", true)]
        [InlineData("    / aa / bb / c.html ", "C:\\www\\ aa\\ bb\\ c.html", "C:\\www\\ aa\\ bb", true)]
        [InlineData("/my folder/", "", "C:\\www\\my folder", true)]
        [InlineData("/my folder", "", "C:\\www\\my folder", true)]
        [InlineData("/my folder/test page.html", "C:\\www\\my folder\\test page.html", "C:\\www\\my folder", true)]
        [InlineData("/ss.avi", "C:\\www\\ss.avi", "C:\\www", true)]
        [InlineData("/ss/../secret.txt", null, null, false)]
        public void ParseAbsolutePath(string absolutePath, string xFileToRead, string xFileToReadDir, bool ErrorMsgIsNull)
        {
            HttpServer vlixHttpServer = new Vlix.HttpServer.HttpServer("C:\\www", 80);
            output.WriteLine("absolutePath  = " + absolutePath);
            vlixHttpServer.TryParseAbsolutePath(vlixHttpServer.Config.WWWDirectoryParsed(), absolutePath, out string fileToRead, out string fileToReadDir, out string errorMsg);
            output.WriteLine("fileToRead    = " + fileToRead);
            output.WriteLine("fileToReadDir = " + fileToReadDir);
            output.WriteLine("errorMsg      = " + errorMsg);
            Assert.Equal(xFileToRead, fileToRead);
            Assert.Equal(xFileToReadDir, fileToReadDir);
            Assert.Equal(ErrorMsgIsNull, errorMsg == null);
        }




        private void CreateWWWDirectory(string parent, string wWWPath)
        {
            string LongText = "";
            for (int n = 1; n < 10; n++)
            {
                string tempFile = Path.Combine(wWWPath, "Page" + n + ".html");
                Directory.CreateDirectory(wWWPath);
                if (!File.Exists(tempFile)) File.WriteAllText(tempFile, "<h1>Hello World From " + parent + "/Page" + n + "!/h1>" + LongText);
            }
        }

        [Theory]
        [InlineData("CN=azrin.vlix.me",StoreName.My)]
        public async void SSLCertTest(string subjectName, StoreName storeName)
        {
            HttpServer vlixHttpServer = new HttpServer(tempWWWPath, 80, 443, "ThisSSLCERTDoesNotExist", StoreName.My, true, 10, 10, true);
            bool errorThrown = false;
            try { await vlixHttpServer.StartAsync(); } catch { errorThrown = true; }
            Assert.True(errorThrown);


            var certs = await SSLCertificateServices.GetSSLCertificates(storeName);
            X509Certificate2 certToUse = null;
            foreach (var cert in certs)
            {
                if (cert.Subject == subjectName) certToUse = cert;
            }
            if (certToUse == null) { throw new Exception("This test requires the Certificate with subject = '" + subjectName + "', to exist in the system. Skip this test if needed"); }

            var res = await SSLCertificateServices.TryFindAndBindLatestSSLCertToPort(443, subjectName, storeName);
            Assert.True(res);

            var certExists = SSLCertificateServices.SSLCertBinded(certToUse.Thumbprint, 443);
            Assert.True(certExists);
            var certExists2 = SSLCertificateServices.SSLCertBinded("some sillly thumbprint that does not exist", 443);
            Assert.True(!certExists2);
            var certExists3 = SSLCertificateServices.SSLCertBinded("some sillly thumbprint that does not exist", 442);//Different Port
            Assert.True(!certExists3);
        }    


        [Fact]
        public async void ReverseProxyRuleTest()
        {
            HttpServer vlixHttpServer = new HttpServer(tempWWWPath, 80, 443, "CN=azrin.vlix.me", StoreName.My, true, 10, 10, true);
            await vlixHttpServer.StartAsync();
            HttpServer reverseProxiedServer = new HttpServer(altTempWWWPath, 5072);
            reverseProxiedServer.WebAPIs.Add(new WebAPIAction("/api/users/search", async (req) =>
            {
                if (req.Query.TryGetValue("country", out string country) && req.Query.TryGetValue("name", out string name))
                {
                    await req.RespondWithText("Search Results for Name='" + name + "', Country='" + country + "'......");
                }
                else await req.RespondEmpty(HttpStatusCode.NotFound);
            }));
            await reverseProxiedServer.StartAsync();

            CreateWWWDirectory("General", tempWWWPath);
            CreateWWWDirectory("Alternative", altTempWWWPath);

            using (var httpClient = new HttpClient())
            {
                //If reqeust on localhost/Page8.html => it will reverse proxy to localhost:5072/Page8.html
                vlixHttpServer.Config.Rules.Clear();
                vlixHttpServer.Config.Rules.Add(new SimpleReverseProxyRule("localhost", "/Page8.html", 5072));
                var resp = await httpClient.GetAsync("http://localhost/Page8.html");
                var revProxyResult = await resp.Content.ReadAsStringAsync();
                Assert.Equal("<h1>Hello World From Alternative/Page8!/h1>", revProxyResult);



                //This should reverse proxy to http://localhost:5072/api/user/search?country=Malaysia&name=Azrin
                vlixHttpServer.Config.Rules.Add(new Rule()
                {
                    RequestMatch = new RequestMatch()
                    {
                        CheckPath = true,
                        PathMatch = "^/[^/]*/[^/]*",
                        PathMatchType = MatchType.Regex
                    },
                    ResponseAction = new ReverseProxyAction()
                    {
                        SetScheme = true,                        
                        Scheme = Scheme.http,
                        SetPort = true,
                        Port = 5072,
                        SetPath = true,
                        UsePathVariable = true,
                        PathAndQuery = "/api/users/search?country=%(?<=^/)[^/]*(?=/)%&name=%(?<=^/[^/]*/)[^/]*(?=/?)%"
                    }
                });
                var resp2 = await httpClient.GetAsync("http://localhost/Malaysia/Azrin"); //This should reverse proxy to http://localhost:5072/api/user/search?country=Malaysia&name=Azrin
                var revProxyResult2 = await resp2.Content.ReadAsStringAsync();

                Assert.Equal("Search Results for Name='Azrin', Country='Malaysia'......", revProxyResult2);

               

            }
            vlixHttpServer.Stop();
        }

        [Fact]
        public async void OtherRulesTest()
        {
            HttpServer vlixHttpServer = new HttpServer(tempWWWPath, 80, 443, "CN=azrin.vlix.me", StoreName.My, true, 10, 10, true);
            await vlixHttpServer.StartAsync();

            CreateWWWDirectory("General", tempWWWPath);
            CreateWWWDirectory("Alternative", altTempWWWPath);

            using (var httpClient = new HttpClient())
            {
                //Test Redirect
                vlixHttpServer.Config.Rules.Add(new SimplePathRedirectRule("/Page1.html", "/Page2.html"));
                var resp = await httpClient.GetAsync("http://localhost/Page1.html"); //This should redirect to page2
                var redirectResult1 = await resp.Content.ReadAsStringAsync();
                Assert.Equal("<h1>Hello World From General/Page2!/h1>", redirectResult1);

                //Test Redirect Failure
                vlixHttpServer.Config.Rules.Add(new SimplePathRedirectRule("/Page3.html", "/SomePageThatDoesNotExist.html"));
                resp = await httpClient.GetAsync("http://localhost/Page3.html");
                Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);

                //Test Alternative WWW Directory
                vlixHttpServer.Config.Rules.Add(new Rule()
                {
                    RequestMatch = new RequestMatch() { CheckPath = true, PathMatch = "*Page5.*", PathMatchType = MatchType.Wildcard },
                    ResponseAction = new AlternativeWWWDirectoryAction() { AlternativeWWWDirectory = altTempWWWPath }
                });
                resp = await httpClient.GetAsync("http://localhost/Page5.html");
                var altWWWResult = await resp.Content.ReadAsStringAsync();
                Assert.Equal("<h1>Hello World From Alternative/Page5!/h1>", altWWWResult);

                //Test Alternative WWW Directory Failure
                vlixHttpServer.Config.Rules.Add(new Rule()
                {
                    RequestMatch = new RequestMatch() { CheckPath = true, PathMatch = "*Page6.*", PathMatchType = MatchType.Wildcard },
                    ResponseAction = new AlternativeWWWDirectoryAction() { AlternativeWWWDirectory = "C:\\SomeDirectoryThatDoesNotExist" }
                });
                resp = await httpClient.GetAsync("http://localhost/Page6.html");
                var altWWWResultFailure = await resp.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
                Assert.Equal(@"File 'C:\SomeDirectoryThatDoesNotExist\Page6.html' does not exist!", altWWWResultFailure);


                //Test Deny Action
                vlixHttpServer.Config.Rules.Add(new SimplePathDenyRule("/Page7.html*"));
                resp = await httpClient.GetAsync("http://localhost/Page7.html");
                Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
            }
            vlixHttpServer.Stop();
        }


        [Fact]
        public async void CacheTest()
        {
            //C:\Users\azrin\AppData\Local\Temp\VlixWebServer\CacheTest\www
            string tempCacheTestPath = Path.Combine(Path.GetTempPath(), "VlixWebServer", "CacheTest", "www");
            HttpServer vlixHttpServer = new HttpServer(tempCacheTestPath, 80, 443, null, StoreName.My, true, 10, 10, true);
            string LongText = "";
            for (int n2 = 0; n2 < 10000; n2++) LongText += Guid.NewGuid().ToString() + "<br />";
            for (int n = 0; n < 30; n++)
            {
                string tempFile = Path.Combine(tempCacheTestPath, "test" + n + ".html");

                Directory.CreateDirectory(tempCacheTestPath);
                if (!File.Exists(tempFile)) File.WriteAllText(tempFile, "<h1>TEST" + n + "</h1>" + LongText);
            }
            await vlixHttpServer.StartAsync();
            double prevTotalCacheInKB = vlixHttpServer.CacheFiles.TotalCacheInKB;
            using (var httpClient = new HttpClient())
            {
                for (int n = 0; n < 30; n++)
                {
                    await httpClient.GetAsync("http://localhost/test" + n + ".html");
                    await httpClient.GetAsync("http://localhost/test" + n + ".html");
                    if (vlixHttpServer.CacheFiles.TotalCacheInKB < vlixHttpServer.Config.MaximumCacheSizeInMB * 1024)
                    {
                        Assert.True(vlixHttpServer.CacheFiles.TotalCacheInKB > prevTotalCacheInKB);
                        if (vlixHttpServer.CacheFiles.TotalCacheInKB > prevTotalCacheInKB) prevTotalCacheInKB = vlixHttpServer.CacheFiles.TotalCacheInKB;
                    }
                    else
                    {
                        await Task.Delay(6000); //Wait for Cache to Clean
                        Assert.True(vlixHttpServer.CacheFiles.TotalCacheInKB < vlixHttpServer.Config.MaximumCacheSizeInMB * 1024); //The Cache Cleaned must be lower than Max Cache
                    }
                }
            }

        }



        [Fact]
        public async void TestWebAPI()
        {
            CreateWWWDirectory("General", tempWWWPath);
            HttpServer httpServer = new HttpServer(tempWWWPath,5022);
            
            httpServer.WebAPIs = new List<WebAPIAction>();
            httpServer.WebAPIs.Add(new WebAPIAction(
                "/testAPI/",
                async (webAPIActionInput) => {
                    dynamic TestAPIResult = new { Id = webAPIActionInput.Query["id"].ToInt(), Name = webAPIActionInput.Query["name"] };
                    await webAPIActionInput.RespondWithJson(TestAPIResult).ConfigureAwait(false);
                },
            "Get"));
            httpServer.WebAPIs.Add(new WebAPIAction(
                "/testAPI2/",
                async (webAPIActionInput) => {
                    await webAPIActionInput.RespondWithJson(webAPIActionInput).ConfigureAwait(false); //This should cause a serialization error
                },
            "Get"));
            bool resStart = await httpServer.StartAsync();
            Assert.True(resStart);
            using (var httpClient = new HttpClient())
            {
                var res = await httpClient.GetAsync("http://localhost:5022/testAPI/?id=234&name=%27my%20api%27");
                string bodyStr = await res.Content.ReadAsStringAsync();
                dynamic resObj = JObject.Parse(bodyStr);
                Assert.True(resObj.Id == 234);
                Assert.True(resObj.Name == "'my api'");


                var res2 = await httpClient.GetAsync("http://localhost:5022/testAPI2/");//This should cause a serialization error
                Assert.Equal(HttpStatusCode.InternalServerError, res2.StatusCode);
                string bodyStr2 = await res2.Content.ReadAsStringAsync();
            }
        }

        [Fact]
        public async void TestWebServerComms()
        {
            WebServer webServer = new WebServer();
            var resStart = await webServer.StartAsync();
            Assert.True(resStart);
            using (var httpClient = new OPHttpClient())
            {
                StoreName storeName = StoreName.My; StoreLocation storeLocation = StoreLocation.LocalMachine;
                List<SSLCertVM> certs = await httpClient.RequestJsonAsync<List<SSLCertVM>>(HttpMethod.Get, "http://localhost:5024/config/getsslcerts?storename="+storeName + "&storelocation=" + storeLocation, "Administrator", "Admin");
                Assert.NotNull(certs);

                WebServerConfig config = await httpClient.RequestJsonAsync<WebServerConfig>(HttpMethod.Get, "http://localhost:5024/config/load", "Administrator","Admin");
                Assert.NotNull(config);

                var req2 = await httpClient.RequestStatusOnlyAsync(HttpMethod.Put, "http://localhost:5024/config/save", "Administrator", "Admin", new StringContent(JsonConvert.SerializeObject(config), Encoding.UTF8, "application/json"));                
                Assert.Equal(HttpStatusCode.OK, req2);

                var req3 = await httpClient.RequestStatusOnlyAsync(HttpMethod.Put, "http://localhost:5024/config/save", "Administrator", "Admin", new StringContent("Some Random Data which is not a proper Json", Encoding.UTF8, "application/json"));
                Assert.Equal(HttpStatusCode.InternalServerError, req3);
            }
        }
    }
}
