using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace Alchemist.WebApp.Api.Common;

public class ApiControllerFeatureProvider(Type[] excludeControllerTypes) : ControllerFeatureProvider
{
    private readonly Type[] _excludeControllerTypes = excludeControllerTypes;
    

    protected override bool IsController(TypeInfo typeInfo)
    {
        return _excludeControllerTypes?.Length > 0
            ? !_excludeControllerTypes.Contains(typeInfo.AsType())
            : base.IsController(typeInfo);
    }
}
