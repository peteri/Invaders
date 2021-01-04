using System.Collections.Generic;

namespace WpfInvaders
{
    internal class GameData
    {
        internal ushort HiScore;
        internal byte Credits;
        internal short IsrDelay;
        internal bool CoinSwitch;

        internal bool SuspendPlay;
        internal bool GameMode;
        internal bool DemoMode;
        internal MainWindow.WaitState WaitState;
        internal bool AnimateSplash;
        internal byte AlienShotReloadRate;

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
        internal SplashAlienAnimation SplashAlienAnimation;
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
        internal bool AdjustScore;
        internal ushort ScoreDelta;
        internal int AlienShotDeltaY;
        internal int NumberOfPlayers;

        internal void SaveReferenceAlienInfo(PlayerData currentPlayer)
        {
            currentPlayer.RefAlienX=RefAlienX;
            currentPlayer.RefAlienY=RefAlienY;
            currentPlayer.RefAlienDeltaX=RefAlienDeltaX;
        }

        internal GameData(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }

        internal void ResetVariables(PlayerData currentPlayer,bool allowQuickMove)
        {
            LineRender.Sprites.Clear();

            RefAlienX = currentPlayer.RefAlienX;
            RefAlienY = currentPlayer.RefAlienY;
            RefAlienDeltaX = currentPlayer.RefAlienDeltaX;
            RefAlienDeltaY = 0x00;
            RackDirectionRightToLeft = RefAlienDeltaX < 0;
            AlienCurIndex = -1;
            PlungerShotActive = true;
            ShotCount = 0;
            SaucerScoreIndex = 0;
            PlayerOk = true;
            AlienShotsEnabled = false;
            AdjustScore = false;
            ScoreDelta = 0;
            AlienFireDelay = 0x30;
            AlienShotDeltaY = -4;
            AlienExplodeTimer = 0;
            ShotSync = 1;
            AlienExploding = false;
            Invaded = false;
            Aliens = new Aliens(MainWindow,this, currentPlayer);
            // Create timer task objects
            TimerObjects = new List<TimerObject>();

            // Players base
            PlayerBase = new PlayerBase(MainWindow, this);
            TimerObjects.Add(PlayerBase);
            if (allowQuickMove)
                PlayerBase.Ticks = 0;
            // Players shot
            PlayerShot = new PlayerShot(MainWindow,this);
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
                if ((ShotCount & 0x01) == 0)
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
