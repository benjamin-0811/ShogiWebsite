namespace ShogiWebsite.Shogi.Pieces
{
    internal class Silver : Piece
    {
        /// <summary>Silver General on the board</summary>
        internal Silver(Player player, Square square) : base(player, true, square) { }

        /// <summary>Silver General on hand<br/>Does not contain an actual square on the board</summary>
        internal Silver(Player player, Board board) : base(player, true, board) { }

        internal override IEnumerable<Square> FindMoves() => isPromoted ? GoldMoves() : SilverMoves();

        private IEnumerable<Square> SilverMoves() => ListMoves(new[] { Forward, FrontLeft, FrontRight, BackLeft, BackRight });
    }
}
