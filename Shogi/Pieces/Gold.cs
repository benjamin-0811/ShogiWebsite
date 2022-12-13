namespace ShogiWebsite.Shogi.Pieces
{
    internal class Gold : Piece
    {
        /// <summary>Gold General on the board</summary>
        internal Gold(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Gold(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        /// <summary>Gold General on hand<br/>Does not contain an actual square on the board</summary>
        internal Gold(Player player, Board board) : base(player, false, board)
        { }

        internal override IEnumerable<Coordinate> FindMoves() => GoldMoves();
    }
}
