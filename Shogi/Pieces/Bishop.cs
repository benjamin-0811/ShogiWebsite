namespace ShogiWebsite.Shogi.Pieces
{
    internal class Bishop : Piece
    {
        /// <summary>Bishop on the board</summary>
        internal Bishop(Player player, Square square) : base(player, true, square)
        {
        }

        /// <summary>Bishop on hand<br/>Does not contain an actual square on the board</summary>
        internal Bishop(Player player, Board board) : base(player, true, board)
        {
        }

        internal override List<Square> FindMoves()
        {
            List<Square> newMoves = new();
            newMoves.AddRange(RangeMoves(new Func<int, bool, Square?>[] { square.NorthEast, square.NorthWest, square.SouthEast, square.SouthWest }));
            if (isPromoted) newMoves.AddRange(ListMoves(new Func<int, bool, Square?>[] { square.North, square.East, square.South, square.West }));
            return newMoves;
        }
    }
}
