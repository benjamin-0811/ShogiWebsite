namespace ShogiWebsite.Shogi.Pieces
{
    internal class Lance : Piece
    {
        /// <summary>Lance on the board</summary>
        internal Lance(Player player, Square square) : base(player, true, square)
        { }

        /// <summary>Lance on hand<br/>Does not contain an actual square on the board</summary>
        internal Lance(Player player, Board board) : base(player, true, board)
        { }

        internal override List<Square> FindMoves() => isPromoted ? GoldMoves() : LanceMoves();

        private List<Square> LanceMoves() => RangeMoves(new Func<int, bool, Square?>[] {
            player.isPlayer1 ? square.North : square.South
        });

        internal override List<Square> FindDrops()
        {
            // Cannot drop Lance at the end
            List<Square> newDrops = new();
            int min = player.isPlayer1 ? 1 : 0;
            int max = player.isPlayer1 ? 8 : 7;
            for (int i = min; i <= max; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Square square = board.squares[j, i];
                    if (square.piece == null) newDrops.Add(square);
                }
            }
            return newDrops;
        }

        internal override void ForcePromote()
        {
            int row = square.rowIndex;
            if (!isPromoted && (player.isPlayer1 ? row == 0 : row == 8)) Promote();
        }
    }
}
