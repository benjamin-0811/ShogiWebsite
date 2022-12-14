namespace ShogiWebsite.Shogi.Pieces
{
    internal class Silver : Piece
    {
        internal Silver(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Silver(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        internal Silver(Player player, Board board) : base(player, true, board) { }

        internal override IEnumerable<Coordinate> FindMoves() => isPromoted ? GoldMoves() : SilverMoves();

        private IEnumerable<Coordinate> SilverMoves() => ListMoves(new[] { Front(), FrontLeft(), FrontRight(), BackLeft(), BackRight() });
    }
}