using ShogiWebsite.Shogi.Pieces;

namespace ShogiWebsite.Shogi
{
    internal struct Coordinate
    {
        private int column;
        private int row;

        internal int Column { get => column; set => column = value; }
        internal int Row { get => row; set => row = value; }

        internal Coordinate(int column, int row)
        {
            this.column = column;
            this.row = row;
        }
    }

    internal class Board
    {
        internal Piece?[,] pieces;
        internal bool isPlayer1Turn;
        internal Player player1;
        internal Player player2;
        /// <summary>no column, row, or piece<br/>used for pieces on hand</summary>
        internal List<string> log;
        internal bool isOver;
        internal Player? winner;
        internal Phase phase;

        internal static readonly char[] columns = { '9', '8', '7', '6', '5', '4', '3', '2', '1' };
        internal static readonly char[] rows = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i' };

        internal Board()
        {
            // Square [ column , row ]
            player1 = new Player(this, true);
            player2 = new Player(this, false);
            pieces = new Piece?[9, 9];
            InitBoard();
            //InitCheckMate();
            isPlayer1Turn = true;
            log = new();
            isOver = false;
            winner = null;
            phase = Phase.ChoosePiece;
            player1.InitLater();
            player2.InitLater();
        }

        internal void SetPiece(Piece? piece, int column, int row)
        {
            pieces[column, row] = piece;
            if (piece != null)
                piece.coordinate = new Coordinate(column, row);
        }

        internal void SetPiece(Piece? piece, Coordinate coords) => SetPiece(piece, coords.Column, coords.Row);

        internal static string CoordinateString(int row, int column) => IsOnBoard(column, row) ? $"{rows[row]}{columns[column]}" : "hand";

        internal static string CoordinateString(Coordinate coordinate) => CoordinateString(coordinate.Column, coordinate.Row);

        internal static bool IsOnBoard(int column, int row) => 0 <= column && column < 9 && 0 <= row && row < 9;

        internal static bool IsOnBoard(Coordinate coordinate) => IsOnBoard(coordinate.Column, coordinate.Row);

        internal Piece? PieceAt(int column, int row) => IsOnBoard(column, row) ? pieces[column, row] : null;

        internal Piece? PieceAt(Coordinate coordinate) => PieceAt(coordinate.Column, coordinate.Row);

        internal enum Phase
        {
            ChoosePiece,
            AskForPromotion,
            SelectTarget
        }

        internal Piece? GetPieceByCoordinateString(string coordinate)
        {
            if (coordinate.Length != 2)
                return null;
            int row = Array.IndexOf(rows, coordinate[0]);
            int column = Array.IndexOf(columns, coordinate[1]);
            return PieceAt(column, row);
        }

        private static void DirectionLookMessage(int distance, string direction, int column, int row)
        {
            BetterConsole.Info($"Looking {distance} squares {direction} of {CoordinateString(row, column)}");
        }

        internal static Coordinate? Direction(Func<int, int, int, bool, Coordinate?> function, Coordinate coord, int distance = 1, bool printLog = true)
        {
            return function(coord.Column, coord.Row, distance, printLog);
        }

        internal static Coordinate? N(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "north", column, row);
            int newRow = row - distance;
            return IsOnBoard(column, newRow) ? new Coordinate(column, newRow) : null;
        }

        internal static Coordinate? N(Coordinate coord, int distance = 1, bool printLog = true) => Direction(N, coord, distance, printLog);

        internal static Coordinate? S(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "south", column, row);
            return N(column, row, -distance, false);
        }

        internal static Coordinate? S(Coordinate coord, int distance = 1, bool printLog = true) => Direction(S, coord, distance, printLog);

        internal static Coordinate? E(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "east", column, row);
            int newColumn = column + distance;
            return IsOnBoard(newColumn, row) ? new Coordinate(newColumn, row) : null;
        }

        internal static Coordinate? E(Coordinate coord, int distance = 1, bool printLog = true) => Direction(E, coord, distance, printLog);

        internal static Coordinate? W(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "west", column, row);
            return E(column, row, -distance, false);
        }

        internal static Coordinate? W(Coordinate coord, int distance = 1, bool printLog = true) => Direction(W, coord, distance, printLog);

        internal static Coordinate? NE(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "north east", column, row);
            Coordinate? n = N(column, row, distance, false);
            return n != null ? E(n.Value, distance, false) : null;
        }

        internal static Coordinate? NE(Coordinate coord, int distance = 1, bool printLog = true) => Direction(NE, coord, distance, printLog);

        internal static Coordinate? NW(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "north west", column, row);
            Coordinate? n = N(column, row, distance, false);
            return n != null ? W(n.Value, distance, false) : null;
        }

        internal static Coordinate? NW(Coordinate coord, int distance = 1, bool printLog = true) => Direction(NW, coord, distance, printLog);

        internal static Coordinate? SE(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "south east", column, row);
            return NW(column, row, -distance, false);
        }

        internal static Coordinate? SE(Coordinate coord, int distance = 1, bool printLog = true) => Direction(SE, coord, distance, printLog);

        internal static Coordinate? SW(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "south west", column, row);
            return NE(column, row, -distance, false);
        }

        internal static Coordinate? SW(Coordinate coord, int distance = 1, bool printLog = true) => Direction(SW, coord, distance, printLog);

        private void InitBoard()
        {
            BetterConsole.Action("Setup all pieces on the board");
            // Pawn
            for (int i = 0; i < 9; i++)
            {
                SetPiece(new Pawn(player2, this), i, 2);
                SetPiece(new Pawn(player1, this), i, 6);
            }
            // Bishop
            SetPiece(new Bishop(player2, this), 7, 1);
            SetPiece(new Bishop(player1, this), 1, 7);
            // Rook
            SetPiece(new Rook(player2, this), 1, 1);
            SetPiece(new Rook(player1, this), 7, 7);
            // Lance
            SetPiece(new Lance(player2, this), 0, 0);
            SetPiece(new Lance(player1, this), 0, 8);
            SetPiece(new Lance(player2, this), 8, 0);
            SetPiece(new Lance(player1, this), 8, 8);
            // Knight
            SetPiece(new Knight(player2, this), 1, 0);
            SetPiece(new Knight(player1, this), 1, 8);
            SetPiece(new Knight(player2, this), 7, 0);
            SetPiece(new Knight(player1, this), 7, 8);
            // Silver General
            SetPiece(new Silver(player2, this), 2, 0);
            SetPiece(new Silver(player1, this), 2, 8);
            SetPiece(new Silver(player2, this), 6, 0);
            SetPiece(new Silver(player1, this), 6, 8);
            // Gold General
            SetPiece(new Gold(player2, this), 3, 0);
            SetPiece(new Gold(player1, this), 3, 8);
            SetPiece(new Gold(player2, this), 5, 0);
            SetPiece(new Gold(player1, this), 5, 8);
            // King
            SetPiece(new King(player2, this), 4, 0);
            SetPiece(new King(player1, this), 4, 8);
        }

        private void InitCheckMate()
        {
            // player1
            SetPiece(new Gold(player1, this), 3, 8);
            SetPiece(new Gold(player1, this), 4, 7);
            SetPiece(new King(player1, this), 4, 8);
            // player2
            SetPiece(new Rook(player2, this), 5, 7);
            SetPiece(new Rook(player2, this), 5, 8);
            SetPiece(new Bishop(player2, this), 8, 2);
            SetPiece(new King(player2, this), 4, 0);
        }

        internal string ToHtml()
        {
            string text = "<div class=\"board\">\n";
            for (int i = 0; i < 9; i++)
            {
                string subText = "";
                for (int j = 0; j < 9; j++)
                    subText += SquareHtml(new Coordinate(j, i));
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
            // Stringbuilder nutzen
            string text = "<div id=\"log\" class=\"log\">";
            int length = log.Count;
            if (length <= 0)
                return text + "</div>";
            text += $"\n{log[0]}";
            for (int i = 1; i < length; i++)
                text += $"\n<br>{log[i]}";
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
            if (isOver)
                return;
            player.PrepareTurn();
        }

        internal string PromotionZone() => $"var promotionZone = {(isPlayer1Turn ? "['a', 'b', 'c']" : "['g', 'h', 'i']")}";

        internal string ForcePawnLancePromotion() => $"var pawnLancePromo = {(isPlayer1Turn ? "'a'" : "'i'")}";

        internal string ForceKnightPromotion() => $"var knightPromo = {(isPlayer1Turn ? "['a', 'b']" : "['h', 'i']")}";

        internal string GameEndHtml()
        {
            if (!isOver)
                return "";
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

        // more square stuff
        internal string? SquareHtml(Coordinate coordinate)
        {
            Piece? piece = PieceAt(coordinate);
            bool isPlayersTurn = piece != null && IsPlayersTurn(piece.player);
            bool notOver = !isOver;
            bool isPromotable = piece != null && piece.canPromote && !piece.IsPromoted();
            string promotable = isPlayersTurn && notOver && isPromotable ? " promotable" : "";
            string forcePromote = "";
            if (isPlayersTurn && notOver && isPromotable)
            {
                if (piece is Pawn or Lance)
                    forcePromote = " forcePromo1";
                else if (piece is Knight)
                    forcePromote = " forcePromo2";
            }
            string text = $"<div id=\"{coordinate.Row}{coordinate.Column}\" class=\"square{promotable}{forcePromote}\"";
            if (isPlayersTurn && notOver)
                text += $" onclick=\"selectMoves(\'{coordinate.Row}{coordinate.Column}\');\"";
            return text + $">\n{HtmlPieceImage(piece)}\n</div>";
        }

        private string HtmlPieceClass(Piece? piece) => piece == null ? "" : $"class=\"{(piece.player.isPlayer1 ? "black" : "white")}-piece\"";

        private string HtmlPieceImage(Piece? piece)
        {
            string class_ = HtmlPieceClass(piece);
            string src = $"src=\"data:image/png;base64,{Images.Get(piece)}\"";
            string alt = $"alt=\"{Names.Get(piece)}\"";
            return $"<img {class_} {src} {alt}>";
        }
    }
}
