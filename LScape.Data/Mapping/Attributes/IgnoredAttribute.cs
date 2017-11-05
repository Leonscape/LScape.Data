using System;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Sets the property to be ignored by mapping
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoredAttribute : Attribute
    {}
}
