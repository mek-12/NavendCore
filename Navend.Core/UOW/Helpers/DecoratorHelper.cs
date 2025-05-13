using System.Reflection;
using Navend.Core.Attributes;

namespace Navend.Core.Decorator.Helper;

internal static class DecoratorHelper {
    internal static bool IsDecorator (Type type) {
        var decoratorAttribute = type.GetCustomAttribute<DecoratorAttribute>();
        return decoratorAttribute is not null;
    }
}