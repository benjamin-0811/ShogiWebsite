namespace ShogiWebsite.Shogi.Pieces
{
    internal class Rook : Piece
    {
        /// <summary>Rook on the board</summary>
        internal Rook(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Rook(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        /// <summary>Rook on hand<br/>Does not contain an actual square on the board</summary>
        internal Rook(Player player, Board board) : base(player, true, board)
        { }

        internal override IEnumerable<Coordinate> FindMoves()
        {
            var moves = RangeMoves(new Func<Coordinate, int, bool, Coordinate?>[] { Board.N, Board.E, Board.S, Board.W });
            if (isPromoted)
                moves = moves.Concat(ListMoves(new Func<Coordinate, int, bool, Coordinate?>[] { Board.NE, Board.NW, Board.SE, Board.SW }));
            return moves;
        }
    }
}
