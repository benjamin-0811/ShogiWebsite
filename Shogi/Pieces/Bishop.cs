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
            var moves = RangeMoves(new Func<Coordinate, int, bool, Coordinate?>[] { Board.NE, Board.NW, Board.SE, Board.SW });
            if (isPromoted)
                moves = moves.Concat(ListMoves(new Func<Coordinate, int, bool, Coordinate?>[] { Board.N, Board.E, Board.S, Board.W }));
            return moves;
        }
    }
}
