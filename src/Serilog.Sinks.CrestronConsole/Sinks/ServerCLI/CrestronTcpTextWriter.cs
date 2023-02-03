using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CrestronTcpServerCLI;

namespace Serilog.Sinks.Crestron
{
    public class CrestronTcpTextWriter : TextWriter
    {
        private readonly Server _tcpServer;
        public CrestronTcpTextWriter(Server tcpServer)
        {
            _tcpServer = tcpServer;
        }

        public CrestronTcpTextWriter(Server tcpServer, IFormatProvider formatProvider) 
            : base(formatProvider)
        {
            _tcpServer = tcpServer;
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

        private string ReplaceLF(string text)
        {
            return Regex.Replace(text, "(?<!\r)\n", "\r\n");
        }

        public override void Write(char value)
        {
            if (FormatProvider != null)
            {
                var msg = Encoding.UTF8.GetBytes(ReplaceLF(value.ToString(FormatProvider)));
                _tcpServer.SendToAllClients(msg);

            }
            else
            {
                var msg = Encoding.UTF8.GetBytes(ReplaceLF(value.ToString()));
                _tcpServer.SendToAllClients(msg);                
            }
        }        

        public override void Write(string value)
        {
            if (FormatProvider != null)
            {
                var msg = ReplaceLF(value.ToString(FormatProvider));
                _tcpServer.SendToAllClients(msg);
            }
            else
            {
                _tcpServer.SendToAllClients(value);
            }
        }

        public override void Write(string message, params object[] args)
        {
            Write(string.Format(message, args));
        }

        public override void WriteLine()
        {
            Write("\r\n");
        }

        public override void WriteLine(string value)
        {
            Write($"{value}\r\n");
        }

        public override void WriteLine(string message, params object[] args)
        {
            Write($"{string.Format(message, args)}\r\n");
        }

        public override void Flush()
        {
            // Do nothing since we don't flush Crestron Console
        }
    }
}
