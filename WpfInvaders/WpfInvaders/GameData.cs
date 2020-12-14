using System;
using System.Collections.Generic;
using System.Text;
using static WpfInvaders.MainWindow;

namespace WpfInvaders
{
    public class GameData
    {
        // 1=right, 2=left
        private static byte[] demoCmds = { 1, 1, 0, 0, 1, 0, 2, 1, 0, 2, 1, 0 };
        public short HiScore;
        public byte Credits;
        public short IsrDelay;
        public bool CoinSwitch;

        public bool SuspendPlay;
        public bool GameMode;
        public bool DemoMode;
        public bool WaitStartLoop;
        public bool AnimateSplash;
        public byte alienShotReloadRate;

        public SplashMajorState SplashMajorState;
        public SplashMinorState SplashMinorState;
        internal int DelayMessagePosition;
        internal string DelayMessage;
        internal int DelayMessageIndex;
        internal int PlayerShot;
        internal bool PlayerAlive;
        internal bool PlayerOk;
        internal bool WaitOnDraw;
        internal bool AlienExploding;
        public int RefAlienDeltaX;
        public int RefAlienDeltaY;
        public int RefAlienY;
        public int RefAlienX;

        public int AlienCurIndex;
        internal int AlienCharacterCurX;
        internal int AlienCharacterCurY;
        internal int AlienCharacterStart;
        internal int AlienCharacterOffset;
        public bool BumpRight;
        public bool BumpLeft;

        internal void ResetVariables()
        {
            RefAlienX = 0x18;
            RefAlienY = 0x78;
            RefAlienDeltaY = 0x00;
            RefAlienDeltaX = 0x02;
            PlayerOk = true;
        }
    }
}
