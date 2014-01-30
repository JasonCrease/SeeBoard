using System;
using System.Drawing;
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

namespace SeePieces
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            Engine.Piece.PieceDecider decider = new Engine.Piece.PieceDecider();
            decider.PieceImagePath = System.IO.Path.GetFullPath(".\\..\\Images\\Pieces\\wRook1.jpg");
            decider.Process();

            PieceImage.Source = BitmapSourceConvert.ToBitmapSource(decider.PieceImage);
            GrayImage.Source = BitmapSourceConvert.ToBitmapSource(decider.GrayImage);
            CannyImage.Source = BitmapSourceConvert.ToBitmapSource(decider.CannyImage);
            HueImage.Source = BitmapSourceConvert.ToBitmapSource(decider.HueImage);
            MaskedImage.Source = BitmapSourceConvert.ToBitmapSource(decider.MaskedImage);
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
