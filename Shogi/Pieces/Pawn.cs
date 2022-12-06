namespace Blank.Shogi.Pieces {
    internal class Pawn : AbstractPiece {
        /// <summary>Pawn on the board</summary>
        internal Pawn(Player player, Square square) : base(player, true, square) {
        }

        /// <summary>Pawn on hand<br/>Does not contain an actual square on the board</summary>
        internal Pawn(Player player, Board board) : base(player, true, board) {
        }

        internal override List<Square> FindMoves() {
            if (isPromoted) {
                return GoldMoves();
            }
            else {
                Square? front = Forward();
                if (front != null && DifferentPlayerAt(front)) {
                    return new() { front };
                }
                return new();
            }
        }

        internal override List<Square> FindDrops() {
            List<Square> newDrops = new();
            int min = player.isPlayer1 ? 1 : 0;
            int max = player.isPlayer1 ? 8 : 7;
            List<int> colWithoutPawn = new();
            bool[] colHasPawn = new bool[9];
            for (int i = 0; i < 9; i++) {
                for (int j = min; j <= max; j++) {
                    AbstractPiece? piece = board.squares[i, j].piece;
                    if (IsOwnPawn(piece)) {
                        colHasPawn[i] = true;
                        break;
                    }
                }
            }
            for (int i = 0; i < colHasPawn.Length; i++) {
                if (!colHasPawn[i]) {
                    colWithoutPawn.Add(i);
                }
            }
            foreach (int i in colWithoutPawn) {
                for (int j = min; j <= max; j++) {
                    Square square = board.squares[i, j];
                    if (square.piece == null && !WouldCheckmate(square)) {
                        newDrops.Add(square);
                    }
                }
            }
            return newDrops;
        }

        private bool IsOwnPawn(AbstractPiece? piece) {
            if (piece == null) {
                return false;
            }
            bool f1 = piece is Pawn;
            bool f2 = !piece.isPromoted;
            bool f3 = player.isPlayer1 == piece.player.isPlayer1;
            if (f1 && f2 && f3) {
                return true;
            }
            return false;
        }

        // WIP
        private bool WouldCheckmate(Square square) {
            BC.Info($"See if {IdentifyingString()} would checkmate the opponent's king.");
            Square currentSquare = this.square;
            this.square = square;
            square.piece = this;
            // find other solution
            // player.Opponent().AfterOpponent();
            player.boardPieces.Add(this);
            bool result = player.Opponent().king.IsCheckmate();
            player.boardPieces.Remove(this);
            square.piece = null;
            this.square = currentSquare;
            return result;
        }

        internal override void ForcePromote() {
            int row = square.rowIndex;
            if (!isPromoted && (player.isPlayer1 ? row == 0 : row == 8)) {
                BC.Action($"Forcing promotion of {IdentifyingString()}.");
                Promote();
            }
        }
    }
}
