﻿using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Crestron;
using Serilog.Sinks.Crestron.Output;
using Serilog.Sinks.Crestron.Themes;
using System;

#nullable enable // Important!

namespace Serilog
{
    /// <summary>
    /// Adds the WriteTo.CrestronConsole() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class CrestronUdpLoggerConfigurationExtensions
    {
        static readonly object DefaultSyncRoot = new object();
        const string DefaultConsoleOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        /// Writes log events to <see cref="Crestron.SimplSharp.CrestronConsole"/>.C
        /// </summary>
        /// <param name="sinkConfiguration">Logger sink configuration.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for
        /// events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
        /// <param name="outputTemplate">A message template describing the format used to write to the sink.
        /// The default is <code>"[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"</code>.</param>
        /// <param name="syncRoot">An object that will be used to `lock` (sync) access to the console output. If you specify this, you
        /// will have the ability to lock on this object, and guarantee that the console sink will not be about to output anything while
        /// the lock is held.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level
        /// to be changed at runtime.</param>
        /// <param name="theme">The theme to apply to the styled output. If not specified,
        /// uses <see cref="ConsoleTheme.None"/>.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="sinkConfiguration"/> is <code>null</code></exception>
        /// <exception cref="ArgumentNullException">When <paramref name="outputTemplate"/> is <code>null</code></exception>
        public static LoggerConfiguration CrestronUdpConsole(
            this LoggerSinkConfiguration sinkConfiguration,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultConsoleOutputTemplate,
            IFormatProvider? formatProvider = null,
            LoggingLevelSwitch? levelSwitch = null,
            ConsoleTheme? theme = null,
            object? syncRoot = null,
            int portNumber = 41700)
        {
            if (sinkConfiguration is null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (outputTemplate is null) throw new ArgumentNullException(nameof(outputTemplate));

            // This is colourized by default in the vanilla Console Sink provided by Serilog
            // using SystemConsoleThemes.Literate;
            // however since users may be connecting with Crestron toolbox which doesn't support most ANSI color 
            // Codes we disable colour by default.
            // Advanced users will be using an SSH client which does and so they'll specify colorized output then
            var appliedTheme = theme ?? ConsoleTheme.None;

            syncRoot ??= DefaultSyncRoot;

            var formatter = new OutputTemplateRenderer(appliedTheme, outputTemplate, formatProvider);
            return sinkConfiguration.Sink(new CrestronUdpSink(appliedTheme, formatter, syncRoot, portNumber), restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>
        /// Writes log events to <see cref="Crestron.SimplSharp.CrestronConsole"/>.C
        /// </summary>
        /// <param name="sinkConfiguration">Logger sink configuration.</param>
        /// <param name="formatter">Controls the rendering of log events into text, for example to log JSON. To
        /// control plain text formatting, use the overload that accepts an output template.</param>
        /// <param name="syncRoot">An object that will be used to `lock` (sync) access to the console output. If you specify this, you
        /// will have the ability to lock on this object, and guarantee that the console sink will not be about to output anything while
        /// the lock is held.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for
        /// events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level
        /// to be changed at runtime.</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="sinkConfiguration"/> is <code>null</code></exception>
        /// <exception cref="ArgumentNullException">When <paramref name="outputTemplate"/> is <code>null</code></exception>
        public static LoggerConfiguration CrestronUdpConsole(
            this LoggerSinkConfiguration sinkConfiguration,
            ITextFormatter formatter,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultConsoleOutputTemplate,
            LoggingLevelSwitch? levelSwitch = null,
            object? syncRoot = null,
            int portNumber = 41700)
        {
            if (sinkConfiguration is null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (outputTemplate is null) throw new ArgumentNullException(nameof(outputTemplate));

            syncRoot ??= DefaultSyncRoot;

            return sinkConfiguration.Sink(new CrestronUdpSink(ConsoleTheme.None, formatter, syncRoot, portNumber), restrictedToMinimumLevel, levelSwitch);
        }
    }
}

#nullable restore
