using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vlix
{
    class Program
    {
        static void Main(string[] args)
        {
            VlixHttpServer vlixHttpServer = new VlixHttpServer(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Vlix\\SimpleHttpServer\\");
            vlixHttpServer.Start();
        }
    }
}
