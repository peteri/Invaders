using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public abstract class TimerObject
    {
        // Unclear if we really need active?
        // Might want to keep it for the saucer.
        public bool IsActive;
        public int Ticks;
        public int ExtraCount;

        public TimerObject(bool isActive, int ticks)
        {
            IsActive = isActive;
            Ticks = ticks;
            ExtraCount = 0;
        }

        public abstract void Action();
    }
}
