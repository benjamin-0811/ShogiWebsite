using ShogiWebsite.Shogi.Pieces;

namespace ShogiWebsite.Shogi
{
    internal abstract class Piece
    {
        internal Player player;
        internal bool isPromoted;
        internal readonly bool canPromote;
        internal Board board;
        internal Coordinate coordinate;

        internal Piece(Player player, bool canPromote, Board board, Coordinate coordinate)
        {
            this.player = player;
            isPromoted = false;
            this.canPromote = canPromote;
            this.board = board;
            this.coordinate = coordinate;
        }

        /// <summary>Constructor for a piece on the board</summary>/// 
        internal Piece(Player player, bool canPromote, Board board, int column, int row) : this(player, canPromote, board, new Coordinate(column, row))
        { }

        internal Piece(Player player, bool canPromote, Board board) : this(player, canPromote, board, -1, -1)
        { }

        /// <summary>Piece on hand.<br/>Does not contain an actual square on the board</summary>
        /*
        internal Piece(Player player, bool canPromote, Board board)
        {
            this.player = player;
            isPromoted = false;
            this.canPromote = canPromote;
            this.board = board;
        }
        */

        /// <summary>
        /// Move this piece from one square to another.<br/>
        /// This piece might promote.<br/>
        /// This piece might capture one of the other player's pieces.<br/>
        /// </summary>
        /// <param name="to">Desitination square</param>
        /// <param name="doesPromote">Does this piece promote?</param>
        /// <returns><c>true</c> if this move was successful</returns>
        internal bool Move(Coordinate to, bool doesPromote)
        {
            var moves = FindMoves();
            string piece1S = IdentifyingString();
            string toS = Board.CoordinateString(coordinate);
            BetterConsole.Info($"Trying to move {piece1S} to {toS}.");
            if (moves != null && moves.Contains(to))
            {
                string piece2S = "";
                Coordinate old = coordinate;
                Piece? piece = board.PieceAt(to);
                char moveType = '-';
                if (piece != null && piece.player != player)
                {
                    piece2S = piece.IdentifyingString();
                    BetterConsole.Info($"There is {piece2S}.");
                    if (piece is not King)
                    {
                        player.ChangeHandPieceAmount(piece, 1);
                        moveType = 'x';
                    }
                }
                board.SetPiece(null, old);
                board.SetPiece(this, to);
                string part1 = $"{Names.Abbreviation(this)}{Board.CoordinateString(old)}";
                bool wasPromoted = isPromoted;
                ForcePromote();
                if (wasPromoted != isPromoted)
                    doesPromote = true;
                string part2 = $"{moveType}{Board.CoordinateString(to)}{(doesPromote ? "+" : "")}";
                board.log.Add($"{player.PlayerNumber()} : {part1}{part2}");
                if (doesPromote)
                    isPromoted = true;
                return true;
            }
            BetterConsole.Error("Illegal move!");
            return false;
        }

        /// <summary>
        /// Move this piece from the player's hand to <paramref name="to"/>.<br/>
        /// This move will be written to the game log.
        /// </summary>
        /// <param name="to">Destination square of this piece</param>
        /// <returns><c>true</c> if this move was successful</returns>
        internal bool MoveFromHand(Coordinate to)
        {
            var drops = FindDrops();
            Piece? piece = board.PieceAt(to);
            string pieceS = IdentifyingString();
            string toS = Board.CoordinateString(to);
            BetterConsole.Info($"Trying to drop {pieceS} on {toS}.");
            if (drops != null && drops.Contains(to) && piece == null && this is not King)
            {
                player.ChangeHandPieceAmount(this, -1);
                BetterConsole.Action($"Removed 1 {Names.Get(this)} from {player.PlayerNumber()}'s hand.");
                board.SetPiece(this, to);
                board.log.Add($"{player.PlayerNumber()} : {Names.Abbreviation(this)}*{Board.CoordinateString(to)}");
                BetterConsole.Action($"Dropped {pieceS} on {toS}.");
                return true;
            }
            BetterConsole.Error($"{pieceS} cannot perform this move!");
            return false;
        }

        /// <summary>Simulate Move And See If Own King Is Check</summary>
        internal bool DoesMoveCheckOwnKing(Coordinate to)
        {
            bool result = false;
            // Save old state
            Coordinate oldCoord = coordinate;
            Piece? oldToPiece = board.PieceAt(to);
            // Make new state
            board.SetPiece(this, to);
            board.SetPiece(null, oldCoord);
            // save result
            if (player.king.IsCheck())
                result = true;
            // restore old state
            board.SetPiece(oldToPiece, to);
            board.SetPiece(this, oldCoord);
            return result;
        }

        internal void Promote()
        {
            string pieceS = IdentifyingString();
            BetterConsole.Info($"Trying to promote {pieceS}.");
            if (canPromote)
            {
                isPromoted = true;
                BetterConsole.Action($"{pieceS} was promoted.");
            }
            else
                BetterConsole.Error($"{pieceS} cannot be promoted!");
        }

        internal string IdentifyingString() => $"{player.PlayerNumber()}'s {Names.Get(this)} on {Board.CoordinateString(coordinate)}";

        internal virtual void ForcePromote() { }

        internal bool IsPromoted() => canPromote && isPromoted;

        internal bool CanMoveTo(int column, int row)
        {
            BetterConsole.Info($"See if {IdentifyingString()} can move to {Board.CoordinateString(row, column)}.");
            IEnumerable<Coordinate> availableSquares = FindMoves();
            return availableSquares.Contains(new Coordinate(column, row));
        }

        internal bool CanMoveTo(Coordinate coord)
        {
            IEnumerable<Coordinate> availableSquares = FindMoves();
            return availableSquares.Contains(coord);
        }

        /// <summary>Find all squares this piece can currently move to.</summary>
        internal abstract IEnumerable<Coordinate> FindMoves();

        /// <summary>
        /// Find all squares where this piece can be dropped from the player's hand
        /// </summary>
        internal virtual IEnumerable<Coordinate> FindDrops()
        {
            BetterConsole.Info($"Trying to find squares to move to for {IdentifyingString()}");
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (board.pieces[i, j] == null)
                        yield return new Coordinate(i, j);
                }
            }
        }

        protected IEnumerable<Coordinate> GoldMoves() => ListMoves(new[] { Front(), FrontLeft(), FrontRight(), Left(), Right(), Back() });

        protected IEnumerable<Coordinate> RangeMoves(Func<int, int, int, bool, Coordinate?>[] directions)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                int distance = 1;
                bool flag = true;
                while (flag)
                {
                    Coordinate? temp = directions[i](coordinate.Column, coordinate.Row, distance, true);
                    if (temp == null)
                        break;
                    flag = CanContinue(temp);
                    if (IsAvailableSquare(temp.Value))
                        yield return temp.Value;
                    distance++;
                }
            }
        }

        /// <summary>Find lines of squares in multiple <paramref name="directions"/>.</summary>
        protected IEnumerable<Coordinate> RangeMoves(Func<Coordinate, int, bool, Coordinate?>[] directions)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                int distance = 1;
                bool flag = true;
                while (flag)
                {
                    Coordinate? temp = directions[i](coordinate, distance, true);
                    if (temp == null)
                        break;
                    flag = CanContinue(temp);
                    if (IsAvailableSquare(temp.Value))
                        yield return temp.Value;
                    distance++;
                }
            }
        }

        protected IEnumerable<Coordinate> ListMoves(Func<int, int, int, bool, Coordinate?>[] directions)
        {
            foreach (Func<int, int, int, bool, Coordinate?> direction in directions)
            {
                Coordinate? coord = GetSquareIfAvailable(direction);
                if (coord != null)
                    yield return coord.Value;
            }
        }

        /// <summary>Find one square in each of the given <paramref name="directions"/>.</summary>
        protected IEnumerable<Coordinate> ListMoves(Func<Coordinate, int, bool, Coordinate?>[] directions)
        {
            List<Coordinate?> sList = new();
            foreach (Func<Coordinate, int, bool, Coordinate?> direction in directions)
            {
                Coordinate? coord = GetSquareIfAvailable(direction);
                if (coord != null)
                    yield return coord.Value;
            }
        }

        /// <summary>
        /// Check if to continue a loop or if this piece can move further
        /// </summary>
        /// <param name="square">Current square in a loop</param>
        protected bool CanContinue(Coordinate? square)
        {
            if (square == null)
                return false;
            Piece? piece = board.PieceAt(square.Value);
            return piece == null;
        }

        /// <summary>
        /// Check if a square is available to move to.
        /// Does however not check if this piece can move there.
        /// </summary>
        /// <param name="square">Square to check</param>
        protected bool IsAvailableSquare(Coordinate square)
        {
            Piece? piece = board.PieceAt(square);
            return piece == null || piece.player != player;
        }

        protected virtual Coordinate? GetSquareIfAvailable(Func<int, int, int, bool, Coordinate?> func, int distance = 1)
        {
            Coordinate? square = func(coordinate.Column, coordinate.Row, distance, true);
            return square != null && IsAvailableSquare(square.Value) ? square : null;
        }

        /// <summary>Checks if a square in a set distance is available</summary>
        /// <param name="func">Function to get a square in a certain direction</param>
        /// <param name="n">Distance to get a square in a certain direction</param>
        /// <returns>
        /// <paramref name="func"/>(<paramref name="n"/>) if it is available,<br/>
        /// <c>null</c> if it's not
        /// </returns>
        protected virtual Coordinate? GetSquareIfAvailable(Func<Coordinate, int, bool, Coordinate?> func, int distance = 1)
        {
            Coordinate? square = func(coordinate, distance, true);
            return square != null && IsAvailableSquare(square.Value) ? square : null;
        }

        internal bool DifferentPlayer(Coordinate coords)
        {
            Piece? piece = board.PieceAt(coords);
            return piece == null || DifferentPlayer(piece);
        }

        internal bool DifferentPlayer(Piece piece) => piece.player != player;

        internal Func<Coordinate, int, bool, Coordinate?> Front() => player.isPlayer1 ? Board.N : Board.S;

        internal Func<Coordinate, int, bool, Coordinate?> Back() => player.isPlayer1 ? Board.S : Board.N;

        internal Func<Coordinate, int, bool, Coordinate?> Left() => player.isPlayer1 ? Board.W : Board.E;

        internal Func<Coordinate, int, bool, Coordinate?> Right() => player.isPlayer1 ? Board.E : Board.W;

        internal Func<Coordinate, int, bool, Coordinate?> FrontLeft() => player.isPlayer1 ? Board.NW : Board.SE;

        internal Func<Coordinate, int, bool, Coordinate?> FrontRight() => player.isPlayer1 ? Board.NE : Board.SW;

        internal Func<Coordinate, int, bool, Coordinate?> BackLeft() => player.isPlayer1 ? Board.SW : Board.NE;

        internal Func<Coordinate, int, bool, Coordinate?> BackRight() => player.isPlayer1 ? Board.SE : Board.NW;

        internal Coordinate? Knight(bool left)
        {
            BetterConsole.Info($"Looking one knight's move away to the {(left ? "left" : "right")} of {IdentifyingString()}");
            Coordinate? front = Front()(coordinate, 2, false);
            Func<Coordinate, int, bool, Coordinate?> side = left ? Left() : Right();
            return front != null ? side(front.Value, 1, false) : null;
        }

        internal string CoordinateString() => Board.CoordinateString(coordinate);
    }
}
