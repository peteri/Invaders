using System.Collections.Generic;

namespace WpfInvaders
{
    internal class GameData
    {
        internal ushort HiScore;
        internal short IsrDelay;
        internal ushort ScoreDelta;
        internal byte Credits;
        internal byte AlienShotReloadRate;

        internal bool SaucerActive;
        internal bool SaucerStart;
        internal bool SaucerHit;
        internal bool PlungerShotActive;

        internal bool GameMode;
        internal bool DemoMode;
        internal bool SuspendPlay;
        internal bool AnimateSplash;

        internal bool CoinSwitch;
        internal bool PlayerOk;
        internal bool WaitOnDraw;
        internal bool FireBounce;
        internal bool AlienExploding;
        internal bool Invaded;
        internal bool SingleAlienIsTypeOne;
        internal bool RackDirectionRightToLeft;
        internal bool AlienShotsEnabled;
        internal bool AdjustScore;

        internal int DelayMessagePosition;
        internal string DelayMessage;
        internal int DelayMessageIndex;
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
        internal int VblankStatus;
        internal int SaucerDelta;
        internal int SaucerX;
        internal int AlienFireDelay;

        internal int ShotSync;
        internal int AlienExplodeTimer;
        internal int AlienShotDeltaY;
        internal int NumberOfPlayers;
        internal int TimeToSaucer;

        internal List<TimerObject> TimerObjects;
        internal MainWindow.WaitState WaitState;
        internal MainWindow.SplashMajorState SplashMajorState;
        internal MainWindow.SplashMinorState SplashMinorState;
        internal PlayerBase PlayerBase;
        internal PlayerShot PlayerShot;
        internal AlienShotRolling AlienShotRolling;
        internal AlienShotPlunger AlienShotPlunger;
        internal AlienShotSquigly AlienShotSquigly;
        internal SplashAlienAnimation SplashAlienAnimation;
        internal MainWindow MainWindow { get; }
        internal Aliens Aliens;

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
            SaucerScoreIndex = 0;
            ScoreDelta = 0;
            AlienExplodeTimer = 0;
            ShotCount = 0;

            PlayerOk = true;
            AlienShotsEnabled = false;
            AdjustScore = false;
            AlienExploding = false;
            Invaded = false;

            SaucerStart = false;
            SaucerActive = false;
            SaucerHit = false;
            PlungerShotActive = true;
            TimeToSaucer = 0x600;
            AlienCurIndex = -1;
            AlienFireDelay = 0x30;
            AlienShotDeltaY = -4;
            ShotSync = 1;
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
            if (SaucerScoreIndex >= AlienShotSquigly.SaucerScores.Length)
            {
                SaucerScoreIndex = 0;
            }
            if (!SaucerActive)
            {
                if ((ShotCount & 0x01) == 0)
                {
                    SaucerDelta = 2;
                    SaucerX = 0x08;
                }
                else
                {
                    SaucerDelta = -2;
                    SaucerX = 0xc0;
                }
            }
        }
    }
}
