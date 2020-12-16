using System.Collections.Generic;
using static WpfInvaders.MainWindow;

namespace WpfInvaders
{
    public class GameData
    {
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
        public bool RackDirectionRightToLeft;
        public List<TimerObject> TimerObjects;
        internal bool AlienShotsEnabled;
        internal int AlienFireDelay;
        public PlayerBase PlayerBase;
        public MainWindow MainWindow { get; }

        public GameData(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }

        internal void ResetVariables()
        {
            RefAlienX = 0x18;
            RefAlienY = 0x78;
            RefAlienDeltaY = 0x00;
            RackDirectionRightToLeft = false;
            RefAlienDeltaX = 2;
            AlienCurIndex = -1;
            PlayerOk = true;
            AlienShotsEnabled = false;
            AlienFireDelay = 0x30;
            TimerObjects = new List<TimerObject>();
            PlayerBase = new PlayerBase(MainWindow, this);
            TimerObjects.Add(PlayerBase);
        }
    }
}
