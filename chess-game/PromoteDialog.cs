using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace chess_game
{
    // small window showing queen, rook, bishop and knight; clicking one picks the promotion
    public class PromoteDialog : Form
    {
        public Piece ChosenPiece { get; private set; } = Piece.None;

        public PromoteDialog(PlayerColour colour, Dictionary<Piece, Image> pieceImages)
        {
            const int size = 80;

            Text = "Promote pawn";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ControlBox = false;          // no close button, so a piece must be chosen
            ShowInTaskbar = false;
            ClientSize = new Size(4 * size, size);
            BackColor = Color.FromArgb(48, 46, 43);

            Piece[] choices = colour == PlayerColour.White
                ? new[] { Piece.wQueen, Piece.wRook, Piece.wBishop, Piece.wKnight }
                : new[] { Piece.bQueen, Piece.bRook, Piece.bBishop, Piece.bKnight };

            for (int i = 0; i < choices.Length; i++)
            {
                Piece piece = choices[i];
                var button = new Button
                {
                    Location = new Point(i * size, 0),
                    Size = new Size(size, size),
                    BackgroundImage = pieceImages[piece],
                    BackgroundImageLayout = ImageLayout.Zoom,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                button.FlatAppearance.BorderSize = 0;
                button.Click += (s, e) =>
                {
                    ChosenPiece = piece;
                    DialogResult = DialogResult.OK; // closes the dialog
                };
                Controls.Add(button);
            }
        }
    }
}