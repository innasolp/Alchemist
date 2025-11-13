using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Alchemist.WebApp.Api.Common;

public class EnumRouteConstraint<TEnum> : IRouteConstraint where TEnum : struct, Enum
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var routeValue))
        {
            var stringValue = routeValue?.ToString();
            if (!string.IsNullOrEmpty(stringValue))
            {
                return int.TryParse(stringValue, out var value) ||
                    Enum.GetNames(typeof(TEnum)).Any(name => name.Equals(stringValue, StringComparison.OrdinalIgnoreCase));
            }
        }
        return false;
    }
}
