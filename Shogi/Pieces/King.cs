namespace ShogiWebsite.Shogi.Pieces
{
    internal class King : Piece
    {
        internal King(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal King(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        internal King(Player player, Board board) : base(player, false, board) { }

        internal override IEnumerable<Coordinate> FindMoves() => ListMoves(new Func<Coordinate, int, Coordinate?>[] { board.N, board.NE, board.E, board.SE, board.S, board.SW, board.W, board.NW });

        internal bool WouldBeCheckAt(Coordinate at)
        {
            bool result = false;
            Coordinate kingSquare = pos;
            Piece? atPiece = board.PieceAt(at);
            board.SetPiece(this, at);
            board.SetPiece(atPiece, kingSquare);
            foreach(Piece opponentPieces in player.Opponent().PlayersPieces())
            {
                if (opponentPieces.FindMoves().Contains(at))
                {
                    result = true;
                    break;
                }
            }
            board.SetPiece(this, kingSquare);
            board.SetPiece(atPiece, at);
            return result;
        }

        internal bool IsCheck() => WouldBeCheckAt(pos);

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
            Coordinate square = attacker.pos;
            IEnumerable<Piece> ownPieces = player.PlayersPieces();
            foreach (Piece capturer in ownPieces)
            {
                IEnumerable<Coordinate> availableSquares = capturer.FindMoves();
                if (capturer == this && !WouldBeCheckAt(square) || availableSquares.Contains(square))
                    yield return capturer;
            }
        }

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
            int x1 = pos.Column;
            int y1 = pos.Row;
            int x2 = piece.pos.Column;
            int y2 = piece.pos.Row;
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
            int x1 = pos.Column;
            int y1 = pos.Row;
            int x2 = piece.pos.Column;
            int y2 = piece.pos.Row;
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
                if (availableSquares.Contains(pos))
                    yield return piece;
            }
        }
    }
}