namespace ShogiWebsite.Shogi.Pieces
{
    internal class Rook : Piece
    {
        /// <summary>Rook on the board</summary>
        internal Rook(Player player, Square square) : base(player, true, square)
        { }

        /// <summary>Rook on hand<br/>Does not contain an actual square on the board</summary>
        internal Rook(Player player, Board board) : base(player, true, board)
        { }

        // Endless loop!
        internal override List<Square> FindMoves()
        {
            List<Square> newMoves = new();
            newMoves.AddRange(RangeMoves(new Func<int, bool, Square?>[] {
                square.North, square.East, square.South, square.West
            }));
            if (isPromoted) newMoves.AddRange(ListMoves(new Func<int, bool, Square?>[] {
                square.NorthEast, square.NorthWest, square.SouthEast, square.SouthWest
            }));
            return newMoves;
        }
    }
}
