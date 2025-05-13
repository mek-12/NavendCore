using System.Reflection;
using Navend.Core.Attributes;

namespace Navend.Core.UOW.Helper;

public static class UnitOfWorkHelper {
    public static bool IsUnitOfWorkEnabled(Type type){
        var decorator =  type.GetCustomAttribute<DecoratorAttribute>();
        if(decorator is null) return true;

        return decorator.IsUowEnabled;
    }
}