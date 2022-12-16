using Crestron.SimplSharp.CrestronSockets;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Serilog.Sinks.Crestron
{
    public class CrestronTcpTextWriter : TextWriter
    {
        private readonly TCPServer _tcpServer;
        public CrestronTcpTextWriter(TCPServer tcpServer)
        {
            _tcpServer = tcpServer;
        }

        public CrestronTcpTextWriter(TCPServer TcpServer, IFormatProvider formatProvider) : base(formatProvider)
        {
            _tcpServer = TcpServer;
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
                _tcpServer.SendDataAsync(msg, msg.Length, TcpSendCallback);
            }
            else
            {
                var msg = Encoding.UTF8.GetBytes(ReplaceLF(value.ToString()));
                _tcpServer.SendDataAsync(msg, msg.Length, TcpSendCallback);
            }
        }

        private void TcpSendCallback(TCPServer myTCPServer, uint clientIndex, int numberOfBytesSent)
        {
            //
        }

        public override void Write(string value)
        {
            if (FormatProvider != null)
            {
                var msg = Encoding.UTF8.GetBytes(ReplaceLF(value.ToString(FormatProvider)));
                _tcpServer.SendDataAsync(msg, msg.Length, TcpSendCallback);
            }
            else
            {
                var msg = Encoding.UTF8.GetBytes(ReplaceLF(value.ToString()));
                _tcpServer.SendDataAsync(msg, msg.Length, TcpSendCallback);
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
