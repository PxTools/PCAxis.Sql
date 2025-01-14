using PCAxis.Sql.ApiUtils;

namespace ManualTests
{
    [Ignore]
    [TestClass]
    public class CodelistsTest
    {
        private readonly string _mainLanguage;
        private readonly string _okVS;
        private readonly string _okGrouping;

        public CodelistsTest()
        {
            _mainLanguage = "no";
            _okVS = "KOKkommuneregion0000008";
            _okGrouping = "EUlandNY";

            /*
            _mainLanguage = "sv";
            _okVS="";
            _okGrouping="";
            */
        }




        [TestMethod]
        public void TestOkValueSet()
        {
            string getId = _okVS;

            PCAxis.Sql.Models.ValueSet actual_data = ApiUtilStatic.GetValueSet(getId, _mainLanguage);
            Assert.IsNotNull(actual_data);
            Assert.AreEqual(2, actual_data.AvailableLanguages.Count);
            Assert.IsTrue(actual_data.AvailableLanguages.Contains(_mainLanguage));
            Assert.IsTrue(actual_data.AvailableLanguages.Contains("en"));

            Assert.AreEqual(actual_data.Id, getId);
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Label));
            Assert.IsTrue(actual_data.Values.Count > 0);
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Values[0].Code));
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Values[0].Text));


            actual_data = ApiUtilStatic.GetValueSet(getId, "en");
            Assert.IsNotNull(actual_data);
            Assert.AreEqual(2, actual_data.AvailableLanguages.Count);
            Assert.IsTrue(actual_data.AvailableLanguages.Contains(_mainLanguage));
            Assert.IsTrue(actual_data.AvailableLanguages.Contains("en"));

            Assert.AreEqual(actual_data.Id, getId);
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Label));
            Assert.IsTrue(actual_data.Values.Count > 0);
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Values[0].Code));
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Values[0].Text));

        }



        [TestMethod]
        public void TestGetGrouping()
        {
            //string getId = "AldGrupp19Grp5";
            //string getId = "GrkretsBydel2002";
            string getId = _okGrouping;

            PCAxis.Sql.Models.Grouping actual_data = ApiUtilStatic.GetGrouping(getId, _mainLanguage);

            Assert.IsNotNull(actual_data);
            Assert.AreEqual(2, actual_data.AvailableLanguages.Count);
            Assert.IsTrue(actual_data.AvailableLanguages.Contains(_mainLanguage));
            Assert.IsTrue(actual_data.AvailableLanguages.Contains("en"));


            PCAxis.Sql.Models.Grouping data_en = ApiUtilStatic.GetGrouping(getId, "en");
        }

        [TestMethod]
        public void TestValidNames()
        {
            // ArgumentException means invalid id (cannot exists in any db)
            // ApplicationException means not found (does no exist in this db)

            string vsId = "Spa3ce ok.   _ :sdfsdDSÆØ";
            Assert.ThrowsException<ApplicationException>(() => ApiUtilStatic.GetValueSet(vsId, "en")
                 );

            List<string> badIds = new();
            badIds.Add("Bad id Semicolon'4");
            badIds.Add("Bad id Semicolon*4");
            badIds.Add("Bad id sd;4");
            badIds.Add("Bad id id12 \n4");
            badIds.Add("Bad%20id ");
            badIds.Add("Bad id 1#2");
            badIds.Add("Bad id 3@4");

            foreach (string badId in badIds)
            {
                Assert.ThrowsException<ArgumentException>(() => ApiUtilStatic.GetValueSet(badId, "en"), "Does not fail for: " + badId
                    );
            }


        }

    }
}
