using System.Reflection;
using Navend.Core.Attributes;

namespace Navend.Core.UOW.Helper;

public static class UnitOfWorkHelper {
    public static bool IsUnitOfWorkEnabled(Type type){
        var decorator =  type.GetCustomAttribute<UOWAttribute>();
        if(decorator is null) return false;

        return decorator.IsUowEnabled;
    }
}