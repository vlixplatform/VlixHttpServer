﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Vlix.HttpServer;
namespace Vlix.ServerConfigUI
{
    public class HttpServerConfigVMSample : HttpServerConfigVM
    {
        public HttpServerConfigVMSample()
        {
            this.ShowAdvanceSettingsWindow = false;
            this.Rules.Add(new RuleVM(new HostAlternativeWWWDirectoryRule("asd", "addd"), this));
        }
    }
    public class ServerConfigVMSample : ServerConfigVM
    {
        public ServerConfigVMSample()
        {
            this.ShowConfigSettingsWindow = false;
            this.ShowLoginWindow = false;
        }
    }
}
