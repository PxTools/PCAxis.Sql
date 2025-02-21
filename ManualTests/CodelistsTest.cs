using PCAxis.Sql.ApiUtils;

namespace ManualTests
{
    [Ignore]
    [TestClass]
    public class CodelistsTest
    {
        private readonly string _mainLanguage;
        private readonly string _okVS;
        private readonly int _okVS_values;

        private readonly string _badVS;
        private readonly string _okGrouping;

        private readonly string _elimMethodC_VS;
        private readonly string _elimMethodC_Code;

        private readonly string _elimMethodA_VS;
        private readonly string _elimMethodN_VS;

        public CodelistsTest()
        {
            _mainLanguage = "no";
            _okVS = "KOKkommuneregion0000008";
            _okVS_values = 467;

            _elimMethodC_VS = "AlleAldre14c";
            _elimMethodC_Code = "999";

            _elimMethodA_VS = "ArbstyrkStat6";
            _elimMethodN_VS = "Beredskap";



            _okGrouping = "EUlandNY";

            /*
            _mainLanguage = "sv";
            _okVS = "RegionKommun07";
            _okGrouping = "RegionA-region_2";

            */

            _badVS = "NoSuchVS";
        }


        [TestMethod]
        public void TestElimCValueSet()
        {
            PCAxis.Sql.Models.ValueSet actual_data = ApiUtilStatic.GetValueSet(_elimMethodC_VS, _mainLanguage);
            Assert.IsTrue(actual_data.Elimination);
            Assert.AreEqual(_elimMethodC_Code, actual_data.EliminationValueCode);
        }

        [TestMethod]
        public void TestElimNValueSet()
        {
            PCAxis.Sql.Models.ValueSet actual_data = ApiUtilStatic.GetValueSet(_elimMethodN_VS, _mainLanguage);
            Assert.IsFalse(actual_data.Elimination);
        }

        [TestMethod]
        public void TestElimAValueSet()
        {
            PCAxis.Sql.Models.ValueSet actual_data = ApiUtilStatic.GetValueSet(_elimMethodA_VS, _mainLanguage);
            Assert.IsTrue(actual_data.Elimination);
        }


        [TestMethod]
        public void TestBadValueSet()
        {
            string getId = _badVS;
            var exception = Assert.ThrowsException<ApplicationException>(() => ApiUtilStatic.GetValueSet(_badVS, "en"));

            StringAssert.StartsWith(exception.Message, "ValueSet NoSuchVS not found for language ");
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
            Assert.AreEqual(_okVS_values, actual_data.Values.Count);
        }



        [TestMethod]
        public void TestGetGrouping()
        {
            //string getId = "AlleAldre48e3";  // pres I  1 mother 5 children 
            //string getId = "GrkretsBydel2002";
            //string getId = "KommSummer"; //tab 07459 pres A
            string getId = "AlleVUtd";  // pres I  12 mother 118 children 
            //string getId = _okGrouping;



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
