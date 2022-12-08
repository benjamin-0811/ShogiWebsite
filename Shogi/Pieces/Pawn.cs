namespace ShogiWebsite.Shogi.Pieces
{
    internal class Pawn : Piece
    {
        /// <summary>Pawn on the board</summary>
        internal Pawn(Player player, Square square) : base(player, true, square) { }

        /// <summary>Pawn on hand<br/>Does not contain an actual square on the board</summary>
        internal Pawn(Player player, Board board) : base(player, true, board) { }

        internal override IEnumerable<Square> FindMoves() => isPromoted ? GoldMoves() : PawnMove();

        private IEnumerable<Square> PawnMove()
        {
            Square? front = Forward();
            if (front != null && DifferentPlayer(front))
                yield return front;
        }

        internal override IEnumerable<Square> FindDrops()
        {
            int min = player.isPlayer1 ? 1 : 0;
            int max = player.isPlayer1 ? 8 : 7;
            foreach (int i in ColumnsWithoutOwnPawns())
            {
                for (int j = min; j <= max; j++)
                {
                    Square square = board.squares[i, j];
                    if (square.piece == null && !WouldCheckmate(square))
                        yield return square;
                }
            }
        }

        private IEnumerable<int> ColumnsWithoutOwnPawns()
        {
            int min = player.isPlayer1 ? 1 : 0;
            int max = player.isPlayer1 ? 8 : 7;
            bool[] colHasPawn = new bool[9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = min; j <= max; j++)
                {
                    Piece? piece = board.squares[i, j].piece;
                    if (IsOwnPawn(piece))
                    {
                        colHasPawn[i] = true;
                        break;
                    }
                }
            }
            for (int i = 0; i < colHasPawn.Length; i++)
            {
                if (!colHasPawn[i])
                    yield return i;
            }
        }

        private bool IsOwnPawn(Piece? piece) => piece is Pawn && !piece.isPromoted && DifferentPlayer(piece);

        // WIP
        private bool WouldCheckmate(Square square)
        {
            BetterConsole.Info($"See if {IdentifyingString()} would checkmate the opponent's king.");
            Square currentSquare = this.square;
            this.square = square;
            square.piece = this;
            player.boardPieces.Add(this);
            bool result = player.Opponent().king.IsCheckmate();
            player.boardPieces.Remove(this);
            square.piece = null;
            this.square = currentSquare;
            return result;
        }

        internal override void ForcePromote()
        {
            int row = square.rowIndex;
            if (!isPromoted && (player.isPlayer1 ? row == 0 : row == 8))
                Promote();
        }
    }
}
