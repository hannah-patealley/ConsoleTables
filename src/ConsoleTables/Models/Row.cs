using System.Collections.Generic;
using System.Linq;

namespace ConsoleTables.Models
{
    public class Row
    {
		public bool IsHeader { get; set; }
		public List<RowValue> Values { get => ValuesDict.Values.ToList(); }
		private Dictionary<Column, RowValue> ValuesDict { get; set; }
		public string Format { get; set; }

		public Row()
		{
			ValuesDict = new Dictionary<Column, RowValue>();
		}

		public Row(IEnumerable<string> values)
			: this()
		{
			foreach (string item in values)
			{
				var tmpCol = new Column(item);
				ValuesDict.Add(tmpCol, new RowValue(tmpCol, item));
			}
		}

		public Row(IEnumerable<RowValue> values)
			: this()
		{
			foreach (var item in values)
			{
				ValuesDict.Add(item.Column, item);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null) return false;

			Row other = obj as Row;
			if (other == null) return false;
			if (this.Values.Count != other.Values.Count) return false;

			bool valuesMatch = true;
			for (int i = 0; i < Values.Count; i++)
			{
				valuesMatch = valuesMatch && this.Values[i].Equals(other.Values[i]);
			}

			return valuesMatch;
		}

		public override int GetHashCode() => Values.Select(v => v.GetHashCode()).Sum();

		public override string ToString() => this.ToString(", ");

		public string[] ToStringArray() => Values.Select(v => v.ToFormattedString()).ToArray();

		public string ToString(string delimiter) => string.Join(delimiter, ToStringArray());

		public string ToString(string delimiter, Dictionary<Column, int> columnLengths, Dictionary<Column, string> columnAlignments)
		{
			var cellStrings = new List<string>();
			cellStrings.Add(string.Empty);

			foreach(var cell in ValuesDict)
			{
				string valString = IsHeader ? cell.Value.ToString() : cell.Value.ToFormattedString();
				int paddingLength = columnLengths[cell.Key] - (Utility.GetTextWidth(valString) - valString.Length);
				string format = $"{{0,{columnAlignments[cell.Key]}{paddingLength}}}";
				cellStrings.Add(string.Format(format, valString));
			}

			cellStrings.Add(string.Empty);

			return string.Join(delimiter, cellStrings);
		}

		public void AddValue(Column column, object value) => ValuesDict.Add(column, new RowValue(column, value));

		public RowValue GetValue(string columnName) => ValuesDict.First(x => x.Key.Name.Equals(columnName)).Value;
		public RowValue GetValue(Column column) => ValuesDict[column];
	}
}
	