using CustomConfigurationProvider.Rules;

namespace Alchemist.Log.Extensions;

internal class SerilogContextArrayPropertyConfigurationRule(string propertyConfigurationKey, string[] values) 
    : CustomArrayRule
{
    public override bool Check(IDictionary<string, string?> data, string sectionName, string? value)
    {
        return value?.Contains(propertyConfigurationKey, StringComparison.InvariantCultureIgnoreCase) == true;
    }

    protected override string[] GetArray(string? value)
    {
        return values;
    }
}