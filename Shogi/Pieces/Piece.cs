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
            IEnumerable<Coordinate> moves = FindMoves();
            string piece1S = IdentifyingString();
            string toS = board.CoordinateString(coordinate);
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
                string part1 = $"{Names.Abbreviation(this)}{board.CoordinateString(old)}";
                bool wasPromoted = isPromoted;
                ForcePromote();
                if (wasPromoted != isPromoted)
                    doesPromote = true;
                string part2 = $"{moveType}{board.CoordinateString(to)}{(doesPromote ? "+" : "")}";
                board.Log($"{player.PlayerNumber()} : {part1}{part2}");
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
            IEnumerable<Coordinate> drops = FindDrops();
            Piece? piece = board.PieceAt(to);
            string pieceS = IdentifyingString();
            string toS = board.CoordinateString(to);
            BetterConsole.Info($"Trying to drop {pieceS} on {toS}.");
            if (drops != null && drops.Contains(to) && piece == null && this is not King)
            {
                player.ChangeHandPieceAmount(this, -1);
                BetterConsole.Action($"Removed 1 {Names.Get(this)} from {player.PlayerNumber()}'s hand.");
                board.SetPiece(this, to);
                board.Log($"{player.PlayerNumber()} : {Names.Abbreviation(this)}*{board.CoordinateString(to)}");
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

        internal string IdentifyingString() => $"{player.PlayerNumber()}'s {Names.Get(this)} on {board.CoordinateString(coordinate)}";

        internal virtual void ForcePromote() { }

        internal void ForcePromote(int rows)
        {
            int row = coordinate.Row;
            if (!isPromoted && (player.isPlayer1 ? row < rows : row >= board.height - rows))
                Promote();
        }
        internal bool IsPromoted() => canPromote && isPromoted;

        internal bool CanMoveTo(int column, int row)
        {
            BetterConsole.Info($"See if {IdentifyingString()} can move to {board.CoordinateString(row, column)}.");
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
        internal virtual IEnumerable<Coordinate> FindDrops() => FindDrops(0);

        internal IEnumerable<Coordinate> FindDrops(int freeRows)
        {
            int min = MinDropRow(freeRows);
            int max = MaxDropRow(freeRows);
            for (int i = min; i <= max; i++)
            {
                for (int j = 0; j < board.width; j++)
                {
                    Coordinate coord = new(i, j);
                    if (board.PieceAt(coord) == null)
                        yield return coord;
                }
            }
        }

        internal int MinDropRow(int freeRows) => player.isPlayer1 ? freeRows : 0;

        internal int MaxDropRow(int freeRows) => board.height - (player.isPlayer1 ? 0 : freeRows) - 1;

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

        internal Func<Coordinate, int, bool, Coordinate?> Front() => player.isPlayer1 ? board.N : board.S;

        internal Func<Coordinate, int, bool, Coordinate?> Back() => player.isPlayer1 ? board.S : board.N;

        internal Func<Coordinate, int, bool, Coordinate?> Left() => player.isPlayer1 ? board.W : board.E;

        internal Func<Coordinate, int, bool, Coordinate?> Right() => player.isPlayer1 ? board.E : board.W;

        internal Func<Coordinate, int, bool, Coordinate?> FrontLeft() => player.isPlayer1 ? board.NW : board.SE;

        internal Func<Coordinate, int, bool, Coordinate?> FrontRight() => player.isPlayer1 ? board.NE : board.SW;

        internal Func<Coordinate, int, bool, Coordinate?> BackLeft() => player.isPlayer1 ? board.SW : board.NE;

        internal Func<Coordinate, int, bool, Coordinate?> BackRight() => player.isPlayer1 ? board.SE : board.NW;

        internal Coordinate? Knight(bool left)
        {
            BetterConsole.Info($"Looking one knight's move away to the {(left ? "left" : "right")} of {IdentifyingString()}");
            Coordinate? front = Front()(coordinate, 2, false);
            Func<Coordinate, int, bool, Coordinate?> side = left ? Left() : Right();
            return front != null ? side(front.Value, 1, false) : null;
        }

        internal string CoordinateString() => board.CoordinateString(coordinate);
    }
}
