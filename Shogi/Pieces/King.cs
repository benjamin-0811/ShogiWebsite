namespace ShogiWebsite.Shogi.Pieces;

internal class King : Piece
{

    internal King(Player player, Board board) : base(player, false, board) { }

    internal override IEnumerable<Coordinate> FindMoves()
    {
        Func<Coordinate, int, Coordinate?>[] directions = new[]
        {
            this.board.N, this.board.NE, this.board.E, this.board.SE,
            this.board.S, this.board.SW, this.board.W, this.board.NW
        };
        return this.ListMoves(directions);
    }

    internal bool WouldBeCheckAt(Coordinate at)
    {
        bool result = false;
        Coordinate kingSquare = this.pos;
        Piece? atPiece = this.board.PieceAt(at);
        this.board.SetPiece(this, at);
        this.board.SetPiece(atPiece, kingSquare);
        IEnumerable<Piece> opponentPieces = this.player.Opponent().PlayersPieces();
        foreach (Piece opponentPiece in opponentPieces)
        {
            if (opponentPiece.FindMoves().Contains(at))
            {
                result = true;
                break;
            }
        }
        this.board.SetPiece(this, kingSquare);
        this.board.SetPiece(atPiece, at);
        return result;
    }

    internal bool IsCheck()
    {
        return this.WouldBeCheckAt(pos);
    }

    internal bool IsCheckmate()
    {
        bool f1 = this.FindMoves().Any();
        IEnumerable<Piece> potentialPieces = this.PiecesThatCheckThisKing();
        if (potentialPieces.Count() != 1)
        {
            return potentialPieces.Count() > 1;
        }
        Piece piece = potentialPieces.First();
        IEnumerable<Piece> capturers = this.GetCapturers(piece);
        bool f2 = capturers.Any();
        bool f3 = this.CanBlockAttacker(piece);
        return !(f1 || f2 || f3);
    }

    private IEnumerable<Piece> GetCapturers(Piece attacker)
    {
        Coordinate square = attacker.pos;
        IEnumerable<Piece> ownPieces = this.player.PlayersPieces();
        foreach (Piece capturer in ownPieces)
        {
            IEnumerable<Coordinate> availableSquares = capturer.FindMoves();
            if (capturer == this && !this.WouldBeCheckAt(square) || availableSquares.Contains(square))
            {
                yield return capturer;
            }
        }
    }

    private bool CanBlockAttacker(IEnumerable<Coordinate> squaresInbetween, bool doDrop)
    {
        bool canBlock = false;
        Dictionary<string, IEnumerable<Coordinate>> lists;
        lists = doDrop ? this.player.GetDropLists() : this.player.GetMoveLists();
        Dictionary<string, IEnumerable<Coordinate>> protect = new();
        int length = squaresInbetween.Count();
        foreach (KeyValuePair<string, IEnumerable<Coordinate>> list in lists)
        {
            IEnumerable<Coordinate> squares = list.Value;
            for (int i = 0; i < length; i++)
            {
                Coordinate square = squaresInbetween.ElementAt(i);
                bool attacksKing = this.board.PieceByCoordString(list.Key) == this;
                if (attacksKing && !this.DoesMoveCheckOwnKing(square) || squares.Contains(square))
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
        IEnumerable<Coordinate> squaresInbetween = this.SquaresBetweenKingAndRangedPiece(piece);
        if (!squaresInbetween.Any())
        {
            return false;
        }
        bool f1 = this.CanBlockAttacker(squaresInbetween, true);
        bool f2 = this.CanBlockAttacker(squaresInbetween, false);
        return f1 || f2;
    }

    private IEnumerable<Coordinate> SquaresBetweenKingAndRangedPiece(Piece piece)
    {
        List<Coordinate> squares = this.SquaresBetweenKingAndBishop(piece).ToList();
        squares.AddRange(this.SquaresBetweenKingAndRook(piece));
        return squares.AsEnumerable();
    }

    private IEnumerable<Coordinate> SquaresBetweenKingAndBishop(Piece piece)
    {
        int x1 = this.pos.Column;
        int y1 = this.pos.Row;
        int x2 = piece.pos.Column;
        int y2 = piece.pos.Row;
        int dx = Math.Abs(x1 - x2);
        int dy = Math.Abs(y1 - y2);
        if (dx <= 1 && dy <= 1)
        {
            return new List<Coordinate>();
        }
        List<Coordinate> squaresInbetween = new();
        if (dx == dy && dx > 1)
        {
            bool minusX = x1 > x2;
            bool minusY = y1 > y2;
            for (int i = 1; i < dx; i++)
            {
                squaresInbetween.Add(new Coordinate(minusX ? x1 - i : x1 + i, minusY ? y1 - i : y1 + i));
            }
        }
        return squaresInbetween;
    }

    private IEnumerable<Coordinate> SquaresBetweenKingAndRook(Piece piece)
    {
        int x1 = this.pos.Column;
        int y1 = this.pos.Row;
        int x2 = piece.pos.Column;
        int y2 = piece.pos.Row;
        int dx = Math.Abs(x1 - x2);
        int dy = Math.Abs(y1 - y2);
        if (dx <= 1 && dy <= 1)
        {
            return new List<Coordinate>();
        }
        List<Coordinate> squaresInbetween = new();
        // vertical
        if (dx == 0 && dy > 1)
        {
            int yMin = Math.Min(y1, y2);
            for (int i = 1; i < dy; i++)
            {
                squaresInbetween.Add(new Coordinate(x1, yMin + i));
            }
        }
        // horizontal
        else if (dx > 1 && dy == 0)
        {
            int xMin = Math.Min(x1, x2);
            for (int i = 1; i < dx; i++)
            {
                squaresInbetween.Add(new Coordinate(x1, xMin + i));
            }
        }
        return squaresInbetween;
    }

    internal IEnumerable<Piece> PiecesThatCheckThisKing()
    {
        IEnumerable<Piece> potentialPieces = this.player.Opponent().PlayersPieces();
        foreach (Piece piece in potentialPieces)
        {
            IEnumerable<Coordinate> availableSquares = piece.FindMoves();
            if (availableSquares.Contains(this.pos))
            {
                yield return piece;
            }
        }
    }
}