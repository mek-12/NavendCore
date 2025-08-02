namespace Navend.Core.Attributes;

public class UOWAttribute : Attribute
{
    public bool IsUowEnabled { get; private set; } = true;
    public UOWAttribute(bool? isUowEnable) : base()
    {
        if (isUowEnable.HasValue)
        {
            IsUowEnabled = isUowEnable.Value;
        }
    }

    public UOWAttribute() : base() { }
}