namespace ShogiWebsite.Shogi.Pieces
{
    internal class Bishop : Piece
    {
        /// <summary>Bishop on the board</summary>
        internal Bishop(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Bishop(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        /// <summary>Bishop on hand<br/>Does not contain an actual square on the board</summary>
        internal Bishop(Player player, Board board) : base(player, true, board)
        { }

        internal override IEnumerable<Coordinate> FindMoves()
        {
            IEnumerable<Coordinate> moves = RangeMoves(new Func<Coordinate, int, bool, Coordinate?>[] { board.NE, board.NW, board.SE, board.SW });
            if (isPromoted)
                moves = moves.Concat(ListMoves(new Func<Coordinate, int, bool, Coordinate?>[] { board.N, board.E, board.S, board.W }));
            return moves;
        }
    }
}
