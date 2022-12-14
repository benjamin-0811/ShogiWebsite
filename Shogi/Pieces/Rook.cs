namespace ShogiWebsite.Shogi.Pieces
{
    internal class Rook : Piece
    {
        internal Rook(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Rook(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        internal Rook(Player player, Board board) : base(player, true, board)
        { }

        internal override IEnumerable<Coordinate> FindMoves()
        {
            IEnumerable<Coordinate> moves = RangeMoves(new Func<Coordinate, int, Coordinate?>[] { board.N, board.E, board.S, board.W });
            if (isPromoted)
                moves = moves.Concat(ListMoves(new Func<Coordinate, int, Coordinate?>[] { board.NE, board.NW, board.SE, board.SW }));
            return moves;
        }
    }
}