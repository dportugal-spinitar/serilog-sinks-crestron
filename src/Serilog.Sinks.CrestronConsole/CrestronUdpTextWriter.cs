using Crestron.SimplSharp.CrestronSockets;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Serilog.Sinks.Crestron
{
    public class CrestronUdpTextWriter : TextWriter
    {
        private readonly UDPServer _UdpServer;
        public CrestronUdpTextWriter(UDPServer UdpServer)
        {
            _UdpServer = UdpServer;
        }

        public CrestronUdpTextWriter(UDPServer UdpServer, IFormatProvider formatProvider) : base(formatProvider)
        {
            _UdpServer = UdpServer;
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
                _UdpServer.SendData(msg, msg.Length);
            }
            else
            {
                var msg = Encoding.UTF8.GetBytes(ReplaceLF(value.ToString()));
                _UdpServer.SendData(msg, msg.Length);
            }
        }

        public override void Write(string value)
        {
            if (FormatProvider != null)
            {
                var msg = Encoding.UTF8.GetBytes(ReplaceLF(value.ToString(FormatProvider)));
                _UdpServer.SendData(msg, msg.Length);
            }
            else
            {
                var msg = Encoding.UTF8.GetBytes(ReplaceLF(value.ToString()));
                _UdpServer.SendData(msg, msg.Length);
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
