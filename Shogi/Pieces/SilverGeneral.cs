namespace Blank.Shogi.Pieces {
    internal class SilverGeneral : AbstractPiece {
        /// <summary>Silver General on the board</summary>
        internal SilverGeneral(Player player, Square square) : base(player, true, square) {
        }

        /// <summary>Silver General on hand<br/>Does not contain an actual square on the board</summary>
        internal SilverGeneral(Player player, Board board) : base(player, true, board) {
        }

        internal override List<Square> FindMoves() {
            return isPromoted ? GoldMoves() : SilverMoves();
        }

        private List<Square> SilverMoves() {
            /*
            List<Square> newMoves = new();
            List<Square?> sList = new();
            sList.Add(GetAvailable(Forward));
            sList.Add(GetAvailable(FrontLeft));
            sList.Add(GetAvailable(FrontRight));
            sList.Add(GetAvailable(BackLeft));
            sList.Add(GetAvailable(BackRight));
            for (int i = 0; i < sList.Count; i++) {
                Square? s1 = sList[i];
                if (s1 != null) {
                    newMoves.Add(s1);
                }
            }
            return newMoves;
            */
            return ListMoves(new Func<int, bool, Square?>[] { Forward, FrontLeft, FrontRight, BackLeft, BackRight });
        }
    }
}
