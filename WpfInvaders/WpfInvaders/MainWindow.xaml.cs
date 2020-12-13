using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfInvaders
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint MM_BeginPeriod(uint uMilliseconds);

        [Flags]
        private enum SwitchState
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
            AnimateCoinAlien
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
        private readonly GameData gameData;
        private SwitchState switchState;
        private readonly Stopwatch frameStopwatch;
        private readonly Stopwatch timeInIsrStopwatch;
        private int timerCount=0;
        private Thread timerThread;
        private ManualResetEvent dieEvent = new ManualResetEvent(false);
        private volatile bool invokeTick;

        public MainWindow()
        {
            InitializeComponent();
            frame = new WriteableBitmap(256, 224, 96, 96, PixelFormats.BlackWhite, null);
            imgScreen.Source = frame;
            imgScreenRotateMirrored.Source = frame;
            playerOne = new PlayerData();
            playerTwo = new PlayerData();
            gameData = new GameData();
            frameStopwatch = Stopwatch.StartNew();
            timeInIsrStopwatch = Stopwatch.StartNew();
            PowerOnReset();
            frameStopwatch.Start();
        }

        private void PowerOnReset()
        {
            DrawStatus();
            gameData.alienShotReloadRate = 8;
            MM_BeginPeriod(8);
            Pause.Content = "Pause";
            gameData.SplashMajorState = SplashMajorState.ToggleAnimateState;
            gameData.SplashMinorState = SplashMinorState.Idle;
            timerThread = new Thread(WaitingTimer);
            invokeTick = true;
            timerThread.Start();
        }

        private void WaitingTimer()
        {
            while (!dieEvent.WaitOne(16))
            {
                if (invokeTick)
                    this.Dispatcher.InvokeAsync(IsrRoutine);
            }
        }
        
        private void IsrRoutine()
        {
            timerCount++;
            if (timerCount==60)
            {
                timerCount = 0;
                var timeTaken = frameStopwatch.ElapsedMilliseconds;
                var timeInIsr = timeInIsrStopwatch.ElapsedMilliseconds;
                timeInIsrStopwatch.Restart();
                frameStopwatch.Restart();

                FrameCounter.Content=string.Format("60 frames took {0}ms timeInIsr is {1}ms", timeTaken,timeInIsr);
            }
            timeInIsrStopwatch.Start();
            // Draw the screen.
            for (int line = 0; line < 224; line++)
            {
                Int32Rect rect = new Int32Rect(0, 0, 256, 1);
                frame.WritePixels(rect, LineRender.RenderLine(line), 32, 0, line);
            }
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
            timeInIsrStopwatch.Stop();
        }

        private void RunWaitTask()
        {
            var message = (gameData.Credits > 1) ? "1 OR 2PLAYERS BUTTON" : "ONLY 1PLAYER  BUTTON";
            WriteText(0x10, (0x28 - 0x24), message);
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
                        int x = (gameData.DelayMessagePosition >> 8) - 0x24;
                        gameData.DelayMessagePosition+=0x100;
                        LineRender.Screen[y + x * LineRender.ScreenWidth] = c;
                        gameData.IsrDelay = 6;
                        gameData.SplashMinorState = SplashMinorState.PrintMessageDelay;
                    }
                    break;
                case SplashMinorState.PrintMessageDelay:
                    if (gameData.IsrDelay == 0)
                        gameData.SplashMinorState = SplashMinorState.PrintMessageCharacter;
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
                    return PrintDelayedMessage(0x3017, gameData.AnimateSplash ? "PLA@" : "PLAY");
                case SplashMajorState.PrintSpaceInvaders:
                    return PrintDelayedMessage(0x2B14, "SPACE  INVADERS");
                case SplashMajorState.PrintAdvanceTable:
                    WriteText(0x10, (0x28 - 0x24), ":SCORE ADVANCE TABLE:");
                    WriteText(0x0e, (0x2c - 0x24), "AB"); // Saucer
                    WriteText(0x0c, (0x2c - 0x24), "\x80\x81"); // Invader type C - sprite 0
                    WriteText(0x0a, (0x2c - 0x24), "\x7c\x7d"); // Invader type B - sprite 1
                    WriteText(0x08, (0x2c - 0x24), "\x60\x61"); // Invader type A - sprite 0
                    gameData.IsrDelay = 0x40;
                    return SplashMinorState.Wait;
                case SplashMajorState.PrintMystery:
                    return PrintDelayedMessage(0x2e0e, "=?MYSTERY");
                case SplashMajorState.Print30Points:
                    return PrintDelayedMessage(0x2e0c, "=30 POINTS");
                case SplashMajorState.Print20Points:
                    return PrintDelayedMessage(0x2e0a, "=20 POINTS");
                case SplashMajorState.Print10Points:
                    return PrintDelayedMessage(0x2e08, "=10 POINTS");
                case SplashMajorState.ScoreTableTwoSecondDelay:
                    return SplashDelay(0x80);
                case SplashMajorState.AnimateY:
                    return SplashMinorState.Idle;
                case SplashMajorState.PlayDemo:
                    WriteText(0x10, (0x2c - 0x24), "PLAY DEMO GAME");
                    return SplashMinorState.Idle;
                case SplashMajorState.AfterPlayDelay:
                    return SplashDelay(0x40);
                case SplashMajorState.InsertCoin:
                    ClearPlayField();
                    WriteText(0x11, (0x2c - 0x24), gameData.AnimateSplash ? "INSERT CCOIN" : "INSERT  COIN");
                    return SplashMinorState.Idle;
                case SplashMajorState.OneOrTwoPlayers:
                    return PrintDelayedMessage(0x2a0d, "<1 OR 2 PLAYERS>");
                case SplashMajorState.OnePlayOneCoin:
                    return PrintDelayedMessage(0x2a0a, ":1 PLAYER  1 COIN");
                case SplashMajorState.TwoPlayerTwoCoins:
                    return PrintDelayedMessage(0x2a07, ":2 PLAYERS 2 COINS");
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
            WriteText(0x013, (0x30 - 0x24), "PRESS");
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

        private void TimeToSaucer()
        {
        }

        private void RunGameObjects()
        {
        }

        private void DrawAlien()
        {
        }

        private void CursorNextAlien()
        {
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
            WriteHexByte(0x01, (0x3c - 0x24), gameData.Credits);
        }

        private void CreditLabel()
        {
            WriteText(0x01, (0x35 - 0x24), "CREDIT ");
        }

        private void HighScore()
        {
            // Original write is to 2f1c
            WriteHexWord(0x1c, (0x2f - 0x24), gameData.HiScore);
        }

        private void PlayerTwoScore()
        {
            // Original write is to 391c
            WriteHexWord(0x1c, (0x39 - 0x24), playerTwo.Score);
        }

        private void PlayerOneScore()
        {
            // Original write is to 271c
            WriteHexWord(0x1c, (0x27 - 0x24), playerOne.Score);
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

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (invokeTick)
            {
                invokeTick = false;
                Pause.Content = "Run";
            }
            else
            {
                invokeTick = true;
                Pause.Content = "Pause";
            }
        }

        private void FrameAdvance_Click(object sender, RoutedEventArgs e)
        {
            IsrRoutine();
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
