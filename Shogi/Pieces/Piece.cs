using ShogiWebsite.Shogi.Pieces;

namespace ShogiWebsite.Shogi;

internal abstract class Piece
{
    internal Player player;
    internal bool isPromoted;
    internal readonly bool canPromote;
    internal Board board;
    internal Coordinate pos;


    internal Piece(Player player, bool canPromote, Board board)
    {
        this.player = player;
        isPromoted = false;
        this.canPromote = canPromote;
        this.board = board;
        pos = new Coordinate(-1, -1);
    }


    internal bool Move(Coordinate to, bool doesPromote)
    {
        IEnumerable<Coordinate> moves = FindMoves();
        if (moves != null && moves.Contains(to))
        {
            Coordinate old = pos;
            Piece? piece = board.PieceAt(to);
            char moveType = '-';
            if (piece != null && DifferentPlayer(piece) && piece is not King)
            {
                player.ChangeHandPieceAmount(piece, 1);
                moveType = 'x';
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
        return false;
    }


    internal bool MoveFromHand(Coordinate to)
    {
        if (CanDropOn(to) && this is not King)
        {
            player.ChangeHandPieceAmount(this, -1);
            board.SetPiece(this, to);
            board.Log($"{player.PlayerNumber()} : {Names.Abbreviation(this)}*{board.CoordinateString(to)}");
            return true;
        }
        return false;
    }


    internal bool DoesMoveCheckOwnKing(Coordinate to)
    {
        bool result = false;
        Coordinate oldCoord = pos;
        Piece? oldToPiece = board.PieceAt(to);
        board.SetPiece(this, to);
        board.SetPiece(null, oldCoord);
        if (player.king.IsCheck())
            result = true;
        board.SetPiece(oldToPiece, to);
        board.SetPiece(this, oldCoord);
        return result;
    }


    internal void Promote()
    {
        if (canPromote)
            isPromoted = true;
    }

    internal virtual void ForcePromote() { }


    internal void ForcePromote(int rowAmount)
    {
        int row = pos.Row;
        bool unPromoted = !isPromoted;
        bool cannotMoveFurther = player.isPlayer1 ? row < rowAmount : row >= board.height - rowAmount;
        if (unPromoted && cannotMoveFurther)
            Promote();
    }


    internal bool IsPromoted() => canPromote && isPromoted;


    internal bool CanMoveTo(Coordinate pos) => FindMoves().Contains(pos);


    internal bool CanDropOn(Coordinate pos) => FindDrops().Contains(pos);


    internal abstract IEnumerable<Coordinate> FindMoves();


    internal virtual IEnumerable<Coordinate> FindDrops() => FindDrops(0);


    internal IEnumerable<Coordinate> FindDrops(int freeRows)
    {
        int min = player.isPlayer1 ? freeRows : 0;
        int max = board.height - freeRows + min - 1;
        for (int i = min; i <= max; i++)
        {
            for (int j = 0; j < board.width; j++)
            {
                Coordinate pos = new(i, j);
                if (board.PieceAt(pos) == null)
                    yield return pos;
            }
        }
    }


    protected IEnumerable<Coordinate> GoldMoves() => ListMoves(new[] { Front(), FrontLeft(), FrontRight(), Left(), Right(), Back() });


    protected IEnumerable<Coordinate> RangeMoves(Func<Coordinate, int, Coordinate?>[] directions)
    {
        foreach (Func<Coordinate, int, Coordinate?> direction in directions)
        {
            bool flag = true;
            for (int i = 1; flag; i++)
            {
                Coordinate? temp = direction(pos, i);
                flag = CanContinue(temp);
                if (IsAvailableSquare(temp))
                {
                    Helper.AssertNotNull(temp);
                    yield return temp.Value;
                }
            }
        }
    }


    protected IEnumerable<Coordinate> ListMoves(Func<Coordinate, int, Coordinate?>[] directions, int distance = 1)
    {
        foreach (Func<Coordinate, int, Coordinate?> direction in directions)
        {
            Coordinate? temp = direction(pos, distance);
            if (IsAvailableSquare(temp))
            {
                Helper.AssertNotNull(temp);
                yield return temp.Value;
            }
        }
    }


    protected bool CanContinue(Coordinate? pos) => pos != null && board.PieceAt(pos.Value) == null;


    protected bool IsAvailableSquare(Coordinate? pos)
    {
        if (pos == null)
            return false;
        Piece? piece = board.PieceAt(pos.Value);
        return piece == null || piece.player != player;
    }


    internal bool DifferentPlayer(Coordinate pos)
    {
        Piece? piece = board.PieceAt(pos);
        return piece == null || DifferentPlayer(piece);
    }


    internal bool DifferentPlayer(Piece piece) => piece.player != player;


    internal Func<Coordinate, int, Coordinate?> Front() => player.isPlayer1 ? board.N : board.S;


    internal Func<Coordinate, int, Coordinate?> Back() => player.isPlayer1 ? board.S : board.N;


    internal Func<Coordinate, int, Coordinate?> Left() => player.isPlayer1 ? board.W : board.E;


    internal Func<Coordinate, int, Coordinate?> Right() => player.isPlayer1 ? board.E : board.W;


    internal Func<Coordinate, int, Coordinate?> FrontLeft() => player.isPlayer1 ? board.NW : board.SE;


    internal Func<Coordinate, int, Coordinate?> FrontRight() => player.isPlayer1 ? board.NE : board.SW;


    internal Func<Coordinate, int, Coordinate?> BackLeft() => player.isPlayer1 ? board.SW : board.NE;


    internal Func<Coordinate, int, Coordinate?> BackRight() => player.isPlayer1 ? board.SE : board.NW;


    internal Coordinate? Knight(bool left)
    {
        Coordinate? front = Front()(pos, 2);
        Func<Coordinate, int, Coordinate?> side = left ? Left() : Right();
        return front != null ? side(front.Value, 1) : null;
    }
}