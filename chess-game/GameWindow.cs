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
        private const int SquareSize = 80;
        private readonly Board board = new Board();
        private readonly Dictionary<Piece, Image> pieceImages = new Dictionary<Piece, Image>();

        private readonly Color lightSquare = Color.FromArgb(221, 221, 221);
        private readonly Color darkSquare = Color.FromArgb(48, 71, 94);
        private int dragFrom = -1; // square being dragged from or -1 if none
        private Point dragPoint; // get mouse position during drag
        private List<Move> legalMoves = new List<Move>();
        private readonly Image moveDot = Resources.moveDot;
        private bool gameOver = false;
        private int SquareAt(Point p) // helper function to get the square index from a mouse click point, returns -1 if outside the board
        {
            if (p.X < 0 || p.Y < 0 || p.X >= 8 * SquareSize || p.Y >= 8 * SquareSize)
                return -1;

            int file = p.X / SquareSize;
            int rank = 7 - p.Y / SquareSize;
            return rank * 8 + file;
        }
        private void UpdateTitle()
        {
            this.Text = $"Chess   eval: {Evaluation.Evaluate(board) / 100.0: + 0.00;-0.00;0.00}";
        }
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
            UpdateTitle();
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
            DrawCheck(e.Graphics);
            DrawMoveDots(e.Graphics);
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
                if (piece == Piece.None || i == dragFrom) continue;

                int rank = i / 8;
                int file = i % 8;

                g.DrawImage(pieceImages[piece], file * SquareSize, (7 - rank) * SquareSize, SquareSize, SquareSize);
            }
            if (dragFrom != -1)
            {
                g.DrawImage(pieceImages[board.Squares[dragFrom]],
                 dragPoint.X - SquareSize / 2, dragPoint.Y - SquareSize / 2, SquareSize, SquareSize);
            }

        }

        private void panelBoard_MouseDown(object sender, MouseEventArgs e)
        {
            if (gameOver) return;
            if (e.Button != MouseButtons.Left) return;

            int square = SquareAt(e.Location);
            if (square == -1) return; // clicked outside board
            Piece piece = board.Squares[square];
            if (piece == Piece.None) return; // empty square
            if (piece.Colour() != board.SideToMove) return; // not this player's turn

            dragFrom = square;
            dragPoint = e.Location;
            legalMoves = MoveGen.GenerateLegalMoves(board);
            panelBoard.Invalidate();
        }

        private void panelBoard_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragFrom == -1) return; // not dragging anything

            dragPoint = e.Location;
            panelBoard.Invalidate();
        }

        private void panelBoard_MouseUp(object sender, MouseEventArgs e)
        {
            if (dragFrom == -1) return;  // not dragging anything

            bool moved = false;
            int to = SquareAt(e.Location);
            if (to != -1 && to != dragFrom) // dropped on a different square
            {
                int index = legalMoves.FindIndex(m => m.From == dragFrom && m.To == to);

                if (index != -1 && legalMoves[index].Promotion != Piece.None) // pawn reached the last rank
                {
                    Piece choice = AskPromotion();
                    index = legalMoves.FindIndex(m => m.From == dragFrom && m.To == to && m.Promotion == choice);
                }

                if (index != -1) // a legal move between these squares exists
                {
                    board.MakeMove(legalMoves[index]);
                    UpdateTitle();
                    moved = true;
                }
            }

            dragFrom = -1;   // stop dragging either way
            panelBoard.Invalidate();
            if (moved) CheckGameOver();
        }
        private void DrawMoveDots(Graphics g) // shows where the dragged piece can move
        {
            if (dragFrom == -1) return;

            foreach (Move move in legalMoves)
            {
                if (move.From != dragFrom) continue;

                int rank = move.To / 8;
                int file = move.To % 8;
                g.DrawImage(moveDot, file * SquareSize, (7 - rank) * SquareSize, SquareSize, SquareSize);
            }
        }
        private void DrawCheck(Graphics g) // red square under the king of the side to move if it is in check
        {
            if (!MoveGen.IsInCheck(board, board.SideToMove)) return;

            int king = MoveGen.FindKing(board, board.SideToMove);
            int rank = king / 8;
            int file = king % 8;
            using (var red = new SolidBrush(Color.FromArgb(200, 220, 50, 50)))
            {
                g.FillRectangle(red, file * SquareSize, (7 - rank) * SquareSize, SquareSize, SquareSize);
            }
        }

        private void CheckGameOver() // ends the game if the player to move has no legal moves
        {
            if (MoveGen.GenerateLegalMoves(board).Count > 0) return; // game continues

            gameOver = true;
            panelBoard.Refresh(); // draw the final position before the message box appears

            if (MoveGen.IsInCheck(board, board.SideToMove))
            {
                string winner = board.SideToMove == PlayerColour.White ? "Black" : "White";
                MessageBox.Show($"Checkmate! {winner} wins.");
            }
            else
            {
                MessageBox.Show("Stalemate! It's a draw.");
            }
        }

        private Piece AskPromotion() // shows the promotion dialog, returns the chosen piece
        {
            using (var dialog = new PromoteDialog(board.SideToMove, pieceImages))
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.ChosenPiece : Piece.None;
            }
        }
    }
}

