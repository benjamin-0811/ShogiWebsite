namespace Blank.Shogi.Pieces {
    internal class Knight : AbstractPiece {
        /// <summary>Knight on the board</summary>
        internal Knight(Player player, Square square) : base(player, true, square) {
        }

        /// <summary>Knight on hand<br/>Does not contain an actual square on the board</summary>
        internal Knight(Player player, Board board) : base(player, true, board) {
        }

        internal override List<Square> FindMoves() {
            return isPromoted ? GoldMoves() : KnightMoves();
        }

        private List<Square> KnightMoves() {
            List<Square> newMoves = new();
            Square? left = KnightMoveLeft();
            if (left != null && Available(left)) {
                newMoves.Add(left);
            }
            Square? right = KnightMoveRight();
            if (right != null && Available(right)) {
                newMoves.Add(right);
            }
            return newMoves;
        }

        internal override List<Square> FindDrops() {
            // Cannot drop Knight on last 2 rows
            List<Square> newMoves = new();
            int min = player.isPlayer1 ? 2 : 0;
            int max = player.isPlayer1 ? 8 : 6;
            for (int i = min; i <= max; i++) {
                for (int j = 0; j < 9; j++) {
                    Square square = board.squares[j, i];
                    if (square.piece == null) {
                        newMoves.Add(square);
                    }
                }
            }
            return newMoves;
        }

        internal override void ForcePromote() {
            int row = square.rowIndex;
            if (!isPromoted && (player.isPlayer1 ? row <= 1 : row >= 7)) {
                Promote();
            }
        }
    }
}
