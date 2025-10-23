using PCAxis.Paxiom;
using PCAxis.Sql.ApiUtils;

namespace ManualTests
{
    [Ignore("Need to connect to a DB to run this, so Github actions can't do it.")]
    [TestClass]
    public class ParserTest
    {


        public ParserTest()
        {
        }

        [TestMethod]
        public void Test2()
        {
            var myLang = "no";
            var builder = new PCAxis.PlugIn.Sql.PXSQLBuilder();
            var id2id = ApiUtilStatic.GetMenuLookupTables(myLang);
            var urlId = "06266";
            var dbId = id2id[urlId].Selection;
            builder.SetPath(dbId);
            builder.SetPreferredLanguage(myLang);
            builder.BuildForSelection();

            var selectAll = Selection.SelectAll(builder.Model.Meta);
            var select3 = new List<Selection>();
            select3.Add(selectAll.ElementAt(0));
            select3.Add(new Selection(selectAll.ElementAt(1).VariableCode));
            select3.Add(new Selection(selectAll.ElementAt(2).VariableCode));
            select3.Add(selectAll.ElementAt(3));
            select3.Add(selectAll.ElementAt(4));
            var select = select3.ToArray();

            builder.BuildForPresentation(select);

            Assert.IsNotNull(builder.Model);

        }

        [TestMethod]
        public void Test3()
        {
            var myLang = "no";
            var builder = new PCAxis.PlugIn.Sql.PXSQLBuilder();
            var id2id = ApiUtilStatic.GetMenuLookupTables(myLang);
            var urlId = "04321";
            var dbId = id2id[urlId].Selection;
            builder.SetPath(dbId);
            builder.SetPreferredLanguage(myLang);
            builder.BuildForSelection();

            var aGrouping = new PCAxis.Paxiom.GroupingInfo("KommuneXTtSted");
            builder.ApplyGrouping("Region", aGrouping, GroupingIncludesType.AggregatedValues);

            var selectAll = Selection.SelectAll(builder.Model.Meta);
            var select3 = new List<Selection>();
            select3.Add(selectAll.ElementAt(0));
            select3.Add(new Selection(selectAll.ElementAt(1).VariableCode));
            select3.Add(new Selection(selectAll.ElementAt(2).VariableCode));
            select3.Add(selectAll.ElementAt(3));
            select3.Add(selectAll.ElementAt(4));
            var select = select3.ToArray();

            builder.BuildForPresentation(select);

            Assert.IsNotNull(builder.Model);
            int expectedVarieblesCount = 3;
            Assert.HasCount(expectedVarieblesCount, builder.Model.Meta.Variables);
            int expectedMatrixSize = 932;
            Assert.AreEqual(expectedMatrixSize, builder.Model.Data.MatrixSize);
        }

        [TestMethod]
        public void TestAnnual()
        {
            var myLang = "no";
            var builder = new PCAxis.PlugIn.Sql.PXSQLBuilder();
            var id2id = ApiUtilStatic.GetMenuLookupTables(myLang);
            var urlId = "05803";
            var dbId = id2id[urlId].Selection;
            builder.SetPath(dbId);
            builder.SetPreferredLanguage(myLang);
            builder.BuildForSelection();

            var mTimeScale = builder.Model.Meta.Variables[1].TimeScale;
            var lala = builder.Model.Meta.ExtendedProperties;

            Assert.AreEqual(TimeScaleType.Annual, mTimeScale);
            Assert.IsNotNull(builder.Model);

            builder.BuildForPresentation(Selection.SelectAll(builder.Model.Meta));

            mTimeScale = builder.Model.Meta.Variables[1].TimeScale;
            Assert.AreEqual(TimeScaleType.Annual, mTimeScale);
            Assert.IsNotNull(builder.Model);

        }


        [TestMethod]
        public void TestWeekly()
        {
            var myLang = "no";
            var builder = new PCAxis.PlugIn.Sql.PXSQLBuilder();
            var id2id = ApiUtilStatic.GetMenuLookupTables(myLang);
            var urlId = "03024";
            var dbId = id2id[urlId].Selection;
            builder.SetPath(dbId);
            builder.SetPreferredLanguage(myLang);
            builder.BuildForSelection();
            var mTimeScale = builder.Model.Meta.Variables[2].TimeScale;

            var lala = builder.Model.Meta.ExtendedProperties;

            Assert.AreEqual(TimeScaleType.Weekly, mTimeScale);
            Assert.IsNotNull(builder.Model);

            builder.BuildForPresentation(Selection.SelectAll(builder.Model.Meta));
            var selectPerfekt = Selection.SelectAll(builder.Model.Meta);

            //selectPerfekt[2].ValueCodes

            mTimeScale = builder.Model.Meta.Variables[2].TimeScale;
            Assert.AreEqual(TimeScaleType.Weekly, mTimeScale);
            Assert.IsNotNull(builder.Model);
        }

        [TestMethod]
        public void TestWeeklyGapInTime()
        {
            var myLang = "no";
            var builder = new PCAxis.PlugIn.Sql.PXSQLBuilder();
            var id2id = ApiUtilStatic.GetMenuLookupTables(myLang);
            var urlId = "03024";
            var dbId = id2id[urlId].Selection;
            builder.SetPath(dbId);
            builder.SetPreferredLanguage(myLang);
            builder.BuildForSelection();

            var mTimeScale = builder.Model.Meta.Variables[2].TimeScale;
            Assert.AreEqual(TimeScaleType.Weekly, mTimeScale);
            Assert.IsNotNull(builder.Model);

            var selectComplete = Selection.SelectAll(builder.Model.Meta);

            var selectGapsTime = new Selection(selectComplete[2].VariableCode);
            selectGapsTime.ValueCodes.Add(selectComplete[2].ValueCodes[0]);
            selectGapsTime.ValueCodes.Add(selectComplete[2].ValueCodes[2]);
            Selection[] selectWithGaps = new[] { selectComplete[0], selectComplete[1], selectGapsTime };
            //selectPerfekt[2].ValueCodes

            builder.BuildForPresentation(selectWithGaps);



            mTimeScale = builder.Model.Meta.Variables[2].TimeScale;
            var lala = builder.Model.Meta.ExtendedProperties;
            Assert.AreEqual(TimeScaleType.Weekly, mTimeScale);
            Assert.IsNotNull(builder.Model);
        }


    }
}
