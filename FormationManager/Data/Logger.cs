using System;
using System.IO;

namespace FormationManager.Data
{
    public static class Logger
    {
        private static readonly object LogLock = new object();

        public static void Log(string message)
        {
            try
            {
                lock (LogLock)
                {
                    string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string dir = Path.Combine(docs, "Mount and Blade II Bannerlord", "Configs", "FormationManager");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    string path = Path.Combine(dir, "log.txt");
                    File.AppendAllText(path, $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
                }
            }
            catch { }
        }
    }
}
