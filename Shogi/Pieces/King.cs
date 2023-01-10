namespace ShogiWebsite.Shogi.Pieces;

internal class King : Piece
{
    internal King(Player player, Board board) : base(player, false, board) { }


    internal override IEnumerable<Coordinate> FindMoves()
    {
        Func<Coordinate, int, Coordinate?>[] directions = new[] { board.N, board.NE, board.E, board.SE, board.S, board.SW, board.W, board.NW };
        return ListMoves(directions);
    }


    internal bool WouldBeCheckAt(Coordinate at)
    {
        bool result = false;
        Coordinate kingSquare = pos;
        Piece? atPiece = board.PieceAt(at);
        board.SetPiece(this, at);
        board.SetPiece(atPiece, kingSquare);
        IEnumerable<Piece> opponentPieces = player.Opponent().PlayersPieces();
        foreach (Piece opponentPiece in opponentPieces)
        {
            if (CanMoveTo(at))
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
            if (capturer == this && !WouldBeCheckAt(square) || CanMoveTo(square))
                yield return capturer;
        }
    }


    private bool CanBlockAttacker(IEnumerable<Coordinate> squaresInbetween, bool doDrop)
    {
        bool canBlock = false;
        Dictionary<string, IEnumerable<Coordinate>> lists;
        lists = doDrop ? player.GetDropLists() : player.GetMoveLists();
        Dictionary<string, IEnumerable<Coordinate>> protect = new();
        int length = squaresInbetween.Count();
        foreach (KeyValuePair<string, IEnumerable<Coordinate>> list in lists)
        {
            IEnumerable<Coordinate> squares = list.Value;
            for (int i = 0; i < length; i++)
            {
                Coordinate square = squaresInbetween.ElementAt(i);
                bool attacksKing = board.PieceByCoordString(list.Key) == this;
                if (attacksKing && !DoesMoveCheckOwnKing(square) || squares.Contains(square))
                {
                    string coord = list.Key;
                    protect[list.Key] = protect[list.Key].Append(square);
                    canBlock = true;
                }
            }
        }
        return canBlock;
    }


    private Dictionary<Piece, IEnumerator<Coordinate>> BlockAttackerMoves(Piece attacker)
    {
        throw new NotImplementedException();
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
                squaresInbetween.Add(new(x1 + (minusX ? -i : i), y1 + (minusY ? -i : i)));
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
            if (CanMoveTo(pos))
                yield return piece;
        }
    }
}