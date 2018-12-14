using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace logrotate
{
    class ServiceManager
    {
        private string serviceName;

        public ServiceManager(string serviceName) { this.serviceName = serviceName; }

        public void Stop()
        {
            RunAs(() =>
            {
                Console.WriteLine("Stop service: " + serviceName);
                var controller = new ServiceController(serviceName);
                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped);
                Console.Write("Success.");
            });
        }

        public void Start()
        {
            RunAs(() =>
            {
                Console.WriteLine("Start service: " + serviceName);
                var controller = new ServiceController(serviceName);
                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running);
                Console.Write("Success.");
            });
        }

        public void Remove()
        {
            RunAs(() =>
            {
                var state = new Hashtable();
                using (var installer = new ServiceProcessInstaller())
                using (var serviceInstaller = new ServiceInstaller())
                {
                    try
                    {
                        installer.Account = ServiceAccount.LocalSystem;
                        installer.Installers.Add(serviceInstaller);
                        installer.Context = serviceInstaller.Context = new InstallContext(null, null);
                        Console.WriteLine("Uninstall service " + serviceName);
                        serviceInstaller.ServiceName = serviceName;
                        installer.Uninstall(null);
                        Console.Write("Success.");
                    }
                    catch (Exception e)
                    {
                        Console.Write("Failed: " + e.Message);
                    }
                }
            });
        }

        static bool IsLowerDotnetFramework()
        {
            return typeof(ServiceInstaller).Assembly.ImageRuntimeVersion.CompareTo("v4") == -1;
        }

        public void Install()
        {
            RunAs(() =>
            {
                var state = new Hashtable();
                using (var installer = new ServiceProcessInstaller())
                using (var serviceInstaller = new ServiceInstaller())
                {
                    try
                    {
                        installer.Account = ServiceAccount.LocalSystem;
                        installer.Installers.Add(serviceInstaller);
                        Console.WriteLine("Install service " + serviceName);

                        installer.Context = serviceInstaller.Context = new InstallContext(null, null);
                        serviceInstaller.Context.Parameters.Add("assemblyPath", IsLowerDotnetFramework()
                            ? (typeof(ServiceManager).Assembly.Location + "\" \"" + serviceName)
                            : ("\"" + typeof(ServiceManager).Assembly.Location + "\" " + serviceName));

                        serviceInstaller.ServiceName = serviceName;
                        serviceInstaller.DisplayName = "Logrotate Daemon";
                        serviceInstaller.Description = "logrotate daemon service";

                        serviceInstaller.StartType = ServiceStartMode.Automatic;

                        installer.Install(state);
                        Console.Write("Success.");
                    }
                    catch (Exception e)
                    {
                        if (state.Count > 0) installer.Rollback(state);
                        Console.Write("Failed: " + e.Message);
                    }
                }
            });
        }

        static void RunAs(Action action)
        {
            var args = Environment.GetCommandLineArgs();
            // 管理员运行
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                try { action(); }
                catch (Exception e) { Console.Error.Write(e.Message); }
            }
            else
            {
                var redirectFile = Path.GetTempFileName();
                try
                {
                    var si = new ProcessStartInfo(args[0]);
                    args[0] = redirectFile;
                    si.Arguments = Program.InternalIoRedirectCommand + " " + string.Join(" ", args.Select(_ => '"' + _ + '"').ToArray());
                    si.Verb = "runas";
                    si.WindowStyle = ProcessWindowStyle.Hidden;
                    si.WorkingDirectory = Environment.CurrentDirectory;
                    using (var io = File.Open(redirectFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var runAs = Process.Start(si);
                        var position = io.Position;
                        var buffer = new byte[1024];
                        while (!runAs.HasExited)
                        {
                            try
                            {
                                var size = io.Read(buffer, 0, buffer.Length);
                                if (size > 0)
                                {
                                    position += size;
                                    Console.Write(Encoding.UTF8.GetString(buffer, 0, size));
                                }
                            }
                            catch { }
                            Thread.Sleep(50);
                        }
                        Console.Write(new StreamReader(io).ReadToEnd());
                    }
                }
                catch (Exception)
                {
                    Console.Error.Write("FAULT: Requires administrator priviledge.");
                }
                finally
                {
                    while (true)
                    {
                        try { File.Delete(redirectFile); break; }
                        catch (Exception) { Thread.Sleep(100); }
                    }
                }
            }
        }
    }
}
