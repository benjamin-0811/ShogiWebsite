namespace ShogiWebsite.Shogi.Pieces
{
    internal class Silver : Piece
    {
        /// <summary>Silver General on the board</summary>
        internal Silver(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Silver(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        /// <summary>Silver General on hand<br/>Does not contain an actual square on the board</summary>
        internal Silver(Player player, Board board) : base(player, true, board) { }

        internal override IEnumerable<Coordinate> FindMoves() => isPromoted ? GoldMoves() : SilverMoves();

        private IEnumerable<Coordinate> SilverMoves() => ListMoves(new[] { Front(), FrontLeft(), FrontRight(), BackLeft(), BackRight() });
    }
}
