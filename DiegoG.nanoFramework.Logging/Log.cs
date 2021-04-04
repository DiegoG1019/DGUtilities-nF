using System;
using System.Collections;
using System.Threading;

namespace DiegoG.nanoFramework.Logging
{
    public static class Log
    {
        public static class DefaultSinks
        {
            public static void DebugConsole(string[] info)
            {
                foreach(var s in info)
                    System.Diagnostics.Debug.WriteLine(s);
            }
        }

        public class LogBuilder
        {
            //string format = null, string dateformat = null, params LoggerSink[] sinks
            private string BuilderFormat;
            private string BuilderDateFormat;
            private LoggerSink[] BuilderSinks;
            private bool BuilderBuffer = false;
            private int BuilderBufferCount;
            private TimeSpan BuilderBufferFlushInterval;
            private LogLevel BuilderLevel;
            private AddendumFunc BuilderAddendum;

            internal LogBuilder() 
            {
                if (IsInit)
                    throw new InvalidOperationException("Cannot build a LogBuilder after the logger has already been built");
            }

            public void Build()
            {
                if (IsInit)
                    throw new InvalidOperationException("Cannot initialize logger twice");

                Sinks = BuilderSinks != null && BuilderSinks.Length > 0 ? BuilderSinks : new LoggerSink[] { DefaultSinks.DebugConsole };
                Format = BuilderFormat ?? "({0}) [{1}] {2}";
                DateFormat = BuilderDateFormat ?? "MM/dd/yy H:mm:ss";
                CurrentLevel = BuilderLevel;
                Addendum = BuilderAddendum;
                if(!BuilderBuffer)
                {
                    EnableBuffer = false;
                    BufferFlushInterval = TimeSpan.Zero;
                }
                else
                {
                    EnableBuffer = true;
                    MessageBuffer = new Queue();
                    BufferSize = BuilderBufferCount;
                    BufferFlushInterval = BuilderBufferFlushInterval;
                    BufferThread = new Thread(BufferFlush);
                    BufferThread.Start();
                }

                IsInit = true;
            }

            public LogBuilder SetFormat(string format) { BuilderFormat = format; return this; }
            public LogBuilder SetDateFormat(string dateformat) { BuilderDateFormat = dateformat; return this; }
            public LogBuilder SetLevel(LogLevel level) { BuilderLevel = level; return this; }
            public LogBuilder SetAddendum(AddendumFunc addendum) { BuilderAddendum = addendum; return this; }
            public LogBuilder SetSinks(params LoggerSink[] sinks)
            {
                BuilderSinks = sinks;
                foreach (var s in BuilderSinks)
                    if (s is null)
                        throw new ArgumentNullException(nameof(sinks), "One or more of the specified sinks were null");
                return this;
            }
            public LogBuilder SetBuffer(int buffersize, TimeSpan flushinterval)
            {
                if (buffersize > 0 && flushinterval > TimeSpan.Zero)
                {
                    BuilderBuffer = true;
                    BuilderBufferCount = buffersize;
                    BuilderBufferFlushInterval = flushinterval;
                }
                else
                    BuilderBuffer = false;
                return this;
            }
        }

        public enum LogLevel : byte
        { Fatal, Error, Warning, Information, Debug, Verbose }

        public static LogLevel CurrentLevel { get; private set; }

        public delegate void LoggerSink(string[] formattedinfo);
        public delegate string AddendumFunc();

        /// <summary>
        /// 0:Date, 1:Level and 2:Info. Defaults to "({0}) [{1}] {2}"
        /// </summary>
        public static string Format { get; private set; }
        /// <summary>
        /// Sets the DateFormat used in Log messages. Defaults to "MM/dd/yy H:mm:ss zzz"
        /// </summary>
        public static string DateFormat { get; private set; }

        /// <summary>
        /// An extra piece of data to be added to the end of each log message in the form of: {message} | {addendum}
        /// </summary>
        public static AddendumFunc Addendum { get; private set; }

        private static LoggerSink[] Sinks;
        private static bool EnableBuffer;
        private static TimeSpan BufferFlushInterval;
        private static int BufferSize;
        private static Queue MessageBuffer;
        private static Thread BufferThread;

        private static string SelectMode(LogLevel lv)
        {
            switch (lv)
            {
                case LogLevel.Fatal:
                    return "FTL";
                case LogLevel.Error:
                    return "ERR";
                case LogLevel.Warning:
                    return "WRN";
                case LogLevel.Information:
                    return "INF";
                case LogLevel.Debug:
                    return "DBG";
                case LogLevel.Verbose:
                    return "VRB";
                default:
                    throw new ArgumentOutOfRangeException(nameof(lv), "Unknown LogLevel enum value");
            }
        }

        private static void MakeMsg(string info, LogLevel level)
        {
            if (!IsInit)
                throw new InvalidOperationException("Cannot use logger before initializing");
            string formattedinfo = string.Format(Format, DateTime.UtcNow.ToString(DateFormat), SelectMode(level), info);
            if (Addendum != null)
                formattedinfo += " | " + Addendum();

            if (EnableBuffer)
            {
                lock (MessageBuffer)
                    MessageBuffer.Enqueue(formattedinfo);
                if (MessageBuffer.Count >= BufferSize)
                    Flush();
            }
            else
            {
                var fi = new string[] { formattedinfo };
                foreach (var s in Sinks)
                    s(fi);
            }
        }

        /// <summary>
        /// Forces the MessageBuffer to flush. Throws an Exception if MessageBuffer is not enabled. This may potentially take a fair amount of time.
        /// </summary>
        public static void Flush()
        {
            if (!EnableBuffer)
                throw new InvalidOperationException("Cannot Flush if Buffering is not enabled.");
            if (MessageBuffer.Count <= 0)
                return;

            string[] msgs = new string[MessageBuffer.Count];
            int index = 0;

            lock (MessageBuffer)
                while(MessageBuffer.Count > 0)
                    msgs[index++] = (string)MessageBuffer.Dequeue();

            foreach (var s in Sinks)
            {
                s(msgs);
                Thread.Sleep(60);
            }
        }

        private static bool IsInit = false;

        /// <summary>
        /// Returns an object to initialize the logger with the specified format, dateformat, and sinks
        /// </summary>
        public static LogBuilder GetBuilder() => new LogBuilder();

        public static void Fatal(string info) => MakeMsg(info, LogLevel.Fatal);
        public static void Fatal(string template, params object[] values) => MakeMsg(string.Format(template, values), LogLevel.Fatal);

        public static void Error(string info)
        {
            if(CurrentLevel >= LogLevel.Error)
                MakeMsg(info, LogLevel.Error);
        }
        public static void Error(string template, params object[] values)
        {
            if(CurrentLevel >= LogLevel.Error)
                MakeMsg(string.Format(template, values), LogLevel.Error);
        }

        public static void Warning(string info)
        {
            if(CurrentLevel >= LogLevel.Warning)
                MakeMsg(info, LogLevel.Warning);
        }
        public static void Warning(string template, params object[] values)
        {
            if (CurrentLevel >= LogLevel.Warning)
                MakeMsg(string.Format(template, values), LogLevel.Warning);
        }

        public static void Information(string info)
        {
            if (CurrentLevel >= LogLevel.Information)
                MakeMsg(info, LogLevel.Information);
        }
        public static void Information(string template, params object[] values)
        {
            if (CurrentLevel >= LogLevel.Information)
                MakeMsg(string.Format(template, values), LogLevel.Information);
        }

        public static void Debug(string info)
        {
            if (CurrentLevel >= LogLevel.Debug)
                MakeMsg(info, LogLevel.Debug);
        }
        public static void Debug(string template, params object[] values)
        {
            if (CurrentLevel >= LogLevel.Debug)
                MakeMsg(string.Format(template, values), LogLevel.Debug);
        }

        public static void Verbose(string info)
        {
            if (CurrentLevel >= LogLevel.Verbose)
                MakeMsg(info, LogLevel.Verbose);
        }
        public static void Verbose(string template, params object[] values)
        {
            if (CurrentLevel >= LogLevel.Verbose)
                MakeMsg(string.Format(template, values), LogLevel.Verbose);
        }

        private static void BufferFlush()
        {
            while (true)
            {
                Thread.Sleep(BufferFlushInterval);
                Flush();
            }
        }
    }
}
