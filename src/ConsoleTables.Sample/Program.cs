using ConsoleTables.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleTables.Sample;

static class Program
{
    static void TestDictionaryTable()
    {
        Dictionary<string, Dictionary<string, object>> data = new Dictionary<string, Dictionary<string, object>>()
        {
            {"A", new Dictionary<string, object>()
            {
                { "A", true },
                { "B", false },
                { "C", true },
            }},
            {"B", new Dictionary<string, object>()
            {
                { "A", false },
                { "B", true },
                { "C", false },
            }},
            {"C", new Dictionary<string, object>()
            {
                { "A", false },
                { "B", false },
                { "C", true },
            }}
        };
        var table = ConsoleTable.FromDictionary(data);

        Console.WriteLine(table.ToString());
    }
        
    static void Main(string[] args)
	{
		Console.WriteLine("Test Dictionary Table:\n");
		TestDictionaryTable();

        var table = new ConsoleTable("one", "two", "three");
        table.AddRow(1, 2, 3)
            .AddRow("this line should be longer 哈哈哈哈", "yes it is", "oh");

		WriteBreak();
        Console.WriteLine("\nFORMAT: Default:\n");
        table.Write();

		WriteBreak();
		Console.WriteLine("\nFORMAT: MarkDown:\n");
        table.Write(Format.MarkDown);

		WriteBreak();
		Console.WriteLine("\nFORMAT: Alternative:\n");
        table.Write(Format.Alternative);
        Console.WriteLine();

		WriteBreak();
		Console.WriteLine("\nFORMAT: Minimal:\n");
        table.Write(Format.Minimal);
        Console.WriteLine();

		WriteBreak();
		table = new ConsoleTable("I've", "got", "nothing");
        table.Write();
        Console.WriteLine();

        var rows = Enumerable.Repeat(new Something(), 10);

		WriteBreak();
		ConsoleTable.From(rows).Write();

		WriteBreak();
		rows = Enumerable.Repeat(new Something(), 0);
        ConsoleTable.From(rows).Write();

		WriteBreak();
		Console.WriteLine("\nNumberAlignment = Alignment.Right\n");
        rows = Enumerable.Repeat(new Something(), 2);
        ConsoleTable
            .From(rows)
            .Configure(o => o.NumberAlignment = Alignment.Right)
            .Write();

        var noCount =
            new ConsoleTable(new ConsoleTableOptions
            {
                ColumnNames = new[] { "one", "two", "three" },
                EnableCount = false
            });

		WriteBreak();
		noCount.AddRow(1, 2, 3).Write();

		WriteBreak();
		Console.WriteLine("Header Row Excluded From Output:");
        table = new ConsoleTable("Header1", "Header2", "Header3");
        table.AddRow("valA1", "valA2", "valA3");
        table.AddRow("valB1", "valB2", "valB3");
        table.AddRow("valC1", "valC2", "valC3");
        table
            .Configure(o => o.IncludeHeaderRow = false)
            .Write(Format.Minimal);

		WriteBreak();
		var rowsWithFormatting = Enumerable.Repeat(new SomethingWithFormatting(), 3);
		Console.WriteLine("Formatting Columns:");
		ConsoleTable
		   .From(rowsWithFormatting)
		   .AddFormattingToColumn(nameof(SomethingWithFormatting.FunctionFormatted), SomethingWithFormatting.FormatString)
		   .Write();

		Console.ReadKey();
    }

	private static void WriteBreak()
	{
		Console.WriteLine("\n\n" + string.Join(string.Empty, Enumerable.Repeat("=", Console.WindowWidth)) + "\n");
	}
}

public class Something
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Khalid Abuhkameh";
    public DateTime Date { get; set; } = DateTime.Now;
    public int NumberOfChildren { get; set; }
    [Ignore] public string IgnoreMe { get; set; } = "I should not be displayed";
}

public class SomethingWithFormatting
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");
	public DateTime Date { get; set; } = DateTime.Now;
	public int NumberOfChildren { get; set; }
	[Format("Formatted by Attribute: '{0}'")] public string AttributeFormatted { get => Id.ToString(); }
	public string FunctionFormatted { get => Id.ToString(); }

	public static string FormatString(object obj)
	{
		if (obj == null) return "NULL";

		string str = obj as string;
		if (str == null) return "NOT STRING";

		return $"Formatted by Function: '{str}'";
	}
}