using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Csg.Extensions.Testing.SqlLocalDb.Tests
{
    [TestClass]
    public class LocalDbHelperTests
    {
        private static string InstanceName = "UnitTestProject";

        static LocalDbHelperTests()
        {
            LocalDbHelper.TryDeleteInstance(InstanceName);
            LocalDbHelper.CreateInstance(InstanceName);            
        }      
        
        [AssemblyCleanup]
        public static void Cleanup()
        {
            LocalDbHelper.DeleteInstance(InstanceName);
        }
 
        [TestMethod]
        public void TestCreateConnectCloseDeleteDatabase()
        {
            var connStr = LocalDbHelper.CreateDatabase("Database1", InstanceName);

            Assert.IsTrue(File.Exists(Environment.ExpandEnvironmentVariables(Path.Combine(LocalDbHelper.DataPath, InstanceName, "Database1_data.mdf"))));

            var conn = new System.Data.SqlClient.SqlConnection(connStr);
            
            conn.Open();
            Assert.AreEqual(System.Data.ConnectionState.Open, conn.State);
            // intentionally leaving database connection open above 

            LocalDbHelper.DeleteDatabase("Database1", InstanceName);
            Assert.IsFalse(File.Exists(Environment.ExpandEnvironmentVariables(Path.Combine(LocalDbHelper.DataPath, InstanceName, "Database1_data.mdf"))));
        }
    }
}
