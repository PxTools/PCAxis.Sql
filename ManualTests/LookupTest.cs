using PCAxis.Sql.ApiUtils;

namespace ManualTests
{
    [Ignore]
    [TestClass]
    public class LookupTest
    {
        private readonly string _mainLanguage;
        private readonly string _okTableId;

        public LookupTest()
        {
            _mainLanguage = "no";
            _okTableId = "07459";

            /*
            _mainLanguage = "sv";
            _okTableId ="";
            */
        }




        [TestMethod]
        public void TestOkValueSet()
        {

            PCAxis.Sql.Models.MenuLookupTables actual_data = ApiUtilStatic.GetMenuLookupTables(_mainLanguage);
            Assert.IsNotNull(actual_data);
            Assert.IsTrue(actual_data.ContainsKey(_okTableId));
        }

    }
}
