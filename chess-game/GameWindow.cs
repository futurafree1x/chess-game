using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Media;
using chess_game.Properties;
using System.Runtime.CompilerServices;

namespace chess_game
{
    public partial class GameWindow : Form
    {
        /*
            1.    Create game window with simple design 
            2.    Definitions for the game board and pieces 
            2.1   Graphical representation of board and pieces 
            3.    Movement for pieces 
            3.1   Basic movement (drag and drop, etc) 
            3.2   Define legal moves 
            3.2.1 special moves 
                 - castling / 
                 - en passant
                 - promotion / 
            3.3   Alternating turns between white and black 
            4.    Determining check and checkmate 

        5. QOL
        5.1. time control 
        5.2. FEN string reader
        5.3. move stack
        5.4. resign button
        */
        private const int SquareSize = 80;
        private readonly Board board = new Board();
        private readonly Dictionary<Piece, Image> pieceImages = new Dictionary<Piece, Image>();
        
        private readonly Color lightSquare = Color.FromArgb(221, 221, 221);
        private readonly Color darkSquare = Color.FromArgb(48, 71, 94);
        public GameWindow()
        {
            InitializeComponent();
            this.Text = "Chess";
            this.Icon = Resources.icon;
            foreach (Piece p in Enum.GetValues(typeof(Piece)))
            {
                if (p != Piece.None)
                    pieceImages[p] = (Image)Resources.ResourceManager.GetObject(p.ToString());
            }
            board.LoadFen(Board.StartFen);  // load board position and get piece images from resources
        }

        private void GameWindow_Load(object sender, EventArgs e)
        {
            this.ClientSize = new Size(1000, 800);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(48, 46, 43);
        }

        private void panelBoard_Paint(object sender, PaintEventArgs e) // draw the chessboard
        {
            DrawBoard(e.Graphics);
            DrawPieces(e.Graphics);
        }

        private void DrawBoard(Graphics g) // draws the board with alternating light and dark squares
        {
            using (var light = new SolidBrush(lightSquare))
            using (var dark = new SolidBrush(darkSquare))
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    for (int file = 0; file < 8; file++)
                    {
                        bool isLight = (rank + file) % 2 == 0;
                        g.FillRectangle(isLight ? light : dark, file * SquareSize, rank * SquareSize, SquareSize, SquareSize);
                    }
                }
            }
        }

        private void DrawPieces(Graphics g) // draws the pieces on the board based on the current state of the board
        {
            for (int i = 0; i < 64; i++)
            {
                Piece piece = board.Squares[i];
                if (piece != Piece.None)
                {
                    int rank = i / 8;
                    int file = i % 8;
                    
                    g.DrawImage(pieceImages[piece], file * SquareSize, (7 - rank) * SquareSize, SquareSize, SquareSize);
                }
            }
        }
    }
}

