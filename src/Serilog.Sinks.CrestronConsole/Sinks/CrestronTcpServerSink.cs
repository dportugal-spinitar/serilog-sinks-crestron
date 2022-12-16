using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Crestron.Themes;
using System;
using System.IO;
using System.Text;

namespace Serilog.Sinks.Crestron
{
    public class CrestronTcpServerSink : ILogEventSink
    {
        readonly ConsoleTheme _theme;
        readonly ITextFormatter _formatter;
        readonly object _syncRoot;

        private CrestronTcpTextWriter output;
        private StringWriter buffer;

        private TCPServer tcpServer;

        const int DefaultWriteBufferCapacity = 256;

        static CrestronTcpServerSink()
        {
            // Disable this for now, we can add it back in if we want this console sink to continue to work
            // When running outside a Crestron appliance
            // WindowsConsole.EnableVirtualTerminalProcessing();  
        }

        public CrestronTcpServerSink(
            ConsoleTheme theme,
            ITextFormatter formatter,
            object syncRoot,
            int portNumber)
        {
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _formatter = formatter;
            _syncRoot = syncRoot ?? throw new ArgumentNullException(nameof(syncRoot));

            tcpServer = new TCPServer("0.0.0.0", portNumber, 1000);
            var result = tcpServer.WaitForConnectionsAlways(ServerWaitForConnectionCallback);
            ErrorLog.Notice($"Starting Log TCP Server on Port:{portNumber}. Result: {result}");
            CrestronConsole.PrintLine($"\r\nStarting Log TCP Server on Port:{portNumber}. Result:{result}");

            output = new CrestronTcpTextWriter(tcpServer);
            buffer = new StringWriter(new StringBuilder(DefaultWriteBufferCapacity));
        }

        //List<uint> ConnectedClients = new List<uint>();

        private void ServerWaitForConnectionCallback(TCPServer myTCPServer, uint clientIndex)
        {
            //CrestronConsole.PrintLine($"Log Tcp Server State:{tcpServer.State}");        
            //CrestronConsole.PrintLine($"\r\nAdding Log Server Client:{clientIndex}");
            //ConnectedClients.Add(clientIndex);
            tcpServer.ReceiveDataAsync(ServerReceivedDataCallback);
        }

        private void ServerReceivedDataCallback(TCPServer myTCPServer, uint clientIndex, int numberOfBytesReceived)
        {
            //CrestronConsole.PrintLine($"Log Tcp Server Received Byte Count:{numberOfBytesReceived}");
            if (numberOfBytesReceived <= 0)
            {
                //while (ConnectedClients.Contains(clientIndex))
                //{
                //CrestronConsole.PrintLine($"\r\nRemoving Log Server Client:{clientIndex}");
                //ConnectedClients.Remove(clientIndex);                    
                //var result = tcpServer.Disconnect(clientIndex); //this is causing an error because it's already disconnected.. 
                //}

                if (tcpServer.NumberOfClientsConnected < tcpServer.MaxNumberOfClientSupported)
                {
                    tcpServer.WaitForConnectionAsync(ServerWaitForConnectionCallback); //wait for more clients
                }
                return;
            }
            else
                tcpServer.ReceiveDataAsync(ServerReceivedDataCallback);
        }

        public void Emit(LogEvent logEvent)
        {
            if (tcpServer.NumberOfClientsConnected < 1)
                return;

            // ANSI escape codes can be pre-rendered into a buffer; however, if we're on Windows and
            // using its console coloring APIs, the color switches would happen during the off-screen
            // buffered write here and have no effect when the line is actually written out.
            if (_theme.CanBuffer)
            {
                lock (_syncRoot)
                {
                    buffer.GetStringBuilder().Clear();
                    _formatter.Format(logEvent, buffer);
                    var formattedLogEventText = buffer.ToString();
                    output.WriteLine(formattedLogEventText);
                    output.Flush();
                }
            }
            else
            {
                lock (_syncRoot)
                {
                    _formatter.Format(logEvent, output);
                    output.Flush();
                }
            }
        }
    }
}
