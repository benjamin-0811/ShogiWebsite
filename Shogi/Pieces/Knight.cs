namespace ShogiWebsite.Shogi.Pieces
{
    internal class Knight : Piece
    {
        internal Knight(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Knight(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        internal Knight(Player player, Board board) : base(player, true, board)
        { }

        internal override IEnumerable<Coordinate> FindMoves() => isPromoted ? GoldMoves() : KnightMoves();

        private IEnumerable<Coordinate> KnightMoves()
        {
            Coordinate? left = Knight(true);
            if (left != null && IsAvailableSquare(left.Value))
                yield return left.Value;
            Coordinate? right = Knight(false);
            if (right != null && IsAvailableSquare(right.Value))
                yield return right.Value;
        }

        internal override IEnumerable<Coordinate> FindDrops() => FindDrops(2);

        internal override void ForcePromote() => ForcePromote(2);
    }
}