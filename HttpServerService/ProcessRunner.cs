using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Vlix
{
    public class ProcessRunnerResult
    {
        public ProcessRunnerResult() { }
        public ProcessRunnerResult(bool Success) { this.Success = Success; }
        public ProcessRunnerResult(string ExStr) { this.Success = false; this.ExStr = ExStr; }
        public bool Success = true;
        public string ExStr = null;
    }

    public class ProcessRunner
    {
        public string Name { get; set; } = null;
        public string File { get; set; } = null;
        public string Arguments { get; set; } = null;
        public bool IsRunning { get; set; } = false;
        public bool AllowMultipleProcesses { get; set; } = false;
        public bool StopProcessWhenMainProgramEnds { get; set; } = true;
        public bool TerminateRunningProcessesBeforeStart { get; set; } = true;
        private static readonly object RunningProccessesUpdateLock = new object();

        public static Dictionary<string , ProcessRunner> RunningProcesses = new Dictionary<string, ProcessRunner>();
        public Action<string> OnOutput { get; set; } = null;
        private Process process;
        public Action OnExit { get; set; } = null;
        public ProcessRunner(string name, string file, string arguments, Action<string> onOutput = null)
        {            
            this.File = file;
            this.Name = name;
            this.Arguments = arguments;
            this.OnOutput = onOutput;
        }
        public ProcessRunner(string name, string file, Action<string> onOutput = null)
        {
            this.File = file;
            this.Name = name;
            this.OnOutput = onOutput;
        }
        public ProcessRunner(string file, Action<string> onOutput = null)
        {
            this.File = file;
            this.OnOutput = onOutput;
        }

          

        public ProcessRunnerResult TryStart(Action onExit = null)
        {
            try
            {                
                if (this.TerminateRunningProcessesBeforeStart)
                {
                    var processes = Process.GetProcessesByName("DummyProcess");
                    foreach (var p in processes) try { p.Kill(); } catch { }
                }
                string fileNameWithoutDirectoryPath = Path.GetFileName(this.File);
                if (string.IsNullOrWhiteSpace(fileNameWithoutDirectoryPath)) return new ProcessRunnerResult("Invalid File Path '" + this.File + "'. File name can't be empty");
                string filePathToExecute;
                if (fileNameWithoutDirectoryPath == this.File)
                {
                    var AppDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    filePathToExecute = Path.Combine(AppDir, fileNameWithoutDirectoryPath); 
                }
                else filePathToExecute = this.File;
                process = new MyProcess(this.Name);
                process.StartInfo.FileName = filePathToExecute;
                process.StartInfo.Arguments = this.Arguments;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                if (this.OnOutput != null)
                {
                    process.StartInfo.RedirectStandardOutput = true;
                    process.OutputDataReceived += (sender, data) => this.OnOutput.Invoke(data.Data);
                    process.StartInfo.RedirectStandardError = true;
                    process.ErrorDataReceived += (sender, data) => this.OnOutput.Invoke(data.Data);
                }
                if (onExit != null)
                {
                    this.OnExit = onExit;
                    process.EnableRaisingEvents = true;
                    process.Exited -= Process_Exited;
                    process.Exited += Process_Exited;
                }
                process.Start();
                if (this.OnOutput != null)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
                lock(RunningProccessesUpdateLock) ProcessRunner.RunningProcesses.Add(this.Name, this);
                this.IsRunning = true;
                return new ProcessRunnerResult();
            }
            catch (Exception ex)
            {
                return new ProcessRunnerResult(ex.ToString());
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            this.OnExit.Invoke();
            this.IsRunning = false;
        }

        /// <summary>
        /// Stops the current process. Upon calling this process, the process will terminate regardless. No exceptions will be thrown.
        /// </summary>
        public void Terminate()
        {
            try
            {
                this.process?.Kill();
            }
            catch { }
            finally
            {
                this.process = null;
                this.IsRunning = false;
            }
        }

        public class MyProcess : Process
        {
            public MyProcess(string processId) : base()
            {
                this.ProcessId = processId;
            }
            public string ProcessId { get; set; }
        }

    }






}
