namespace ShogiWebsite.Shogi.Pieces
{
    internal class Gold : Piece
    {
        /// <summary>Gold General on the board</summary>
        internal Gold(Player player, Square square) : base(player, false, square)
        { }

        /// <summary>Gold General on hand<br/>Does not contain an actual square on the board</summary>
        internal Gold(Player player, Board board) : base(player, false, board)
        { }

        internal override List<Square> FindMoves() => GoldMoves();
    }
}
