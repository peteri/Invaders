using System.Collections.Generic;

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

        public MainWindow.SplashMajorState SplashMajorState;
        public MainWindow.SplashMinorState SplashMinorState;
        internal int DelayMessagePosition;
        internal string DelayMessage;
        internal int DelayMessageIndex;
        internal bool PlayerOk;
        internal bool WaitOnDraw;
        internal bool FireBounce;
        internal bool AlienExploding;
        internal bool SaucerActive;
        public int RefAlienDeltaX;
        public int RefAlienDeltaY;
        public int RefAlienY;
        public int RefAlienX;
        public int ShotCount;
        public int SaucerScoreIndex;

        public int AlienCurIndex;
        // Location of the next alien to draw
        internal int AlienCharacterCurX;
        internal int AlienCharacterCurY;
        internal int AlienCharacterStart;
        internal int AlienCharacterCurXOffset;
        // Location of an alien to explode
        internal int AlienExplodeX;
        internal int AlienExplodeXOffset;
        internal int AlienExplodeY;

        public bool RackDirectionRightToLeft;
        public List<TimerObject> TimerObjects;
        internal bool AlienShotsEnabled;
        internal int AlienFireDelay;
        public PlayerBase PlayerBase;
        public PlayerShot PlayerShot;
        public AlienRollingShot AlienRollingShot;
        public AlienPlungerShot AlienPlungerShot;
        public AlienSquiglyShot AlienSquiglyShot;
        public MainWindow MainWindow { get; }
        public int SingleAlienOffset { get; internal set; }
        public int VblankStatus { get; internal set; }

        public int SaucerDelta;
        public int SaucerX;

        public PlayerData CurrentPlayer;
        public Aliens Aliens;
        internal int ShotSync;
        internal bool SaucerHit;
        internal int AlienExplodeTimer;

        public GameData(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }

        internal void ResetVariables(PlayerData currentPlayer)
        {
            RefAlienX = 0x18;
            RefAlienY = 0x78;
            RefAlienDeltaY = 0x00;
            RackDirectionRightToLeft = false;
            RefAlienDeltaX = 2;
            AlienCurIndex = -1;
            ShotCount = 0;
            SaucerScoreIndex = 0;
            PlayerOk = true;
            AlienShotsEnabled = false;
            AlienFireDelay = 0x30;
            Aliens = new Aliens(this, currentPlayer);
            // Create timer task objects
            TimerObjects = new List<TimerObject>();

            // Players base
            PlayerBase = new PlayerBase(MainWindow, this);
            TimerObjects.Add(PlayerBase);

            // Players shot
            PlayerShot = new PlayerShot(MainWindow, this);
            TimerObjects.Add(PlayerShot);

            // Alien shots
            AlienRollingShot = new AlienRollingShot(MainWindow, this);
            TimerObjects.Add(AlienRollingShot);
            AlienPlungerShot = new AlienPlungerShot(MainWindow, this);
            TimerObjects.Add(AlienPlungerShot);
            AlienSquiglyShot = new AlienSquiglyShot(MainWindow, this);
            TimerObjects.Add(AlienSquiglyShot);
        }

        public void IncremeentSaucerScoreAndShotCount()
        {
            ShotCount++;
            SaucerScoreIndex++;
            if (!SaucerActive)
            {
                if ((ShotCount & 0x01)==0)
                {
                    SaucerDelta = 2;
                    SaucerX = 29;
                }
                else
                {
                    SaucerDelta = -2;
                    SaucerX = 0xe0;
                }
            }
        }
    }
}
