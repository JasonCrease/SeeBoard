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
        int pieceX = 0;
        int pieceY = 0;

        public MainWindow()
        {
            this.Left = 0;
            this.Top = 0;

            InitializeComponent();

            ButtonGo_Click(this, null);
        }

        Engine.Engine m_Engine;

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            m_Engine = new Engine.Engine();
            m_Engine.BoardImagePath = System.IO.Path.GetFullPath(".\\..\\Images\\Boards\\Board1.jpg");
            m_Engine.Process();

            sw.Stop();
            TextBlockTimeTaken.Text = sw.ElapsedMilliseconds.ToString() + "ms";

            OrigImage.Source = BitmapSourceConvert.ToBitmapSource(m_Engine.Board.BoardImage);
            OrigImageWithQuads.Source = BitmapSourceConvert.ToBitmapSource(m_Engine.Board.BoardImageWithBoxes); 
            ShowPieceImage();
            //GrayImage.Source = BitmapSourceConvert.ToBitmapSource(engine.Board.GrayImage);
            //CannyImage.Source = BitmapSourceConvert.ToBitmapSource(engine.Board.CannyImage);
            //LinesImage.Source = BitmapSourceConvert.ToBitmapSource(engine.Board.LinesImage);
            WarpedImage.Source = BitmapSourceConvert.ToBitmapSource(m_Engine.Board.WarpedImage);
            WarpedCannyImage.Source = BitmapSourceConvert.ToBitmapSource(m_Engine.Board.WarpedCannyImage);
            WarpedLinesImage.Source = BitmapSourceConvert.ToBitmapSource(m_Engine.Board.WarpedLinesImage);
            GridBoxesImage.Source = BitmapSourceConvert.ToBitmapSource(m_Engine.Board.GridQuadsImage);
        }

        private void ShowPieceImage()
        {
            pieceX %= 8;
            pieceY %= 8;
            PieceImage.Source = BitmapSourceConvert.ToBitmapSource(m_Engine.PieceFinder.PieceImages[pieceX, pieceY]);
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

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            pieceX++;
            if(pieceX == 8)
            {
                pieceX = 0;
                pieceY++;

            }
            ShowPieceImage();
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            pieceX++;
            if (pieceX == -1)
            {
                pieceX = 0;
                pieceY--;

            }
            ShowPieceImage();
        }
    }
}
