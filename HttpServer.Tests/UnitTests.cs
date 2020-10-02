using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vlix;
using Xunit;
using Xunit.Abstractions;

namespace HttpServer.Tests
{

    public class UnitTests
    {

        private readonly ITestOutputHelper output;
        public UnitTests(ITestOutputHelper output)
        {
            this.output = output;
        }


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
            VlixHttpServer vlixHttpServer = new VlixHttpServer("C:\\www");
            output.WriteLine("absolutePath  = " + absolutePath);
            vlixHttpServer.TryParseAbsolutePath(absolutePath, out string fileToRead, out string fileToReadDir, out string errorMsg);
            output.WriteLine("fileToRead    = " + fileToRead);
            output.WriteLine("fileToReadDir = " + fileToReadDir);
            output.WriteLine("errorMsg      = " + errorMsg);
            Assert.Equal(xFileToRead, fileToRead);
            Assert.Equal(xFileToReadDir, fileToReadDir);
            Assert.Equal(ErrorMsgIsNull, errorMsg==null);
        }
    }
}
