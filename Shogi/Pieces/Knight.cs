namespace ShogiWebsite.Shogi.Pieces
{
    internal class Knight : Piece
    {
        /// <summary>Knight on the board</summary>
        internal Knight(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Knight(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        /// <summary>Knight on hand<br/>Does not contain an actual square on the board</summary>
        internal Knight(Player player, Board board) : base(player, true, board)
        { }

        internal override IEnumerable<Coordinate> FindMoves() => isPromoted ? GoldMoves() : KnightMoves();

        private IEnumerable<Coordinate> KnightMoves()
        {
            var left = Knight(true);
            if (left != null && IsAvailableSquare(left.Value))
                yield return left.Value;
            var right = Knight(false);
            if (right != null && IsAvailableSquare(right.Value))
                yield return right.Value;
        }

        internal override IEnumerable<Coordinate> FindDrops()
        {
            int min = player.isPlayer1 ? 2 : 0;
            int max = player.isPlayer1 ? 8 : 6;
            for (int i = min; i <= max; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var coord = new Coordinate(i, j);
                    if (board.PieceAt(coord) == null)
                        yield return coord;
                }
            }
        }

        internal override void ForcePromote()
        {
            int row = coordinate.Row;
            if (!isPromoted && (player.isPlayer1 ? row <= 1 : row >= 7)) Promote();
        }
    }
}
