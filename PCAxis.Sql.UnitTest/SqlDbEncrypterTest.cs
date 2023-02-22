using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PCAxis.Encryption.UnitTest
{
	[TestClass]
	public class SqlDbEncrypterTest
	{
		[TestMethod]
		public void EncryptShouldReturnFalse()
		{

			//Arrange,
			var filepath = "Nopath/Sqldb.Config";


			// Act
			var result = PCAxis.Encryption.SqlDbEncrypter.Decrypt(filepath);


			//Assert
			Assert.AreEqual(result, false);

		}
	}
}