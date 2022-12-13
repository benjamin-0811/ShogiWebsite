namespace ShogiWebsite.Shogi.Pieces
{
    internal class Lance : Piece
    {
        /// <summary>Lance on the board</summary>
        internal Lance(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Lance(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        /// <summary>Lance on hand<br/>Does not contain an actual square on the board</summary>
        internal Lance(Player player, Board board) : base(player, true, board)
        { }

        internal override IEnumerable<Coordinate> FindMoves() => isPromoted ? GoldMoves() : LanceMoves();

        private IEnumerable<Coordinate> LanceMoves() => RangeMoves(new Func<Coordinate, int, bool, Coordinate?>[] { Front() });

        internal override IEnumerable<Coordinate> FindDrops()
        {
            int min = player.isPlayer1 ? 1 : 0;
            int max = player.isPlayer1 ? 8 : 7;
            for (int i = min; i <= max; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Coordinate coord = new(i, j);
                    if (board.PieceAt(coord) == null)
                        yield return coord;
                }
            }
        }

        internal override void ForcePromote()
        {
            int row = coordinate.Row;
            if (!isPromoted && (player.isPlayer1 ? row == 0 : row == 8))
                Promote();
        }
    }
}
