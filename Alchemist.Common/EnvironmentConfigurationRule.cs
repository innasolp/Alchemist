using CustomConfigurationProvider.Rules;

namespace Alchemist.Common;

public class EnvironmentConfigurationRule : CustomOrdinaryRule
{
    public override bool Check(IDictionary<string, string?> data, string sectionName, string? value)
    {
        return value?.ContainsLocalHost() == true;
    }

    protected override string? GetValue(string? value)
    {
        return value?.SetEnvironmentLocalHostIfNeed();
    }
}