namespace ShogiWebsite.Shogi.Pieces;

internal class Rook : Piece
{
    internal Rook(Player player, Board board) : base(player, true, board)
    { }


    internal override IEnumerable<Coordinate> FindMoves()
    {
        IEnumerable<Coordinate> moves = RangeMoves(new[] { board.N, board.E, board.S, board.W });
        if (isPromoted)
            moves = moves.Concat(ListMoves(new[] { board.NE, board.NW, board.SE, board.SW }));
        return moves;
    }
}