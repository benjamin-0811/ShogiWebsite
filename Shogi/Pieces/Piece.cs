using ShogiWebsite.Shogi.Pieces;
using System.Text;

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
        this.isPromoted = false;
        this.canPromote = canPromote;
        this.board = board;
        this.pos = new Coordinate(-1, -1);
    }

    internal bool Move(Coordinate to, bool doesPromote)
    {
        IEnumerable<Coordinate> moves = this.FindMoves();
        if (moves != null && moves.Contains(to))
        {
            Coordinate old = this.pos;
            Piece? piece = this.board.PieceAt(to);
            char moveType = '-';
            if (piece != null && this.DifferentPlayer(piece) && piece is not King)
            {
                this.player.ChangeHandPieceAmount(piece, 1);
                moveType = 'x';
            }
            this.board.SetPiece(null, old);
            this.board.SetPiece(this, to);
            string part1 = $"{Names.Abbreviation(this)}{this.board.CoordinateString(old)}";
            bool wasPromoted = this.isPromoted;
            this.ForcePromote();
            if (wasPromoted != this.isPromoted)
            {
                doesPromote = true;
            }
            string part2 = $"{moveType}{this.board.CoordinateString(to)}{(doesPromote ? "+" : "")}";
            this.board.Log($"{this.player.PlayerNumber()} : {part1}{part2}");
            if (doesPromote)
            {
                this.isPromoted = true;
            }
            return true;
        }
        return false;
    }

    internal bool MoveFromHand(Coordinate to)
    {
        IEnumerable<Coordinate> drops = this.FindDrops();
        bool emptyTo = this.board.PieceAt(to) == null;
        bool canDropTo = drops != null && drops.Contains(to) && emptyTo;
        if (canDropTo && this is not King)
        {
            this.player.ChangeHandPieceAmount(this, -1);
            this.board.SetPiece(this, to);
            StringBuilder logBuilder = new StringBuilder()
                .Append(this.player.PlayerNumber())
                .Append(" : ")
                .Append(Names.Abbreviation(this))
                .Append('*')
                .Append(this.board.CoordinateString(to));
            this.board.Log(logBuilder.ToString());
            return true;
        }
        return false;
    }

    internal bool DoesMoveCheckOwnKing(Coordinate to)
    {
        bool result = false;
        Coordinate oldCoord = this.pos;
        Piece? oldToPiece = this.board.PieceAt(to);
        this.board.SetPiece(this, to);
        this.board.SetPiece(null, oldCoord);
        if (this.player.king.IsCheck())
        {
            result = true;
        }
        this.board.SetPiece(oldToPiece, to);
        this.board.SetPiece(this, oldCoord);
        return result;
    }

    internal void Promote()
    {
        if (this.canPromote)
        {
            this.isPromoted = true;
        }
    }

    // internal string IdentifyingString() => $"{player.PlayerNumber()}'s {Names.Get(this)} on {CoordinateString()}";

    internal virtual void ForcePromote() { }

    internal void ForcePromote(int rowAmount)
    {
        int row = this.pos.Row;
        bool unPromoted = !this.isPromoted;
        bool cannotMoveFurther = this.player.isPlayer1 ? row < rowAmount : row >= this.board.height - rowAmount;
        if (unPromoted && cannotMoveFurther)
        {
            this.Promote();
        }
    }

    internal bool IsPromoted()
    {
        return this.canPromote && this.isPromoted;
    }

    internal bool CanMoveTo(Coordinate pos)
    {
        IEnumerable<Coordinate> availableSquares = this.FindMoves();
        return availableSquares.Contains(pos);
    }

    internal abstract IEnumerable<Coordinate> FindMoves();

    internal virtual IEnumerable<Coordinate> FindDrops()
    {
        return this.FindDrops(0);
    }

    internal IEnumerable<Coordinate> FindDrops(int freeRows)
    {
        int min = this.MinDropRow(freeRows);
        int max = this.MaxDropRow(freeRows);
        for (int i = min; i <= max; i++)
        {
            for (int j = 0; j < this.board.width; j++)
            {
                Coordinate pos = new(i, j);
                if (this.board.PieceAt(pos) == null)
                {
                    yield return pos;
                }
            }
        }
    }

    internal int MinDropRow(int freeRows)
    {
        return this.player.isPlayer1 ? freeRows : 0;
    }

    internal int MaxDropRow(int freeRows)
    {
        return this.board.height - (this.player.isPlayer1 ? 0 : freeRows) - 1;
    }

    protected IEnumerable<Coordinate> GoldMoves()
    {
        Func<Coordinate, int, Coordinate?>[] directions = new[]
        {
            this.Front(), this.FrontLeft(), this.FrontRight(),
            this.Left(), this.Right(), this.Back()
        };
        return this.ListMoves(directions);
    }

    protected IEnumerable<Coordinate> RangeMoves(Func<Coordinate, int, Coordinate?>[] directions)
    {
        foreach (Func<Coordinate, int, Coordinate?> direction in directions)
        {
            bool flag = true;
            for (int i = 1; flag; i++)
            {
                Coordinate? temp = direction(this.pos, i);
                flag = this.CanContinue(temp);
                if (this.IsAvailableSquare(temp))
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
            Coordinate? temp = direction(this.pos, distance);
            if (this.IsAvailableSquare(temp))
            {
                Helper.AssertNotNull(temp);
                yield return temp.Value;
            }
        }
    }

    protected bool CanContinue(Coordinate? pos)
    {
        if (pos == null)
        {
            return false;
        }
        return this.board.PieceAt(pos.Value) == null;
    }

    protected bool IsAvailableSquare(Coordinate? pos)
    {
        if (pos == null)
        {
            return false;
        }
        Piece? piece = this.board.PieceAt(pos.Value);
        return piece == null || piece.player != this.player;
    }

    internal bool DifferentPlayer(Coordinate pos)
    {
        Piece? piece = this.board.PieceAt(pos);
        return piece == null || this.DifferentPlayer(pos);
    }

    internal bool DifferentPlayer(Piece piece)
    {
        return piece.player != this.player;
    }

    internal Func<Coordinate, int, Coordinate?> Front()
    {
        return this.player.isPlayer1 ? this.board.N : this.board.S;
    }

    internal Func<Coordinate, int, Coordinate?> Back()
    {
        return this.player.isPlayer1 ? this.board.S : this.board.N;
    }

    internal Func<Coordinate, int, Coordinate?> Left()
    {
        return this.player.isPlayer1 ? this.board.W : this.board.E;
    }

    internal Func<Coordinate, int, Coordinate?> Right()
    {
        return this.player.isPlayer1 ? this.board.E : this.board.W;
    }

    internal Func<Coordinate, int, Coordinate?> FrontLeft()
    {
        return this.player.isPlayer1 ? this.board.NW : this.board.SE;
    }

    internal Func<Coordinate, int, Coordinate?> FrontRight()
    {
        return this.player.isPlayer1 ? this.board.NE : this.board.SW;
    }

    internal Func<Coordinate, int, Coordinate?> BackLeft()
    {
        return this.player.isPlayer1 ? this.board.SW : this.board.NE;
    }

    internal Func<Coordinate, int, Coordinate?> BackRight()
    {
        return this.player.isPlayer1 ? this.board.SE : this.board.NW;
    }

    internal Coordinate? Knight(bool left)
    {
        Coordinate? front = this.Front()(this.pos, 2);
        Func<Coordinate, int, Coordinate?> side = left ? this.Left() : this.Right();
        return front != null ? side(front.Value, 1) : null;
    }

    internal string CoordinateString()
    {
        return this.board.CoordinateString(this.pos);
    }
}