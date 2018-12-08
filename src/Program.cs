using System;
using System.IO;
using System.ServiceProcess;

namespace logrotate
{
    class Program : ServiceBase
    {
        internal const string InternalIoRedirectCommand = "--io-redirect-to-file";

        private LogRotaterDaemon daemon;
        public Program(string serviceName) { ServiceName = serviceName; }

        protected override void OnStart(string[] args)
        {
            daemon = new LogRotaterDaemon("logrotate");
            daemon.Start();
        }

        protected override void OnStop()
        {
            daemon.Stop();
        }

        const string DefaultServicName = "logrotate";

        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                if (args.Length == 0 || args[0] == "-?" || args[0] == "-h")
                {
                    DisplayUsage();
                    return;
                }
                
                switch (args[0])
                {
                    case "--install":
                    case "-i":
                        new ServiceManager(args.Length == 1 ? DefaultServicName : args[1]).Install();
                        break;
                    case "--remove":
                    case "-r":
                        new ServiceManager(args.Length == 1 ? DefaultServicName : args[1]).Remove();
                        break;
                    case "--start":
                        new ServiceManager(args.Length == 1 ? DefaultServicName : args[1]).Start();
                        break;
                    case "--stop":
                        new ServiceManager(args.Length == 1 ? DefaultServicName : args[1]).Stop();
                        break;
                    case InternalIoRedirectCommand:
                        if (args.Length == 1 || !File.Exists(args[1])) Environment.Exit(1);
                        using (var io = File.CreateText(args[1]))
                        {
                            Console.SetError(io);
                            Console.SetOut(io);
                            var trimArgs = new string[args.Length - 2];
                            Array.Copy(args, 2, trimArgs, 0, trimArgs.Length);
                            Main(trimArgs);
                        }
                        break;
                    default:
                        // service command
                        using (var client = PipeFactory.CreateClient())
                        {
                            try
                            {
                                client.Connect(1);
                                using (var writer = new StreamWriter(client) { AutoFlush = true })
                                {
                                    writer.WriteLine(string.Join(" ", args));
                                    client.WaitForPipeDrain();
                                    Console.Write(new StreamReader(client).ReadToEnd());
                                }
                            }
                            catch (IOException) { }
                            catch (System.TimeoutException)
                            {
                                Console.WriteLine("Connect logrotate service timeout.");
                            }
                        }
                        break;
                }
            }
            else
            {
                using (var service = new Program(args.Length == 0 ? DefaultServicName : args[0]))
                {
                    Run(service);
                }
            }
        }

        static void DisplayUsage()
        {
            Console.WriteLine("Usage: logrotate command");
            Console.WriteLine("  help command");
            Console.WriteLine("     --help    -h  print current usage.");
            Console.WriteLine("  config command");
            Console.WriteLine("     reload    reload current lograte configuration.");
            Console.WriteLine("     status    print current lograte status.");
            Console.WriteLine("  service command");
            Console.WriteLine("       --install [serviceName] -i  install specified service.");
            Console.WriteLine("       --remove [serviceName]  -u  unstall specified service.");
            Console.WriteLine("       --start [serviceName]    start specified service.");
            Console.WriteLine("       --stop [serviceName]     stop specified service.");
            Console.WriteLine("             use lograte as serviceName when not specified");
        }
    }
}
