#if UNITY_EDITOR
using System;

namespace Services.Cheats
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class CheatGroupAttribute : Attribute
    {
        public string GroupName { get; }

        public CheatGroupAttribute(string groupName)
        {
            GroupName = groupName ?? "General";
        }
    }
}
#endif
