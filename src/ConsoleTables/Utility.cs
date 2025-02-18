using System.Linq;
using Wcwidth;

namespace ConsoleTables
{
    internal static  class Utility
	{
		public static int GetTextWidth(string value)
		{
			if (value == null)
				return 0;

			var length = value.ToCharArray().Sum(c => UnicodeCalculator.GetWidth(c));
			return length;
		}
	}
}
