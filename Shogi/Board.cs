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
        internal readonly int width;
        internal readonly int height;
        internal Piece?[,] pieces;
        internal bool isPlayer1Turn;
        internal Player player1;
        internal Player player2;
        internal HtmlBuilder log;
        internal bool isOver;
        internal Player? winner;
        internal Phase phase;

        internal readonly string[] columns;
        internal readonly string[] rows;

        internal Board()
        {
            width = 9;
            height = 9;
            columns = Columns().ToArray();
            rows = Rows().ToArray();
            // Square [ column , row ]
            pieces = new Piece?[9, 9];
            player1 = new Player(this, true);
            player2 = new Player(this, false);
            InitBoard();
            //InitCheckMate();
            isPlayer1Turn = true;
            log = new HtmlBuilder().Id("log").Class("log");
            isOver = false;
            winner = null;
            phase = Phase.ChoosePiece;
            player1.InitLater();
            player2.InitLater();
        }

        internal void Log(string newEntry) => log.Child(new HtmlBuilder("p").Child(newEntry));

        private IEnumerable<string> Columns()
        {
            for (int i = 0; i < width; i++)
                yield return $"{width - i}";
        }

        private IEnumerable<string> Rows()
        {
            for (int i = 0; i < height; i++)
                yield return $"{(char)('a' + i)}";
        }

        internal void SetPiece(Piece? piece, int column, int row)
        {
            if (!IsOnBoard(column, row))
                return;
            pieces[column, row] = piece;
            if (piece != null)
                piece.coordinate = new Coordinate(column, row);
        }

        internal void SetPiece(Piece? piece, Coordinate coords) => SetPiece(piece, coords.Column, coords.Row);

        internal string CoordinateString(int row, int column) => IsOnBoard(column, row) ? $"{rows[row]}{columns[column]}" : "hand";

        internal string CoordinateString(Coordinate coordinate) => CoordinateString(coordinate.Column, coordinate.Row);

        internal bool IsOnBoard(int column, int row) => 0 <= column && column < width && 0 <= row && row < height;

        internal bool IsOnBoard(Coordinate coordinate) => IsOnBoard(coordinate.Column, coordinate.Row);

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

        private void DirectionLookMessage(int distance, string direction, int column, int row)
        {
            BetterConsole.Info($"Looking {distance} squares {direction} of {CoordinateString(row, column)}");
        }

        internal Coordinate? Direction(Func<int, int, int, bool, Coordinate?> function, Coordinate coord, int distance = 1, bool printLog = true)
        {
            return function(coord.Column, coord.Row, distance, printLog);
        }

        internal Coordinate? N(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "north", column, row);
            int newRow = row - distance;
            return IsOnBoard(column, newRow) ? new Coordinate(column, newRow) : null;
        }

        internal Coordinate? N(Coordinate coord, int distance = 1, bool printLog = true) => Direction(N, coord, distance, printLog);

        internal Coordinate? S(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "south", column, row);
            return N(column, row, -distance, false);
        }

        internal Coordinate? S(Coordinate coord, int distance = 1, bool printLog = true) => Direction(S, coord, distance, printLog);

        internal Coordinate? E(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "east", column, row);
            int newColumn = column + distance;
            return IsOnBoard(newColumn, row) ? new Coordinate(newColumn, row) : null;
        }

        internal Coordinate? E(Coordinate coord, int distance = 1, bool printLog = true) => Direction(E, coord, distance, printLog);

        internal Coordinate? W(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "west", column, row);
            return E(column, row, -distance, false);
        }

        internal Coordinate? W(Coordinate coord, int distance = 1, bool printLog = true) => Direction(W, coord, distance, printLog);

        internal Coordinate? NE(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "north east", column, row);
            Coordinate? n = N(column, row, distance, false);
            return n != null ? E(n.Value, distance, false) : null;
        }

        internal Coordinate? NE(Coordinate coord, int distance = 1, bool printLog = true) => Direction(NE, coord, distance, printLog);

        internal Coordinate? NW(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "north west", column, row);
            Coordinate? n = N(column, row, distance, false);
            return n != null ? W(n.Value, distance, false) : null;
        }

        internal Coordinate? NW(Coordinate coord, int distance = 1, bool printLog = true) => Direction(NW, coord, distance, printLog);

        internal Coordinate? SE(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "south east", column, row);
            return NW(column, row, -distance, false);
        }

        internal Coordinate? SE(Coordinate coord, int distance = 1, bool printLog = true) => Direction(SE, coord, distance, printLog);

        internal Coordinate? SW(int column, int row, int distance = 1, bool printLog = true)
        {
            if (printLog)
                DirectionLookMessage(distance, "south west", column, row);
            return NE(column, row, -distance, false);
        }

        internal Coordinate? SW(Coordinate coord, int distance = 1, bool printLog = true) => Direction(SW, coord, distance, printLog);

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

        internal HtmlBuilder LogToHtml()
        {
            return log;
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

        internal HtmlBuilder GameEndHtml()
        {
            HtmlBuilder builder = new();
            if (!isOver)
                return builder;
            HtmlBuilder button = new HtmlBuilder().Class("button").Property("onclick", "restart").Child("New Game");
            HtmlBuilder message = new HtmlBuilder("p").Child($"The Winner is: Player {(winner == player1 ? "1" : "2")!}");
            HtmlBuilder box = new HtmlBuilder().Class("overlay-box").Child(button).Child(message);
            return builder.Id("game-end-overlay").Child(box);
        }

        // more square stuff
        internal HtmlBuilder SquareHtml(Coordinate coordinate)
        {
            string coordString = CoordinateString(coordinate);
            HtmlBuilder builder = new HtmlBuilder().Id(coordString).Class("square");
            Piece? piece = PieceAt(coordinate);
            if (piece != null && IsPlayersTurn(piece.player) && !isOver)
            {
                if (piece.canPromote && !piece.IsPromoted())
                {
                    builder.Class("promotable");
                    if (piece is Pawn or Lance)
                        builder.Class("forcePromo1");
                    else if (piece is Knight)
                        builder.Class("forcePromo2");
                }
                builder.Property("onclick", coordString);
            }
            return builder.Child(HtmlPieceImage(piece));
        }

        private HtmlBuilder HtmlPieceImage(Piece? piece)
        {
            HtmlBuilder builder = new("img");
            if (piece != null)
                builder.Class($"{(piece.player.isPlayer1 ? "black" : "white")}-piece");
            builder.Property("src", $"data:image/png;base64,{Images.Get(piece)}");
            builder.Property("alt", Names.Get(piece));
            return builder;
        }
    }
}
