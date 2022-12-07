namespace ShogiWebsite.Shogi.Pieces
{
    internal class King : Piece
    {
        /// <summary>King on the board</summary>
        internal King(Player player, Square square) : base(player, false, square) { }

        /// <summary>null King<br/>Does not contain an actual square on the board</summary>
        internal King(Player player, Board board) : base(player, false, board) { }

        internal override IEnumerable<Square> FindMoves() => ListMoves(new[] { square.North, square.NorthEast, square.East, square.SouthEast, square.South, square.SouthWest, square.West, square.NorthWest });

        internal bool WouldBeCheckAt(Square at)
        {
            bool result = false;
            // Save old state
            Square kingSquare = square;
            Piece? atPiece = at.piece;
            // Create new state
            at.piece = this;
            square = at;
            kingSquare.piece = atPiece;
            if(atPiece != null) atPiece.square = kingSquare;
            // See if king would be in check
            foreach(var opponentMoves in player.Opponent().GetMoveLists())
            {
                if (opponentMoves.Value.Contains(at)) result = true;
            }
            // Return to old state
            square = kingSquare;
            kingSquare.piece = this;

            return result;
        }

        internal bool IsCheck() => WouldBeCheckAt(square);

        // Only call when certain the king is check
        internal bool IsCheckmate()
        {
            bool f1 = FindMoves().Any();
            Piece[] potentialPieces = PiecesThatCheckThisKing();
            if (potentialPieces.Length <= 0) return false;
            if (potentialPieces.Length > 1) return true;
            Piece piece = potentialPieces[0];
            bool f2 = CanCaptureAttacker(piece);
            bool f3 = CanBlockAttacker(piece);
            if (f1 || f2 || f3) return false;
            return true;
        }

        private bool CanCaptureAttacker(Piece piece)
        {
            Square square = piece.square;
            IEnumerable<Piece> ownPieces = player.boardPieces;
            List<Piece> piecesToCaptureAttacker = new();
            foreach (var capturer in ownPieces)
            {
                IEnumerable<Square> availableSquares = capturer.FindMoves();
                if (capturer == this && !DoesMoveCheckOwnKing(square) || availableSquares.Contains(square) )
                {
                    piecesToCaptureAttacker.Add(capturer);
                    string coord = capturer.CoordinateString();
                    if (protectMoves.ContainsKey(coord)) protectMoves[coord].Add(square);
                    else protectMoves.Add(coord, new List<Square>() { square });
                }
            }
            if (piecesToCaptureAttacker.Any()) return true;
            return false;
        }

        /// <summary>
        /// Find out if the player can prevent a checkmate
        /// by dropping (if <paramref name="doDrop"/>) or moving a piece.
        /// </summary>
        /// <param name="squaresInbetween">squares between the king and the attacker</param>
        /// <param name="doDrop"><c>true</c>: drop a piece<br/><c>false</c>: move a piece</param>
        private bool CanBlockAttacker(IEnumerable<Square> squaresInbetween, bool doDrop)
        {
            bool canBlock = false;
            var lists = doDrop ? player.dropLists : player.moveLists;
            var protect = doDrop ? protectDrops : protectMoves;
            int length = squaresInbetween.Count();
            foreach (var list in lists)
            {
                var squares = list.Value;
                for (int i = 0; i < length; i++)
                {
                    Square square = squaresInbetween.ElementAt(i);
                    Square? orig = board.GetSquareByCoordinate(list.Key);
                    if (orig != null && orig.piece == this && !DoesMoveCheckOwnKing(square) || squares.Contains(square))
                    {
                        string coord = list.Key;
                        if (protect.ContainsKey(coord)) protect[coord].Add(square);
                        else protect.Add(coord, new List<Square>() { square });
                        canBlock = true;
                    }
                }
            }
            return canBlock;
        }

        private bool CanBlockAttacker(Piece piece)
        {
            IEnumerable<Square> squaresInbetween = SquaresBetweenKingAndRangedPiece(piece);
            if (!squaresInbetween.Any()) return false;
            bool f1 = CanBlockAttacker(squaresInbetween, true);
            bool f2 = CanBlockAttacker(squaresInbetween, false);
            return f1 || f2;
        }

        private IEnumerable<Square> SquaresBetweenKingAndRangedPiece(Piece piece)
        {
            List<Square> squares = SquaresBetweenKingAndBishop(piece).ToList();
            squares.AddRange(SquaresBetweenKingAndRook(piece));
            return squares.AsEnumerable();
        }

        private IEnumerable<Square> SquaresBetweenKingAndBishop(Piece piece)
        {
            int x1 = square.colIndex;
            int y1 = square.rowIndex;
            int x2 = piece.square.colIndex;
            int y2 = piece.square.rowIndex;
            int dx = Math.Abs(x1 - x2);
            int dy = Math.Abs(y1 - y2);
            if (dx <= 1 && dy <= 1) return new List<Square>();
            List<Square> squaresInbetween = new();
            if (dx == dy && dx > 1)
            {
                bool minusX = x1 > x2;
                bool minusY = y1 > y2;
                for (int i = 1; i < dx; i++) squaresInbetween.Add(board.squares[minusX ? x1 - i : x1 + i, minusY ? y1 - i : y1 + i]);
            }
            return squaresInbetween;
        }

        private IEnumerable<Square> SquaresBetweenKingAndRook(Piece piece)
        {
            int x1 = square.colIndex;
            int y1 = square.rowIndex;
            int x2 = piece.square.colIndex;
            int y2 = piece.square.rowIndex;
            int dx = Math.Abs(x1 - x2);
            int dy = Math.Abs(y1 - y2);
            if (dx <= 1 && dy <= 1) return new List<Square>();
            List<Square> squaresInbetween = new();
            // vertical
            if (dx == 0 && dy > 1)
            {
                int yMin = Math.Min(y1, y2);
                for (int i = 1; i < dy; i++) squaresInbetween.Add(board.squares[x1, yMin + i]);
            }
            // horizontal
            else if (dx > 1 && dy == 0)
            {
                int xMin = Math.Min(x1, x2);
                for (int i = 1; i < dx; i++) squaresInbetween.Add(board.squares[x1, xMin + i]);
            }
            return squaresInbetween;
        }

        internal Piece[] PiecesThatCheckThisKing()
        {
            var potentialPieces = player.Opponent().boardPieces;
            var pieces = new List<Piece>();
            foreach (Piece piece in potentialPieces)
            {
                var availableSquares = piece.FindMoves();
                if (availableSquares.Contains(square)) pieces.Add(piece);
            }
            return pieces.ToArray();
        }
    }
}
