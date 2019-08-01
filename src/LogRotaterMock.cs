using System;

namespace logrotate
{
    public class LogRotaterMock
    {
        public void Execute(string[] args)
        {
            DateTime mockTime;
            string mockFile;
            if(args.Length <= 1)
            {
                mockTime = DateTime.Now;
                mockFile = LogRotaterDaemon.GlobalOptionsFile;
            }
            else
            {
                if (!DateTime.TryParse(args[1], out mockTime))
                    throw new InvalidOperationException("invalid mock time " + args[1]);

                if (args.Length > 2) mockFile = args[2];
                else mockFile = LogRotaterDaemon.GlobalOptionsFile;
            }

            Console.WriteLine("executing mock: ");
            Console.WriteLine("  file: " + mockFile);
            Console.WriteLine("  time: " + mockTime.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.Write(" load mock rotater configuration,");
            var provider = new FileLogRotateOptionsBuilderProvider(mockFile);
            var builders = provider.CreateBuilders();

            Console.WriteLine(" found " + builders.Length + " rotater" + (builders.Length > 1 ? "s" : ""));

            foreach (var builder in builders)
            {
                var options = builder.Build();
                Console.WriteLine(" -------------------- mock --------------------");
                options.Store(Console.Out);
                var rotater = LogRotater.Create(options);
                Console.WriteLine(" > rotating: ");
                rotater.Rotate(mockTime);
                Console.WriteLine(" < completed");
                Console.WriteLine(" ----------------------------------------------");
            }

            Console.WriteLine("mock completed");
        }
    }
}
