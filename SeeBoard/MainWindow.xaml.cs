using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace SeeBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.Left = 1550;
            this.Top = 30;

            InitializeComponent();

            //ButtonGo_Click(this, null);
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            Engine.Engine engine = new Engine.Engine(); 
            engine.BoardImagePath = System.IO.Path.GetFullPath(".\\..\\Images\\EmptyBoards\\Board9.jpg");
            engine.Process();

            OrigImage.Source = BitmapSourceConvert.ToBitmapSource(engine.Board.BoardImage);
            GrayImage.Source = BitmapSourceConvert.ToBitmapSource(engine.Board.GrayImage);
            CannyImage.Source = BitmapSourceConvert.ToBitmapSource(engine.Board.CannyImage);
            LinesImage.Source = BitmapSourceConvert.ToBitmapSource(engine.Board.LinesImage);
            WarpedImage.Source = BitmapSourceConvert.ToBitmapSource(engine.Board.WarpedImage);
            WarpedCannyImage.Source = BitmapSourceConvert.ToBitmapSource(engine.Board.WarpedCannyImage);
            WarpedLinesImage.Source = BitmapSourceConvert.ToBitmapSource(engine.Board.WarpedLinesImage);
            GridBoxesImage.Source = BitmapSourceConvert.ToBitmapSource(engine.Board.GridQuadsImage);
        }

        public static class BitmapSourceConvert
        {
            [DllImport("gdi32")]
            private static extern int DeleteObject(IntPtr o);

            public static BitmapSource ToBitmapSource(Emgu.CV.IImage image)
            {
                using (System.Drawing.Bitmap source = image.Bitmap)
                {
                    IntPtr ptr = source.GetHbitmap();

                    BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        ptr,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                    DeleteObject(ptr);
                    return bs;
                }
            }
        }
    }
}
