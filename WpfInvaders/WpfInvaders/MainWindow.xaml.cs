using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly WriteableBitmap frame;
        private readonly DispatcherTimer dispatcherTimer;

        public MainWindow()
        {
            InitializeComponent();
            frame = new WriteableBitmap(256, 224, 96, 96, PixelFormats.BlackWhite, null);
            imgScreen.Source = frame;
            imgScreenRotateMirrored.Source = frame;
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1.0 / 50.0);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.IsEnabled = true;
            DrawStatus();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            for (int line = 0; line < 224; line++)
            {
                Int32Rect rect = new Int32Rect(0, 0, 256, 1);
                frame.WritePixels(rect, LineRender.RenderLine(line), 32, 0, line);
            }
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
            WriteText(0x01, (0x35 - 0x24), "CREDIT ");
        }

        private void CreditLabel()
        {
            WriteHexByte(0x01, (0x3c - 0x24), 0x00);
        }

        private void HighScore()
        {
            // Original write is to 2f1c
            WriteHexWord(0x1c, (0x2f - 0x24) , 0x00);
        }

        private void PlayerTwoScore()
        {
            // Original write is to 391c
            WriteHexWord(0x1c, (0x39 - 0x24) , 0x00);
        }

        private void PlayerOneScore()
        {
            // Original write is to 271c
            WriteHexWord(0x1c, (0x27 - 0x24), 0x00);
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
    }
}
