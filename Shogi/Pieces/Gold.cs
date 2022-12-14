namespace ShogiWebsite.Shogi.Pieces
{
    internal class Gold : Piece
    {
        internal Gold(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Gold(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        internal Gold(Player player, Board board) : base(player, false, board)
        { }

        internal override IEnumerable<Coordinate> FindMoves() => GoldMoves();
    }
}