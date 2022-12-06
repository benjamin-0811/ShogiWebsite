namespace ShogiWebsite.Shogi.Pieces
{
    internal class Knight : Piece
    {
        /// <summary>Knight on the board</summary>
        internal Knight(Player player, Square square) : base(player, true, square) { }

        /// <summary>Knight on hand<br/>Does not contain an actual square on the board</summary>
        internal Knight(Player player, Board board) : base(player, true, board) { }

        internal override IEnumerable<Square> FindMoves() => isPromoted ? GoldMoves() : KnightMoves();

        private IEnumerable<Square> KnightMoves()
        {
            var left = KnightMoveLeft();
            if (left != null && IsAvailableSquare(left)) yield return left;
            var right = KnightMoveRight();
            if (right != null && IsAvailableSquare(right)) yield return right;
        }

        internal override IEnumerable<Square> FindDrops()
        {
            int min = player.isPlayer1 ? 2 : 0;
            int max = player.isPlayer1 ? 8 : 6;
            for (int i = min; i <= max; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var square = board.squares[j, i];
                    if (square.piece == null) yield return square;
                }
            }
        }

        internal override void ForcePromote()
        {
            int row = square.rowIndex;
            if (!isPromoted && (player.isPlayer1 ? row <= 1 : row >= 7)) Promote();
        }
    }
}
