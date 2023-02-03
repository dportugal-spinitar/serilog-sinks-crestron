using Crestron.SimplSharp;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Crestron.Themes;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Serilog.Sinks.Crestron
{
    public delegate void EmitHandler(SimplSharpString logEvent);
    public class CrestronSimplPlusSink : ILogEventSink
    {
        readonly ConsoleTheme _theme;
        readonly ITextFormatter _formatter;

        private CrestronConsoleTextWriter output;
        private StringWriter buffer;

        const int DefaultWriteBufferCapacity = 256;

        public EmitHandler EmitHandler { get; set; }

        public CrestronSimplPlusSink()
        {

        }
        public CrestronSimplPlusSink(        
            ConsoleTheme theme,
            ITextFormatter formatter)
        {
                _theme = theme ?? throw new ArgumentNullException(nameof(theme));
                _formatter = formatter;
                
                output = new CrestronConsoleTextWriter();
                buffer = new StringWriter(new StringBuilder(DefaultWriteBufferCapacity));
            }
        }

        public void Emit(LogEvent logEvent)
        {
            logEvent.RenderMessage();
        }
    }
    

    public static class CrestronSimplPlusSinkExtension
    { 
    }


}
