﻿using System;
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
        private static extern uint MM_BeginPeriod(uint uMilliseconds);

        [Flags]
        public enum SwitchState
        {
            None = 0x00,
            Left = 0x01,
            Right = 0x02,
            Fire = 0x04,
            Coin = 0x08,
            PlayOnePlayer = 0x10,
            PlayTwoPlayer = 0x20,
        }

        public enum SplashMinorState
        {
            Idle,
            Wait,
            PrintMessageCharacter,
            PrintMessageDelay,
            AnimateYAlien,
            AnimateCoinAlien,
            PlayDemoWaitDeath
        }

        public enum SplashMajorState
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
            AnimateY,
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
        private PlayerData currentPlayer;
        private readonly GameData gameData;
        public SwitchState switchState;
        private readonly Stopwatch frameStopwatch;
        private readonly Stopwatch timeInIsrStopwatch;
        private int timerCount = 0;
        private Thread timerThread;
        private readonly ManualResetEvent dieEvent = new ManualResetEvent(false);
        private volatile bool invokeTick;

        public MainWindow()
        {
            InitializeComponent();
            // Set the multimedia timer to 1ms
            MM_BeginPeriod(1);
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

        private void GameTick()
        {
            gameData.IsrDelay--;
            HandleCoinSwitch();
            if (!gameData.SuspendPlay)
            {
                if (gameData.GameMode || gameData.DemoMode)
                {
                    CursorNextAlien();
                    DrawAlien();
                    RunGameObjects();
                    TimeToSaucer();
                    // Probably wrong right now...
                    if (gameData.DemoMode)
                        IsrTasksSplashScreen();
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

        private void RenderScreen()
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
                        byte c = (byte)gameData.DelayMessage[gameData.DelayMessageIndex++];
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
                    PlayerFireOrDemo();

                    if (gameData.PlayerBase.Alive != PlayerBase.PlayerAlive.Alive)
                    {
                        gameData.PlayerShot = 0;
                        gameData.SplashMinorState = SplashMinorState.Idle;
                    }
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
                    WriteText(0x10, 4, ":SCORE ADVANCE TABLE:");
                    WriteText(0x0e, 7, "\xa0\xa1\xa2"); // Saucer 
                    WriteText(0x0c, 8, "\x80\x81"); // Invader type C - sprite 0
                    WriteText(0x0a, 8, "\x7c\x7d"); // Invader type B - sprite 1
                    WriteText(0x08, 8, "\x60\x61"); // Invader type A - sprite 0
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
                case SplashMajorState.AnimateY:
                    return SplashMinorState.Idle;
                case SplashMajorState.PlayDemo:
                    ClearPlayField();
                    playerOne.ShipsRem = 3;
                    currentPlayer = playerOne;
                    RemoveShip();
                    gameData.ResetVariables();
                    currentPlayer.InitAliens();
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
                    return PrintDelayedMessage(0x060a, ":1 PLAYER  1 COIN");
                case SplashMajorState.TwoPlayerTwoCoins:
                    return PrintDelayedMessage(0x0607, ":2 PLAYERS 2 COINS");
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

        private void ClearPlayField()
        {
            int i = 2;
            while (i < (LineRender.ScreenHeight * LineRender.ScreenWidth))
            {
                LineRender.Screen[i] = 0x20;
                i++;
                if ((i & 0x1f) >= 0x1c) i += 6;
            }
        }
        private void PlayerShotAndBump()
        {
            PlayerShotHit();
            RackBump();
        }

        private void RackBump()
        {
            if (gameData.RackDirectionRightToLeft)
            {
                if (CheckPlayFieldLineIsBlank(9) == false)
                {
                    gameData.RefAlienDeltaX = 2;
                    gameData.RefAlienDeltaY = -8;
                    gameData.RackDirectionRightToLeft = false;
                }
            }
            else
            {
                if (CheckPlayFieldLineIsBlank(213) == false)
                {
                    gameData.RefAlienDeltaX = -2;
                    gameData.RefAlienDeltaY = -8;
                    gameData.RackDirectionRightToLeft = true;
                }
            }
        }

        private bool CheckPlayFieldLineIsBlank(int line)
        {
            var data = LineRender.RenderLine(line);
            for (int i = 4; i < 27; i++)
            {
                if (data[i] != 0) return false;
            }
            return true;
        }

        private void PlayerShotHit()
        {
        }

        private void PlayerFireOrDemo()
        {
            PlayerShotAndBump();
        }

        private void DrawBottomLine()
        {
            int i = 2;
            for (int j = 0; j < LineRender.ScreenHeight; j++)
            {
                LineRender.Screen[i] = (byte)'_';
                i += LineRender.ScreenWidth;
            }
        }

        private void ShieldsToScreen()
        {
            WriteText(0x7, 4, "\x00\x01\x02  \x06\x07\x08\x09  \x0e\x0f\x10  \x14\x15\x16\x17");
            WriteText(0x6, 4, "\x03\x04\x05  \x0a\x0b\x0c\x0d  \x11\x12\x13  \x18\x19\x1a\x1b");
        }

        private void RemoveShip()
        {
            if (currentPlayer.ShipsRem == 0)
                return;
            WriteText(0x01, 0x01, (currentPlayer.ShipsRem & 0x0f).ToString());
            currentPlayer.ShipsRem--;
            int x = 0x03;
            int y = 0x01;
            int numShips = currentPlayer.ShipsRem;
            while (numShips != 0)
            {
                WriteText(y, x, "\xb8\xb9");
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

        private void RunGameObjects()
        {
            foreach (var timerObject in gameData.TimerObjects)
            {
                if (timerObject.IsActive)
                {
                    if (timerObject.Ticks == 0)
                    {
                        timerObject.Action();
                    }
                    else
                    {
                        timerObject.Ticks--;
                    }
                }
            }
        }

        private void DrawAlien()
        {
            if (gameData.AlienExploding)
            {
                ExplodeAlienTimer();
            }
            if (currentPlayer.Aliens[gameData.AlienCurIndex] != 0)
            {
                int currOffset = gameData.AlienCharacterCurY + gameData.AlienCharacterCurX * LineRender.ScreenWidth;
                // Side effect of the original shift logic is that the row above the current invader is cleared
                LineRender.Screen[currOffset + 1] = 0x20;
                LineRender.Screen[currOffset + 33] = 0x20;
                if (gameData.RackDirectionRightToLeft)
                    LineRender.Screen[currOffset + 65] = 0x20;

                switch (gameData.AlienCharacterOffset)
                {
                    case 0:
                        LineRender.Screen[currOffset] = (byte)gameData.AlienCharacterStart;
                        LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 1);
                        break;
                    case 2:
                        LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 6);
                        LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 7);
                        // Going right to left
                        if (gameData.RackDirectionRightToLeft)
                        {
                            if (LineRender.Screen[currOffset + 64] == (byte)(gameData.AlienCharacterStart + 4))
                                LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 2);
                            else
                                LineRender.Screen[currOffset + 64] = 0x20;
                        }
                        break;
                    case 4:
                        if (gameData.RackDirectionRightToLeft)
                        {
                            // Going Right to left
                            if (LineRender.Screen[currOffset] == gameData.AlienCharacterStart + 5)
                                LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 4);
                            else
                                LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 2);
                            LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 3);
                            if (LineRender.Screen[currOffset + 64] != 0x20)
                                LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 5);
                            else
                                LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 4);
                        }
                        else
                        {
                            // Going left to right... Alien on our left?
                            if (LineRender.Screen[currOffset - 32] == gameData.AlienCharacterStart + 3)
                                LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 4);
                            else
                                LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 2);
                            LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 3);
                            // Going left to right... Another alien on our right?
                            if (LineRender.Screen[currOffset + 64] == gameData.AlienCharacterStart + 6)
                                LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 10);
                            else
                                LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 5);
                        }
                        break;
                    case 6:
                        if (gameData.RackDirectionRightToLeft)
                        {
                            LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 8);
                            LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 9);
                        }
                        else
                        {
                            // Going Left to right... No partial alien to left of us make this cell blank
                            if ((gameData.AlienCharacterCurX == 0) || (LineRender.Screen[currOffset - 32] != (gameData.AlienCharacterStart + 8)))
                                LineRender.Screen[currOffset] = 0x20;
                            else
                                LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 9);
                            LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 8);
                            // Alien to the right of us?
                            if (LineRender.Screen[currOffset + 64] == (gameData.AlienCharacterStart + 4))
                                LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 11);
                            else
                                LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 9);
                        }
                        break;
                }
            }
        }

        private void ExplodeAlienTimer()
        {
            throw new NotImplementedException();
        }

        private void CursorNextAlien()
        {
            if (!gameData.PlayerOk)
                return;
            // Alien been drawn yet?
            if (gameData.WaitOnDraw)
                return;
            int timesThroughAliens = 0;
            do
            {
                gameData.AlienCurIndex++;
                if (gameData.AlienCurIndex == 55)
                {
                    gameData.AlienCurIndex = 0;
                    timesThroughAliens++;
                    gameData.RefAlienX += gameData.RefAlienDeltaX;
                    gameData.RefAlienY += gameData.RefAlienDeltaY;
                    gameData.RefAlienDeltaY = 0;
                    //StopIsr();
                }
            } while ((currentPlayer.Aliens[gameData.AlienCurIndex] == 0) && (timesThroughAliens < 2));
            if (timesThroughAliens >= 2)
            {
                // TODO: Should this flag something.
                return;
            }
            CalculateAlienPosition();
            if (gameData.AlienCharacterCurY == 4)
            {
                KillPlayer();
                return;
            }
            gameData.WaitOnDraw = false;
        }

        private void KillPlayer()
        {
            throw new NotImplementedException();
        }

        private void CalculateAlienPosition()
        {
            // Find the positon of our alien
            gameData.AlienCharacterCurY = gameData.RefAlienY >> 3;
            int curAlien = gameData.AlienCurIndex;
            int alienRow = 0;
            while (curAlien >= 11)
            {
                curAlien -= 11;
                gameData.AlienCharacterCurY += 2;
                alienRow++;
            }
            gameData.AlienCharacterCurX = (gameData.RefAlienX >> 3) + (curAlien << 1);
            gameData.AlienCharacterOffset = gameData.RefAlienX & 0x7;
            gameData.AlienCharacterStart = 0x60 + ((alienRow >> 1) << 4);
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

        private byte BcdAdd(byte a, byte b)
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

        private void WriteText(int y, int x, string text)
        {
            foreach (char c in text)
                LineRender.Screen[y + (x++) * LineRender.ScreenWidth] = (byte)c;
        }

        private void DrawNumCredits()
        {
            WriteHexByte(0x01, 0x18, gameData.Credits);
        }

        private void CreditLabel()
        {
            WriteText(0x01, 0x11, "CREDIT ");
        }

        private void HighScore()
        {
            // Original write is to 2f1c
            WriteHexWord(0x1c, 0x0b, gameData.HiScore);
        }

        private void PlayerTwoScore()
        {
            // Original write is to 391c
            WriteHexWord(0x1c, 0x15, playerTwo.Score);
        }

        private void PlayerOneScore()
        {
            // Original write is to 271c
            WriteHexWord(0x1c, 0x03, playerOne.Score);
        }

        private void DrawScreenHead()
        {
            // Note semi-colon is really minus
            WriteText(0x1e, 0x00, " SCORE<1> HI;SCORE SCORE<2> ");
        }

        private void WriteHexWord(int y, int x, short w)
        {
            WriteText(y, x, w.ToString("X4"));
        }

        private void WriteHexByte(int y, int x, byte b)
        {
            WriteText(y, x, b.ToString("X2"));
        }

        private void ClearScreen()
        {
            for (int i = 0; i < (LineRender.ScreenHeight * LineRender.ScreenWidth); i++)
                LineRender.Screen[i] = 0x20;
        }

        public void StopIsr()
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

        private void SaveScreenShot(string fname)
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
            }
            switchState |= setMask;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dieEvent.Set();
        }
    }
}
