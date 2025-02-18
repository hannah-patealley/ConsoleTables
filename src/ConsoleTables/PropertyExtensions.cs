using System;
using System.Linq;
using System.Reflection;
using ConsoleTables.Attributes;

namespace ConsoleTables
{
    internal static class PropertyExtensions
	{
		internal static bool IsIgnored(this PropertyInfo property)
		{
			object[] ignoreAttributes = property.GetCustomAttributes(typeof(IgnoreAttribute), true);
			return ignoreAttributes.Any();
		}

		internal static Func<object,string> GetFormatter(this PropertyInfo property)
		{
			FormatAttribute formatAttribute = property.GetCustomAttributes(typeof(FormatAttribute), true).FirstOrDefault() as FormatAttribute;

			if(formatAttribute != null)
			{
				return obj => string.Format(formatAttribute.FormatString, obj);
			}

			return (obj) => obj.ToString();
		}
	}
}
