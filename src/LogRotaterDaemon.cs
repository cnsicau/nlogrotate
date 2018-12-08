using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace logrotate
{
    public class LogRotaterDaemon : IDisposable
    {
        private readonly string optionsFile;
        private LogRotateOptions[] options;
        private LogRotater[] rotaters;

        private Timer rotateTimer;
        private int rotateSecond = -1;

        private NamedPipeServerStream configPipe;

        public LogRotaterDaemon(string optionsFile)
        {
            this.optionsFile = optionsFile;
            configPipe = PipeFactory.CreateServer();
            rotateTimer = new Timer(OnTick);
        }

        public void Start()
        {
            LoadConfig();
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
                        LoadConfig();
                        writer.WriteLine("load " + options.Length + " log rotater" + (options.Length > 1 ? "s" : "") + ".");
                        foreach (var opt in options)
                        {
                            writer.WriteLine(" " + opt.Root + "\\" + opt.Filter + " { ");
                            writer.WriteLine("   " + opt.RotateType.ToString().ToLower() + " " + opt.RotateArguments);
                            writer.WriteLine("   rotate " + opt.Rotate);
                            writer.WriteLine("   compress " + (opt.Compress ? "on" : "off"));
                            writer.WriteLine("   delaycompress " + opt.DelayCompress);
                            if (opt.IncludeSubDirs) writer.WriteLine("   includesubdirs");
                            writer.WriteLine(" }");
                        }
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
                    foreach (var opt in options)
                    {
                        writer.WriteLine(" " + opt.Root + "\\" + opt.Filter + " { ");
                        writer.WriteLine("   " + opt.RotateType.ToString().ToLower() + " " + opt.RotateArguments);
                        writer.WriteLine("   rotate " + opt.Rotate);
                        writer.WriteLine("   compress " + (opt.Compress ? "on" : "off"));
                        writer.WriteLine("   delaycompress " + opt.DelayCompress);
                        if (opt.IncludeSubDirs) writer.WriteLine("   includesubdirs");
                        writer.WriteLine(" }");
                    }
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

        public void LoadConfig()
        {
            var builders = new FileLogRotateOptionsBuilderProvider(optionsFile).CreateBuilders();
            var rotaters = new LogRotater[builders.Length];
            var options = new LogRotateOptions[builders.Length];
            for (int i = 0; i < builders.Length; i++)
            {
                options[i] = builders[i].Build();
                rotaters[i] = LogRotater.Create(options[i]);
            }
            this.rotaters = rotaters;
            this.options = options;
        }

        public void Dispose()
        {
            rotateTimer.Dispose();
        }
    }
}
