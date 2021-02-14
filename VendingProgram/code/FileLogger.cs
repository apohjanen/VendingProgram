using System;
using System.IO;

namespace VendingProgram {

    /// A very simple logging class for writing logs into a file.

    class FileLogger : IDebugLogger {
        
        private string LogDirectory {get; set;}
        private string FileName {get; set;}
        private string LogFileSavePath {get; set;}
        private bool createNewLogOnEveryRun = false;

        public FileLogger () {
            this.LogDirectory = Directory.GetCurrentDirectory();
            
            // Should we create a new log on every run using date & time?
            if (createNewLogOnEveryRun) {
                this.FileName = "DebugLog" + DateTime.Now + ".txt";
            } else {
                this.FileName = "DebugLog.txt";
            }

            this.LogFileSavePath = this.LogDirectory + "/" + this.FileName;
        }

        public void LogMessage (string message) {
            
            if (!File.Exists(LogFileSavePath)) {
                using (StreamWriter sw = File.CreateText(LogFileSavePath))
                {
                    sw.WriteLineAsync("Beginning of log " + DateTime.Now);
                    sw.WriteLineAsync("================");
                }
            } else {
                using (StreamWriter sw = File.AppendText(LogFileSavePath)) {
                    sw.WriteLineAsync("["+DateTime.Now+"] " + message);
                }
            }
        }
    }
}