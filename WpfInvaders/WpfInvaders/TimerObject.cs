using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public abstract class TimerObject
    {

        public TimerObject(bool isActive, int ticks)
        {
            IsActive = isActive;
            Ticks = ticks;
            ExtraCount = 0;
        }

        public bool IsActive;
        public int Ticks;
        public int ExtraCount;

        public abstract void Action();
    }
}
