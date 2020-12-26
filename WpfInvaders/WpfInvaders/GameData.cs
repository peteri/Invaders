using System.Collections.Generic;

namespace WpfInvaders
{
    internal class GameData
    {
        internal short HiScore;
        internal byte Credits;
        internal short IsrDelay;
        internal bool CoinSwitch;

        internal bool SuspendPlay;
        internal bool GameMode;
        internal bool DemoMode;
        internal bool WaitStartLoop;
        internal bool AnimateSplash;
        internal byte alienShotReloadRate;

        internal MainWindow.SplashMajorState SplashMajorState;
        internal MainWindow.SplashMinorState SplashMinorState;
        internal int DelayMessagePosition;
        internal string DelayMessage;
        internal int DelayMessageIndex;
        internal bool PlayerOk;
        internal bool WaitOnDraw;
        internal bool FireBounce;
        internal bool AlienExploding;
        internal bool SaucerActive;
        internal bool Invaded;
        internal int RefAlienDeltaX;
        internal int RefAlienDeltaY;
        internal int RefAlienY;
        internal int RefAlienX;
        internal int ShotCount;
        internal int SaucerScoreIndex;

        internal int AlienCurIndex;
        // Location of the next alien to draw
        internal int AlienCharacterCurX;
        internal int AlienCharacterCurY;
        internal int AlienCharacterStart;
        internal int AlienCharacterCurXOffset;
        // Location of an alien to explode
        internal int AlienExplodeX;
        internal int AlienExplodeXOffset;
        internal int AlienExplodeY;

        internal bool RackDirectionRightToLeft;
        internal List<TimerObject> TimerObjects;
        internal bool AlienShotsEnabled;
        internal int AlienFireDelay;
        internal PlayerBase PlayerBase;
        internal PlayerShot PlayerShot;
        internal AlienShotRolling AlienShotRolling;
        internal AlienShotPlunger AlienShotPlunger;
        internal AlienShotSquigly AlienShotSquigly;
        internal MainWindow MainWindow { get; }
        internal bool SingleAlienIsTypeOne;
        internal int VblankStatus;

        internal int SaucerDelta;
        internal int SaucerX;

        internal Aliens Aliens;
        internal int ShotSync;
        internal bool SaucerHit;
        internal int AlienExplodeTimer;
        internal bool PlungerShotActive;

        internal GameData(MainWindow mainWindow)
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
            PlayerShot = new PlayerShot(this);
            TimerObjects.Add(PlayerShot);

            // Alien shots
            AlienShotRolling = new AlienShotRolling(MainWindow, this);
            TimerObjects.Add(AlienShotRolling);
            AlienShotPlunger = new AlienShotPlunger(MainWindow, this);
            TimerObjects.Add(AlienShotPlunger);
            AlienShotSquigly = new AlienShotSquigly(MainWindow, this);
            TimerObjects.Add(AlienShotSquigly);
        }

        internal void IncremeentSaucerScoreAndShotCount()
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
