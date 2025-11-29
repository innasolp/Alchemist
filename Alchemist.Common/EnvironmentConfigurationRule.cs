using CustomConfigurationProvider;

namespace Alchemist.Common;

public class EnvironmentConfigurationRule : ICustomConfigurationRule
{
    public bool Check(string sectionName, string value)
    {
        return value?.ContainsLocalHost() == true;
    }

    public string TransformValue(string value)
    {
        return value.SetEnvironmentLocalHostIfNeed();
    }
}
