using ShogiWebsite.Shogi.Pieces;

namespace ShogiWebsite.Shogi
{
    internal class Board
    {
        internal Square[,] squares;
        internal bool isPlayer1Turn;
        internal Player player1;
        internal Player player2;
        /// <summary>no column, row, or piece<br/>used for pieces on hand</summary>
        internal Square nullSquare;
        internal List<string> log;
        internal bool isOver;
        internal Player? winner;

        internal Board()
        {
            // Square [ column , row ]
            player1 = new Player(this, true);
            player2 = new Player(this, false);
            squares = new Square[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    squares[i, j] = new Square(i, j, null, this);
                }
            }
            InitPieces();
            //InitCheckMate();
            isPlayer1Turn = true;
            nullSquare = new Square(this);
            log = new();
            isOver = false;
            winner = null;
            player1.InitLater();
            player2.InitLater();
        }

        internal Square? GetSquareByCoordinate(string coordinate)
        {
            if (coordinate.Length != 2) return null;
            int row = Array.IndexOf(Square.rows, $"{coordinate[0]}");
            int column = Array.IndexOf(Square.columns, $"{coordinate[1]}");
            return nullSquare.SquareAt(column, row);
        }

        internal void InitPieces()
        {
            BetterConsole.Action("Setup all pieces on the board");
            // Pawn
            for (int i = 0; i < 9; i++)
            {
                squares[i, 2].piece = new Pawn(player2, squares[i, 2]);
                squares[i, 6].piece = new Pawn(player1, squares[i, 6]);
            }
            // Bishop
            squares[7, 1].piece = new Bishop(player2, squares[7, 1]);
            squares[1, 7].piece = new Bishop(player1, squares[1, 7]);
            // Rook
            squares[1, 1].piece = new Rook(player2, squares[1, 1]);
            squares[7, 7].piece = new Rook(player1, squares[7, 7]);
            // Lance
            squares[0, 0].piece = new Lance(player2, squares[0, 0]);
            squares[0, 8].piece = new Lance(player1, squares[0, 8]);
            squares[8, 0].piece = new Lance(player2, squares[8, 0]);
            squares[8, 8].piece = new Lance(player1, squares[8, 8]);
            // Knight
            squares[1, 0].piece = new Knight(player2, squares[1, 0]);
            squares[1, 8].piece = new Knight(player1, squares[1, 8]);
            squares[7, 0].piece = new Knight(player2, squares[7, 0]);
            squares[7, 8].piece = new Knight(player1, squares[7, 8]);
            // Silver General
            squares[2, 0].piece = new Silver(player2, squares[2, 0]);
            squares[2, 8].piece = new Silver(player1, squares[2, 8]);
            squares[6, 0].piece = new Silver(player2, squares[6, 0]);
            squares[6, 8].piece = new Silver(player1, squares[6, 8]);
            // Gold General
            squares[3, 0].piece = new Gold(player2, squares[3, 0]);
            squares[3, 8].piece = new Gold(player1, squares[3, 8]);
            squares[5, 0].piece = new Gold(player2, squares[5, 0]);
            squares[5, 8].piece = new Gold(player1, squares[5, 8]);
            // King
            squares[4, 0].piece = new King(player2, squares[4, 0]);
            squares[4, 8].piece = new King(player1, squares[4, 8]);
        }

        internal void InitCheckMate()
        {
            // player1
            squares[3, 8].piece = new Gold(player1, squares[3, 8]);
            squares[4, 7].piece = new Gold(player1, squares[4, 7]);
            squares[4, 8].piece = new King(player1, squares[4, 8]);
            // player2
            squares[5, 7].piece = new Rook(player2, squares[5, 7]);
            squares[5, 8].piece = new Rook(player2, squares[5, 8]);
            squares[8, 2].piece = new Bishop(player2, squares[8, 2]);
            squares[4, 0].piece = new King(player2, squares[4, 0]);

        }

        internal string ToHtml(bool isOver)
        {
            string text = "<div class=\"board\">\n";
            for (int i = 0; i < 9; i++)
            {
                string subText = "";
                for (int j = 0; j < 9; j++) subText += squares[j, i].ToHtml(isOver);
                text += $"<div class=\"row\">{subText}</div>";
            }
            return text + "</div>";
        }

        internal string JavascriptMoveLists()
        {
            Player currentPlayer = isPlayer1Turn ? player1 : player2;
            return currentPlayer.JavascriptMoveLists();
        }

        internal bool IsPlayersTurn(Player player) => (player.isPlayer1 && isPlayer1Turn) || !(player.isPlayer1 || isPlayer1Turn);

        internal string LogToHtml()
        {
            string text = "<div id=\"log\" class=\"log\">";
            int length = log.Count;
            if (length <= 0) return text + "</div>";
            text += $"\n{log[0]}";
            for (int i = 1; i < length; i++) text += $"\n<br>{log[i]}";
            return text + "\n</div>";
        }

        internal void EndGame(Player winner)
        {
            isOver = true;
            this.winner = winner;
        }

        internal void PlayerTurn()
        {
            Player player = isPlayer1Turn ? player1 : player2;
            player.AfterOpponent();
            if (isOver) return;
            player.PrepareTurn();
        }

        internal string PromotionZone() => $"var promotionZone = {(isPlayer1Turn ? "['a', 'b', 'c']" : "['g', 'h', 'i']")}";

        internal string ForcePawnLancePromotion() => $"var pawnLancePromo = {(isPlayer1Turn ? "'a'" : "'i'")}";

        internal string ForceKnightPromotion() => $"var knightPromo = {(isPlayer1Turn ? "['a', 'b']" : "['h', 'i']")}";

        internal string GameEndHtml()
        {
            if (!isOver) return "";
            var builder = new LinesBuilder();
            builder
                .Line("<div id=\"game-end-overlay\">")
                .Line("<div style=\"background-color:#ffffff;border:2px solid #000000;width:600px;heigth:300px;\">")
                .Line($"<p>The Winner is: Player {(winner == player1 ? "1" : "2")!}</p>")
                .Line("<div class=\"button\" onclick=\"restart();\">")
                .Line("New Game")
                .Line("</div>")
                .Line("</div>")
                .Line("</div>");
            return builder.Build();
        }
    }
}
