namespace ShogiWebsite.Shogi.Pieces
{
    internal class King : Piece
    {
        /// <summary>King on the board</summary>
        internal King(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal King(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        /// <summary>null King<br/>Does not contain an actual square on the board</summary>
        internal King(Player player, Board board) : base(player, false, board) { }

        internal override IEnumerable<Coordinate> FindMoves() => ListMoves(new Func<Coordinate, int, bool, Coordinate?>[] { board.N, board.NE, board.E, board.SE, board.S, board.SW, board.W, board.NW });

        internal bool WouldBeCheckAt(Coordinate at)
        {
            bool result = false;
            // Save old state
            Coordinate kingSquare = coordinate;
            Piece? atPiece = board.PieceAt(at);
            // Create new state
            board.SetPiece(this, at);
            board.SetPiece(atPiece, kingSquare);
            // See if king would be in check
            foreach(Piece opponentPieces in player.Opponent().PlayersPieces())
            {
                IEnumerable<Coordinate> moves = opponentPieces.FindMoves();
                if (moves.Contains(at))
                {
                    result = true;
                    break;
                }
            }
            // Return to old state
            board.SetPiece(this, kingSquare);
            board.SetPiece(atPiece, at);

            return result;
        }

        internal bool IsCheck() => WouldBeCheckAt(coordinate);

        // Only call when certain the king is check
        internal bool IsCheckmate()
        {
            bool f1 = FindMoves().Any();
            IEnumerable<Piece> potentialPieces = PiecesThatCheckThisKing();
            if (potentialPieces.Count() != 1)
                return potentialPieces.Count() > 1;
            Piece piece = potentialPieces.First();
            IEnumerable<Piece> capturers = GetCapturers(piece);
            bool f2 = capturers.Any();
            bool f3 = CanBlockAttacker(piece);
            return !(f1 || f2 || f3);
        }

        private IEnumerable<Piece> GetCapturers(Piece attacker)
        {
            Coordinate square = attacker.coordinate;
            IEnumerable<Piece> ownPieces = player.PlayersPieces();
            foreach (Piece capturer in ownPieces)
            {
                IEnumerable<Coordinate> availableSquares = capturer.FindMoves();
                if (capturer == this && !WouldBeCheckAt(square) || availableSquares.Contains(square))
                    yield return capturer;
            }
        }

        /// <summary>
        /// Find out if the player can prevent a checkmate
        /// by dropping (if <paramref name="doDrop"/>) or moving a piece.
        /// </summary>
        /// <param name="squaresInbetween">squares between the king and the attacker</param>
        /// <param name="doDrop"><c>true</c>: drop a piece<br/><c>false</c>: move a piece</param>
        private bool CanBlockAttacker(IEnumerable<Coordinate> squaresInbetween, bool doDrop)
        {
            bool canBlock = false;
            Dictionary<string, List<Coordinate>> lists = doDrop ? player.dropLists : player.moveLists;
            var protect = doDrop ? protectDrops : protectMoves;
            int length = squaresInbetween.Count();
            foreach (KeyValuePair<string, List<Coordinate>> list in lists)
            {
                List<Coordinate> squares = list.Value;
                for (int i = 0; i < length; i++)
                {
                    Coordinate square = squaresInbetween.ElementAt(i);
                    Coordinate? orig = board.GetSquareByCoordinate(list.Key);
                    if (orig != null && board.PieceAt(orig.Value) == this && !DoesMoveCheckOwnKing(square) || squares.Contains(square))
                    {
                        string coord = list.Key;
                        if (protect.ContainsKey(coord))
                            protect[coord].Add(square);
                        else
                            protect.Add(coord, new List<Coordinate>() { square });
                        canBlock = true;
                    }
                }
            }
            return canBlock;
        }

        private Dictionary<Piece, IEnumerator<Coordinate>> BlockAttackerMoves(Piece attacker)
        {

        }

        private bool CanBlockAttacker(Piece piece)
        {
            IEnumerable<Coordinate> squaresInbetween = SquaresBetweenKingAndRangedPiece(piece);
            if (!squaresInbetween.Any())
                return false;
            bool f1 = CanBlockAttacker(squaresInbetween, true);
            bool f2 = CanBlockAttacker(squaresInbetween, false);
            return f1 || f2;
        }

        private IEnumerable<Coordinate> SquaresBetweenKingAndRangedPiece(Piece piece)
        {
            List<Coordinate> squares = SquaresBetweenKingAndBishop(piece).ToList();
            squares.AddRange(SquaresBetweenKingAndRook(piece));
            return squares.AsEnumerable();
        }

        private IEnumerable<Coordinate> SquaresBetweenKingAndBishop(Piece piece)
        {
            int x1 = coordinate.Column;
            int y1 = coordinate.Row;
            int x2 = piece.coordinate.Column;
            int y2 = piece.coordinate.Row;
            int dx = Math.Abs(x1 - x2);
            int dy = Math.Abs(y1 - y2);
            if (dx <= 1 && dy <= 1)
                return new List<Coordinate>();
            List<Coordinate> squaresInbetween = new();
            if (dx == dy && dx > 1)
            {
                bool minusX = x1 > x2;
                bool minusY = y1 > y2;
                for (int i = 1; i < dx; i++)
                    squaresInbetween.Add(new Coordinate(minusX ? x1 - i : x1 + i, minusY ? y1 - i : y1 + i));
            }
            return squaresInbetween;
        }

        private IEnumerable<Coordinate> SquaresBetweenKingAndRook(Piece piece)
        {
            int x1 = coordinate.Column;
            int y1 = coordinate.Row;
            int x2 = piece.coordinate.Column;
            int y2 = piece.coordinate.Row;
            int dx = Math.Abs(x1 - x2);
            int dy = Math.Abs(y1 - y2);
            if (dx <= 1 && dy <= 1)
                return new List<Coordinate>();
            List<Coordinate> squaresInbetween = new();
            // vertical
            if (dx == 0 && dy > 1)
            {
                int yMin = Math.Min(y1, y2);
                for (int i = 1; i < dy; i++)
                    squaresInbetween.Add(new Coordinate(x1, yMin + i));
            }
            // horizontal
            else if (dx > 1 && dy == 0)
            {
                int xMin = Math.Min(x1, x2);
                for (int i = 1; i < dx; i++)
                    squaresInbetween.Add(new Coordinate(x1, xMin + i));
            }
            return squaresInbetween;
        }

        internal IEnumerable<Piece> PiecesThatCheckThisKing()
        {
            IEnumerable<Piece> potentialPieces = player.Opponent().PlayersPieces();
            foreach (Piece piece in potentialPieces)
            {
                IEnumerable<Coordinate> availableSquares = piece.FindMoves();
                if (availableSquares.Contains(coordinate))
                    yield return piece;
            }
        }
    }
}
