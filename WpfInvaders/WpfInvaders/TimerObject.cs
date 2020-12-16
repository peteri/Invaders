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
        }

        public bool IsActive { get; set; }
        public int Ticks { get; set; }

        public abstract void Action();
    }
}
