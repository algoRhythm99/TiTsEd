using System;

namespace TiTsEd.Common
{
    public class BoolEventArgs : EventArgs
    {
        public bool Value { get; private set; }
        public BoolEventArgs(bool value)
        {
            Value = value;
        }
    }
}
