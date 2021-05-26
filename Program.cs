using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GetTheText
{
	class Program
	{
		static void Main(string[] args)
		{
			foreach (var f in args)
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


		static string[] TranslationMethods = {
			"Tr", // Godot
			"_", "_n", "_p", "_pn", "gettext" // NGetText/Other
			};
	public class TextPosition
	{
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