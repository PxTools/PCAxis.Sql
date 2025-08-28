using PCAxis.Sql.ApiUtils;

namespace ManualTests
{
    [Ignore("Need to connect to the to to run this")]
    [TestClass]
    public class HealthCheckTest
    {
        [TestMethod]
        public void TestGetGrouping()
        {
            string query = "select 1";



            var active = ApiUtilStatic.IsDbConnectionHealthy(query);

            Assert.IsTrue(active, "Database connection should be healthy");


        }
    }
}
