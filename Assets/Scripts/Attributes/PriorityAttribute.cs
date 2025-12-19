namespace Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class PriorityAttribute : System.Attribute
    {
        public int Priority { get; }

        public PriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}