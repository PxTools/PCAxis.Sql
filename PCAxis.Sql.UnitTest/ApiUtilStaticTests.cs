using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PCAxis.Sql.ApiUtils;

namespace PCAxis.Sql.UnitTest
{

    [TestClass]
    public class ApiUtilStaticTests
    {

        [TestMethod]
        [TestCategory("Unit")]
        //[DataRow("valid_id_string")]
        //[DataRow("1.1")]
        //[DataRow("v123")]
        //[DataRow("123")]
        //[DataRow("COICOP MI1301 (fin)")]
        //[DataRow("COFOG2+3-siffer")]
        [DataRow("F_AntalBarn<6Bakgr")]
        [DataRow("F_AntalBarn>6Bakgr")]
        public void ValidateIdString_GivenValidValues_ReturnsSameValue(string value)
        {
            string result = InvokeValidateIdString(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ValidateIdString_GivenNull_ThrowsArgumentException()
        {
            // Act + Assert
            Assert.ThrowsExactly<ArgumentException>(() => InvokeValidateIdString(null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ValidateIdString_GivenInvalidCharacters_ThrowsArgumentException()
        {
            // Arrange
            var invalidId = "table/id";

            Assert.ThrowsExactly<ArgumentException>(() => InvokeValidateIdString(invalidId));
        }

        private static string InvokeValidateIdString(string input)
        {
            var method = typeof(ApiUtilStatic).GetMethod("ValidateIdString", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method);

            try
            {
                return (string)method.Invoke(null, new object[] { input });
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
