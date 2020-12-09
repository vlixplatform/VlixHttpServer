using System;
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

namespace Vlix.ServerConfigUI
{
    public class UCLogConsoleVMSample : UCLogConsoleVM
    {
        public UCLogConsoleVMSample()
        {
            this.ShowTimeStamp = true;
            this.ConsoleLogsCollection = new ObservableCollection<ConsoleLogVM>
            {
                new ConsoleLogVM(DateTime.Now, "This is Log 1"),
                new ConsoleLogVM(DateTime.Now, "This is Log 2"),
                new ConsoleLogVM(DateTime.Now, "This is Log 3. This is a very long log. I want to see what Happens when the log is too long. Like not just too long but waayyyyy long. Like super duper long.. longer than longest. If anything was the longest. I should stop writing now cause its already starting to get toooooo long"),
                new ConsoleLogVM(DateTime.Now, "This is Log 4"),
                new ConsoleLogVM(DateTime.Now, "This is Log 5. This is a very long log. I want to see what Happens when the log is too long\r\n. Like not just too long but waayyyyy long. Like super duper long..\r\nlonger than longest. If anything was the longest. I should stop writing now cause its already starting to get toooooo long"),
                new ConsoleLogVM(DateTime.Now, "This is Log 6"),
                new ConsoleLogVM(DateTime.Now, "This is Log 7")
            };
        }
    }
}
