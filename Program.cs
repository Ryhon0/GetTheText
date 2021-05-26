using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CommandLine;

namespace GetTheText
{
	class Program
	{
		public class Options
		{
			[Option('m', "methods", Required = false, HelpText = "List of translation method names, separated with colons.")]
			public string Methods { get; set; }

			[Option('a', "mttributes", Required = false, HelpText = "List of translation attribute names, separated with colons.")]
			public string Attributes { get; set; }
			[Value(0, HelpText = "Input files to be processed.", Required = true)]
			public IEnumerable<string> Files { get; set; }
		}

		static void Main(string[] args)
		{
			string[] files = {};
			var x = Parser.Default.ParseArguments<Options>(args)
			.WithParsed<Options>(o =>
			{
				if (o.Methods != null)
					TranslationMethods = o.Methods.Split(',');
				if (o.Attributes != null)
					TranslationAttributes = o.Attributes.Split(',');

				files = o.Files.ToArray();
			});


			foreach (var f in files)
			{
				if (!File.Exists(f))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Error.WriteLine($"File '{f}' not found");
					Console.Error.WriteLine();
					Console.ResetColor();
					continue;
				}

				var fullPath = Path.GetFullPath(f);

				var currentFile = f;
				var code = new StreamReader(currentFile).ReadToEnd();

				var tree = SyntaxFactory.ParseCompilationUnit(code);

				// Invocations
				foreach (var a in tree.DescendantNodes().OfType<InvocationExpressionSyntax>())
				{
					{
						// Check method name
						var n = a.ChildNodes().First();
						if (n is MemberAccessExpressionSyntax ma)
						{
							var i = n.ChildNodes().OfType<IdentifierNameSyntax>().Last();

							if (!TranslationMethods.Contains(i.ToString()))
								continue;

						}
						else if (n is IdentifierNameSyntax)
						{
							if (!TranslationMethods.Contains(n.ToString()))
								continue;
						}
						else continue;
					}

					var pos = new TextPosition(code, a.SpanStart);

					// No arguments, skip
					if (a.ArgumentList?.Arguments.Any() != true)
					{
						WriteError(fullPath + pos + " => " + a);
						WriteError("Translation method found but no arguments found");
						WriteError();
						continue;
					}

					// Get first argument
					var str = a.ArgumentList.Arguments.First().ChildNodes().First();
					if (str is LiteralExpressionSyntax && (str.IsKind(SyntaxKind.StringLiteralExpression)))
					{
						Console.WriteLine("# " + currentFile + pos);
						Console.WriteLine("msgid " + (str.GetText()[0] == '@' ? str.ToString()[1..] : str.ToString()));
						Console.WriteLine("msgstr \"\"");
						Console.WriteLine();
					}
					else
					{
						WriteError(fullPath + pos + " => " + a);
						WriteError("Translation method found but the first argument is not a stirng literal");
						WriteError();
					}
				}

				// Attributes
				foreach (var a in tree.DescendantNodes().OfType<AttributeSyntax>())
				{
					var name = a.ChildNodes().OfType<IdentifierNameSyntax>().First().ToString();

					if (TranslationAttributes.Contains(name))
					{
						var pos = new TextPosition(code, a.SpanStart);

						// No arguments, skip
						if (a.ArgumentList?.Arguments.Any() != true)
						{
							WriteError(fullPath + pos + " => " + a);
							WriteError("Translation attribute found but no arguments found");
							WriteError();
							continue;
						}

						// Get the first argument
						var str = a.ArgumentList.Arguments.First().ChildNodes().First();
						if (str is LiteralExpressionSyntax && (str.IsKind(SyntaxKind.StringLiteralExpression)))
						{
							Console.WriteLine("# " + currentFile + pos);
							Console.WriteLine("msgid " + (str.GetText()[0] == '@' ? str.ToString()[1..] : str.ToString()));
							Console.WriteLine("msgstr \"\"");
							Console.WriteLine();
						}
						else
						{
							WriteError(fullPath + pos + " => " + a);
							WriteError("Translation attribute found but the first argument is not a stirng literal");
							WriteError();
						}
					}
				}
			}
		}

		static string[] TranslationMethods = {
			"Tr", // Godot
			"_", "_n", "_p", "_pn", "gettext" // NGetText/Other
			};
		static string[] TranslationAttributes =
		{
			"Description"
		};

		static void WriteError() => Console.Error.WriteLine();
		static void WriteError(object o)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine(o);
			Console.ResetColor();
		}
	}

	public class TextPosition
	{
		public TextPosition(string input, int pos)
		{
			var lines = new string(input.Take(pos).ToArray()).Split('\n').ToList();

			Line = lines.Count;
			Position = lines.Last().Length + 1;
		}
		public TextPosition(int l, int p)
		{
			Line = l;
			Position = p;
		}

		public int Line;
		public int Position;
		public override string ToString()
		{
			return $"({Line},{Position})";
		}
	}
}