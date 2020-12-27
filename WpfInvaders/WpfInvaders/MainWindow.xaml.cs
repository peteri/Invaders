using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
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

        internal enum SplashMinorState
        {
            Idle,
            Wait,
            PrintMessageCharacter,
            PrintMessageDelay,
            AnimateYAlien,
            AnimateCoinAlien,
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
            AnimateY1,
            AnimateY2,
            AnimateY3,
            AnimateY4,
            AnimateY5,
            PlayDemo,
            AfterPlayDelay,
            InsertCoin,
            OneOrTwoPlayers,
            OnePlayOneCoin,
            TwoPlayerTwoCoins,
            AfterCoinDelay,
            AnimateCoinExplode,
            ToggleAnimateState
        }

        private readonly WriteableBitmap frame;
        private readonly PlayerData playerOne;
        private readonly PlayerData playerTwo;
        internal PlayerData CurrentPlayer;
        private readonly GameData gameData;
        internal SwitchState switchState;
        private readonly Stopwatch frameStopwatch;
        private readonly Stopwatch timeInIsrStopwatch;
        private int timerCount = 0;
        private Thread timerThread;
        private readonly ManualResetEvent dieEvent = new ManualResetEvent(false);
        private volatile bool invokeTick;
        // Holding down Right Ctrl gives type B aliens
        // Holding down Left  Ctrl gives type C aliens
        private int DiagnosticsAlientType = 0x80;

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
        }

        private void PowerOnReset()
        {
            DrawStatus();
            gameData.alienShotReloadRate = 8;
            Pause.Content = "Pause";
            gameData.SplashMajorState = SplashMajorState.ToggleAnimateState;
            gameData.SplashMinorState = SplashMinorState.Idle;
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
            timerCount++;
            if (timerCount > 59)
            {
                timerCount = 0;
                var timeTaken = frameStopwatch.ElapsedMilliseconds;
                var timeInIsr = timeInIsrStopwatch.ElapsedMilliseconds;
                frameStopwatch.Restart();
                timeInIsrStopwatch.Restart();
                FrameCounter.Content = string.Format("60 frames took {0}ms timeInIsr is {1}ms", timeTaken, timeInIsr);
            }
            timeInIsrStopwatch.Start();
            RenderScreen();
            GameTick();
            timeInIsrStopwatch.Stop();
        }

        private void HandleSpriteCollisions()
        {
            if (gameData?.PlayerShot?.ShotSprite.Collided() ?? false)
                gameData.AlienExploding = true;
            if (gameData?.AlienShotRolling.Shot.Collided() ?? false)
                gameData.AlienShotRolling.Collided();
            if (gameData?.AlienShotPlunger.Shot.Collided() ?? false)
                gameData.AlienShotPlunger.Collided();
            if (gameData?.AlienShotSquigly.Shot.Collided() ?? false)
                gameData.AlienShotSquigly.Collided();
        }

        private void GameTick()
        {
            gameData.IsrDelay--;
            HandleCoinSwitch();
            if (!gameData.SuspendPlay)
            {
                if (gameData.GameMode || gameData.DemoMode)
                {
                    HandleSpriteCollisions();
                    GameLoopStep();
                    // Pretend we're on the first half of the screen
                    gameData.VblankStatus = 0;
                    // Move every except the player
                    RunGameObjects(true);
                    // Normally the game loop runs for ever
                    // But do it here instead as we only have one isr
                    // required to get the players first shot off
                    // correctly in demo mode.
                    GameLoopStep();
                    gameData.Aliens.CursorNextAlien();

                    // Mame draws the screen at this point
                    // when single stepping...
                    RenderScreen();
                    // Clear the collision flags as in our real world
                    // application we won't bother doing this.
                    foreach (var sprite in LineRender.Sprites)
                        sprite.ClearCollided();

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
                        if (!gameData.WaitStartLoop)
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
            if (gameData.DemoMode)
            {
                PlayerFireOrDemo();
                PlayerShotHit();
                IsrTasksSplashScreen();
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
            var message = (gameData.Credits > 1) ? "1 OR 2PLAYERS BUTTON" : "ONLY 1PLAYER  BUTTON";
            WriteText(0x10, 0x04, message);
            // TODO: Poll player one and two buttons....
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
                        gameData.IsrDelay = 6;
                        gameData.SplashMinorState = SplashMinorState.PrintMessageDelay;
                    }
                    break;
                case SplashMinorState.PrintMessageDelay:
                    if (gameData.IsrDelay == 0)
                        gameData.SplashMinorState = SplashMinorState.PrintMessageCharacter;
                    break;
                case SplashMinorState.PlayDemoWaitDeath:
                    if (gameData.PlayerBase.Alive != PlayerBase.PlayerAlive.Alive)
                    {
                        gameData.PlayerShot.Status = PlayerShot.ShotStatus.Available;
                        gameData.SplashMinorState = SplashMinorState.PlayDemoWaitEndofExplosion;
                    }
                    break;
                case SplashMinorState.PlayDemoWaitEndofExplosion:
                    if (gameData.PlayerBase.Alive == PlayerBase.PlayerAlive.Alive)
                    {
                        gameData.PlayerShot.Status = PlayerShot.ShotStatus.Available;
                        gameData.DemoMode = false;
                        gameData.SplashMinorState = SplashMinorState.Idle;
                        // Hide all the sprites.
                        foreach (var sprite in LineRender.Sprites)
                            sprite.Visible = false;
                    }
                    break;
                case SplashMinorState.AnimateYAlien:
                    gameData.SplashMinorState = gameData.SplashAlienAnimation.Animate();
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
                    ClearPlayField();
                    return SplashDelay(0x40);
                case SplashMajorState.PrintPlay:
                    return PrintDelayedMessage(0x0c17, gameData.AnimateSplash ? "PLA@" : "PLAY");
                case SplashMajorState.PrintSpaceInvaders:
                    return PrintDelayedMessage(0x0714, "SPACE  INVADERS");
                case SplashMajorState.PrintAdvanceTable:
                    WriteText(0x10, 4, "*SCORE ADVANCE TABLE*");
                    WriteUnmappedText(0x0e, 7, "\x20\x21\x22"); // Saucer 
                    WriteUnmappedText(0x0c, 8, "\xa0\xa1"); // Invader type C - sprite 0
                    WriteUnmappedText(0x0a, 8, "\xbc\xbd"); // Invader type B - sprite 1
                    WriteUnmappedText(0x08, 8, "\x80\x81"); // Invader type A - sprite 0
                    gameData.IsrDelay = 0x40;
                    return SplashMinorState.Wait;
                case SplashMajorState.PrintMystery:
                    return PrintDelayedMessage(0x0a0e, "=? MYSTERY");
                case SplashMajorState.Print30Points:
                    return PrintDelayedMessage(0x0a0c, "=30 POINTS");
                case SplashMajorState.Print20Points:
                    return PrintDelayedMessage(0x0a0a, "=20 POINTS");
                case SplashMajorState.Print10Points:
                    return PrintDelayedMessage(0x0a08, "=10 POINTS");
                case SplashMajorState.ScoreTableTwoSecondDelay:
                    return SplashDelay(0x80);
                case SplashMajorState.AnimateY1:
                    if (gameData.AnimateSplash == false)
                    {
                        gameData.SplashMajorState = SplashMajorState.PlayDemo - 1;
                        return SplashMinorState.Idle;
                    }
                    gameData.SplashAlienAnimation = new SplashAlienAnimation();
                    LineRender.Sprites.Add(gameData.SplashAlienAnimation.AlienMovingY);
                    StopIsr();
                    return AnimateY(223, 123, 0);
                case SplashMajorState.AnimateY2:
                    WriteText(0x17, 0x0c, "PLA ");
                    return AnimateY(120, 224, 2);
                case SplashMajorState.AnimateY3:
                    return SplashDelay(0x40);
                case SplashMajorState.AnimateY4:
                    return AnimateY(224, 120, 4);
                case SplashMajorState.AnimateY5:
                    gameData.SplashAlienAnimation.AlienMovingY.Visible = false;
                    WriteText(0x17, 0x0c, "PLAY");
                    return SplashDelay(0x80);
                case SplashMajorState.PlayDemo:
                    ClearPlayField();
                    playerOne.ShipsRem = 3;
                    CurrentPlayer = playerOne;
                    RemoveShip();
                    gameData.ResetVariables(CurrentPlayer);
                    CurrentPlayer.InitAliens();
                    playerOne.ResetShields();
                    ShieldsToScreen();
                    DrawBottomLine();
                    gameData.DemoMode = true;
                    return SplashMinorState.PlayDemoWaitDeath;
                case SplashMajorState.AfterPlayDelay:
                    return SplashDelay(0x40);
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
                case SplashMajorState.AnimateCoinExplode:
                    return SplashMinorState.Idle;
                case SplashMajorState.AfterCoinDelay:
                    return SplashDelay(0x80);
                case SplashMajorState.ToggleAnimateState:
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

        private SplashMinorState AnimateY(int startX, int targetX, int image)
        {
            gameData.SplashAlienAnimation.Init(0x17 * 8, startX, targetX, image);
            return SplashMinorState.AnimateYAlien;
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
            gameData.WaitStartLoop = true;
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
            if (gameData.PlayerShot.ShotSprite.Y >= 0xce)
            {
                gameData.SaucerHit = true;
                gameData.PlayerShot.Status = PlayerShot.ShotStatus.AlienExploded;
                gameData.AlienExploding = false;

            }
            bool playerHitAlien = false;
            if (gameData.PlayerShot.ShotSprite.Y >= gameData.RefAlienY)
            {
                int rowY = gameData.RefAlienY;
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
                    int colX = gameData.RefAlienX + col * 0x10;
                    if (CurrentPlayer.Aliens[alienIndex] != 0)
                    {
                        CurrentPlayer.Aliens[alienIndex] = 0;
                        gameData.AlienExplodeTimer = 0x10;
                        gameData.AlienExplodeX = colX >> 3;
                        gameData.AlienExplodeXOffset = colX & 0x07;
                        gameData.AlienExplodeY = rowY >> 3;
                        gameData.Aliens.ExplodeAlien();
                        gameData.PlayerShot.ShotSprite.Visible = false;
                        gameData.PlayerShot.Status = PlayerShot.ShotStatus.AlienExploding;
                        playerHitAlien = true;
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
                LineRender.Screen[i] = 0x53;
                i += LineRender.ScreenWidth;
            }
        }

        private static void ShieldsToScreen()
        {
            WriteUnmappedText(0x7, 4, "\x00\x01\x02##\x06\x07\x08\x09##\x0e\x0f\x10##\x14\x15\x16\x17");
            WriteUnmappedText(0x6, 4, "\x03\x04\x05##\x0a\x0b\x0c\x0d##\x11\x12\x13##\x18\x19\x1a\x1b");
        }

        private void RemoveShip()
        {
            if (CurrentPlayer.ShipsRem == 0)
                return;
            WriteText(0x01, 0x01, (CurrentPlayer.ShipsRem & 0x0f).ToString());
            CurrentPlayer.ShipsRem--;
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

        private static void WriteHexWord(int y, int x, short w)
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
                case Key.LeftShift: clearMask = SwitchState.Fire; break;
                case Key.D1: clearMask = SwitchState.PlayOnePlayer; break;
                case Key.D2: clearMask = SwitchState.PlayTwoPlayer; break;
                case Key.D3: clearMask = SwitchState.Coin; break;
                case Key.F4:
                    StopIsr();
                    new CharacterMapWindow().Show();
                    break;
                case Key.RightCtrl: DiagnosticsAlientType = 0x80; break;
                case Key.LeftCtrl: DiagnosticsAlientType = 0x80; break;
                case Key.F5: DiagnosticPages.ShowShiftedInvaders(this, DiagnosticsAlientType); break;
                case Key.F6: DiagnosticPages.ShowExplodedInvaders(this, gameData, 0, DiagnosticsAlientType); break;
                case Key.F7: DiagnosticPages.ShowExplodedInvaders(this, gameData, 1, DiagnosticsAlientType); break;
                case Key.F8: DiagnosticPages.ShowExplodedInvaders(this, gameData, 2, DiagnosticsAlientType); break;
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
                case Key.LeftShift: setMask = SwitchState.Fire; break;
                case Key.D1: setMask = SwitchState.PlayOnePlayer; break;
                case Key.D2: setMask = SwitchState.PlayTwoPlayer; break;
                case Key.D3: setMask = SwitchState.Coin; break;
                // Used by diagnostics
                case Key.RightCtrl: DiagnosticsAlientType = 0x90; break;
                case Key.LeftCtrl: DiagnosticsAlientType = 0xa0; break;
            }
            switchState |= setMask;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dieEvent.Set();
        }
    }
}
