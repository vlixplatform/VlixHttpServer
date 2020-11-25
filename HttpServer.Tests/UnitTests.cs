using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Vlix.HttpServer;
using Xunit;
using Xunit.Abstractions;
using static Vlix.HttpServer.Redirect;

namespace HttpServer.Tests
{

    public class UnitTests
    {

        private readonly ITestOutputHelper output;
        public UnitTests(ITestOutputHelper output)
        {
            this.output = output;
        }


        //[Theory(Skip ="No Need")]        
        [Theory]
        [InlineData("/questions/9122708/unit-sharp.html", "C:\\www\\questions\\9122708\\unit-sharp.html", "C:\\www\\questions\\9122708",true)]                
        [InlineData("/questions/aaa/test.html", "C:\\www\\questions\\aaa\\test.html", "C:\\www\\questions\\aaa", true)]
        [InlineData("/questions/aaa/unit", "", "C:\\www\\questions\\aaa\\unit", true)]
        [InlineData("/questions/aaa/", "", "C:\\www\\questions\\aaa", true)]
        [InlineData("/questions/aaa.html/", "", "C:\\www\\questions\\aaa.html", true)]
        [InlineData("/questions/aaa.html/sss", "", "C:\\www\\questions\\aaa.html\\sss", true)]
        [InlineData("/questions/aaa.html/rt.htm", "C:\\www\\questions\\aaa.html\\rt.htm", "C:\\www\\questions\\aaa.html", true)]
        [InlineData("", "", "C:\\www", true)]
        [InlineData("  ", "", "C:\\www", true)]
        [InlineData("  /my folder /", "","C:\\www\\my folder", true)]
        [InlineData("  /my folder /test.htm", "C:\\www\\my folder\\test.htm", "C:\\www\\my folder",true)]
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
        [InlineData("/ss/../secret.txt", null, null, false )]      
        public void ParseAbsolutePath(string absolutePath, string xFileToRead, string xFileToReadDir, bool ErrorMsgIsNull)
        {
            Vlix.HttpServer.HttpServer vlixHttpServer = new Vlix.HttpServer.HttpServer("C:\\www");
            output.WriteLine("absolutePath  = " + absolutePath);
            vlixHttpServer.TryParseAbsolutePath(absolutePath, out string fileToRead, out string fileToReadDir, out string errorMsg);
            output.WriteLine("fileToRead    = " + fileToRead);
            output.WriteLine("fileToReadDir = " + fileToReadDir);
            output.WriteLine("errorMsg      = " + errorMsg);
            Assert.Equal(xFileToRead, fileToRead);
            Assert.Equal(xFileToReadDir, fileToReadDir);
            Assert.Equal(ErrorMsgIsNull, errorMsg==null);
        }



        //[Theory]
        //[InlineData(new Redirect() { From = new RedirectFrom() { AnyHostName=false, HostNameWildCard=null, AnyPort=false, Port=80,AnyPath=true,PathWildCard=null}, To= new RedirectTo() { HTTPS=true,HostName=null,Port= } }, null, null, false)]
        //public string Process(Redirect redirect, string requestURL, string redirectURL)
        //{
            
        //}



        [Fact]
        public async void CacheTest()
        {
            string tempWWWPath = Path.Combine(Path.GetTempPath(),"www");
            Vlix.HttpServer.HttpServer vlixHttpServer = new Vlix.HttpServer.HttpServer(tempWWWPath, 80,443,true,10,10,true);
            string LongText = "";
            for (int n2 = 0; n2 < 10000; n2++) LongText += Guid.NewGuid().ToString() + "<br />";
            for (int n =0;n<30;n++)
            {
                string tempFile = Path.Combine(tempWWWPath, "test" + n + ".html");
                
                Directory.CreateDirectory(tempWWWPath);
                if (!File.Exists(tempFile)) File.WriteAllText(tempFile, "<h1>TEST" + n + "</h1>" + LongText);
            }
            HTTPStreamResult hTTPStreamResult = null;
            vlixHttpServer.OnHTTPStreamResult = (sR) => hTTPStreamResult = sR;
            vlixHttpServer.Start();
            double prevTotalCacheInKB = vlixHttpServer.CacheFiles.TotalCacheInKB;
            using (var httpClient = new HttpClient())
            {
                for (int n = 0; n < 30; n++)
                {
                    await httpClient.GetAsync("http://localhost/test" + n + ".html");
                    Assert.True(!hTTPStreamResult.ObtainedFromCache);
                    await httpClient.GetAsync("http://localhost/test" + n + ".html");
                    Assert.True(hTTPStreamResult.ObtainedFromCache);
                    if (vlixHttpServer.CacheFiles.TotalCacheInKB < vlixHttpServer.MaximumCacheSizeInMB * 1024)
                    {
                        Assert.True(vlixHttpServer.CacheFiles.TotalCacheInKB > prevTotalCacheInKB);
                        if (vlixHttpServer.CacheFiles.TotalCacheInKB > prevTotalCacheInKB) prevTotalCacheInKB = vlixHttpServer.CacheFiles.TotalCacheInKB;
                    }
                    else
                    {
                        await Task.Delay(6000); //Wait for Cache to Clean
                        Assert.True(vlixHttpServer.CacheFiles.TotalCacheInKB < vlixHttpServer.MaximumCacheSizeInMB *1024); //The Cache Cleaned must be lower than Max Cache
                    }
                }
            }

        }

    }
}
