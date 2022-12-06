namespace ShogiWebsite.Shogi.Pieces
{
    internal class Bishop : Piece
    {
        /// <summary>Bishop on the board</summary>
        internal Bishop(Player player, Square square) : base(player, true, square) { }

        /// <summary>Bishop on hand<br/>Does not contain an actual square on the board</summary>
        internal Bishop(Player player, Board board) : base(player, true, board) { }

        internal override IEnumerable<Square> FindMoves()
        {
            var moves = RangeMoves(new[] { square.NorthEast, square.NorthWest, square.SouthEast, square.SouthWest });
            if (isPromoted) moves = moves.Concat(ListMoves(new[] { square.North, square.East, square.South, square.West }));
            return moves;
        }
    }
}
