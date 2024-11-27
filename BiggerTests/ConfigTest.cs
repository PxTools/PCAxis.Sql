using System.Configuration;

using PCAxis.Sql.DbConfig;

namespace BiggerTests
{
    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            //System.Configuration.ConfigurationFileMap configMap = new ConfigurationFileMap("./app.config");
            //System.Configuration.Configuration configuration = System.Configuration.ConfigurationManager.OpenMappedMachineConfiguration(configMap);
            //var sdfdsz = configuration.AppSettings["dsa"];
            //string value = configuration.AppSettings["TestKey"];
            string path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            var k = ConfigurationManager.AppSettings["TestKey"];
            Assert.AreEqual("testvalue", k);
        }

        [TestMethod]
        public void TestMethod2()
        {

            var k = ConfigurationManager.AppSettings["dbconfigFile"];
            Assert.AreEqual("SqlDb.config", k);
        }



        [TestMethod]
        public void TestGetsDefaultDatabase()
        {
            var dbConf = SqlDbConfigsStatic.DefaultDatabase;

            var langCount = dbConf.GetAllLanguages().Count;
            Assert.IsTrue(langCount > 1);
        }

    }
}
