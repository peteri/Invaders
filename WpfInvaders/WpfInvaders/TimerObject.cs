namespace WpfInvaders
{
    internal abstract class TimerObject
    {
        // Unclear if we really need active?
        // Might want to keep it for the saucer.
        internal bool IsActive;
        internal int Ticks;
        internal int ExtraCount;

        internal TimerObject(bool isActive, int ticks)
        {
            IsActive = isActive;
            Ticks = ticks;
            ExtraCount = 0;
        }

        internal abstract void Action();
    }
}
