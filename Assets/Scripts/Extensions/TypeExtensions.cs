using System;
using System.Reflection;
using ModestTree;

namespace Extensions
{
    public static class TypeExtensions
    {
        public static T TryGetAttribute<T>(this MemberInfo provider) where T : Attribute
        {
            return provider.AllAttributes<T>().OnlyOrDefault();
        }
    }
}