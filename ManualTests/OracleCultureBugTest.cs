using System.Data;
using System.Data.Common;
using System.Globalization;

using Oracle.ManagedDataAccess.Client;

using PCAxis.Paxiom;
using PCAxis.Sql.ApiUtils;
using PCAxis.Sql.DbConfig;

namespace ManualTests
{
    [Ignore("Need to connect to a DB to run this, so Github actions can't do it.")]
    [TestClass]
    public class OracleCultureBugTest
    {
        public OracleCultureBugTest()
        {
        }

        [TestMethod]
        public void TestFromDual()
        {
            // This applies to Norwegians using Oracle :-)
            //  This test just shows the problem:
            //  4 of our tables has some content where 'aA' occures in the code
            //  This has worked for years in the old pxweb, but gives error-message
            //  like Column 'OppgViaAppData_NilCnt' does not belong to table Table. in the new api
            //  running on the same server.
            //  
            //  (the issue is SBANK-687 )
            //
            // Solution run with CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var dbConf = SqlDbConfigsStatic.DefaultDatabase;
            var connectionString = dbConf.GetDefaultConnString();

            var columnamesInSelect = new List<string> { "a1AA", "a2aA", "a3aa", "a4Aa", "b5oE", "c6aE" };
            var sqlCols = "";
            var myFirst = true;
            foreach (string colname in columnamesInSelect)
            {
                if (myFirst)
                {
                    myFirst = false;
                }
                else
                {
                    sqlCols += " , ";
                }
                sqlCols += "'" + colname + "' as " + colname;
            }
            string sqlString = "select " + sqlCols + " from dual";
            Console.WriteLine("Culture: " + CultureInfo.CurrentCulture.Name);
            Console.WriteLine("sql: " + sqlString);


            CultureInfo.CurrentCulture = new CultureInfo("nb-NO");

            DataSet ds_nor = new DataSet();
            DataSet ds_invariant = new DataSet();
            ds_invariant.Locale = CultureInfo.InvariantCulture;

            using (DbDataAdapter pxDataAdapter = new OracleDataAdapter(sqlString, connectionString))
            {
                pxDataAdapter.Fill(ds_nor);
                pxDataAdapter.Fill(ds_invariant);
            }

            DataRow row_nor = ds_nor.Tables[0].Rows[0];
            DataRow row_invariant = ds_invariant.Tables[0].Rows[0];

            foreach (string colname in columnamesInSelect)
            {
                if (colname.Equals("a2aA"))
                {
                    Assert.IsNull(row_nor.Table.Columns[colname]);
                    Assert.IsNotNull(row_nor.Table.Columns[colname.ToLower()]);
                    Assert.IsNotNull(row_nor.Table.Columns[colname.ToUpper()]);

                    //Can extract using Upper
                    Assert.AreEqual<string>(colname, row_nor[colname.ToUpper()].ToString());

                    Assert.IsNotNull(row_invariant.Table.Columns[colname]);
                    Assert.AreEqual<string>(colname, row_invariant[colname].ToString());
                }
                else
                {
                    Assert.IsNotNull(row_nor.Table.Columns[colname]);
                    Assert.IsNotNull(row_invariant.Table.Columns[colname]);
                    Assert.AreEqual<string>(colname, row_invariant[colname].ToString());
                }
            }
        }





        [TestMethod]
        public void TestBugHunt_s687()
        {
            //comment this out to get error
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var myLang = "no";
            var builder = new PCAxis.PlugIn.Sql.PXSQLBuilder();
            var id2id = ApiUtilStatic.GetMenuLookupTables(myLang);

            var urlId = "12348";
            var dbId = id2id[urlId].Selection;
            builder.SetPath(dbId);
            builder.SetPreferredLanguage(myLang);
            builder.BuildForSelection();

            var selectAll = Selection.SelectAll(builder.Model.Meta);
            var selections = new List<Selection>();
            var sel0 = new Selection("ContentsCode");
            sel0.ValueCodes.Add("SosMedieJobb");
            sel0.ValueCodes.Add("OppgViaAppData");

            selections.Add(sel0);

            var sel1 = new Selection("Tid");
            sel1.ValueCodes.Add("2018");
            selections.Add(sel1);

            var sel2 = new Selection("Kjonn");
            selections.Add(sel2);

            var sel3 = new Selection("Alder");
            selections.Add(sel3);

            var select = selections.ToArray();

            builder.BuildForPresentation(select);

            Assert.IsNotNull(builder.Model);

        }



    }
}
