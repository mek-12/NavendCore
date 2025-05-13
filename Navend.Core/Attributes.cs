namespace Navend.Core.Attributes;

public class DecoratorAttribute : Attribute {
    public bool IsUowEnabled { get; private set; } = true;
    public DecoratorAttribute(bool? isUowEnable): base() {
        if(isUowEnable.HasValue){
            IsUowEnabled = isUowEnable.Value;
        }
    }

    public DecoratorAttribute():base() {}
 }