using System;

//https://www.grasshopper3d.com/forum/topics/expiring-solutions

namespace TestComponent
{
    public class ModifiableEventArgs : EventArgs
    {
        public ModifiableEventArgs(double value)
        {
            Value = value;
        }
        public double Value { get; set; }
    }
}
