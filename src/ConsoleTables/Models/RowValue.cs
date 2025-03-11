

namespace ConsoleTables.Models
{
    public class RowValue
    {
		public Column Column { get; set; }
		private object _value;
		public object Value
		{
			get
			{
				if (_value == null) return string.Empty;
				else return _value;
			}
			set
			{
				_value = value;
			}
		}

		public RowValue(Column column)
		{
			Column = column;
		}

		public RowValue(Column column, object value)
			: this(column)
		{
			Value = value;
		}

		public override bool Equals(object obj)
		{
			if (obj == null) return false;

			RowValue other = obj as RowValue;
			if (other == null) return false;

			return this.Column.Equals(other) &&
				this.Value.Equals(other.Value);
		}

		public override int GetHashCode() => Column.GetHashCode() + Value.GetHashCode();

		public override string ToString() => Value.ToString();

		public string ToFormattedString() => Column.StringFormatter(Value);
	}
}
