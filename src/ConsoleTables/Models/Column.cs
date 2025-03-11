using System;
using System.Reflection;

namespace ConsoleTables.Models
{
    public class Column
    {
		public string Name { get; set; }
		public Type Type { get; set; }
		public Func<object,string> StringFormatter { get; set; }

		public Column(string name, Type type)
		{
			Name = name;
			Type = type;
			StringFormatter = x => x.ToString();
		}

		public Column(string name)
			: this(name, typeof(object))
		{ }

		public Column(PropertyInfo property)
			: this(property.Name, property.PropertyType)
		{
			StringFormatter = property.GetFormatter();
		}

		public static Column FromString(string name) => new Column(name);
		public static Column FromProperty(PropertyInfo property) => new Column(property);

		public override bool Equals(object obj)
		{
			if (obj == null) return false;

			Column otherCol = obj as Column;
			if (otherCol == null) return false;

			return this.Name.Equals(otherCol.Name);
		}

		public override int GetHashCode() => Name.GetHashCode();

		public override string ToString() => Name;
	}
}
