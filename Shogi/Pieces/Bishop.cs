namespace ShogiWebsite.Shogi.Pieces;

internal class Bishop : Piece
{
    internal Bishop(Player player, Board board) : base(player, true, board)
    { }


    internal override IEnumerable<Coordinate> FindMoves()
    {
        IEnumerable<Coordinate> moves = RangeMoves(new[] { board.NE, board.NW, board.SE, board.SW });
        if (isPromoted)
            moves = moves.Concat(ListMoves(new[] { board.N, board.E, board.S, board.W  }));
        return moves;
    }
}