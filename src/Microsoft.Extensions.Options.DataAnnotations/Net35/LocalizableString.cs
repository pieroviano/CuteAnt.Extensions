#if NET35
namespace System.ComponentModel.DataAnnotations
{
    internal class LocalizableString
    {
        public LocalizableString(string shortNameName)
        {
            Value=shortNameName;
        }

        public string Value { get; set; }
        public Type ResourceType { get; set; }

        public string GetLocalizableValue()
        {
            return Value;
        }
    }
}
#endif