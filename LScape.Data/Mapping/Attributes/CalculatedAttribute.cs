using System;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Sets the property to a calculated mapping
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CalculatedAttribute : Attribute
    {}
}
