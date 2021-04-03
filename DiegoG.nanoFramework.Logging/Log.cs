using System;
using System.Collections;

namespace DiegoG.nanoFramework.Logging
{
    public static class Log
    {
        public enum LogLevel : byte
        { Fatal, Error, Warning, Information, Debug, Verbose }

        public static LogLevel CurrentLevel { get; private set; }

        private static string SelectMode(LogLevel lv)
        {
            switch (lv)
            {
                case LogLevel.Fatal:
                    return $"({DateTime.UtcNow}) [FTL] {{0}}";
                case LogLevel.Error:
                    return $"({DateTime.UtcNow}) [ERR] {{0}}";
                case LogLevel.Warning:
                    return $"({DateTime.UtcNow}) [WRN] {{0}}";
                case LogLevel.Information:
                    return $"({DateTime.UtcNow}) [INF] {{0}}";
                case LogLevel.Debug:Que
                    return $"({DateTime.UtcNow}) [DBG] {{0}}";
                case LogLevel.Verbose:
                    return $"({DateTime.UtcNow}) [VRB] {{0}}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(lv), "Unknown LogLevel enum value");
            }
        }
        private static void MakeMsg(string info, LogLevel level) => System.Diagnostics.Debug.WriteLine(string.Format(SelectMode(level), info));

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
    }
}
