namespace Blank.Shogi.Pieces
{
    internal class GoldGeneral : AbstractPiece
    {
        /// <summary>Gold General on the board</summary>
        internal GoldGeneral(Player player, Square square) : base(player, false, square) {
        }

        /// <summary>Gold General on hand<br/>Does not contain an actual square on the board</summary>
        internal GoldGeneral(Player player, Board board) : base(player, false, board) {
        }

        internal override List<Square> FindMoves() {
            return GoldMoves();
        }
    }
}
