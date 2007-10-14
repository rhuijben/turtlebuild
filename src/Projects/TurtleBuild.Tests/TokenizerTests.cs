using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using NUnit.Framework;
using System.IO;
using System.Xml;
using QQn.TurtleUtils.Tokens.Converters;

namespace TurtleTests
{
	[TestFixture]
	public class TokenizerTests
	{
		public class SimpleCommandLine
		{
			[PositionToken(0)]
			public FileInfo File1;

			[RestToken, TokenDescription("Files")]
			public string[] Files;

			[Token("?", "h", "help")]
			public bool ShowHelp;
		}

		public class SimpleConnectionBlock
		{
			[Token("Data Source")]
			public string DataSource;

			[Token("Integrated Security")]
			public string IntegratedSecurity;

			[Token("UserId")]
			public string UserId;
		}

		public class MultiItem
		{
			[Token("File")]
			public string[] Files;

			[Token("?", "h", "help")]
			public bool ShowHelp;
		}

		[Test]
		public void TestParseCommandLine()
		{
			SimpleCommandLine scl;
			Assert.That(Tokenizer.TryParseCommandLine(new string[] { "-?" }, out scl), Is.True);

			Assert.That(scl.ShowHelp, Is.True, "Show help is true via args");
			Assert.That(scl.Files, Is.Null, "No files via args");

			Assert.That(Tokenizer.TryParseCommandLine("-?", out scl), Is.True);

			Assert.That(scl.ShowHelp, Is.True, "Show help is true via line");
			Assert.That(scl.Files, Is.Null, "No files via line");

			Assert.That(Tokenizer.TryParseCommandLine("a.b c.d e.f g.h", out scl), Is.True);
			Assert.That(scl.File1, Is.Not.Null, "Has file1");
			Assert.That(scl.Files, Is.Not.Null, "Has files");
			
			Assert.That(scl.Files.Length, Is.EqualTo(3), "Has 3 files");

			Assert.That(Tokenizer.TryParseCommandLine("-a.b c.d e.f", out scl), Is.False);
		}

		IList<string> SplitKv(string word)
		{
			return Tokenizer.GetWords(word, null, '\0', EscapeMode.DoubleItem, new char[] { '=' });
		}

		[Test]
		public void SplitConnectionString()
		{
			IList<string> words = Tokenizer.GetWords(
				"Data Source=myserver;Integrated Security=SSPI;UserId=Bert== Huijben", new string[] { "\"\"", "''" }, '\0', EscapeMode.None, new char[] { ';' });

			Assert.That(words.Count, Is.EqualTo(3), "3 words");
			Assert.That(words[0], Is.EqualTo("Data Source=myserver"));
			Assert.That(words[1], Is.EqualTo("Integrated Security=SSPI"));
			Assert.That(words[2], Is.EqualTo("UserId=Bert== Huijben"));

			IList<string> kv = SplitKv(words[0]);
			Assert.That(kv.Count, Is.EqualTo(2));
			Assert.That(kv[0], Is.EqualTo("Data Source"));
			Assert.That(kv[1], Is.EqualTo("myserver"));

			kv = SplitKv(words[1]);
			Assert.That(kv.Count, Is.EqualTo(2));
			Assert.That(kv[0], Is.EqualTo("Integrated Security"));
			Assert.That(kv[1], Is.EqualTo("SSPI"));

			kv = SplitKv(words[2]);
			Assert.That(kv.Count, Is.EqualTo(2));
			Assert.That(kv[0], Is.EqualTo("UserId"));
			Assert.That(kv[1], Is.EqualTo("Bert= Huijben"));
		}

		[Test]
		public void SplitCommandLine()
		{
			IList<string> cmds = Tokenizer.GetCommandlineWords(@"""c:\program files\microsoft office\word.exe"" /q /u ""f:\test directory/file"" /q""""QQn /r:""www """);
			Assert.That(cmds.Count, Is.EqualTo(6));
		}

		[Test]
		public void ParseConnectionString()
		{
			SimpleConnectionBlock to;
			Assert.That(Tokenizer.TryParseConnectionString("Data Source=myserver;Integrated Security=SSPI;UserId=Bert== Huijben", out to), Is.True, "Can parse string");

			Assert.That(to.DataSource, Is.EqualTo("myserver"));
			Assert.That(to.IntegratedSecurity, Is.EqualTo("SSPI"));
			Assert.That(to.UserId, Is.EqualTo("Bert= Huijben"));
		}

		[Test]
		public void MultipleItem()
		{
			MultiItem scl;

			Assert.That(Tokenizer.TryParseConnectionString("File=.a;File=.b;File=.c", out scl), Is.True, "Can parse string");


			Assert.That(scl.Files.Length, Is.EqualTo(3));
			Assert.That(scl.Files[0], Is.EqualTo(".a"));
			Assert.That(scl.Files[1], Is.EqualTo(".b"));
			Assert.That(scl.Files[2], Is.EqualTo(".c"));
		}

		[Test]
		public void XmlTest()
		{
			MultiItem scl;

			XmlDocument doc = new XmlDocument();

			doc.LoadXml("<q File='banaan' h='true' />");

			Assert.That(Tokenizer.TryParseXml(doc.DocumentElement, out scl), Is.True, "Can parse xml");


			Assert.That(scl.Files.Length, Is.EqualTo(1));
			Assert.That(scl.Files[0], Is.EqualTo("banaan"));
			Assert.That(scl.ShowHelp, Is.True);
		}

		public class Item
		{
			[Token("Id")]
			public string Id;

			[Token("name")]
			public string Name;
		}

		public class Group
		{
			[Token("Id")]
			public string Id;

			[TokenGroup("Item")]
			public List<Item> Items = new List<Item>();
		}

		public class Pack
		{
			[Token("Id")]
			public string Id;

			[TokenGroup("Group")]
			public Group Group;
		}

		[Test]
		public void XmlGroupText()
		{
			string xml = @"
				<Pack Id='packId'>
					<Group Id='groupId'>
						<Item Id='item1Id' name='Item1' />
						<Item Id='item2Id' name='Item2' />
					</Group>
				</Pack>";

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);

			Pack p;

			Assert.That(Tokenizer.TryParseXml(doc.DocumentElement, out p));

			Assert.That(p, Is.Not.Null);
			Assert.That(p.Id, Is.EqualTo("packId"));
			Assert.That(p.Group, Is.Not.Null);
			Assert.That(p.Group.Id, Is.EqualTo("groupId"));
			Assert.That(p.Group.Items, Is.Not.Null);
			Assert.That(p.Group.Items.Count, Is.EqualTo(2));
			Assert.That(p.Group.Items[0].Id, Is.EqualTo("item1Id"));
			Assert.That(p.Group.Items[0].Name, Is.EqualTo("Item1"));
			Assert.That(p.Group.Items[1].Id, Is.EqualTo("item2Id"));
			Assert.That(p.Group.Items[1].Name, Is.EqualTo("Item2"));


			XmlDocument doc2 = new XmlDocument();
			using (XmlWriter xw = doc2.CreateNavigator().AppendChild())
			{
				xw.WriteStartElement("Pack");
				Tokenizer.TryWriteXml(xw, p);
				xw.WriteEndDocument();
			}

			Assert.That(doc2.DocumentElement.OuterXml, Is.EqualTo("<Pack Id=\"packId\"><Group Id=\"groupId\"><Item Id=\"item1Id\" name=\"Item1\" /><Item Id=\"item2Id\" name=\"Item2\" /></Group></Pack>"));
		}

		class TestItemWithNullable
		{
			[Token("date"), Token("dateX", TypeConverter=typeof(TicksDateTimeConverter))]
			public DateTime? date = DateTime.MinValue;
		}

		[Test]
		public void TestNullable()
		{
			TestItemWithNullable item;
			Assert.That(Tokenizer.TryParseCommandLine("-date 2007-08-17", out item), Is.True);
			Assert.That(item.date.HasValue);
			Assert.That(item.date.Value, Is.EqualTo(new DateTime(2007, 08, 17)));

			Assert.That(Tokenizer.TryParseCommandLine("-dateX U20070817", out item), Is.True);
			Assert.That(item.date.HasValue);
			Assert.That(item.date.Value, Is.EqualTo(new DateTime(0x20070817L, DateTimeKind.Utc)));

			Assert.That(Tokenizer.TryParseCommandLine("-dateX L20070817", out item), Is.True);
			Assert.That(item.date.HasValue);
			Assert.That(item.date.Value, Is.EqualTo(new DateTime(0x20070817L, DateTimeKind.Local)));
		}
	}
}