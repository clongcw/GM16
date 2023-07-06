using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.Constants
{
    public static class Environments
    {
        private static string _appDataPath;
        private static string _logFilePath;

        public static string DataBaseConnectionString = @"DataSource=" + GetCurrentProjectPath + @"\DataBase\DataBase.db";
        public static string ProtocolConnectionString = @"DataSource=" + GetCurrentProjectPath + @"\DataBase\Protocol.db";
        public static string UserConnectionString = @"DataSource=" + GetCurrentProjectPath + @"\DataBase\User.db";

        public static string AppDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_appDataPath))
                    _appDataPath = Environment.ExpandEnvironmentVariables($"{Environment.CurrentDirectory}");
                if (!Directory.Exists(_appDataPath)) Directory.CreateDirectory(_appDataPath);
                return _appDataPath;
            }
        }

        public static string LogFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_logFilePath)) _logFilePath = Path.Combine(AppDataPath, Fields.LogFileName);
                return _logFilePath;
            }
        }

        public static string GetCurrentProjectPath => Environment.CurrentDirectory.Replace(@"\bin\Debug", @"\bin\Debug");
    }
}
