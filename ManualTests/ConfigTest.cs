using System.Configuration;

using PCAxis.Sql.DbConfig;

namespace ManualTests
{
    [Ignore]
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

        [TestMethod]
        public void TestGetValueSet()
        {
            string vsId = "KOKkommuneregion0000008";
            PCAxis.Sql.BuilderLess.EntryPoint entryPoint = new PCAxis.Sql.BuilderLess.EntryPoint();
            PCAxis.Sql.Models.ValueSet vs = entryPoint.GetValueSet(vsId, "no");
            PCAxis.Sql.Models.ValueSet vs_en = entryPoint.GetValueSet(vsId, "en");


        }



        [TestMethod]
        public void TestGet2ValueSet()
        {
            string vsId = "NoSuchVS";

            PCAxis.Sql.BuilderLess.EntryPoint entryPoint = new PCAxis.Sql.BuilderLess.EntryPoint();
            PCAxis.Sql.Models.ValueSet vs = entryPoint.GetValueSet(vsId, "no");

            PCAxis.Sql.Models.ValueSet vs_en = entryPoint.GetValueSet(vsId, "en");


        }

        [TestMethod]
        public void TestGetGrouping()
        {
            //string getId = "AldGrupp19Grp5";
            //string getId = "GrkretsBydel2002";
            string getId = "EUlandNY";

            PCAxis.Sql.BuilderLess.EntryPoint entryPoint = new();
            PCAxis.Sql.Models.Grouping data_no = entryPoint.GetGrouping(getId, "no");

            PCAxis.Sql.Models.Grouping data_en = entryPoint.GetGrouping(getId, "en");


        }

    }
}
