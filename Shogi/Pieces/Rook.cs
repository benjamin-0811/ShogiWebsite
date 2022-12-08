namespace ShogiWebsite.Shogi.Pieces
{
    internal class Rook : Piece
    {
        /// <summary>Rook on the board</summary>
        internal Rook(Player player, Square square) : base(player, true, square) { }

        /// <summary>Rook on hand<br/>Does not contain an actual square on the board</summary>
        internal Rook(Player player, Board board) : base(player, true, board) { }

        internal override IEnumerable<Square> FindMoves()
        {
            var moves = RangeMoves(new[] { square.North, square.East, square.South, square.West });
            if (isPromoted)
                moves = moves.Concat(ListMoves(new[] { square.NorthEast, square.NorthWest, square.SouthEast, square.SouthWest }));
            return moves;
        }
    }
}
