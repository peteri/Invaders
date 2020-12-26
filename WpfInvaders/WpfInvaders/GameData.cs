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
        internal bool Invaded;
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
        public AlienShotRolling AlienShotRolling;
        public AlienShotPlunger AlienShotPlunger;
        public AlienShotSquigly AlienShotSquigly;
        public MainWindow MainWindow { get; }
        public bool SingleAlienIsTypeOne { get; internal set; }
        public int VblankStatus { get; internal set; }

        public int SaucerDelta;
        public int SaucerX;

        public Aliens Aliens;
        internal int ShotSync;
        internal bool SaucerHit;
        internal int AlienExplodeTimer;
        internal bool PlungerShotActive;

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
            PlungerShotActive = true;
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
            AlienShotRolling = new AlienShotRolling(MainWindow, this);
            TimerObjects.Add(AlienShotRolling);
            AlienShotPlunger = new AlienShotPlunger(MainWindow, this);
            TimerObjects.Add(AlienShotPlunger);
            AlienShotSquigly = new AlienShotSquigly(MainWindow, this);
            TimerObjects.Add(AlienShotSquigly);
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
