using Blank.Shogi.Pieces;
using Blank;

namespace Blank.Shogi {
    internal abstract class AbstractPiece {
        internal Player player;
        internal bool isPromoted;
        internal readonly bool canPromote;
        internal Board board;
        internal Square square;
        protected List<Square> moves;
        protected List<Square> drops;
        internal bool isOnBoard;

        internal List<Square> GetMoves() {
            return new List<Square>(moves);
        }

        internal List<Square> GetDrops() {
            return new List<Square>(drops);
        }

        internal void UpdateMoves() {
            BC.Action($"Update Move List for {IdentifyingString()}");
            moves = new();
            moves = FindMoves();
        }

        internal void UpdateDrops() {
            BC.Action($"Update Drop List for {IdentifyingString()}");
            drops = new();
            drops = FindDrops();
        }

        /// <summary>Constructor for a piece on the board</summary>
        internal AbstractPiece(Player player, bool canPromote, Square square) {
            this.player = player;
            isPromoted = false;
            this.canPromote = canPromote;
            board = square.board;
            this.square = square;
            isOnBoard = true;
            moves = new();
            drops = new();
        }

        /// <summary>Piece on hand.<br/>Does not contain an actual square on the board</summary>
        internal AbstractPiece(Player player, bool canPromote, Board board) {
            this.player = player;
            isPromoted = false;
            this.canPromote = canPromote;
            this.board = board;
            square = board.nullSquare;
            isOnBoard = false;
            moves = new();
            drops = new();
        }

        /// <summary>
        /// Move this piece from one square to another.<br/>
        /// This piece might promote.<br/>
        /// This piece might capture one of the other player's pieces.<br/>
        /// </summary>
        /// <param name="to">Desitination square</param>
        /// <param name="doesPromote">Does this piece promote?</param>
        /// <returns><c>true</c> if this move was successful</returns>
        internal bool Move(Square to, bool doesPromote) {
            List<Square> s = GetMoves();
            string piece1S = IdentifyingString();
            string toS = to.CoordinateString();
            BC.Info($"Trying to move {piece1S} to {toS}.");
            if (s != null && s.Contains(to)) {
                string piece2S = "";
                Square old = square;
                AbstractPiece? piece = to.piece;
                char moveType = '-';
                if (piece != null && piece.player != player) {
                    BC.Info($"There is {piece2S}.");
                    piece2S = piece.IdentifyingString();
                    if (piece is not King) {
                        player.ChangeHandPieceAmount(piece, 1);
                        moveType = 'x';
                    }
                }
                old.piece = null;
                to.piece = this;
                square = to;
                string part1 = $"{PieceNames.Abbr(this)}{old.CoordinateString()}";
                bool wasPromoted = isPromoted;
                ForcePromote();
                if (wasPromoted != isPromoted) doesPromote = true;
                string part2 = $"{moveType}{to.CoordinateString()}{(doesPromote ? "+" : "")}";
                board.log.Add($"Player {(player.isPlayer1 ? 1 : 2)} : {part1}{part2}");
                if (doesPromote) isPromoted = true;
                return true;
            }
            BC.Error("Illegal move!");
            return false;
        }

        /// <summary>
        /// Move this piece from the player's hand to <paramref name="to"/>.<br/>
        /// This move will be written to the game log.
        /// </summary>
        /// <param name="to">Destination square of this piece</param>
        /// <returns><c>true</c> if this move was successful</returns>
        internal bool MoveFromHand(Square to) {
            List<Square> s = GetDrops();
            AbstractPiece? piece = to.piece;
            string pieceS = IdentifyingString();
            string toS = to.CoordinateString();
            BC.Info($"Trying to drop {pieceS} on {toS}.");
            if (s != null && s.Contains(to) && piece == null && piece is not King) {
                player.ChangeHandPieceAmount(this, -1);
                BC.Action($"Removed 1 {PieceNames.Get(this)} from Player {(player.isPlayer1 ? "1" : "2")}'s hand.");
                to.piece = this;
                to.piece.isOnBoard = true;
                square = to;
                board.log.Add($"Player {(player.isPlayer1 ? 1 : 2)} : {PieceNames.Abbr(this)}*{to.column}{to.row}");
                BC.Action($"Dropped {pieceS} on {toS}.");
                return true;
            }
            BC.Error($"{pieceS} cannot perform this move!");
            return false;
        }

        /// <summary>Simulate Move And See If Own King Is Check</summary>
        internal bool Smasiokic(Square to) {
            bool result = false;
            // Save old state
            Square oldSquare = square;
            AbstractPiece? oldToPiece = to.piece;
            // Make new state
            to.piece = this;
            square = to;
            oldSquare.piece = null;
            player.UpdateMoves();
            player.Opponent().UpdateMoves();
            // save result
            if (player.king.IsCheck()) {
                result = true;
            }
            // restore old state
            to.piece = oldToPiece;
            if (oldToPiece != null) {
                oldToPiece.square = to;
            }
            oldSquare.piece = this;
            square = oldSquare;
            UpdateMoves();
            player.Opponent().UpdateMoves();
            return result;
        }

        internal void Promote() {
            string pieceS = IdentifyingString();
            BC.Info($"Trying to promote {pieceS}.");
            if (canPromote) {
                isPromoted = true;
                BC.Action($"{pieceS} was promoted.");
            }
            else BC.Error($"{pieceS} cannot be promoted!");
        }

        internal string IdentifyingString() {
            return $"Player {(player.isPlayer1 ? "1" : "2")}'s {PieceNames.Get(this)} on {square.CoordinateString()}";
        }

        internal virtual void ForcePromote() { }

        internal bool CanMoveTo(Square square) {
            BC.Info($"See if {IdentifyingString()} can move to {square.CoordinateString()}.");
            List<Square> availableSquares = GetMoves();
            if (availableSquares == null) return false;
            return availableSquares.Contains(square);
        }

        /// <summary>Find all squares this piece can currently move to.</summary>
        internal abstract List<Square> FindMoves();

        /// <summary>
        /// Find all squares where this piece can be dropped from the player's hand
        /// </summary>
        internal virtual List<Square> FindDrops() {
            BC.Info($"Trying to find squares to move to for {IdentifyingString()}");
            List<Square> newDrops = new();
            for (int i = 0; i < 9; i++) {
                for (int j = 0; j < 9; j++) {
                    Square square = board.squares[i, j];
                    if (square.piece == null) {
                        newDrops.Add(square);
                    }
                }
            }
            return newDrops;
        }

        protected List<Square> GoldMoves() {
            /*
            List<Square> newMoves = new();
            List<Square?> sList = new();
            sList.Add(GetAvailable(Forward));
            sList.Add(GetAvailable(FrontLeft));
            sList.Add(GetAvailable(FrontRight));
            sList.Add(GetAvailable(Left));
            sList.Add(GetAvailable(Right));
            sList.Add(GetAvailable(Back));
            for (int i = 0; i < sList.Count; i++) {
                Square? s1 = sList[i];
                if (s1 != null) newMoves.Add(s1);
            }
            return newMoves;
            */
            return ListMoves(new Func<int, bool, Square?>[] { Forward, FrontLeft, FrontRight, Left, Right, Back});
        }

        /// <summary>Find lines of squares in multiple <paramref name="directions"/>.</summary>
        protected List<Square> RangeMoves(Func<int, bool, Square?>[] directions) {
            List<Square> newMoves = new();
            for (int i = 0; i < directions.Length; i++) {
                // Square? temp = square;
                int distance = 1;
                bool flag = true;
                while (flag) {
                    Square? temp = directions[i](distance, true);
                    if (temp == null) {
                        break;
                    }
                    flag = CanContinue(temp);
                    if (Available(temp)) {
                        newMoves.Add(temp);
                    }
                    distance++;
                }
            }
            return newMoves;
        }

        // See if I should add the ability to use different distances, as of now it seems kinda useless.
        /// <summary>Find one square in each of the given <paramref name="directions"/>.</summary>
        protected List<Square> ListMoves(Func<int, bool, Square?>[] directions) {
            List<Square> newMoves = new();
            List<Square?> sList = new();
            foreach(Func<int, bool, Square?> direction in directions) {
                sList.Add(GetAvailable(direction));
            }
            for (int i = 0; i < sList.Count; i++) {
                Square? s1 = sList[i];
                if (s1 != null) newMoves.Add(s1);
            }
            return newMoves;
        }

        /// <summary>
        /// Check if to continue a loop or if this piece can move further
        /// </summary>
        /// <param name="square">Current square in a loop</param>
        protected static bool CanContinue(Square? square) {
            if (square == null) return false;
            AbstractPiece? piece = square.piece;
            if (piece == null) return true;
            return false;
        }

        /// <summary>
        /// Check if a square is available to move to.
        /// Does however not check if this piece can move there.
        /// </summary>
        /// <param name="square">Square to check</param>
        protected bool Available(Square? square) {
            if (square == null) return false;
            AbstractPiece? piece = square.piece;
            if (piece == null) return true;
            if (piece.player == player) return false;
            return true;
        }

        /// <summary>Checks if a square in a set distance is available</summary>
        /// <param name="func">Function to get a square in a certain direction</param>
        /// <param name="n">Distance to get a square in a certain direction</param>
        /// <returns>
        /// <paramref name="func"/>(<paramref name="n"/>) if it is available,<br/>
        /// <c>null</c> if it's not
        /// </returns>
        protected virtual Square? GetAvailable(Func<int, bool, Square?> func, int n = 1) {
            Square? square = func(n, true);
            if (square != null && Available(square)) return square;
            return null;
        }

        internal bool DifferentPlayerAt(Square square) {
            string destS = square.CoordinateString();
            BC.Info($"See if there is another player or no player on {destS}");
            AbstractPiece? piece = square.piece;
            if (piece == null) {
                BC.Info($"There is no piece on {destS}");
                return true;
            }
            else if (piece.player != player) {
                BC.Info($"There is another player's piece on {destS}");
                return true;
            }
            else {
                BC.Info($"There is the same player's piece on {destS}");
                return false;
            }
        }

        internal Square? Forward(int distance = 1, bool log = true) {
            if (log) {
                BC.Info($"Looking {distance} squares in front of {IdentifyingString()}");
            }
            return player.isPlayer1 ? square.North(distance, false) : square.South(distance, false);
        }

        internal Square? Back(int distance = 1, bool log = true) {
            if (log) {
                BC.Info($"Looking {distance} squares behind {IdentifyingString()}");
            }
            return player.isPlayer1 ? square.South(distance, false) : square.North(distance, false);
        }

        internal Square? Left(int distance = 1, bool log = true) {
            if (log) {
                BC.Info($"Looking {distance} squares left to {IdentifyingString()}");
            }
            return player.isPlayer1 ? square.West(distance, false) : square.East(distance, false);
        }

        internal Square? Right(int distance = 1, bool log = true) {
            if (log) {
                BC.Info($"Looking {distance} squares right to {IdentifyingString()}");
            }
            return player.isPlayer1 ? square.East(distance, false) : square.West(distance, false);
        }

        internal Square? FrontLeft(int distance = 1, bool log = true) {
            if (log) {
                BC.Info($"Looking {distance} squares to the front left of {IdentifyingString()}");
            }
            return player.isPlayer1 ? square.NorthWest(distance, false) : square.SouthEast(distance, false);
        }

        internal Square? FrontRight(int distance = 1, bool log = true) {
            if (log) {
                BC.Info($"Looking {distance} squares to the front right of {IdentifyingString()}");
            }
            return player.isPlayer1 ? square.NorthEast(distance, false) : square.SouthWest(distance, false);
        }

        internal Square? BackLeft(int distance = 1, bool log = true) {
            if (log) {
                BC.Info($"Looking {distance} squares to the back left of {IdentifyingString()}");
            }
            return player.isPlayer1 ? square.SouthWest(distance, false) : square.NorthEast(distance, false);
        }

        internal Square? BackRight(int distance = 1, bool log = true) {
            if (log) {
                BC.Info($"Looking {distance} squares to the back right of {IdentifyingString()}");
            }
            return player.isPlayer1 ? square.SouthEast(distance, false) : square.NorthWest(distance, false);
        }

        internal Square? KnightMoveLeft() {
            BC.Info($"Looking one knight's move away to the left of {IdentifyingString()}");
            return square.KnightMove(player, true);
        }

        internal Square? KnightMoveRight() {
            BC.Info($"Looking one knight's move away to the right of {IdentifyingString()}");
            return square.KnightMove(player, false);
        }

        internal string CoordinateString() => square.CoordinateString();
    }
}
