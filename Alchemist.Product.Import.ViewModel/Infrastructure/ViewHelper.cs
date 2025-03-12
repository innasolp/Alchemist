using Alchemist.Product.Interfaces;
using System.ComponentModel;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ViewHelper
{
    public static TabType[] Tabs { get; }

    public static Dictionary<TabType, string> TabNames { get; }

    public static Dictionary<TabType, string> TabViewNames { get; }

    public static ShopSettingType[] ShopSettingTypes { get; }

    public static Dictionary<ShopSettingType, string> ShopSettingTypeNames { get; }

    static ViewHelper()
    {
        Tabs = Enum.GetValues<TabType>();
        var tabType = typeof(TabType);
        TabNames = Tabs.ToDictionary(t => t, t => GetEnumMemberDescription<TabType, DescriptionAttribute>(tabType, (d) => d.Description, t));
        TabViewNames = Tabs.ToDictionary(t => t, t => GetEnumMemberDescription<TabType, CategoryAttribute>(tabType, a => a.Category, t));

        ShopSettingTypes = Enum.GetValues<ShopSettingType>();
        var shopSettingType = typeof(ShopSettingType);
        ShopSettingTypeNames = ShopSettingTypes.ToDictionary(t => t, t => GetEnumMemberDescription<ShopSettingType, DescriptionAttribute>(shopSettingType, (d) => d.Description, t));
    }

    private static string GetEnumMemberDescription<TEnum, TAttribute>(Type enumType, Func<TAttribute, string> getAttrValue, TEnum enumMember)
    {
        var memberInfos = enumType.GetMember(enumMember.ToString());
        var enumValueMemberInfo = memberInfos.FirstOrDefault(m =>
        m.DeclaringType == enumType);
        var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(TAttribute), false);
        return getAttrValue((TAttribute)valueAttributes[0]);
    }
}
