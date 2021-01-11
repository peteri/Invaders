using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfInvaders
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern uint TimeBeginPeriod(uint uMilliseconds);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        private static extern uint TimeEndPeriod(uint uMilliseconds);

        [Flags]
        internal enum SwitchState
        {
            None = 0x00,
            Left = 0x01,
            Right = 0x02,
            Fire = 0x04,
            Coin = 0x08,
            PlayOnePlayer = 0x10,
            PlayTwoPlayer = 0x20,
        }

        internal enum WaitState
        {
            Idle,
            WaitForPlayerButton,
            PlayerDeathPause,
            WaitForPlayMessage,
            GameOverPlayerX,
            GameOver,
            GameOverPlayerXMsg,
            GameOverMsg,
            GameOverDelay
        }

        internal enum SplashMinorState
        {
            Idle,
            Wait,
            PrintMessageCharacter,
            PrintMessageDelay,
            AnimateSplashAlien,
            AwaitCoinShotEndOfExplosion,
            PlayDemoWaitDeath,
            PlayDemoWaitEndofExplosion
        }

        internal enum SplashMajorState
        {
            OneSecondDelay,
            PrintPlay,
            PrintSpaceInvaders,
            PrintAdvanceTable,
            PrintMystery,
            Print30Points,
            Print20Points,
            Print10Points,
            ScoreTableTwoSecondDelay,
            AnimateAlienInToGetY,
            AnimateAlienPullingYOff,
            AnimateAlienHidingOffStageDelay,
            AnimateAlienPushingYBackOn,
            AnimateAlienJobDoneDelay,
            AnimateAlienRemoval,
            PlayDemo,
            AfterPlayDelay,
            InsertCoin,
            OneOrTwoPlayers,
            OnePlayOneCoin,
            TwoPlayerTwoCoins,
            AnimateCoinExplodeAlienInDelay,
            AnimateCoinExplodeAlienIn,
            AnimateCoinExplodeFireBullet,
            AnimateCoinExplodeRemoveExtraC,
            AfterCoinDelay,
            ToggleAnimateState
        }

        // Game related stuff
        private readonly PlayerData playerOne;
        private readonly PlayerData playerTwo;
        internal PlayerData CurrentPlayer;
        private readonly GameData gameData;
        internal SwitchState switchState;

        // Stuff related to WPF and diagnostics
        private readonly WriteableBitmap frame;
        private readonly Stopwatch frameStopwatch;
        private readonly Stopwatch timeInIsrStopwatch;
        private int timerCount = 0;
        private Thread timerThread;
        private readonly ManualResetEvent dieEvent = new ManualResetEvent(false);
        private volatile bool invokeTick;
        private volatile bool InIsr;
        private bool replayMode;
        private int frameCounter = -6;
        private int replayIndex = 0;
        // Holding down Right Ctrl gives type B aliens
        // Holding down Left  Ctrl gives type C aliens
        private int DiagnosticsAlienType = 0x80;
        private bool shiftKeyDown;
        private SwitchState lastSwitchState;
        private List<(int framecount, SwitchState switchState)> switches;
        bool tweakFlag;
        private const int SplashDelayOneSecond= 0x40;
        private const int SplashDelayTwoSecond = 0x80;

        public MainWindow()
        {
            InitializeComponent();
            frame = new WriteableBitmap(256, 224, 96, 96, PixelFormats.BlackWhite, null);
            //imgScreen.Source = frame;
            imgScreenRotateMirrored.Source = frame;
            playerOne = new PlayerData();
            playerTwo = new PlayerData();
            gameData = new GameData(this);
            frameStopwatch = Stopwatch.StartNew();
            timeInIsrStopwatch = Stopwatch.StartNew();
            PowerOnReset();
            frameStopwatch.Start();
            switches = new List<(int framecount, SwitchState switchState)>();
        }

        private void PowerOnReset()
        {
            DrawStatus();
            Pause.Content = "Pause";
            gameData.SplashMajorState = SplashMajorState.ToggleAnimateState;
            gameData.SplashMinorState = SplashMinorState.Idle;
            gameData.WaitState = WaitState.Idle;
            gameData.GameMode = false;
            gameData.DemoMode = false;
            timerThread = new Thread(WaitingTimer);
            StartIsr();
            timerThread.Start();
        }

        private void WaitingTimer()
        {
            // Set the multimedia timer to 1ms
            TimeBeginPeriod(1);
            // run at 60Hz by waiting for 16ms 17ms 17ms 
            // which gives 3 interrupts per 50ms.
            while (!dieEvent.WaitOne(16))
            {
                if (invokeTick)
                    this.Dispatcher.InvokeAsync(IsrRoutine);
                if (dieEvent.WaitOne(17))
                    break;
                if (invokeTick)
                    this.Dispatcher.InvokeAsync(IsrRoutine);
                if (dieEvent.WaitOne(17))
                    break;
                if (invokeTick)
                    this.Dispatcher.InvokeAsync(IsrRoutine);
            }
            TimeEndPeriod(1);
        }

        private void IsrRoutine()
        {
            InIsr = true;
            frameCounter++;
            timerCount++;
            if (replayMode)
            {
                var curSwitch = switches[replayIndex];
                if (frameCounter == curSwitch.framecount)
                {
                    switchState = curSwitch.switchState;
                    replayIndex++;
                    if (replayIndex >= switches.Count)
                    {
                        replayMode = false;
                        StopIsr();
                    }
                }
            }
            else
            {
                if (switchState != lastSwitchState)
                {
                    switches.Add((frameCounter, switchState));
                    lastSwitchState = switchState;
                }
            }
            if (timerCount > 59)
            {
                timerCount = 0;
                var timeTaken = frameStopwatch.ElapsedMilliseconds;
                var timeInIsr = timeInIsrStopwatch.ElapsedMilliseconds;
                frameStopwatch.Restart();
                timeInIsrStopwatch.Restart();
                FrameStats.Content = string.Format("60 frames took {0}ms timeInIsr is {1}ms", timeTaken, timeInIsr);
            }
            FrameCounter.Content = string.Format("Framcount {0}", frameCounter);
            timeInIsrStopwatch.Start();
            RenderScreen();
            GameTick();
            timeInIsrStopwatch.Stop();
            InIsr = false;
        }

        private void GameTick()
        {
            gameData.IsrDelay--;
            HandleCoinSwitch();
            if (!gameData.SuspendPlay)
            {
                if (gameData.GameMode || gameData.DemoMode)
                {
                    GameLoopStep();
                    // Pretend we're on the first half of the screen
                    gameData.VblankStatus = 0;
                    // Move every except the player
                    if (!tweakFlag)
                        RunGameObjects(true);
                    tweakFlag = false;
                    // Normally the game loop runs for ever
                    // But do it here instead as we only have one isr
                    // required to get the players first shot off
                    // correctly in demo mode.
                    GameLoopStep();
                    gameData.Aliens.CursorNextAlien();

                    // Mame draws the screen at this point
                    // when single stepping...
                    RenderScreen();

                    // Now we do the bottom half of screen isr case
                    gameData.VblankStatus = 0x80;
                    // Copy the shot sync like the normal routines do
                    gameData.ShotSync = gameData.AlienShotRolling.ExtraCount;
                    // Draw the alien
                    gameData.Aliens.DrawAlien();
                    // Move everyone including the player
                    RunGameObjects(false);
                    // Adjust time to saucer
                    TimeToSaucer();
                    // Rerun the code that normally would run in a tight loop
                    GameLoopStep();
                }
                else
                {
                    if (gameData.Credits != 0)
                    {
                        if (gameData.WaitState == WaitState.Idle)
                        {
                            EnterWaitStartLoop();
                        }
                    }
                    else
                    {
                        IsrTasksSplashScreen();
                    }
                }
            }
            else
            {
                RunWaitTask();
            }
        }

        private void GameLoopStep()
        {
            PlayerFireOrDemo();
            PlayerShotHit();
            CurrentPlayer.CountAliens();
            if (gameData.DemoMode)
            {
                IsrTasksSplashScreen();
            }
            else
            {
                AdjustScore();
                if (CurrentPlayer.NumAliens == 0)
                {
                    CurrentPlayer.InitAliens();
                    CurrentPlayer.ResetShields();
                    CurrentPlayer.RefAlienX = 0x18;
                    CurrentPlayer.RackCount = (CurrentPlayer.RackCount & 0x07) + 1;
                    CurrentPlayer.RefAlienY = Aliens.AlienStartRow[CurrentPlayer.RackCount];
                    CurrentPlayer.RefAlienDeltaX = 2;
                    gameData.ResetVariables(CurrentPlayer, false);
                    ClearPlayField();
                    DrawBottomLine();
                    PlayerData.DrawShields();
                }
                SetShotReloadRate();
                CheckExtraShip();
                if (CurrentPlayer.NumAliens < 9)
                    gameData.AlienShotDeltaY = -5;
            }
        }

        private void SetShotReloadRate()
        {
            if (CurrentPlayer.Score < 0x0299)
                gameData.AlienShotReloadRate = 0x30;
            else if (CurrentPlayer.Score < 0x1999)
                gameData.AlienShotReloadRate = 0x10;
            else if (CurrentPlayer.Score < 0x2999)
                gameData.AlienShotReloadRate = 0x0b;
            else if (CurrentPlayer.Score < 0x3999)
                gameData.AlienShotReloadRate = 0x08;
            else
                gameData.AlienShotReloadRate = 0x07;
        }

        private void CheckExtraShip()
        {
            if (!CurrentPlayer.ExtraShipAvailable)
                return;
            if (CurrentPlayer.Score < 0x1500)
                return;
            CurrentPlayer.ShipsRem++;
            CurrentPlayer.ExtraShipAvailable = false;
            WriteText(0x01, 0x01, (CurrentPlayer.ShipsRem & 0x0f).ToString());
            DrawShips();
        }

        private void AdjustScore()
        {
            if (gameData.AdjustScore)
            {
                gameData.AdjustScore = false;
                CurrentPlayer.Score = BcdAdd(CurrentPlayer.Score, gameData.ScoreDelta);
                if (CurrentPlayer == playerOne)
                    PlayerOneScore();
                else
                    PlayerTwoScore();
            }
        }

        internal void RenderScreen()
        {
            // Draw the screen.
            for (int line = 0; line < 224; line++)
            {
                var renderResult = LineRender.RenderLine(line);
                Int32Rect rect = new Int32Rect(0, 0, 256, 1);
                frame.WritePixels(rect, renderResult, 32, 0, line);
            }
        }

        private void RunWaitTask()
        {
            switch (gameData.WaitState)
            {
                case WaitState.WaitForPlayerButton:
                    CheckStartButtons();
                    break;
                case WaitState.PlayerDeathPause:
                    if (gameData.IsrDelay == 0)
                        PlayPlayer(CurrentPlayer == playerOne ? playerTwo : playerOne, true);
                    break;
                case WaitState.WaitForPlayMessage:
                    if (gameData.IsrDelay == 0)
                        StartGame();
                    else
                        PromptPlayer();
                    break;
                case WaitState.GameOverPlayerX:
                    gameData.DelayMessage = (CurrentPlayer == playerOne) ? "GAME OVER PLAYER <1>" : "GAME OVER PLAYER <2>";
                    gameData.DelayMessageIndex = 0;
                    gameData.IsrDelay = 1;
                    gameData.WaitState = WaitState.GameOverPlayerXMsg;
                    gameData.DelayMessagePosition = 0x0403;
                    break;
                case WaitState.GameOverPlayerXMsg:
                    if (gameData.IsrDelay == 0)
                    {
                        if (gameData.DelayMessageIndex >= gameData.DelayMessage.Length)
                        {
                            var otherPlayer = (CurrentPlayer == playerOne) ? playerTwo : playerOne;
                            gameData.WaitState = (otherPlayer.ShipsRem != 0) ? WaitState.PlayerDeathPause : WaitState.GameOver;
                            gameData.IsrDelay = 0x80;
                        }
                        else
                        {
                            byte c = CharacterRom.Map[gameData.DelayMessage[gameData.DelayMessageIndex++]];
                            int y = gameData.DelayMessagePosition & 0xff;
                            int x = gameData.DelayMessagePosition >> 8;
                            gameData.DelayMessagePosition += 0x100;
                            LineRender.Screen[y + x * LineRender.ScreenWidth] = c;
                            gameData.IsrDelay = 5;
                        }
                    }
                    break;
                case WaitState.GameOver:
                    gameData.DelayMessage = "GAME OVER";
                    gameData.DelayMessageIndex = 0;
                    gameData.IsrDelay = 1;
                    gameData.WaitState = WaitState.GameOverMsg;
                    gameData.DelayMessagePosition = 0x0918;
                    gameData.GameMode = false;
                    gameData.DemoMode = false;
                    break;
                case WaitState.GameOverMsg:
                    if (gameData.IsrDelay == 0)
                    {
                        if (gameData.DelayMessageIndex >= gameData.DelayMessage.Length)
                        {
                            gameData.WaitState = WaitState.GameOverDelay;
                            gameData.IsrDelay = 0x80;
                        }
                        else
                        {
                            byte c = CharacterRom.Map[gameData.DelayMessage[gameData.DelayMessageIndex++]];
                            int y = gameData.DelayMessagePosition & 0xff;
                            int x = gameData.DelayMessagePosition >> 8;
                            gameData.DelayMessagePosition += 0x100;
                            LineRender.Screen[y + x * LineRender.ScreenWidth] = c;
                            gameData.IsrDelay = 6;
                        }
                    }
                    break;
                case WaitState.GameOverDelay:
                    if (gameData.IsrDelay == 0)
                    {
                        // Time to reset the world!
                        gameData.WaitState = WaitState.Idle;
                        gameData.SuspendPlay = false;
                        ClearPlayField();
                    }
                    break;
            }
        }

        private void CheckStartButtons()
        {
            var message = (gameData.Credits > 1) ? "1 OR 2PLAYERS BUTTON" : "ONLY 1PLAYER  BUTTON";
            WriteText(0x10, 0x04, message);
            // TODO: Poll player one and two buttons....
            if ((switchState & SwitchState.PlayOnePlayer) != 0)
                StartGame(1);
            if ((switchState & SwitchState.PlayTwoPlayer) != 0)
                StartGame(2);
        }

        internal void KillPlayer()
        {
            if (CurrentPlayer.Score > gameData.HiScore)
            {
                gameData.HiScore = CurrentPlayer.Score;
                HighScore();
            }
            DisplayShipCount();
        }

        private void StartGame(int numberOfPlayers)
        {
            // Reset back the splash screen stuff
            gameData.SplashMajorState = SplashMajorState.ToggleAnimateState;
            gameData.SplashMinorState = SplashMinorState.Idle;
            gameData.NumberOfPlayers = numberOfPlayers;
            gameData.Credits = BcdSub(gameData.Credits, (byte)numberOfPlayers);
            playerOne.Reset();
            playerTwo.Reset();
            PlayerOneScore();
            PlayerTwoScore();
            DrawNumCredits();
            PlayPlayer(playerOne, false);
        }

        private void PlayPlayer(PlayerData player, bool allowQuickMove)
        {
            CurrentPlayer = player;
            gameData.ResetVariables(CurrentPlayer, allowQuickMove);
            gameData.GameMode = true;
            gameData.IsrDelay = 0x80;
            gameData.WaitState = WaitState.WaitForPlayMessage;
            RemoveShip();
            ClearPlayField();
            WriteText(0x11, 0x07, CurrentPlayer == playerOne ? "PLAY PLAYER<1>" : "PLAY PLAYER<2>");
        }

        internal void PlayerShipBlownUp()
        {
            gameData.SuspendPlay = true;
            CurrentPlayer.CopyBitmapCharToShield();
            gameData.SaveReferenceAlienInfo(CurrentPlayer);

            if (CurrentPlayer.ShipsRem == 0)
            {
                KillPlayer();
                gameData.WaitState = (gameData.NumberOfPlayers == 2) ? WaitState.GameOverPlayerX : WaitState.GameOver;
                return;
            }
            var otherPlayer = (CurrentPlayer == playerOne) ? playerTwo : playerOne;
            bool onePlayerLeft = (gameData.NumberOfPlayers == 1) || (otherPlayer.ShipsRem == 0);
            if (onePlayerLeft)
            {
                RemoveShip();
                gameData.SuspendPlay = false;
                gameData.WaitState = WaitState.Idle;
            }
            else
            {
                gameData.IsrDelay = 0x80;
                gameData.WaitState = WaitState.PlayerDeathPause;
            }
        }


        private void PromptPlayer()
        {
            if ((gameData.IsrDelay & 0x07) == 3)
            {
                if (CurrentPlayer == playerOne)
                    PlayerOneScore();
                else
                    PlayerTwoScore();
            }
            else if ((gameData.IsrDelay & 0x07) == 7)
            {
                int x = (CurrentPlayer == playerOne) ? 0x03 : 0x15;
                WriteText(0x1c, x, "    ");
            }
        }

        private void StartGame()
        {
            ClearPlayField();
            CurrentPlayer.CopyShieldToBitmapChar();
            PlayerData.DrawShields();
            DrawBottomLine();
            gameData.SuspendPlay = false;
            gameData.WaitState = WaitState.Idle;
        }

        private void IsrTasksSplashScreen()
        {
            switch (gameData.SplashMinorState)
            {
                case SplashMinorState.Idle:
                    gameData.SplashMinorState = AdvanceToNextMajorState();
                    break;
                case SplashMinorState.Wait:
                    if (gameData.IsrDelay == 0)
                        gameData.SplashMinorState = AdvanceToNextMajorState();
                    break;
                case SplashMinorState.PrintMessageCharacter:
                    if (gameData.DelayMessageIndex >= gameData.DelayMessage.Length)
                    {
                        gameData.SplashMinorState = SplashMinorState.Idle;
                    }
                    else
                    {
                        byte c = CharacterRom.Map[gameData.DelayMessage[gameData.DelayMessageIndex++]];
                        int y = gameData.DelayMessagePosition & 0xff;
                        int x = gameData.DelayMessagePosition >> 8;
                        gameData.DelayMessagePosition += 0x100;
                        LineRender.Screen[y + x * LineRender.ScreenWidth] = c;
                        gameData.IsrDelay = 5;
                        gameData.SplashMinorState = SplashMinorState.PrintMessageDelay;
                    }
                    break;
                case SplashMinorState.PrintMessageDelay:
                    if (gameData.IsrDelay == 0)
                        gameData.SplashMinorState = SplashMinorState.PrintMessageCharacter;
                    break;
                case SplashMinorState.PlayDemoWaitDeath:
                    if ((gameData.PlayerBase.Alive != PlayerBase.PlayerAlive.Alive) || (gameData.Credits != 0))
                    {
                        gameData.PlayerShot.Status = PlayerShot.ShotStatus.Available;
                        gameData.SplashMinorState = SplashMinorState.PlayDemoWaitEndofExplosion;
                    }
                    break;
                case SplashMinorState.PlayDemoWaitEndofExplosion:
                    if ((gameData.PlayerBase.Alive == PlayerBase.PlayerAlive.Alive) || (gameData.Credits != 0))
                    {
                        gameData.PlayerShot.Status = PlayerShot.ShotStatus.Available;
                        gameData.DemoMode = false;
                        gameData.SplashMinorState = SplashMinorState.Idle;
                        // Hide all the sprites.
                        foreach (var sprite in LineRender.Sprites)
                            sprite.Visible = false;
                        tweakFlag = true;
                    }
                    break;
                case SplashMinorState.AnimateSplashAlien:
                    gameData.SplashMinorState = gameData.SplashAlienAnimation.Animate();
                    break;
                case SplashMinorState.AwaitCoinShotEndOfExplosion:
                    gameData.VblankStatus = 0x80;
                    gameData.AlienShotSquigly.Action();
                    if (gameData.AlienShotSquigly.ShotActive == false)
                        gameData.SplashMinorState = SplashMinorState.Idle;
                    break;
            }
        }

        private SplashMinorState AdvanceToNextMajorState()
        {
            if (gameData.SplashMajorState == SplashMajorState.ToggleAnimateState)
                gameData.SplashMajorState = SplashMajorState.OneSecondDelay;
            else
                gameData.SplashMajorState++;
            switch (gameData.SplashMajorState)
            {
                case SplashMajorState.OneSecondDelay:
                    gameData.AlienShotReloadRate = 8;
                    ClearPlayField();
                    return SplashDelay(SplashDelayOneSecond);
                case SplashMajorState.PrintPlay:
                    return PrintDelayedMessage(0x0c17, gameData.AnimateSplash ? "PLA@" : "PLAY");
                case SplashMajorState.PrintSpaceInvaders:
                    return PrintDelayedMessage(0x0714, "SPACE  INVADERS");
                case SplashMajorState.PrintAdvanceTable:
                    WriteText(0x10, 4, "*SCORE ADVANCE TABLE*");
                    WriteUnmappedText(0x0e, 7, "\x24\x25\x26"); // Saucer 
                    WriteUnmappedText(0x0c, 8, "\xa0\xa1"); // Invader type C - sprite 0
                    WriteUnmappedText(0x0a, 8, "\xbc\xbd"); // Invader type B - sprite 1
                    WriteUnmappedText(0x08, 8, "\x80\x81"); // Invader type A - sprite 0
                    return SplashDelay(SplashDelayOneSecond);
                case SplashMajorState.PrintMystery:
                    return PrintDelayedMessage(0x0a0e, "=? MYSTERY");
                case SplashMajorState.Print30Points:
                    return PrintDelayedMessage(0x0a0c, "=30 POINTS");
                case SplashMajorState.Print20Points:
                    return PrintDelayedMessage(0x0a0a, "=20 POINTS");
                case SplashMajorState.Print10Points:
                    return PrintDelayedMessage(0x0a08, "=10 POINTS");
                case SplashMajorState.ScoreTableTwoSecondDelay:
                    return SplashDelay(SplashDelayTwoSecond);
                case SplashMajorState.AnimateAlienInToGetY:
                    if (gameData.AnimateSplash == false)
                    {
                        gameData.SplashMajorState = SplashMajorState.PlayDemo - 1;
                        return SplashMinorState.Idle;
                    }
                    gameData.SplashAlienAnimation = new SplashAlienAnimation();
                    LineRender.Sprites.Add(gameData.SplashAlienAnimation.AlienMovingY);
                    return AnimateSplashAlien(223, 123, 0);
                case SplashMajorState.AnimateAlienPullingYOff:
                    WriteText(0x17, 0x0c, "PLA ");
                    return AnimateSplashAlien(120, 221, 2);
                case SplashMajorState.AnimateAlienHidingOffStageDelay:
                    return SplashDelay(SplashDelayOneSecond);
                case SplashMajorState.AnimateAlienPushingYBackOn:
                    return AnimateSplashAlien(221, 120, 4);
                case SplashMajorState.AnimateAlienJobDoneDelay:
                    return SplashDelay(SplashDelayOneSecond);
                case SplashMajorState.AnimateAlienRemoval:
                    gameData.SplashAlienAnimation.AlienMovingY.Visible = false;
                    WriteText(0x17, 0x0c, "PLAY");
                    return SplashDelay(0x80);
                case SplashMajorState.PlayDemo:
                    Debug.Print("Starting demo game");
                    ClearPlayField();
                    CurrentPlayer = playerOne;
                    playerOne.Reset();
                    RemoveShip();
                    gameData.ResetVariables(CurrentPlayer, false);
                    PlayerData.DrawShields();
                    DrawBottomLine();
                    gameData.DemoMode = true;
                    return SplashMinorState.PlayDemoWaitDeath;
                case SplashMajorState.AfterPlayDelay:
                    return SplashDelay(SplashDelayOneSecond);
                case SplashMajorState.InsertCoin:
                    ClearPlayField();
                    WriteText(0x11, 0x08, gameData.AnimateSplash ? "INSERT CCOIN" : "INSERT  COIN");
                    return SplashMinorState.Idle;
                case SplashMajorState.OneOrTwoPlayers:
                    return PrintDelayedMessage(0x060d, "<1 OR 2 PLAYERS>");
                case SplashMajorState.OnePlayOneCoin:
                    return PrintDelayedMessage(0x060a, "*1 PLAYER  1 COIN");
                case SplashMajorState.TwoPlayerTwoCoins:
                    return PrintDelayedMessage(0x0607, "*2 PLAYERS 2 COINS");
                case SplashMajorState.AnimateCoinExplodeAlienInDelay:
                    if (gameData.AnimateSplash == false)
                    {
                        gameData.SplashMajorState = SplashMajorState.AfterCoinDelay - 1;
                        return SplashMinorState.Idle;
                    }
                    return SplashDelay(SplashDelayTwoSecond);
                case SplashMajorState.AnimateCoinExplodeAlienIn:
                    gameData.SplashAlienAnimation = new SplashAlienAnimation();
                    LineRender.Sprites.Add(gameData.SplashAlienAnimation.AlienMovingY);
                    return AnimateSplashAlien(0, 115, 0, 0x1a * 8);
                case SplashMajorState.AnimateCoinExplodeFireBullet:
                    gameData.AlienShotSquigly.Shot.X = 115 + 8;
                    gameData.AlienShotSquigly.Shot.Y = 0x1a * 8 - 0x0b;
                    gameData.AlienShotDeltaY = -1;
                    gameData.ShotSync = 2;
                    return SplashMinorState.AwaitCoinShotEndOfExplosion;
                case SplashMajorState.AnimateCoinExplodeRemoveExtraC:
                    WriteText(0x11, 0x08, "INSERT  COIN");
                    return SplashMinorState.Idle;
                case SplashMajorState.AfterCoinDelay:
                    return SplashDelay(SplashDelayTwoSecond);
                case SplashMajorState.ToggleAnimateState:
                    if (gameData.SplashAlienAnimation != null)
                        gameData.SplashAlienAnimation.AlienMovingY.Visible = false;
                    gameData.AnimateSplash = !gameData.AnimateSplash;
                    return SplashMinorState.Idle;
                default:
                    return SplashMinorState.Idle;
            }
        }

        private SplashMinorState SplashDelay(short delay)
        {
            gameData.IsrDelay = delay;
            return SplashMinorState.Wait;
        }

        private SplashMinorState AnimateSplashAlien(int startX, int targetX, int image, int y = 0x17 * 8)
        {
            gameData.SplashAlienAnimation.Init(y, startX, targetX, image);
            return SplashMinorState.AnimateSplashAlien;
        }

        private SplashMinorState PrintDelayedMessage(int screenPosition, string message)
        {
            gameData.DelayMessagePosition = screenPosition;
            gameData.DelayMessage = message;
            gameData.DelayMessageIndex = 0;
            return SplashMinorState.PrintMessageCharacter;
        }

        private void EnterWaitStartLoop()
        {
            gameData.WaitState = WaitState.WaitForPlayerButton;
            gameData.SuspendPlay = true;
            ClearPlayField();
            WriteText(0x13, 0x0c, "PRESS");
        }

        private static void ClearPlayField()
        {
            int i = 2;
            while (i < (LineRender.ScreenHeight * LineRender.ScreenWidth))
            {
                LineRender.Screen[i] = 0x23;
                i++;
                if ((i & 0x1f) >= 0x1c) i += 6;
            }
        }

        private void PlayerShotHit()
        {
            if (gameData.PlayerShot.Status != PlayerShot.ShotStatus.NormalMove)
                return;
            if (gameData.PlayerShot.ShotSprite.Y >= 0xd8)
            {
                gameData.PlayerShot.Status = PlayerShot.ShotStatus.HitSomething;
                gameData.AlienExploding = false;
            }
            if (!gameData.AlienExploding)
                return;
            if (gameData.PlayerShot.ShotSprite.Y >= 0xce)   // Hit saucer
            {
                gameData.SaucerHit = true;
                gameData.PlayerShot.Status = PlayerShot.ShotStatus.AlienExploded;
                gameData.AlienExploding = false;
                return;
            }
            bool playerHitAlien = false;
            if (gameData.PlayerShot.ShotSprite.Y >= gameData.RefAlienY)
            {
                // Adjust so hitting alien as the rack bumps gives us the correct row
                // Aliens are either on gameData.RefAlienY or gameData.RefAlienY+8
                int rowY = gameData.RefAlienY + 8;
                int alienIndex = 0;
                while (rowY < gameData.PlayerShot.ShotSprite.Y)
                {
                    rowY += 0x10;
                    alienIndex += 11;
                }

                int col = FindColumn(gameData.PlayerShot.ShotSprite.X);

                if ((col >= 0) && (col <= 10))
                {
                    alienIndex += col;
                    if (CurrentPlayer.Aliens[alienIndex] != 0)
                    {
                        Debug.Print("frameCount {0} deleting alien index={1} row={2} col={3} ShotX={4} ShotY={5} RefAlienX={6} RefAlienY={7}",
                            frameCounter,
                            alienIndex, alienIndex / 11, alienIndex % 11,
                            gameData.PlayerShot.ShotSprite.X, gameData.PlayerShot.ShotSprite.Y,
                            gameData.RefAlienX, gameData.RefAlienY);
                        int colX = gameData.RefAlienX + col * 0x10;
                        // If we haven't draw this alien yet then the adjust the ColX back
                        // to the correct position. Y is correct as we use the sprite
                        // position rounded.
                        if (alienIndex > gameData.AlienCurIndex && CurrentPlayer.NumAliens != 1)
                        {
                            colX -= gameData.RefAlienDeltaX;
                        }
                        CurrentPlayer.Aliens[alienIndex] = 0;
                        gameData.AlienExplodeTimer = 0x10;
                        gameData.AlienExplodeX = colX >> 3;
                        gameData.AlienExplodeXOffset = colX & 0x07;
                        gameData.AlienExplodeY = gameData.PlayerShot.ShotSprite.Y >> 3;
                        gameData.Aliens.ExplodeAlien();
                        gameData.PlayerShot.ShotSprite.Visible = false;
                        gameData.PlayerShot.Status = PlayerShot.ShotStatus.AlienExploding;
                        playerHitAlien = true;
                        switch (alienIndex / 11)
                        {
                            case 0:
                            case 1: gameData.ScoreDelta = 0x10; break;
                            case 2:
                            case 3: gameData.ScoreDelta = 0x20; break;
                            case 4: gameData.ScoreDelta = 0x30; break;
                        }
                        gameData.AdjustScore = true;
                    }
                }
            }

            if (!playerHitAlien)
            {
                // bullet can't be in the aliens
                gameData.PlayerShot.Status = PlayerShot.ShotStatus.HitSomething;
                gameData.AlienExploding = false;
                return;
            }
        }

        internal int FindColumn(int X)
        {
            if (X < gameData.RefAlienX)
                return -1;
            int colX = gameData.RefAlienX + 0x10;
            int column = 0;
            while (colX <= X)
            {
                colX += 0x10;
                column++;
            }
            return column;
        }

        private void PlayerFireOrDemo()
        {
            // He's dead jim...
            if (gameData.PlayerBase.Alive != PlayerBase.PlayerAlive.Alive)
                return;
            // Base isn't on screen yet
            if (gameData.PlayerBase.Ticks != 0)
                return;
            if (gameData.PlayerShot.Status != PlayerShot.ShotStatus.Available)
                return;
            if (gameData.GameMode)
            {
                if (gameData.FireBounce)
                {
                    if ((switchState & SwitchState.Fire) == 0)
                        gameData.FireBounce = false;
                }
                else
                {
                    if ((switchState & SwitchState.Fire) == SwitchState.Fire)
                    {
                        gameData.FireBounce = true;
                        gameData.PlayerShot.Status = PlayerShot.ShotStatus.Initiated;
                    }
                }
            }
            else
            {
                gameData.PlayerShot.Status = PlayerShot.ShotStatus.Initiated;
                PlayerBase.IncrementDemoCommand();
            }
        }

        private static void DrawBottomLine()
        {
            int i = 2;
            for (int j = 0; j < LineRender.ScreenHeight; j++)
            {
                LineRender.Screen[i] = 0xf3;
                i += LineRender.ScreenWidth;
            }
        }

        internal void RemoveShip()
        {
            if (CurrentPlayer.ShipsRem == 0)
                return;
            DisplayShipCount();
            CurrentPlayer.ShipsRem--;
            DrawShips();
        }

        internal void DisplayShipCount()
        {
            WriteText(0x01, 0x01, (CurrentPlayer.ShipsRem & 0x0f).ToString());
        }

        private void DrawShips()
        {
            int x = 0x03;
            int y = 0x01;
            int numShips = CurrentPlayer.ShipsRem;
            while (numShips != 0)
            {
                WriteUnmappedText(y, x, "\x56\x57");
                x += 2;
                numShips--;
            }
            while (x != 0x11)
            {
                WriteText(y, x, "  ");
                x += 2;
            }
        }

        private void TimeToSaucer()
        {
            if (gameData.RefAlienX >= 0x78) return;
            if (gameData.TimeToSaucer == 0)
            {
                gameData.TimeToSaucer = 600;
                gameData.SaucerStart = true;
            }
            gameData.TimeToSaucer--;
        }

        private void RunGameObjects(bool SkipPlayer)
        {
            foreach (var timerObject in gameData.TimerObjects)
            {
                if (SkipPlayer && timerObject == gameData.PlayerBase)
                    continue;
                if (timerObject.IsActive)
                {
                    if (timerObject.Ticks == 0)
                    {
                        if (timerObject.ExtraCount != 0)
                        {
                            timerObject.ExtraCount--;
                        }
                        else
                        {
                            timerObject.Action();
                        }
                    }
                    else
                    {
                        timerObject.Ticks--;
                    }
                }
            }
        }

        private void HandleCoinSwitch()
        {
            if ((switchState & SwitchState.Coin) == 0 && gameData.CoinSwitch)
            {
                if (gameData.Credits != 0x99)
                {
                    gameData.Credits = BcdAdd(gameData.Credits, 0x01);
                    DrawNumCredits();
                }
            };
            gameData.CoinSwitch = (switchState & SwitchState.Coin) != 0;
        }

        private static byte BcdAdd(byte a, byte b)
        {
            int halfCarry = 0;
            int lowNybble = (a & 0x0f) + (b & 0x0f);
            if (lowNybble > 9)
            {
                lowNybble = (lowNybble + 6) & 0x0f;
                halfCarry = 0x10;
            }
            int highNybble = (a & 0xf0) + (b & 0xf0) + halfCarry;
            if (highNybble > 99)
            {
                highNybble = (highNybble + 6) & 0xf0;
            }
            return (byte)((highNybble & 0xf0) + (lowNybble & 0x0f));
        }

        private static ushort BcdAdd(ushort a, ushort b)
        {
            int halfCarry = 0;
            int nybble000f = (a & 0x000f) + (b & 0x000f);
            if (nybble000f > 9)
            {
                nybble000f = (nybble000f + 0x0006) & 0x000f;
                halfCarry = 0x10;
            }

            int nybllle00f0 = (a & 0x00f0) + (b & 0x00f0) + halfCarry;
            if (nybllle00f0 > 0x90)
            {
                nybllle00f0 = (nybllle00f0 + 0x0060) & 0x00f0;
                halfCarry = 0x100;
            }

            int nybble0f00 = (a & 0x0f00) + (b & 0x0f00) + halfCarry;
            if (nybble0f00 > 0x900)
            {
                nybble0f00 = (nybble0f00 + 0x0600) & 0x0f00;
                halfCarry = 0x1000;
            }

            int nyblllef000 = (a & 0xf000) + (b & 0xf000) + halfCarry;
            if (nyblllef000 > 0x9000)
            {
                nyblllef000 = (nyblllef000 + 0x6000) & 0xf000;
            }
            return (ushort)((nyblllef000 & 0xf000) + (nybble0f00 & 0x0f00) + (nybllle00f0 & 0x00f0) + (nybble000f & 0x000f));
        }

        private static byte BcdSub(byte a, byte b)
        {
            int halfCarry = 0;
            int lowNybble = (a & 0x0f) - (b & 0x0f);
            if (lowNybble < 0)
            {
                lowNybble = (lowNybble + 0xa) & 0x0f;
                halfCarry = 0x10;
            }
            int highNybble = (a & 0xf0) - ((b & 0xf0) + halfCarry);
            if (highNybble > 99)
            {
                highNybble = (highNybble + 0xa) & 0xf0;
            }
            return (byte)((highNybble & 0xf0) + (lowNybble & 0x0f));
        }


        private void DrawStatus()
        {
            ClearScreen();
            DrawScreenHead();
            PlayerOneScore();
            PlayerTwoScore();
            HighScore();
            CreditLabel();
            DrawNumCredits();
        }

        internal static void WriteUnmappedText(int y, int x, string text)
        {
            foreach (char c in text)
                LineRender.Screen[y + (x++) * LineRender.ScreenWidth] = (byte)c;
        }

        internal static void WriteText(int y, int x, string text)
        {
            foreach (char c in text)
                LineRender.Screen[y + (x++) * LineRender.ScreenWidth] = CharacterRom.Map[c];
        }

        private void DrawNumCredits()
        {
            WriteHexByte(0x01, 0x18, gameData.Credits);
        }

        private static void CreditLabel()
        {
            WriteText(0x01, 0x11, "CREDIT ");
        }

        private void HighScore()
        {
            WriteHexWord(0x1c, 0x0b, gameData.HiScore);
        }

        private void PlayerTwoScore()
        {
            WriteHexWord(0x1c, 0x15, playerTwo.Score);
        }

        private void PlayerOneScore()
        {
            WriteHexWord(0x1c, 0x03, playerOne.Score);
        }

        private static void DrawScreenHead()
        {
            WriteText(0x1e, 0x00, " SCORE<1> HI-SCORE SCORE<2> ");
        }

        private static void WriteHexWord(int y, int x, ushort w)
        {
            WriteText(y, x, w.ToString("X4"));
        }

        private static void WriteHexByte(int y, int x, byte b)
        {
            WriteText(y, x, b.ToString("X2"));
        }

        internal static void ClearScreen()
        {
            for (int i = 0; i < (LineRender.ScreenHeight * LineRender.ScreenWidth); i++)
                LineRender.Screen[i] = 0x23;
        }

        internal void StopIsr()
        {
            invokeTick = false;
            Pause.Content = "Run";
        }

        private void StartIsr()
        {
            invokeTick = true;
            Pause.Content = "Pause";
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (invokeTick)
            {
                StopIsr();
            }
            else
            {
                StartIsr();
            }
        }

        private void FrameAdvance_Click(object sender, RoutedEventArgs e)
        {
            IsrRoutine();
        }

        internal void SaveScreenShot(string fname)
        {
            using FileStream stream = new FileStream(fname, FileMode.Create);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(frame));
            encoder.Save(stream);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SwitchState clearMask = SwitchState.None;
            switch (e.Key)
            {
                case Key.A: clearMask = SwitchState.Left; break;
                case Key.D: clearMask = SwitchState.Right; break;
                case Key.Space: clearMask = SwitchState.Fire; break;
                case Key.D1: clearMask = SwitchState.PlayOnePlayer; break;
                case Key.D2: clearMask = SwitchState.PlayTwoPlayer; break;
                case Key.D3: clearMask = SwitchState.Coin; break;
                case Key.RightCtrl:
                case Key.LeftCtrl: DiagnosticsAlienType = 0x80; break;
                case Key.LeftShift:
                case Key.RightShift: shiftKeyDown = false; break;
                case Key.P:
                    if (shiftKeyDown)
                        FrameAdvance_Click(null, null);
                    else
                        Pause_Click(null, null);
                    break;
                case Key.F1: StopIsr(); new HelpWindow().ShowDialog(); break;
                case Key.F3: StopIsr(); DiagnosticPages.ShowInitialInvaders(this, gameData, DiagnosticsAlienType); break;
                case Key.F4: StopIsr(); new CharacterMapWindow().ShowDialog(); break;
                case Key.F5: StopIsr(); DiagnosticPages.ShowShiftedInvaders(this, DiagnosticsAlienType); break;
                case Key.F6: StopIsr(); DiagnosticPages.ShowExplodedInvaders(this, gameData, 0, DiagnosticsAlienType, shiftKeyDown); break;
                case Key.F7: StopIsr(); DiagnosticPages.ShowExplodedInvaders(this, gameData, 1, DiagnosticsAlienType, shiftKeyDown); break;
                case Key.F8: StopIsr(); DiagnosticPages.ShowExplodedInvaders(this, gameData, 2, DiagnosticsAlienType, shiftKeyDown); break;
                case Key.F12: SaveScreenShot("c:\\temp\\invader.png"); break;
            }
            switchState &= ~clearMask;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            SwitchState setMask = SwitchState.None;
            switch (e.Key)
            {
                case Key.A: setMask = SwitchState.Left; break;
                case Key.D: setMask = SwitchState.Right; break;
                case Key.Space: setMask = SwitchState.Fire; break;
                case Key.D1: setMask = SwitchState.PlayOnePlayer; break;
                case Key.D2: setMask = SwitchState.PlayTwoPlayer; break;
                case Key.D3: setMask = SwitchState.Coin; break;
                // Used by diagnostics
                case Key.RightCtrl: DiagnosticsAlienType = 0x90; break;
                case Key.LeftCtrl: DiagnosticsAlienType = 0xa0; break;
                case Key.LeftShift:
                case Key.RightShift: shiftKeyDown = true; break;
            }
            switchState |= setMask;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dieEvent.Set();
        }

        private void AdvanceToFrame_Click(object sender, RoutedEventArgs e)
        {
            StopIsr();
            if (InIsr == true)
                return;
            int targetFrame = int.Parse(targetCounter.Text);
            while (frameCounter < targetFrame)
            {
                IsrRoutine();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            StopIsr();
            using BinaryWriter writer = new BinaryWriter(File.Open(saveFilename.Text, FileMode.Create));
            writer.Write(switches.Count);
            foreach (var state in switches)
            {
                writer.Write(state.framecount);
                writer.Write((int)state.switchState);
            }
        }

        private async void Replay_Click(object sender, RoutedEventArgs e)
        {
            StopIsr();
            dieEvent.Set();
            await Task.Delay(500);
            dieEvent.Reset();
            using BinaryReader reader = new BinaryReader(File.Open(saveFilename.Text, FileMode.Open));
            int count = reader.ReadInt32();
            switches = new List<(int framecount, SwitchState switchState)>();
            while (count > 0)
            {
                int frameCount = reader.ReadInt32();
                SwitchState switchState = (SwitchState)reader.ReadInt32();
                switches.Add((frameCount, switchState));
                count--;
            }
            replayMode = true;
            frameCounter = 0;
            replayIndex = 0;
            PowerOnReset();
        }
    }
}
