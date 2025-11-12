using PCAxis.Sql.ApiUtils;

namespace ManualTests
{
    [Ignore("Need to connect to the to to run this")]
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
            var exception = Assert.ThrowsExactly<ApplicationException>(() => ApiUtilStatic.GetValueSet(_badVS, "en"));

            StringAssert.StartsWith(exception.Message, "ValueSet NoSuchVS not found for language ");
        }


        [TestMethod]
        public void TestOkValueSet()
        {
            string getId = _okVS;

            PCAxis.Sql.Models.ValueSet actual_data = ApiUtilStatic.GetValueSet(getId, _mainLanguage);
            Assert.IsNotNull(actual_data);
            Assert.HasCount(2, actual_data.AvailableLanguages);
            Assert.Contains(_mainLanguage, actual_data.AvailableLanguages);
            Assert.Contains("en", actual_data.AvailableLanguages);

            Assert.AreEqual(actual_data.Id, getId);
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Label));
            Assert.IsNotEmpty(actual_data.Values);
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Values[0].Code));
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Values[0].Text));


            actual_data = ApiUtilStatic.GetValueSet(getId, "en");
            Assert.IsNotNull(actual_data);
            Assert.HasCount(2, actual_data.AvailableLanguages);
            Assert.Contains(_mainLanguage, actual_data.AvailableLanguages);
            Assert.Contains("en", actual_data.AvailableLanguages);

            Assert.AreEqual(actual_data.Id, getId);
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Label));
            Assert.IsNotEmpty(actual_data.Values);
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Values[0].Code));
            Assert.IsFalse(String.IsNullOrEmpty(actual_data.Values[0].Text));
            Assert.HasCount(_okVS_values, actual_data.Values);
        }



        [TestMethod]
        public void TestGetGrouping()
        {
            //string getId = "AlleAldre48e3";  // pres I  1 mother 5 children
            //string getId = "GrkretsBydel2002";
            //string getId = "KommSummer"; //tab 07459 pres A
            //string getId = "AlleVUtd";  // pres I  12 mother 118 children
            string getId = _okGrouping;



            PCAxis.Sql.Models.Grouping actual_data = ApiUtilStatic.GetGrouping(getId, _mainLanguage);

            Assert.IsNotNull(actual_data);
            Assert.HasCount(2, actual_data.AvailableLanguages);
            Assert.Contains(_mainLanguage, actual_data.AvailableLanguages);
            Assert.Contains("en", actual_data.AvailableLanguages);

            PCAxis.Sql.Models.Grouping data_en = ApiUtilStatic.GetGrouping(getId, "en");
            Assert.IsNotNull(data_en);
        }




        [TestMethod]
        public void TestSSBSortedSelectionGrouping()
        {
            string getId = "KommNyeste";

            PCAxis.Sql.Models.Grouping actual_data = ApiUtilStatic.GetGrouping(getId, "en");

            Assert.IsNotNull(actual_data);
            Assert.HasCount(423, actual_data.Values);
            Assert.AreEqual("Municipalities 2018-2019", actual_data.Label);
            Assert.AreEqual("0101", actual_data.Values[0].Code);
            Assert.AreEqual("Halden (-2019)", actual_data.Values[0].Text);
            Assert.AreEqual("0104", actual_data.Values[1].Code);
            Assert.AreEqual("Moss (-2019)", actual_data.Values[1].Text);
            Assert.AreEqual("2030", actual_data.Values[422].Code);

        }


        [TestMethod]
        public void TestValidNames()
        {
            // ArgumentException means invalid id (cannot exists in any db)
            // ApplicationException means not found (does no exist in this db)

            string vsId = "Spa3ce ok.   _ :sdfsdDS��";
            Assert.ThrowsExactly<ApplicationException>(() => ApiUtilStatic.GetValueSet(vsId, "en"));

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
                Assert.ThrowsExactly<ArgumentException>(() => ApiUtilStatic.GetValueSet(badId, "en"), "Does not fail for: " + badId);
            }


        }

    }
}
