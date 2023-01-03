namespace ShogiWebsite.Shogi.Pieces;

internal class Pawn : Piece
{
    internal Pawn(Player player, Board board) : base(player, true, board)
    { }

    internal override IEnumerable<Coordinate> FindMoves()
    {
        return this.isPromoted ? this.GoldMoves() : this.PawnMove();
    }

    private IEnumerable<Coordinate> PawnMove()
    {
        Coordinate? front = this.Front()(this.pos, 1);
        if (front != null && this.DifferentPlayer(front.Value))
        {
            yield return front.Value;
        }
    }

    internal override IEnumerable<Coordinate> FindDrops()
    {
        IEnumerable<Coordinate> tempDrops = this.FindDrops(1);
        IEnumerable<int> acceptableColumns = this.ColumnsWithoutOwnPawns();
        foreach (Coordinate coord in tempDrops)
        {
            if (acceptableColumns.Contains(coord.Column) && !this.WouldCheckmate(coord))
            {
                yield return coord;
            }
        }
    }

    private IEnumerable<int> ColumnsWithoutOwnPawns()
    {
        bool[] colHasPawn = new bool[this.board.width];
        foreach(Piece? piece in this.board.pieces)
        {
            if (piece != null && this.IsOwnPawn(piece))
            {
                colHasPawn[piece.pos.Column] = true;
            }
        }
        for (int i = 0; i < colHasPawn.Length; i++)
        {
            if (!colHasPawn[i])
            {
                yield return i;
            }
        }
    }

    private bool IsOwnPawn(Piece? piece)
    {
        return piece is Pawn && !piece.isPromoted && this.DifferentPlayer(piece);
    }

    private bool WouldCheckmate(Coordinate square)
    {
        Coordinate currentSquare = this.pos;
        this.pos = square;
        this.board.SetPiece(this, square);
        bool result = this.player.Opponent().king.IsCheckmate();
        this.board.SetPiece(null, square);
        this.pos = currentSquare;
        return result;
    }

    internal override void ForcePromote()
    {
        this.ForcePromote(1);
    }
}