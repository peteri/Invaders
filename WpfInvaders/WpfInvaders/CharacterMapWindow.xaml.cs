using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfInvaders
{
    /// <summary>
    /// Interaction logic for CharacterMapWindow.xaml
    /// </summary>
    public partial class CharacterMapWindow : Window
    {
        private readonly WriteableBitmap frame;

        public CharacterMapWindow()
        {
            InitializeComponent();
            frame = new WriteableBitmap(408, 408, 96, 96, PixelFormats.Bgr24, null);
            imgCharMapRotated.Source = frame;
        }

        private void FillGrid()
        {
            for (int c = 0; c < 256; c++)
            {
                DrawCharacter((c & 0x0f) * 24 + 16, (c >> 4) * 24 + 16, c);
            }
        }

        private void DrawCharacter(int x, int y, int c)
        {
            int stride = 3 * 24;
            var buffer = new byte[stride * 24];
            for (int i = 0; i < 8; i++)
            {
                int offs = i * 9;

                byte b = (byte)((c < 0x20) ? LineRender.BitmapChar[c * 8 + i] : CharacterRom.Characters[c * 8 + i]);
                for (int j = 0; j < 8; j++)
                {
                    byte pixelColor = (byte)(((b & 0x1) == 0x01) ? 0xff : 0x00);
                    byte borderColor = 0x40;
                    for (int k = 0; k < 6; k++) buffer[offs + k] = pixelColor;
                    for (int k = 6; k < 9; k++) buffer[offs + k] = borderColor;
                    offs += stride;
                    for (int k = 0; k < 6; k++) buffer[offs + k] = pixelColor;
                    for (int k = 6; k < 9; k++) buffer[offs + k] = borderColor;
                    offs += stride;
                    for (int k = 0; k < 9; k++) buffer[offs + k] = borderColor;
                    offs += stride;
                    b = (byte)(b >> 1);
                }
            }
            Int32Rect rect = new Int32Rect(0, 0, 24, 24);
            frame.WritePixels(rect, buffer, stride, x, y);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FillGrid();
        }
    }
}
