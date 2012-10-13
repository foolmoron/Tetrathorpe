using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetrathorpe
{
    interface Controllable
    {
        void aButton(int index);
        void bButton(int index);
        void xButton(int index);
        void yButton(int index);
    }
}
