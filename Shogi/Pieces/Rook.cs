namespace Blank.Shogi.Pieces {
    internal class Rook : AbstractPiece {
        /// <summary>Rook on the board</summary>
        internal Rook(Player player, Square square) : base(player, true, square) {
        }

        /// <summary>Rook on hand<br/>Does not contain an actual square on the board</summary>
        internal Rook(Player player, Board board) : base(player, true, board) {
        }

        // Endless loop!
        internal override List<Square> FindMoves() {
            List<Square> newMoves = new();
            /*
            Square? temp = square;
            Func<int, Square?>[] directions = new Func<int, Square?>[] { temp.North, temp.East, temp.South, temp.West };
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
            newMoves.AddRange(RangeMoves(new Func<int, bool, Square?>[] { square.North, square.East, square.South, square.West }));
            if (isPromoted) {
                /*
                List<Square?> sList = new();
                sList.Add(GetAvailable(square.NorthEast));
                sList.Add(GetAvailable(square.NorthWest));
                sList.Add(GetAvailable(square.SouthEast));
                sList.Add(GetAvailable(square.SouthWest));
                for (int i = 0; i < sList.Count; i++) {
                    Square? s1 = sList[i];
                    if (s1 != null) {
                        newMoves.Add(s1);
                    }
                }
                */
                newMoves.AddRange(ListMoves(new Func<int, bool, Square?>[] { square.NorthEast, square.NorthWest, square.SouthEast, square.SouthWest }));
            }
            return newMoves;
        }
    }
}
