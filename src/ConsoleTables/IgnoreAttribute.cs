using System;

namespace ConsoleTables
{
    [AttributeUsage(
        AttributeTargets.Field |
        AttributeTargets.Property )]
    public class IgnoreAttribute : System.Attribute
    {
        public IgnoreAttribute()
        { }
    }
}
