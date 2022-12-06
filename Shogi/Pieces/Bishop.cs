namespace Blank.Shogi.Pieces
{
    internal class Bishop : AbstractPiece
    {
        /// <summary>Bishop on the board</summary>
        internal Bishop(Player player, Square square) : base(player, true, square) {
        }

        /// <summary>Bishop on hand<br/>Does not contain an actual square on the board</summary>
        internal Bishop(Player player, Board board) : base(player, true, board) {
        }

        internal override List<Square> FindMoves() {
            List<Square> newMoves = new();
            /*
            Square? temp = square;
            Func<int, Square?>[] directions = new Func<int, Square?>[] { square.NorthEast, temp.NorthWest, temp.SouthEast, temp.SouthWest };
            for (int i = 0; i < directions.Length; i++) {
                temp = square;
                int distance = 1;
                bool flag = true;
                while (flag) {
                    temp = directions[i](distance);
                    if (temp == null) {
                        flag = false;
                        break;
                    }
                    flag = CanContinue(temp);
                    if (Available(temp)) {
                        newMoves.Add(temp);
                    }
                    distance++;
                }
            }
            */
            newMoves.AddRange(RangeMoves(new Func<int, bool, Square?>[] { square.NorthEast, square.NorthWest, square.SouthEast, square.SouthWest }));
            if (isPromoted) {
                /*
                List<Square?> sList = new();
                sList.Add(GetAvailable(square.North));
                sList.Add(GetAvailable(square.East));
                sList.Add(GetAvailable(square.South));
                sList.Add(GetAvailable(square.West));
                for (int i = 0; i < sList.Count; i++) {
                    Square? s1 = sList[i];
                    if (s1 != null) {
                        newMoves.Add(s1);
                    }
                }
                */
                newMoves.AddRange(ListMoves(new Func<int, bool, Square?>[] { square.North, square.East, square.South, square.West }));
            }
            return newMoves;
        }
    }
}
