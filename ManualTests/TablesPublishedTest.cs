using PCAxis.Sql.ApiUtils;

namespace ManualTests
{
    [Ignore("Need to connect to the to to run this, so Github actions cant do it.")]
    [TestClass]
    public class TablesPublishTest
    {
        private readonly DateTime dt_2025_05_24;
        private readonly DateTime dt_2025_05_25;

        private readonly DateTime dt_empty;

        public TablesPublishTest()
        {
            dt_2025_05_24 = new DateTime(2025, 5, 24, 8, 0, 0, DateTimeKind.Local);
            dt_2025_05_25 = new DateTime(2025, 5, 25, 8, 0, 0, DateTimeKind.Local);
            dt_empty = new DateTime(2025, 5, 25, 7, 0, 0, DateTimeKind.Local);
        }

        [TestMethod]
        public void TestTablesPublishNormal()
        {
            List<string> expected_data = new List<string> { "07888", "10701", "10729", "10738", "10745", "10746", "10748", "10749", "11018" };
            List<string> actual_data = ApiUtilStatic.GetTablesPublishedBetween(dt_2025_05_24, dt_2025_05_25);
            Assert.IsNotNull(actual_data);
            CollectionAssert.AreEquivalent(expected_data, actual_data);
        }

        [TestMethod]
        public void TestTablesPublishEmptyl()
        {
            List<string> expected_data = new List<string>();
            List<string> actual_data = ApiUtilStatic.GetTablesPublishedBetween(dt_empty, dt_empty);
            Assert.IsNotNull(actual_data);
            CollectionAssert.AreEquivalent(expected_data, actual_data);
        }

        [TestMethod]
        public void TestTablesPublish_FromAfterTo_ThrowsArgumentException()
        {
            var exception = Assert.ThrowsExactly<ArgumentException>(() => ApiUtilStatic.GetTablesPublishedBetween(dt_2025_05_25, dt_2025_05_24));
            StringAssert.StartsWith(exception.Message, "'from' date cannot be later than 'to' date.");
        }

    }
}
