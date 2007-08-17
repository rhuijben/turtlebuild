using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System.IO;
using QQn.TurtleUtils.Cryptography;

namespace TurtleTests
{
	[TestFixture]
	public class CryptoTests
	{
		string _snkFile;
		string SnkFile
		{
			get
			{
				if (_snkFile == null)
				{
					_snkFile = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\..\\Libraries\\QQn.TurtleUtils\\QQn.TurtleUtils.snk");

					Assert.That(File.Exists(_snkFile), "Snk file exists");
				}
				return _snkFile;
			}
		}

		[Test]
		public void TestMyHash()
		{
			StrongNameKey snkIn = StrongNameKey.LoadFrom(SnkFile);
			byte[] data = new byte[20];
			byte[] signature = snkIn.SignHash(data);
			byte[] publicKeyData = snkIn.GetPublicKeyData();


			StrongNameKey snkVerify = StrongNameKey.LoadFrom(publicKeyData);
			Assert.That(snkVerify.VerifyHash(data, signature), Is.True, "Verification completed");
		}

		[Test]
		public void TestTokens()
		{
			StrongNameKey snkIn = StrongNameKey.LoadFrom(SnkFile);
			Assert.That(snkIn.PublicKeyToken, Is.EqualTo(QQnCryptoHelpers.HashString(typeof(QQnCryptoHelpers).Assembly.GetName().GetPublicKeyToken())), "Our public-token api matches the .Net one");
		}


		void SignAndVerify(HashType hashType)
		{
			string hash = QQnCryptoHelpers.CalculateFileHash(SnkFile, hashType);

			Assert.That(QQnCryptoHelpers.VerifyFileHash(SnkFile, hash), Is.True, "Hash of type {0} ok", hashType);
		}

		[Test]
		public void TestSign()
		{
			Assert.That(QQnCryptoHelpers.CalculateFileHash(SnkFile, HashType.Null), Is.EqualTo("31a02f23fb06dc6326428843782c049d14e90fb14f74704909e6e6ba1a2592c0,type=SHA256,size=596"), "SHA256++Hash matches");
			Assert.That(QQnCryptoHelpers.CalculateFileHash(SnkFile, HashType.SHA1), Is.EqualTo("dcd59a19afe007b2a7e706d100f8fcdeb4e4e1e9"), "SHA1 Hash matches");
			Assert.That(QQnCryptoHelpers.CalculateFileHash(SnkFile, HashType.MD5), Is.EqualTo("2b19ab067c03e05d55f62702c4a79864"), "MD5 Hash matches");

			SignAndVerify(HashType.MD5);
			SignAndVerify(HashType.SHA1);
			SignAndVerify(HashType.SHA256);
			SignAndVerify(HashType.SHA512);

			SignAndVerify(HashType.SHA1 | HashType.PlusSize);
			SignAndVerify(HashType.SHA1 | HashType.PlusType);
			SignAndVerify(HashType.SHA1 | HashType.PlusSize | HashType.PlusType);
		}

		[Test]
		public void GuidTests()
		{
			Guid baseGuid = new Guid("0D166EB6-69BC-47DB-A243-C31ECE55E715");
			string item = "http://qqn.qqn.nl/qqn//qqn.qqn";

			Guid calcGuid = QQnCryptoHelpers.GuidFromHash(baseGuid, Encoding.UTF8.GetBytes(item));

			Assert.That(calcGuid, Is.EqualTo(new Guid("014C3195-DE6C-5A19-9563-163716A49EC6")), "Calculated guids are equal");

			calcGuid = QQnCryptoHelpers.GuidFromString(baseGuid, item);

			Assert.That(calcGuid, Is.EqualTo(new Guid("014C3195-DE6C-5A19-9563-163716A49EC6")), "Calculated guids are equal");
		}
	}
}
