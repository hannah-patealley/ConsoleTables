using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Wcwidth;
using ConsoleTables.Models;
using System.Data.Common;

namespace ConsoleTables
{
    public class ConsoleTable
    {
		public string CellDivider { get; private set; }
        public IList<Column> Columns { get; }
        public IList<string> ColumnNames { get => Columns.Select(x => x.Name).ToList(); }
		public IList<Row> Rows { get; }

		public Dictionary<Column, int> ColumnLengths { get; }
		public Dictionary<Column, string> ColumnAlignment { get; }

		public ConsoleTableOptions Options { get; }

        public static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),  typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),
            typeof(uint), typeof(float)
        };

		internal ConsoleTable()
			: this(new ConsoleTableOptions())
		{
		}

		public ConsoleTable(params string[] columns)
            : this(new ConsoleTableOptions { ColumnNames = new List<string>(columns) })
        {
		}

		public ConsoleTable(IEnumerable<Column> columns)
			: this(new ConsoleTableOptions())
		{
			Columns = columns.ToList();
		}

		public ConsoleTable(ConsoleTableOptions options)
        {
            Options = options ?? throw new ArgumentNullException("options");
            Rows = new List<Row>();
            Columns = options.ColumnNames.Select(Column.FromString).ToList();
			ColumnLengths = new Dictionary<Column, int>();
			ColumnAlignment = new Dictionary<Column, string>();
			CellDivider = options.CellDivider ?? string.Empty;
		}

		public ConsoleTable AddColumn(IEnumerable<string> columns)
		{
			foreach (var col in columns)
				Columns.Add(Column.FromString(col));
			return this;
		}

		public ConsoleTable AddColumn(IEnumerable<Tuple<string,Type>> columnInfo)
		{
			foreach (var col in columnInfo)
				Columns.Add(new Column(col.Item1, col.Item2));
			return this;
		}

		public ConsoleTable AddColumn(IEnumerable<Column> columns)
		{
			foreach (var col in columns)
				Columns.Add(col);
			return this;
		}

        public ConsoleTable AddRow(params object[] values)
        {
			return AddRow(values.ToList());
		}

		public ConsoleTable AddRow(List<object> values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			if (!Columns.Any())
				throw new Exception("Please set the columns first");

			if (Columns.Count != values.Count())
				throw new Exception(
					$"The number columns in the row ({Columns.Count}) does not match the values ({values.Count()})");

			var row = new Row();
			for (int i = 0; i < Columns.Count; i++)
			{
				row.AddValue(Columns[i], values[i]);
			}
			Rows.Add(row);
			return this;
		}

		public ConsoleTable Configure(Action<ConsoleTableOptions> action)
        {
            action(Options);
            return this;
		}

		public ConsoleTable AddFormattingToColumn(IEnumerable<string> columnNames, Func<object, string> formatter)
		{
			return AddFormattingToColumn(list => list.Where(x => columnNames.Contains(x.Name)), formatter);
		}

		public ConsoleTable AddFormattingToColumn(string columnName, Func<object, string> formatter)
		{
			return AddFormattingToColumn(list => list.Where(x => x.Name.Equals(columnName)), formatter);
		}

		public ConsoleTable AddFormattingToColumn(Func<IEnumerable<Column>, Column> columnSelector, Func<object, string> formatter)
		{
			return AddFormattingToColumn(list => new Column[] { columnSelector.Invoke(list) }, formatter);
		}

		public ConsoleTable AddFormattingToColumn(Func<IEnumerable<Column>, IEnumerable<Column>> columnSelector, Func<object,string> formatter)
		{
			IEnumerable<Column> selectedColumns = columnSelector.Invoke(this.Columns);
			foreach (var column in selectedColumns)
			{
				column.StringFormatter = formatter;
			}
			return this;
		}

		public static ConsoleTable FromDictionary(Dictionary<string, Dictionary<string, object>> values)
		{
			var table = new ConsoleTable();

			var columNames = values.SelectMany(x => x.Value.Keys).Distinct().ToList();
			columNames.Insert(0, "");
			table.AddColumn(columNames);

			foreach (var row in values)
			{
				var rowValues = new List<object> { row.Key };
				foreach (var columName in columNames.Skip(1))
				{
					rowValues.Add(row.Value.TryGetValue(columName, out var value) ? value : "");
				}

				table.AddRow(rowValues.ToArray());
			}

			return table;
		}

        public static ConsoleTable From<T>(IEnumerable<T> values)
        {
			var columns = new List<Column>();
			PropertyInfo[] properties = typeof(T).GetProperties();

			foreach (PropertyInfo prop in properties)
			{
				if (prop.IsIgnored()) continue;
				columns.Add(Column.FromProperty(prop));
			}

			var table = new ConsoleTable(columns);

			foreach(var item in values)
			{
				var properyNames = table.Columns.Select(c => c.Name);
				object[] rowValues = properyNames.Select(p => typeof(T).GetProperty(p).GetValue(item)).ToArray();
				table.AddRow(rowValues);
			}

            return table;
        }

        public static ConsoleTable From(DataTable dataTable)
        {
            var table = new ConsoleTable();

            var columns = dataTable.Columns
                .Cast<DataColumn>()
                .Select(x => x.ColumnName)
                .ToList();

            table.AddColumn(columns);

            foreach (DataRow row in dataTable.Rows)
            {
                var items = row.ItemArray.Select(x => x is byte[] data ? Convert.ToBase64String(data) : x.ToString())
                    .ToArray();
                table.AddRow(items);
            }

            return table;
        }


		public string ToString(Func<Row, string> rowFormatFunc, Func<int, string> rowDividerFunc, bool divideAllRows = true)
		{
			SetColumnLengths();
			SetNumberAlignment();

			Row headerRow = new Row(Columns.Select(x => new RowValue(x, x.Name)))
			{
				IsHeader = true
			};
			string headerRowString = rowFormatFunc.Invoke(headerRow);

			var rowDivider = rowDividerFunc.Invoke(headerRowString.Length);

			var builder = new StringBuilder();
			if (Options.IncludeHeaderRow)
			{
				if(divideAllRows) builder.AppendLine(rowDivider);
				builder.AppendLine(headerRowString);
				builder.AppendLine(rowDivider);
			}

			foreach (var row in Rows)
			{
				builder.AppendLine(rowFormatFunc.Invoke(row));
				if(divideAllRows) builder.AppendLine(rowDivider);
			}

			if (Options.EnableCount)
			{
				builder.AppendLine("");
				builder.AppendFormat(" Count: {0}", Rows.Count);
			}

			return builder.ToString();
		}

		public override string ToString()
		{
			Func<int, string> rowDivFunc = rowLength =>
			{
				return " " + string.Join("", Enumerable.Repeat("-", rowLength-2)) + " ";
			};

			return this.ToString(FormatRow, rowDivFunc);
		}

		internal string ToMarkDownString()
		{
			CellDivider = "|";

			Func<int, string> rowDivFunc = rowLength =>
			{
				return string.Join(CellDivider, Enumerable.Repeat("---", Columns.Count()));
			};

			return this.ToString(FormatRowWithoutPadding, rowDivFunc, divideAllRows: false);
		}

        public string ToMinimalString()
        {
			CellDivider = "  ";

			Func<int, string> rowDivFunc = rowLength =>
			{
				return string.Join(string.Empty, Enumerable.Repeat("-", rowLength));
			};

			return this.ToString(FormatRow, rowDivFunc, divideAllRows: false);
		}

		public string ToStringAlternative()
		{
			CellDivider = " + ";

			Func<int, string> rowDivFunc = rowLength =>
			{
				string div = CellDivider.Trim();
				List<string> stringParts = new List<string>();
				stringParts.Add(string.Empty);
				stringParts.AddRange(ColumnLengths.Select(colLen => string.Join(string.Empty, Enumerable.Repeat("-", colLen.Value+2))));
				stringParts.Add(string.Empty);
				return " " + string.Join(div, stringParts) + " ";
			};

			return this.ToString(FormatRow, rowDivFunc);
        }

		private string FormatRow(Row row) => row.ToString(CellDivider, ColumnLengths, ColumnAlignment);

		private string FormatRowWithoutPadding(Row row) => row.ToString(CellDivider);


		private void SetNumberAlignment()
		{
			ColumnAlignment.Clear();
			foreach (Column column in Columns)
			{
				if(Options.NumberAlignment == Alignment.Right
					&& NumericTypes.Contains(column.Type))
				{
					ColumnAlignment.Add(column, "");
				}
				else
				{
					ColumnAlignment.Add(column, "-");
				}
			}
		}

		private void SetColumnLengths()
		{
			ColumnLengths.Clear();
			foreach (var column in Columns)
			{
				List<string> values = Rows.Select(row => row.GetValue(column).ToFormattedString()).ToList();
				values.Add(column.Name);

				int maxLength = values
					.Where(v => v != null)
					.Select(v => v.Length)
					.Max();

				ColumnLengths.Add(column, maxLength);
			}
        }


        public void Write(Format format = ConsoleTables.Format.Default)
        {
			SetColumnLengths();
			SetNumberAlignment();

            switch (format)
            {
                case ConsoleTables.Format.Default:
                    Options.OutputTo.WriteLine(ToString());
                    break;
                case ConsoleTables.Format.MarkDown:
                    Options.OutputTo.WriteLine(ToMarkDownString());
                    break;
                case ConsoleTables.Format.Alternative:
                    Options.OutputTo.WriteLine(ToStringAlternative());
                    break;
                case ConsoleTables.Format.Minimal:
                    Options.OutputTo.WriteLine(ToMinimalString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

	}

    public class ConsoleTableOptions
    {
        public IEnumerable<string> ColumnNames { get; set; } = new List<string>();

        public bool EnableCount { get; set; } = true;

        /// <summary>
        /// Enable only from a list of objects
        /// </summary>
        public Alignment NumberAlignment { get; set; } = Alignment.Left;

        /// <summary>
        /// The <see cref="TextWriter"/> to write to. Defaults to <see cref="Console.Out"/>.
        /// </summary>
        public TextWriter OutputTo { get; set; } = Console.Out;

        public bool IncludeHeaderRow { get; set; } = true;

        public string CellDivider { get; set; } = " | ";

	}

    public enum Format
    {
        Default = 0,
        MarkDown = 1,
        Alternative = 2,
        Minimal = 3
    }

    public enum Alignment
    {
        Left,
        Right
    }
}
