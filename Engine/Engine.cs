using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class Engine
    {
        public Board Board
        {
            get
            {
                return m_Board;
            }
        }

        public string BoardImagePath
        {
            get;
            set;
        }

        Board m_Board;

        public Engine()
        {
        }

        public void Process()
        {
            m_Board = new Board();

            // Build original image, downscaled slightly
            m_Board.BoardImage = new Image<Bgr, byte>(BoardImagePath).Resize(400, 300, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, true);

            m_Board.FindBoard();
        }

    }
}
