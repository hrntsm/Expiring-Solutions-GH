using System;

namespace TestComponent
{
    public interface IModifiable
    {
        event EventHandler<ModifiableEventArgs> ModifyValue;
    }
}
