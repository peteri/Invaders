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

namespace WpfInvaders
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap frame;

        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < 256; i++)
                LineRender.BitmapChar[i] = (byte)i;
            for (int i = 0; i < 256; i++)
                LineRender.Screen[i] = (byte)i;
            frame = new WriteableBitmap(256, 224, 96, 96, PixelFormats.BlackWhite, null);
            imgScreen.Source = frame;
        }

        private void btnFrameAdance_Click(object sender, RoutedEventArgs e)
        {
            for (int line = 0; line < 224; line++)
            {
                Int32Rect rect = new Int32Rect(0, 0, 256, 1);
                frame.WritePixels(rect, LineRender.RenderLine(line), 32, 0, line);
            }
        }
    }
}
