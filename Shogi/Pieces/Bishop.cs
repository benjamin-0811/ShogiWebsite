namespace ShogiWebsite.Shogi.Pieces
{
    internal class Bishop : Piece
    {
        internal Bishop(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Bishop(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        internal Bishop(Player player, Board board) : base(player, true, board)
        { }

        internal override IEnumerable<Coordinate> FindMoves()
        {
            IEnumerable<Coordinate> moves = RangeMoves(new Func<Coordinate, int, Coordinate?>[] { board.NE, board.NW, board.SE, board.SW });
            if (isPromoted)
                moves = moves.Concat(ListMoves(new Func<Coordinate, int, Coordinate?>[] { board.N, board.E, board.S, board.W }));
            return moves;
        }
    }
}