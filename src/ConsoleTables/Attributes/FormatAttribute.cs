using System;

namespace ConsoleTables.Attributes
{
    [AttributeUsage(
        AttributeTargets.Field |
        AttributeTargets.Property )]
    public class FormatAttribute : Attribute
    {
		public string FormatString;

		public FormatAttribute()
			: this(null)
        { }

		public FormatAttribute(string formatString)
		{
			FormatString = formatString;
		}
	}
}
