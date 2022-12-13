namespace ShogiWebsite.Shogi.Pieces
{
    internal class Pawn : Piece
    {
        /// <summary>Pawn on the board</summary>
        internal Pawn(Player player, Board board, Coordinate coordinate) : base(player, true, board, coordinate)
        { }

        internal Pawn(Player player, Board board, int column, int row) : base(player, true, board, column, row)
        { }

        /// <summary>Pawn on hand<br/>Does not contain an actual square on the board</summary>
        internal Pawn(Player player, Board board) : base(player, true, board)
        { }

        internal override IEnumerable<Coordinate> FindMoves() => isPromoted ? GoldMoves() : PawnMove();

        private IEnumerable<Coordinate> PawnMove()
        {
            Coordinate? front = Front()(coordinate, 1, true);
            if (front != null && DifferentPlayer(front.Value))
                yield return front.Value;
        }

        internal override IEnumerable<Coordinate> FindDrops()
        {
            int min = MinDropRow(1);
            int max = MaxDropRow(1);
            foreach (int i in ColumnsWithoutOwnPawns())
            {
                for (int j = min; j <= max; j++)
                {
                    Piece? piece = board.pieces[i, j];
                    Coordinate coord = new(i, j);
                    if (piece == null && !WouldCheckmate(coord))
                        yield return coord;
                }
            }
        }

        private IEnumerable<int> ColumnsWithoutOwnPawns()
        {
            int min = MinDropRow(1);
            int max = MaxDropRow(1);
            bool[] colHasPawn = new bool[board.width];
            for (int i = 0; i < board.width; i++)
            {
                for (int j = min; j <= max; j++)
                {
                    Piece? piece = board.pieces[i, j];
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

        private bool WouldCheckmate(Coordinate square)
        {
            Coordinate currentSquare = coordinate;
            coordinate = square;
            board.SetPiece(this, square);
            bool result = player.Opponent().king.IsCheckmate();
            board.SetPiece(null, square);
            coordinate = currentSquare;
            return result;
        }

        internal override void ForcePromote() => ForcePromote(1);
    }
}
