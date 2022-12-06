using ShogiWebsite.Shogi.Pieces;

namespace ShogiWebsite.Shogi
{
    internal class Square
    {
        internal readonly string column;
        internal readonly int colIndex;
        internal readonly string row;
        internal readonly int rowIndex;
        internal Piece? piece;
        internal readonly Board board;

        internal static readonly string[] columns = { "9", "8", "7", "6", "5", "4", "3", "2", "1" };
        internal static readonly string[] rows = { "a", "b", "c", "d", "e", "f", "g", "h", "i" };

        internal Square(int column, int row, Piece? piece, Board board)
        {
            colIndex = column;
            this.column = columns[colIndex];
            rowIndex = row;
            this.row = rows[rowIndex];
            this.piece = piece;
            this.board = board;
        }

        /// <summary>null-square<br/>no column, row or piece</summary>
        internal Square(Board board)
        {
            column = row = "null";
            colIndex = rowIndex = -1;
            piece = null;
            this.board = board;
        }

        internal string CoordinateString()
        {
            if (this == board.nullSquare) return "hand";
            return $"{rows[rowIndex]}{columns[colIndex]}";
        }

        internal static bool IsOnBoard(int index) => 0 <= index && index < 9;

        internal Square? SquareAt(int column, int row) => IsOnBoard(column) && IsOnBoard(row) ? board.squares[column, row] : null;

        internal Square? North(int distance = 1, bool printLog = true)
        {
            if (printLog) BetterConsole.Info($"Looking {distance} squares north of {CoordinateString()}");
            int newRowIndex = rowIndex - distance;
            return SquareAt(colIndex, newRowIndex);
        }

        internal Square? South(int distance = 1, bool printLog = true)
        {
            if (printLog) BetterConsole.Info($"Looking {distance} squares south of {CoordinateString()}");
            int newRowIndex = rowIndex + distance;
            return SquareAt(colIndex, newRowIndex);
        }

        internal Square? East(int distance = 1, bool printLog = true)
        {
            if (printLog) BetterConsole.Info($"Looking {distance} squares east of {CoordinateString()}");
            int newColIndex = colIndex + distance;
            return SquareAt(newColIndex, rowIndex);
        }

        internal Square? West(int distance = 1, bool printLog = true)
        {
            if (printLog) BetterConsole.Info($"Looking {distance} squares west of {CoordinateString()}");
            int newColIndex = colIndex - distance; ;
            return SquareAt(newColIndex, rowIndex);
        }

        internal Square? NorthEast(int distance = 1, bool printLog = true)
        {
            if (printLog) BetterConsole.Info($"Looking {distance} squares north east of {CoordinateString()}");
            Square? north = North(distance, false);
            if (north == null) return null;
            return north.East(distance, false);
        }

        internal Square? NorthWest(int distance = 1, bool printLog = true)
        {
            if (printLog) BetterConsole.Info($"Looking {distance} squares north west of {CoordinateString()}");
            Square? north = North(distance, false);
            if (north == null) return null;
            return north.West(distance, false);
        }

        internal Square? SouthEast(int distance = 1, bool printLog = true)
        {
            if (printLog) BetterConsole.Info($"Looking {distance} squares south east of {CoordinateString()}");
            Square? south = South(distance, false);
            if (south == null) return null;
            return south.East(distance, false);
        }

        internal Square? SouthWest(int distance = 1, bool printLog = true)
        {
            if (printLog) BetterConsole.Info($"Looking {distance} squares north west of {CoordinateString()}.");
            Square? south = South(distance, false);
            if (south == null) return null;
            return south.West(distance, false);
        }

        internal Square? KnightMove(bool isPlayer1, bool left)
        {
            string vert = isPlayer1 ? "north" : "south";
            string hori = left ? (isPlayer1 ? "west" : "east") : (isPlayer1 ? "east" : "west");
            BetterConsole.Info($"Looking one knight's move {vert} {hori} of {CoordinateString()}.");
            Square? front = isPlayer1 ? North(2, false) : South(2, false);
            if (front == null) return null;
            if (left) return isPlayer1 ? front.West(1, false) : front.East(1, false);
            return isPlayer1 ? front.East(1, false) : front.West(1, false);
        }

        internal Square? KnightMove(Player player, bool left) => KnightMove(player.isPlayer1, left);

        internal string? ToHtml(bool isOver)
        {
            bool f1 = piece != null && board.isPlayer1Turn == piece.player.isPlayer1;
            bool f2 = !isOver;
            bool f3 = piece != null && piece.canPromote && !piece.isPromoted;
            string promotable = f1 && f2 && f3 ? " promotable" : "";
            string forcePromote = "";
            if (f1 && f2 && f3)
            {
                if (piece is Pawn or Lance) forcePromote = " forcePromo1";
                else if (piece is Knight) forcePromote = " forcePromo2";
            }
            string text = $"<div id=\"{row}{column}\" class=\"square{promotable}{forcePromote}\"";
            if (f1 && f2) text += $" onclick=\"selectMoves(\'{row}{column}\');\"";
            return text + $">\n{HtmlPieceImage()}\n</div>";
        }

        private string HtmlPieceClass()
        {
            if (piece == null) return "";
            string color = piece.player.isPlayer1 ? "black" : "white";
            return $"class=\"{color}-piece\"";
        }

        private string HtmlPieceImage()
        {
            string class_ = HtmlPieceClass();
            string src = $"src=\"data:image/png;base64,{Images.Get(piece)}\"";
            string alt = $"alt=\"{Names.Get(piece)}\"";
            return $"<img {class_} {src} {alt}>";
        }
    }
}
