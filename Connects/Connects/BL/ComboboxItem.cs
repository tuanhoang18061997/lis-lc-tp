using System;

namespace Connects.BL
{
    public class ComboboxItemString
    {
        public string Code { get; set; }
        public string Text { get; set; }
        public long Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
