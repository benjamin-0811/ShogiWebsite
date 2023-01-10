namespace ShogiWebsite.Shogi.Pieces;

internal class Pawn : Piece
{
    internal Pawn(Player player, Board board) : base(player, true, board)
    { }


    internal override IEnumerable<Coordinate> FindMoves() => isPromoted ? GoldMoves() : PawnMove();


    private IEnumerable<Coordinate> PawnMove()
    {
        Coordinate? front = Front()(pos, 1);
        if (front != null && DifferentPlayer(front.Value))
            yield return front.Value;
    }


    internal override IEnumerable<Coordinate> FindDrops()
    {
        IEnumerable<Coordinate> tempDrops = FindDrops(1);
        IEnumerable<int> acceptableColumns = ColumnsWithoutOwnPawns();
        foreach (Coordinate coord in tempDrops)
        {
            if (acceptableColumns.Contains(coord.Column) && !WouldCheckmate(coord))
                yield return coord;
        }
    }


    private IEnumerable<int> ColumnsWithoutOwnPawns()
    {
        bool[] colHasPawn = new bool[board.width];
        foreach (Piece? piece in board.pieces)
        {
            if (piece != null && IsOwnPawn(piece))
                colHasPawn[piece.pos.Column] = true;
        }

        for (int i = 0; i < colHasPawn.Length; i++)
        {
            if (!colHasPawn[i])
                yield return i;
        }
    }


    private bool IsOwnPawn(Piece? piece) => piece is Pawn && !piece.isPromoted && DifferentPlayer(piece);


    private bool WouldCheckmate(Coordinate square)
    {
        Coordinate currentSquare = pos;
        pos = square;
        board.SetPiece(this, square);
        bool result = player.Opponent().king.IsCheckmate();
        board.SetPiece(null, square);
        pos = currentSquare;
        return result;
    }


    internal override void ForcePromote() => ForcePromote(1);
}