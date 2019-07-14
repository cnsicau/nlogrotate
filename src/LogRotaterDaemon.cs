using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace logrotate
{
    public class LogRotaterDaemon : IDisposable
    {
        public const string GlobalOptionsFile = "logrotate";
        public const string GlobalOptionsDirectory = "logrotate.d";

        private LogRotateOptions[] options;
        private LogRotater[] rotaters;

        private Timer rotateTimer;
        private int rotateSecond = -1;

        private NamedPipeServerStream configPipe;

        public LogRotaterDaemon()
        {
            configPipe = PipeFactory.CreateServer();
            rotateTimer = new Timer(OnTick);
        }

        public void Start()
        {
            Config();
            this.rotateTimer = new Timer(OnTick, null, 450, 450);
            configPipe.BeginWaitForConnection(Config, null);
        }

        void OnTick(object state)
        {
            var now = DateTime.Now;
            if (rotateSecond == now.Second) return;
            rotateSecond = now.Second;

            ThreadPool.UnsafeQueueUserWorkItem(ExecuteRotater, now);
        }

        void ExecuteRotater(object state)
        {
            var now = (DateTime)state;

            foreach (var rotater in rotaters)
            {
                try { rotater.Rotate(now); }
                catch (Exception e) { Trace.TraceError("rotate error : " + e); }
            }
        }

        public void Stop()
        {
            rotateTimer.Change(0, Timeout.Infinite);
            rotateSecond = -1;
        }

        private void Config(IAsyncResult ar)
        {
            try
            {
                configPipe.EndWaitForConnection(ar);

                var command = new StreamReader(configPipe).ReadLine();
                var writer = new StreamWriter(configPipe) { AutoFlush = true };
                if (command == "reload")
                {
                    try
                    {
                        Config();
                        writer.WriteLine("load " + options.Length + " log rotater" + (options.Length > 1 ? "s" : "") + ".");
                        WriteOptions(writer);
                    }
                    catch (Exception e)
                    {
                        writer.Write("reload failed : " + e.Message);
                    }
                }
                else if (command == "status")
                {
                    writer.WriteLine(options.Length + " log rotater" + (options.Length > 1 ? "s are " : " is ")
                            + (rotateSecond == -1 ? "stopped" : "running") + ".");
                    WriteOptions(writer);
                }
                else
                {
                    writer.WriteLine("invalid command: " + command);
                }
                configPipe.WaitForPipeDrain();
            }
            catch (Exception e)
            {
                Trace.TraceError("config fault: " + e);
            }
            finally
            {
                configPipe.Disconnect();
                configPipe.BeginWaitForConnection(Config, ar.AsyncState);
            }
        }
        void WriteOptions(TextWriter writer)
        {
            foreach (var opt in options)
            {
                writer.WriteLine(" " + opt.Root + "\\" + opt.Filter + " { ");
                writer.WriteLine("   " + opt.RotateType.ToString().ToLower() + " " + opt.RotateArguments);
                writer.WriteLine("   rotate " + opt.Rotate);
                writer.WriteLine("   compress " + (opt.Compress ? "on" : "off"));
                writer.WriteLine("   delaycompress " + opt.DelayCompress);
                if (opt.PreScripts.Length > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("   prerotate");
                    foreach (var script in opt.PreScripts) writer.WriteLine("      " + script);
                    writer.WriteLine("   endscript");
                }
                if (opt.PostScripts.Length > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("   postrotate");
                    foreach (var script in opt.PostScripts) writer.WriteLine("      " + script);
                    writer.WriteLine("   endscript");
                }
                if (opt.IncludeSubDirs) writer.WriteLine("   includesubdirs");
                writer.WriteLine(" }");
            }
        }

        public void Config()
        {
            var rotaters = new List<LogRotater>();
            var options = new List<LogRotateOptions>();

            var providers = new LogRotateOptionsBuilderProvider[] {
                    new FileLogRotateOptionsBuilderProvider(GlobalOptionsFile),
                    new DirectoryLogRotateOptionsBuilderProvider(GlobalOptionsDirectory)
            };
            foreach (var provider in providers)
            {
                var builders = provider.CreateBuilders();
                for (int i = 0; i < builders.Length; i++)
                {
                    var opt = builders[i].Build();
                    rotaters.Add(LogRotater.Create(opt));
                    options.Add(opt);
                }
            }
            this.rotaters = rotaters.ToArray();
            this.options = options.ToArray();
        }

        public void Dispose()
        {
            rotateTimer.Dispose();
        }
    }
}
