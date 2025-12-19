namespace Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, Inherited = false)] 
    public sealed class AssetKeyAttribute : System.Attribute 
    {
        public string Key { get; }

        public AssetKeyAttribute(string key)
        {
            Key = key;
        }
    }
}