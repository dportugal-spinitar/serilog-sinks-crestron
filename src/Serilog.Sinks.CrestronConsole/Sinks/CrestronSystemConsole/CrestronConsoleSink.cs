﻿using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Crestron.Themes;
using System;
using System.IO;
using System.Text;

namespace Serilog.Sinks.Crestron
{
    public class CrestronConsoleSink : ILogEventSink
    {
        readonly ConsoleTheme _theme;
        readonly ITextFormatter _formatter;
        readonly object _syncRoot;

        private CrestronConsoleTextWriter output;
        private StringWriter buffer;

        const int DefaultWriteBufferCapacity = 256;

        static CrestronConsoleSink()
        {
            // Disable this for now, we can add it back in if we want this console sink to continue to work
            // When running outside a Crestron appliance
            // WindowsConsole.EnableVirtualTerminalProcessing();  
        }

        public CrestronConsoleSink(
            ConsoleTheme theme,
            ITextFormatter formatter,
            object syncRoot)
        {
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _formatter = formatter;
            _syncRoot = syncRoot ?? throw new ArgumentNullException(nameof(syncRoot));

            output = new CrestronConsoleTextWriter();
            buffer = new StringWriter(new StringBuilder(DefaultWriteBufferCapacity));
        }

        public void Emit(LogEvent logEvent)
        {
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
