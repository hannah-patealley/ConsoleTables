using System;

namespace ConsoleTables.Attributes
{
    [AttributeUsage(
        AttributeTargets.Field |
        AttributeTargets.Property )]
    public class IgnoreAttribute : Attribute
    {
        public IgnoreAttribute()
        { }
    }
}
