using CustomConfigurationProvider;

namespace Alchemist.Log.Extensions;

internal class SerilogContextPropertyConfigurationRule(string propertyConfigurationKey, string propertyConfigurationValue) 
    : ICustomConfigurationRule
{
    public bool Check(string sectionName, string value)
    {
        return value.Contains(propertyConfigurationKey, StringComparison.InvariantCultureIgnoreCase);
    }

    public string TransformValue(string value)
    {
        return value.Replace(propertyConfigurationKey, propertyConfigurationValue);
    }
}