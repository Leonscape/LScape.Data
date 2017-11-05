using System;

namespace LScape.Data.Mapping
{
    /// <summary>
    /// Used to denote a property is a key field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
    }
}
