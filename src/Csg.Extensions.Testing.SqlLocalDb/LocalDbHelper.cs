using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Csg.Extensions.Testing.SqlLocalDb
{
    public static class LocalDbHelper
    {
        public static string DataPath = @"%LOCALAPPDATA%\LocalDb";
        
        private static bool TryFindFirstPath(IEnumerable<string> paths, out string matchingPath)
        {
            foreach (var path in paths)
            {
                string s = System.Environment.ExpandEnvironmentVariables(path);
                if (System.IO.File.Exists(s))
                {
                    matchingPath = s;
                    return true;
                }
            }

            matchingPath = null;
            return false;
        }

        private static int Exec(string cmd, string arguments, bool useCmd = true, System.IO.StreamWriter output = null)
        {


            string fileName = useCmd ? "cmd.exe" : cmd;
            string args = useCmd ? string.Concat("/c ", cmd, " ",arguments): arguments;

            if (output == null)
            {
                System.Diagnostics.Debug.WriteLine($"Executing {fileName} {args}");
            }
            else
            {
                output.WriteLine($"Executing {fileName} {args}");
            }

            var _processInfo = new ProcessStartInfo(fileName, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var _process = Process.Start(_processInfo);

            _process.WaitForExit();

            string _output = _process.StandardOutput.ReadToEnd();
            string _error = _process.StandardError.ReadToEnd();

            var _exitCode = _process.ExitCode;

            if (output == null)
            {
                System.Diagnostics.Debug.WriteLine(_output);
                System.Diagnostics.Debug.WriteLine(_error);
            }
            else
            {
                output.WriteLine(_output);
                output.WriteLine(_error);
            }

            _process.Close();

            return _exitCode;
        }

        public static void CreateInstance(string instanceName)
        {
            Exec("sqllocaldb.exe", $"create {instanceName} -s");
            string dataPath = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables(DataPath), instanceName);

            if (!System.IO.Directory.Exists(dataPath))
            {
                System.IO.Directory.CreateDirectory(dataPath);
            }
        }

        public static string CreateDatabase(string dbName, string instanceName)
        {
            Exec("sqllocaldb.exe", $"create {instanceName} -s");

            string connectionString = $"Data Source=(localdb)\\{instanceName};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            string dataPath = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables(DataPath), instanceName);
            
            using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
IF EXISTS (SELECT 1 FROM sys.databases WHERE [name]='{dbName}') 
DROP DATABASE [{dbName}]; 
CREATE DATABASE [{dbName}] 
ON PRIMARY ( Name = {dbName}_data, FILENAME = '{dataPath}\{dbName}_data.mdf')
LOG ON ( Name = {dbName}_log, FILENAME = '{dataPath}\{dbName}_log.ldf')
;";

                cmd.ExecuteNonQuery();
            }

            connectionString = string.Concat(connectionString, $";Initial Catalog={dbName}");

            return connectionString;
        }
                
        public static void DeleteDatabase(string databaseName, string instanceName)
        {
            string connectionString = $"Data Source=(localdb)\\{instanceName};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $"IF EXISTS (SELECT 1 FROM sys.databases WHERE [name]='{databaseName}') DROP DATABASE [{databaseName}];";
                cmd.ExecuteNonQuery();
            }
        }

        public static void StopInstance(string instanceName)
        {
            int resultCode = Exec("sqllocaldb.exe", $"stop {instanceName} -k");

            if (resultCode != 0)
            {
                throw new Exception($"sqllocaldb.exe stop result code {resultCode}");
            }
        }

        public static void DeleteInstance(string instanceName, bool deleteFiles = true)
        {
            StopInstance(instanceName);

            var resultCode = Exec("sqllocaldb.exe", $"delete {instanceName}");
            if (resultCode != 0)
            {
                throw new Exception($"sqllocaldb.exe delete result code {resultCode}");
            }

            string dataPath = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables(DataPath), instanceName);
            var files = System.IO.Directory.GetFiles(dataPath);
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }
        }
    }
}
