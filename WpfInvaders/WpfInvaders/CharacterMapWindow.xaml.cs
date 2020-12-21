using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfInvaders
{
    /// <summary>
    /// Interaction logic for CharacterMapWindow.xaml
    /// </summary>
    public partial class CharacterMapWindow : Window
    {
        private readonly WriteableBitmap frame;
        private const int PixelSize = 3;
        private const int BytesPerPixel = 3;
        private const int BorderSize = 32;
        private const int CharacterSize = 8;
        private const int CellSizeInPixesl = (PixelSize * CharacterSize);
        private const int GridLength = CellSizeInPixesl * 16 + BorderSize;
        private const int TotalSize = GridLength + (BorderSize / 2);

        public CharacterMapWindow()
        {
            InitializeComponent();
            frame = new WriteableBitmap(TotalSize, TotalSize, 96, 96, PixelFormats.Bgr24, null);
            imgCharMapRotated.Source = frame;
        }

        private void FillGrid()
        {
            for (int c = 0; c < 256; c++)
            {
                DrawCharacter((c & 0x0f) * CellSizeInPixesl + BorderSize, (c >> 4) * CellSizeInPixesl + BorderSize, c);
            }

            // Draw some border lines and put a c
            int quarterBorder = BorderSize / 4;
            int lineLength = GridLength - quarterBorder;
            var buffer = new byte[BytesPerPixel * lineLength];
            int x = 0;
            for (int i = 0; i < lineLength; i++)
            {
                buffer[x++] = 0xff;
                buffer[x++] = 0xff;
                buffer[x++] = 0xff;
            }

            Int32Rect horizontalLine = new Int32Rect(0, 0, lineLength, 1);
            Int32Rect verticalLine = new Int32Rect(0, 0, 1, lineLength);
            int offset = BorderSize - 1;
            for (int i = 0; i < 17; i++)
            {
                frame.WritePixels(horizontalLine, buffer, lineLength * BytesPerPixel, quarterBorder, offset);
                frame.WritePixels(verticalLine, buffer, BytesPerPixel, offset, quarterBorder);
                offset += CellSizeInPixesl;
            }
            // Draw top of character boxes.
            horizontalLine = new Int32Rect(0, 0, GridLength - BorderSize, 1);
            verticalLine = new Int32Rect(0, 0, 1, GridLength - BorderSize);
            frame.WritePixels(horizontalLine, buffer, (GridLength - BorderSize) * BytesPerPixel, BorderSize - 1, quarterBorder);
            frame.WritePixels(verticalLine, buffer, BytesPerPixel, quarterBorder, BorderSize - 1);

            // Put some labels on
            for (int i = 0; i < 16; i++)
            {
                buffer = new byte[BytesPerPixel * 8 * 8];
                for (int j = 0; j < 8; j++)
                {
                    int c = (i < 10) ? i + 0x30 : i + 0x37;
                    byte b = CharacterRom.Characters[c * 8 + j];
                    int offs = j * BytesPerPixel;
                    for (int k = 0; k < 8; k++)
                    {
                        byte pixelColor = (byte)(((b & 0x1) == 0x01) ? 0xff : 0x00);
                        b = (byte)(b >> 1);
                        buffer[offs] = pixelColor;
                        buffer[offs + 1] = pixelColor;
                        buffer[offs + 2] = pixelColor;
                        offs += 8 * BytesPerPixel;
                    }
                }
                var characterBox = new Int32Rect(0, 0, 8, 8);
                int labelPos =(i+1) * CellSizeInPixesl + BorderSize/2;
                frame.WritePixels(characterBox, buffer, BytesPerPixel * 8, labelPos, BorderSize / 2);
                frame.WritePixels(characterBox, buffer, BytesPerPixel * 8, BorderSize / 2, labelPos);
            }

            using FileStream stream = new FileStream("c:\\temp\\charactermap.png", FileMode.Create);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(frame));
            encoder.Save(stream);
        }

        private void DrawCharacter(int x, int y, int c)
        {
            int stride = BytesPerPixel * PixelSize * CharacterSize;
            var buffer = new byte[stride * PixelSize * CharacterSize];
            for (int i = 0; i < CharacterSize; i++)
            {
                int offs = i * BytesPerPixel * PixelSize;

                byte b = (byte)((c < 0x20) ? LineRender.BitmapChar[c * 8 + i] : CharacterRom.Characters[c * 8 + i]);
                for (int j = 0; j < CharacterSize; j++)
                {
                    byte pixelColor = (byte)(((b & 0x1) == 0x01) ? 0xff : 0x00);
                    byte borderColor = 0x40;
                    for (int k = 0; k < (PixelSize - 1) * BytesPerPixel; k++) buffer[offs + k] = pixelColor;
                    for (int k = (PixelSize - 1) * BytesPerPixel; k < PixelSize * BytesPerPixel; k++) buffer[offs + k] = borderColor;
                    offs += stride;
                    for (int k = 0; k < (PixelSize - 1) * BytesPerPixel; k++) buffer[offs + k] = pixelColor;
                    for (int k = (PixelSize - 1) * BytesPerPixel; k < PixelSize * BytesPerPixel; k++) buffer[offs + k] = borderColor;
                    offs += stride;
                    for (int k = 0; k < (PixelSize) * BytesPerPixel; k++) buffer[offs + k] = borderColor;
                    offs += stride;
                    b = (byte)(b >> 1);
                }
            }
            Int32Rect rect = new Int32Rect(0, 0, PixelSize * 8, PixelSize * 8);
            frame.WritePixels(rect, buffer, stride, x, y);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FillGrid();
        }
    }
}
