namespace ShogiWebsite.Shogi.Pieces
{
    internal class King : Piece
    {
        internal Dictionary<string, List<Square>> protectMoves;
        internal Dictionary<string, List<Square>> protectDrops;

        /// <summary>King on the board</summary>
        internal King(Player player, Square square) : base(player, false, square)
        {
            protectMoves = new();
            protectDrops = new();
        }

        /// <summary>null King<br/>Does not contain an actual square on the board</summary>
        internal King(Player player, Board board) : base(player, false, board)
        {
            protectMoves = new();
            protectDrops = new();
        }

        internal void SetMoves(List<Square> moves)
        {
            this.moves = moves;
        }

        internal override List<Square> FindMoves()
        {
            return ListMoves(new Func<int, bool, Square?>[] { square.North, square.NorthEast, square.East, square.SouthEast, square.South, square.SouthWest, square.West, square.NorthWest });
        }

        internal bool WouldBeCheckAt(Square at)
        {
            Square? nLeft = at.KnightMove(player.isPlayer1, true);
            if (nLeft != null && nLeft.piece is Knight && DifferentPlayerAt(nLeft)) return true;
            Square? nRight = at.KnightMove(player.isPlayer1, false);
            if (nRight != null && nRight.piece is Knight && DifferentPlayerAt(nRight)) return true;
            List<Square?> squares = new() { at.North(), at.NorthEast(), at.East(), at.SouthEast(), at.South(), at.SouthWest(), at.West(), at.NorthWest() }; //GetMoves();
            foreach (Square? square in squares)
            {
                if (square == null) continue;
                Piece? piece = square.piece;
                if (piece != null && piece.player != player)
                {
                    List<Square> availabeSquares = piece.GetMoves();
                    if (availabeSquares.Contains(at)) return true;
                }
            }
            if (WouldBeCheckedBy<Bishop>(at)) return true;
            if (WouldBeCheckedBy<Rook>(at)) return true;
            if (WouldBeCheckedBy<Lance>(at)) return true;
            return false;
        }

        private bool WouldBeCheckedBy<T>(Square at) where T : Piece
        {
            List<T> allPieces = player.Opponent().AllPiecesOfType<T>();
            square.piece = null;
            foreach (Piece piece in allPieces)
            {
                piece.UpdateMoves();
                List<Square> availableSquares = piece.GetMoves();
                if (availableSquares.Contains(at))
                {
                    square.piece = this;
                    piece.UpdateMoves();
                    return true;
                }
            }
            square.piece = this;
            return false;
        }

        internal bool IsCheck() => WouldBeCheckAt(square);

        /// <summary>Simulate Move And See If Check</summary>
        internal bool Smasic(Square to)
        {
            bool result = false;
            // Save old state
            Square oldKingSquare = square;
            Piece? oldToPiece = to.piece;
            // Make new state
            to.piece = this;
            square = to;
            oldKingSquare.piece = null;
            UpdateMoves();
            player.Opponent().UpdateMoves();
            // save result
            if (IsCheck()) result = true;
            // restore old state
            to.piece = oldToPiece;
            if (oldToPiece != null) oldToPiece.square = to;
            oldKingSquare.piece = this;
            square = oldKingSquare;
            UpdateMoves();
            player.Opponent().UpdateMoves();
            return result;
        }

        // Only call when certain the king is check
        internal bool IsCheckmate()
        {
            bool f1 = GetMoves().Any();
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
            List<Piece> ownPieces = player.boardPieces;
            List<Piece> piecesToCaptureAttacker = new();
            foreach (Piece capturer in ownPieces)
            {
                List<Square> availableSquares = capturer.GetMoves();
                if (capturer == this)
                {
                    if (!Smasic(square))
                    {
                        if (availableSquares.Contains(square))
                        {
                            piecesToCaptureAttacker.Add(capturer);
                            string coord = capturer.CoordinateString();
                            if (protectMoves.ContainsKey(coord)) protectMoves[coord].Add(square);
                            else protectMoves.Add(coord, new List<Square>() { square });
                        }
                    }
                }
                else
                {
                    if (availableSquares.Contains(square))
                    {
                        piecesToCaptureAttacker.Add(capturer);
                        string coord = capturer.CoordinateString();
                        if (protectMoves.ContainsKey(coord)) protectMoves[coord].Add(square);
                        else protectMoves.Add(coord, new List<Square>() { square });
                    }
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
        private bool CanBlockAttacker(List<Square> squaresInbetween, bool doDrop)
        {
            bool canBlock = false;
            Dictionary<string, List<Square>> lists = doDrop ? player.dropLists : player.moveLists;
            Dictionary<string, List<Square>> protect = doDrop ? protectDrops : protectMoves;
            int length = squaresInbetween.Count;
            foreach (KeyValuePair<string, List<Square>> list in lists)
            {
                List<Square> squares = list.Value;
                for (int i = 0; i < length; i++)
                {
                    Square square = squaresInbetween[i];
                    Square? orig = board.GetSquareByCoordinate(list.Key);
                    if (orig != null && orig.piece == this)
                    {
                        if (!Smasic(square))
                        {
                            if (squares.Contains(square))
                            {
                                string coord = list.Key;
                                if (protect.ContainsKey(coord)) protect[coord].Add(square);
                                else protect.Add(coord, new List<Square>() { square });
                                canBlock = true;
                            }
                        }
                    }
                    else
                    {
                        if (squares.Contains(square))
                        {
                            string coord = list.Key;
                            if (protect.ContainsKey(coord)) protect[coord].Add(square);
                            else protect.Add(coord, new List<Square>() { square });
                            canBlock = true;
                        }
                    }
                }
            }
            return canBlock;
        }

        private bool CanBlockAttacker(Piece piece)
        {
            List<Square> squaresInbetween = SquaresBetweenKingAndRangedPiece(piece);
            if (!squaresInbetween.Any()) return false;
            bool f1 = CanBlockAttacker(squaresInbetween, true);
            bool f2 = CanBlockAttacker(squaresInbetween, false);
            return f1 || f2;
        }

        private List<Square> SquaresBetweenKingAndRangedPiece(Piece piece)
        {
            List<Square> squares = SquaresBetweenKingAndBishop(piece);
            squares.AddRange(SquaresBetweenKingAndRook(piece));
            return squares;
        }

        private List<Square> SquaresBetweenKingAndBishop(Piece piece)
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

        private List<Square> SquaresBetweenKingAndRook(Piece piece)
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
            List<Piece> potentialPieces = player.Opponent().boardPieces;
            List<Piece> pieces = new();
            foreach (Piece piece in potentialPieces)
            {
                List<Square> availableSquares = piece.GetMoves();
                if (availableSquares.Contains(square)) pieces.Add(piece);
            }
            return pieces.ToArray();
        }

        internal void ResetProtection()
        {
            protectMoves = new();
            protectDrops = new();
        }
    }
}
