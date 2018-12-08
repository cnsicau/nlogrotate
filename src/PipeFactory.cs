using System;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace logrotate
{
    static class PipeFactory
    {
        static readonly string pipeName;

        static PipeFactory()
        {
            var bytes = Encoding.UTF8.GetBytes(AppDomain.CurrentDomain.BaseDirectory);
            bytes = MD5.Create().ComputeHash(bytes);
            pipeName = "logrotate/instance/" + BitConverter.ToString(bytes);
        }

        static internal NamedPipeServerStream CreateServer()
        {
            var security = new PipeSecurity();
            security.AddAccessRule(new PipeAccessRule("Users", PipeAccessRights.FullControl, System.Security.AccessControl.AccessControlType.Allow));
            return new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0, 0, security);
        }

        static internal NamedPipeClientStream CreateClient()
        {
            return new NamedPipeClientStream(".", pipeName);
        }
    }
}
