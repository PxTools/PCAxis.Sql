using PCAxis.Sql.ApiUtils;

namespace ManualTests
{
    [Ignore]
    [TestClass]
    public class LookupTest
    {
        private readonly string _mainLanguage;
        private readonly string _okTableId;
        private readonly string _okFolderId;

        public LookupTest()
        {
            _mainLanguage = "no";
            _okTableId = "07459";
            _okFolderId = "KPI";

            /*
            _mainLanguage = "sv";
            _okTableId ="";
            */
        }




        [TestMethod]
        public void TestLookupTables()
        {

            PCAxis.Sql.Models.MenuLookupTables actual_data = ApiUtilStatic.GetMenuLookupTables(_mainLanguage);
            Assert.IsNotNull(actual_data);
            Assert.IsTrue(actual_data.ContainsKey(_okTableId));
        }

        [TestMethod]
        public void TestLookupFolders()
        {
            PCAxis.Sql.Models.MenuLookupFolders actual_data = ApiUtilStatic.GetMenuLookupFolders(_mainLanguage);
            Assert.IsNotNull(actual_data);
            Assert.IsTrue(actual_data.ContainsKey(_okFolderId));
            actual_data = ApiUtilStatic.GetMenuLookupFolders("en");
            Assert.IsNotNull(actual_data);
            Assert.IsTrue(actual_data.ContainsKey(_okFolderId));
        }

    }
}
