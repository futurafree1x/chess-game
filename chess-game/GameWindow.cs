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
        public GameWindow()
        {
            InitializeComponent();
            this.Text = "Chess";
            this.Icon = Resources.icon;
        }

        private void GameWindow_Load(object sender, EventArgs e)
        {
            
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Size = new Size(1000,800);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(48, 46, 43);
        }
    }
}
