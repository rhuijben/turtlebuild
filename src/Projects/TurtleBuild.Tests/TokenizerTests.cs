using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokenizer;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System.IO;

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
	}
}