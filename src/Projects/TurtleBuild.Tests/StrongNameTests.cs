using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QQn.TurtleUtils.Streams;
using NUnit.Framework.SyntaxHelpers;
using System.IO;

namespace TurtleTests
{
	[TestFixture]
	public class StrongNameTests
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

		string _tmpPath;
		string TmpPath
		{
			get
			{
				if (_tmpPath == null)
					_tmpPath = Path.GetTempFileName();

				return _tmpPath;
			}
		}

		[TestFixtureTearDown]
		public void DeleteTmp()
		{
			if (File.Exists(TmpPath))
				File.Delete(TmpPath);
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
			Assert.That(QQnCryptoHelpers.HashString(snkIn.PublicKeyToken), Is.EqualTo(QQnCryptoHelpers.HashString(typeof(QQnCryptoHelpers).Assembly.GetName().GetPublicKeyToken())), "Our public-token api matches the .Net one");
		}

		[Test]
		public void CreateSignedFile()
		{
			StrongNameKey snkIn = StrongNameKey.LoadFrom(SnkFile);

			string file;
			using (FileStream f = File.Create(file = TmpPath))
			using (AssuredStream s = new AssuredStream(f, snkIn, "TestPackage"))
			using (BinaryWriter bw = new BinaryWriter(s))
			{
				Assert.That(s.Position, Is.EqualTo(0L), "Position 0 within signed file");
				bw.Write("Test String");

				Assert.That(s.Position, Is.Not.EqualTo(0L), "Position 0 within signed file");
			}

			using (FileStream f = File.OpenRead(file))
			using (AssuredStream s = new AssuredStream(f, VerificationMode.Full))
			using (BinaryReader br = new BinaryReader(s))
			{
				Assert.That(s.Position, Is.EqualTo(0L), "Position 0 within signed file");				
				Assert.That(br.ReadString(), Is.EqualTo("Test String"));

				Assert.That(s.Position, Is.Not.EqualTo(0L), "Position not 0 within signed file");

				Assert.That(s.HashString, Is.Not.Null, "Hash is set");
			}
		}

		[Test]
		public void CreateTrippleSignedFile()
		{
			// Tests whether a stream within a signed stream behaves as an outer stream
			StrongNameKey snkIn = StrongNameKey.LoadFrom(SnkFile);

			string file;
			using (FileStream f = File.Create(file = TmpPath))
			using (AssuredStream s = new AssuredStream(f, snkIn, "TestPackage"))
			using (AssuredStream s2 = new AssuredStream(s, snkIn, "InnerPackage"))
			using (AssuredStream s3 = new AssuredStream(s2, snkIn, "InnerPackage"))
			using (BinaryWriter bw = new BinaryWriter(s3))
			{
				Assert.That(s3.Position, Is.EqualTo(0L), "Position 0 within signed file");
				bw.Write("Test String");

				Assert.That(s3.Position, Is.Not.EqualTo(0L), "Position 0 within signed file");
			}

			using (FileStream f = File.OpenRead(file))
			using (AssuredStream s = new AssuredStream(f, VerificationMode.Full))
			using (AssuredStream s2 = new AssuredStream(s, VerificationMode.Full))
			using (AssuredStream s3 = new AssuredStream(s2, VerificationMode.Full))
			using (BinaryReader br = new BinaryReader(s3))
			{
				Assert.That(s3.Position, Is.EqualTo(0L), "Position 0 within signed file");
				Assert.That(br.ReadString(), Is.EqualTo("Test String"));

				Assert.That(s3.Position, Is.Not.EqualTo(0L), "Position not 0 within signed file");

				Assert.That(s.HashString, Is.Not.Null, "Hash is set");
			}
		}

		[Test]
		public void CreateSignedSubFile()
		{
			// Tests whether a stream within a signed stream behaves as an outer stream
			StrongNameKey snkIn = StrongNameKey.LoadFrom(SnkFile);

			string file;
			using (FileStream f = File.Create(file = TmpPath))
			using (AssuredStream s = new AssuredStream(f, snkIn, "TestPackage"))
			using (AssuredSubStream s2 = new AssuredSubStream(s, VerificationMode.Full))
			using (BinaryWriter bw = new BinaryWriter(s2))
			{
				Assert.That(s2.Position, Is.EqualTo(0L), "Position 0 within signed file");
				bw.Write("Test String");

				Assert.That(s2.Position, Is.Not.EqualTo(0L), "Position 0 within signed file");
			}

			using (FileStream f = File.OpenRead(file))
			using (AssuredStream s = new AssuredStream(f, VerificationMode.Full))
			using (AssuredSubStream s2 = new AssuredSubStream(s, VerificationMode.Full))
			using (BinaryReader br = new BinaryReader(s2))
			{
				Assert.That(s2.Position, Is.EqualTo(0L), "Position 0 within signed file");
				Assert.That(br.ReadString(), Is.EqualTo("Test String"));

				Assert.That(s2.Position, Is.Not.EqualTo(0L), "Position not 0 within signed file");

				Assert.That(s.HashString, Is.Not.Null, "Hash is set");
			}
		}

		[Test]
		public void CreateHashedSubFile()
		{
			string file;
			using (FileStream f = File.Create(file = TmpPath))
			using (AssuredSubStream s2 = new AssuredSubStream(f, VerificationMode.Full))
			using (BinaryWriter bw = new BinaryWriter(s2))
			{
				Assert.That(s2.Position, Is.EqualTo(0L), "Position 0 within signed file");
				bw.Write("Test String");

				Assert.That(s2.Position, Is.Not.EqualTo(0L), "Position 0 within signed file");
			}

			using (FileStream f = File.OpenRead(file))
			using (AssuredSubStream s2 = new AssuredSubStream(f, VerificationMode.Full))
			using (BinaryReader br = new BinaryReader(s2))
			{
				Assert.That(s2.Position, Is.EqualTo(0L), "Position 0 within signed file");
				Assert.That(br.ReadString(), Is.EqualTo("Test String"));

				Assert.That(s2.Position, Is.Not.EqualTo(0L), "Position not 0 within signed file");
			}
		}

	}
}
