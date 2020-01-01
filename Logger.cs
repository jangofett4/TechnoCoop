using System;
using System.IO;

namespace TechnoCoop
{
    public class Logger
    {
        public StreamWriter LogStream;

        public Logger(StreamWriter s)
        {
            LogStream = s;
        }

        public void Log(object msg, params object[] format)
        {
            LogStream.WriteLine(string.Format(msg.ToString(), format));
        }

        public void Info(object msg, params object[] format)
        {
            Log(string.Format("{0} [INFO] {1}", DateTime.Now.ToString(), msg), format);
        }

        public void Warn(object msg, params object[] format)
        {
            Log(string.Format("{0} [WARN] {1}", DateTime.Now.ToString(), msg), format);
        }

        public void Error(object msg, params object[] format)
        {
            Log(string.Format("{0} [ERR] {1}", DateTime.Now.ToString(), msg), format);
        }

        public void Fatal(object msg, params object[] format)
        {
            Log(string.Format("{0} [FATAL] {1}", DateTime.Now.ToString(), msg), format);
            Environment.Exit(1);
        }

        public void Flush()
        {
            LogStream.Flush();
        }
    }
}