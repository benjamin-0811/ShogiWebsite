using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Blank.Shogi.Pieces;

namespace Blank.Shogi {
    internal class Player {
        internal Board board;
        internal bool isPlayer1;
        internal List<AbstractPiece> boardPieces;
        internal KeyValuePair<AbstractPiece, int>[] hand;
        internal King king;
        internal bool isCheck;
        internal bool isCheckmate;
        internal Dictionary<string, List<Square>> moveLists;
        internal Dictionary<string, List<Square>> dropLists;

        internal Player(Board board, bool isPlayer1) {
            this.board = board;
            this.isPlayer1 = isPlayer1;
            boardPieces = new();
            hand = Array.Empty<KeyValuePair<AbstractPiece, int>>();
            king = new King(this, board);
            isCheck = false;
            isCheckmate = false;
            moveLists = new();
            dropLists = new();
        }

        internal void InitLater() {
            boardPieces = PlayersPieces();
            hand = InitHand();
            king = PlayersKing();
        }

        /// <summary>
        /// Call after the opponent's turn, before preparing this player's move.
        /// </summary>
        internal void AfterOpponent() {
            isCheck = isCheckmate = false;
            BC.Action($"Setup Player {(isPlayer1 ? 1 : 2)}'s turn.");
            boardPieces = PlayersPieces();
            Opponent().boardPieces = Opponent().PlayersPieces();
            king.ResetProtection();
            UpdateMoves();
            UpdateDrops();
            Opponent().UpdateMoves();
            moveLists = GetMoveLists();
            dropLists = GetDropLists();
            isCheck = king.IsCheck();
            if (isCheck) {
                RemoveDangerousKingMoves();
                isCheckmate = king.IsCheckmate();
                if (isCheckmate) {
                    board.EndGame(Opponent());
                    return;
                }
            }
        }

        internal void UpdateMoves() {
            boardPieces = PlayersPieces();
            foreach (AbstractPiece piece in boardPieces) {
                piece.UpdateMoves();
            }
        }

        internal void UpdateDrops() {
            foreach (KeyValuePair<AbstractPiece, int> piece in hand) {
                if (piece.Value > 0) {
                    piece.Key.UpdateDrops();
                }
            }
        }

        /// <summary>
        /// Call before taking a turn.<br/>
        /// Don't call if the player is checkmate
        /// </summary>
        internal void PrepareTurn() {
            BC.Action($"Prepare Player {(isPlayer1 ? 1 : 2)}'s turn.");
            if (isCheckmate) {
                board.EndGame(Opponent());
                return;
            }
            moveLists = GetMoveLists();
            dropLists = GetDropLists();
            if (board.isOver) return;
            if (isCheck) {
                moveLists = king.protectMoves;
                moveLists[king.square.CoordinateString()] = king.GetMoves();
                RemoveDangerousKingMoves();
                dropLists = king.protectDrops;
            }
            else RemoveDangerousMoves();
        }

        private void RemoveDangerousKingMoves() {
            List<Square> newMoves = new();
            foreach(Square square in moveLists[king.square.CoordinateString()]) {
                if (!king.Smasic(square)) {
                    newMoves.Add(square);
                }
            }
            moveLists[king.square.CoordinateString()] = newMoves;
            king.SetMoves(newMoves);
        }

        /// <summary>
        /// Remove any move of any piece that would land the king in check.
        /// </summary>
        private void RemoveDangerousMoves() {
            BC.Action("Remove any dangerous moves this player could possibly make.");
            Dictionary<string, List<Square>> newMoves = new();
            foreach (KeyValuePair<string, List<Square>> list in moveLists) {
                Square? square = board.GetSquareByCoordinate(list.Key);
                if (square == null) {
                    continue;
                }
                AbstractPiece? piece = square.piece;
                if (piece == null) {
                    continue;
                }
                foreach (Square square1 in list.Value) {
                    if (piece is King king) {
                        if (!king.WouldBeCheckAt(square1)) {
                            string dictKey = list.Key;
                            if (newMoves.ContainsKey(dictKey))
                                newMoves[dictKey].Add(square1);
                            else {
                                var newValue = new KeyValuePair<string, List<Square>>(dictKey, new List<Square> { square1 });
                                newMoves = newMoves.Append(newValue).ToDictionary(x => x.Key, x => x.Value);
                            }
                        }
                    }
                    else {
                        if (!piece.Smasiokic(square1)) {
                            string dictKey = list.Key;
                            if (newMoves.ContainsKey(dictKey))
                                newMoves[dictKey].Add(square1);
                            else {
                                var newValue = new KeyValuePair<string, List<Square>>(dictKey, new List<Square> { square1 });
                                newMoves = newMoves.Append(newValue).ToDictionary(x => x.Key, x => x.Value);
                            }
                        }
                    }
                }
            }
            moveLists = newMoves;



        }

        internal Player Opponent() {
            if (this == board.player1) return board.player2;
            else if (this == board.player2) return board.player1;
            else throw new Exception("Couldn't find opponent for this player. The opponent may not be initialized.");
        }

        private KeyValuePair<AbstractPiece, int>[] InitHand() {
            KeyValuePair<AbstractPiece, int>[] hand = new KeyValuePair<AbstractPiece, int>[7];
            hand[0] = new KeyValuePair<AbstractPiece, int>(new Pawn(this, board), 0);
            hand[1] = new KeyValuePair<AbstractPiece, int>(new Lance(this, board), 0);
            hand[2] = new KeyValuePair<AbstractPiece, int>(new Knight(this, board), 0);
            hand[3] = new KeyValuePair<AbstractPiece, int>(new SilverGeneral(this, board), 0);
            hand[4] = new KeyValuePair<AbstractPiece, int>(new GoldGeneral(this, board), 0);
            hand[5] = new KeyValuePair<AbstractPiece, int>(new Bishop(this, board), 0);
            hand[6] = new KeyValuePair<AbstractPiece, int>(new Rook(this, board), 0);
            return hand;
        }

        private KeyValuePair<AbstractPiece, int> NewHandPiece(AbstractPiece piece, int newAmount) => piece switch {
            Pawn _ => new KeyValuePair<AbstractPiece, int>(new Pawn(this, board), newAmount),
            Bishop _ => new KeyValuePair<AbstractPiece, int>(new Bishop(this, board), newAmount),
            Rook _ => new KeyValuePair<AbstractPiece, int>(new Rook(this, board), newAmount),
            Lance _ => new KeyValuePair<AbstractPiece, int>(new Lance(this, board), newAmount),
            Knight _ => new KeyValuePair<AbstractPiece, int>(new Knight(this, board), newAmount),
            SilverGeneral _ => new KeyValuePair<AbstractPiece, int>(new SilverGeneral(this, board), newAmount),
            GoldGeneral _ => new KeyValuePair<AbstractPiece, int>(new GoldGeneral(this, board), newAmount),
            _ => new KeyValuePair<AbstractPiece, int>(new Pawn(this, board), newAmount),
        };

        internal Dictionary<string, List<Square>> GetMoveLists() {
            Dictionary<string, List<Square>> dict = new();
            foreach (AbstractPiece piece in boardPieces) {
                dict[piece.square.CoordinateString()] = piece.GetMoves();
            }
            return dict;
        }

        internal Dictionary<string, List<Square>> GetDropLists() {
            Dictionary<string, List<Square>> dict = new();
            foreach (KeyValuePair<AbstractPiece, int> piece in hand) {
                if (piece.Value > 0) {
                    dict[PieceNames.Abbr(piece.Key)] = piece.Key.GetDrops();
                }
            }
            return dict;
        }

        /// <summary>
        /// Change the amount of pieces of the
        /// <paramref name="piece"/>'s kind on this player's hand.
        /// </summary>
        /// <param name="piece">
        /// piece who's amount on the player's hand<br/>
        /// should be changed (only used for its type)
        /// </param>
        /// <param name="change">
        /// amount of pieces to add to (+)<br/>
        /// or subtract from (-) the player's hand
        /// </param>
        internal void ChangeHandPieceAmount(AbstractPiece piece, int change) {
            Type pieceType = piece.GetType();
            if (pieceType == typeof(King)) return;
            int index = -1;
            int amount = 0;
            for (int i = 0; i < hand.Length; i++) {
                KeyValuePair<AbstractPiece, int> handPiece = hand[i];
                if (handPiece.Key.GetType() == pieceType) {
                    index = i;
                    amount = handPiece.Value;
                    break;
                }
            }
            if (index < 0) return;
            hand[index] = NewHandPiece(piece, amount + change);
        }

        /// <summary>Find the piece on this player's hand by the abbreviation <paramref name="abbr"/>.</summary>
        /// <param name="abbr"></param>
        internal AbstractPiece PieceFromHandByAbbr(string abbr) => abbr switch {
            "P" => hand[0].Key,
            "L" => hand[1].Key,
            "N" => hand[2].Key,
            "S" => hand[3].Key,
            "G" => hand[4].Key,
            "B" => hand[5].Key,
            "R" => hand[6].Key,
            _ => hand[0].Key
        };

        /// <summary>Convert the player's hand to its HTML representation.</summary>
        internal string HtmlHand(bool isOver) {
            string text = "<div class=\"hand\">";
            foreach (KeyValuePair<AbstractPiece, int> handPiece in hand) {
                AbstractPiece piece = handPiece.Key;
                int amount = handPiece.Value;
                string image = PieceImages.Get(piece);
                text += $"<div class=\"handPiece\" style=\"background-image:url('data:image/png;base64,{image}')\"";
                if (amount > 0 && board.IsPlayersTurn(this) && !isOver) {
                    string abbr = PieceNames.Abbr(piece);
                    text += $" id=\"{abbr}\" onclick=\"selectMoves('{abbr}')\"";
                }
                text += $">{amount}</div>";
            }
            return text + "</div>";
        }

        /// <summary>Find all pieces of this player on the board.</summary>
        internal List<AbstractPiece> PlayersPieces() {
            List<AbstractPiece> pieces = new();
            for (int i = 0; i < 9; i++) {
                for (int j = 0; j < 9; j++) {
                    AbstractPiece? piece = board.squares[i, j].piece;
                    if (piece != null && this == piece.player)
                        pieces.Add(piece);
                }
            }
            return pieces;
        }

        /// <summary>
        /// Find all <typeparamref name="T"/>(s) of this player on the board.
        /// </summary>
        /// <typeparam name="T">Type of piece to find</typeparam>
        internal List<T> AllPiecesOfType<T>() where T : AbstractPiece {
            List<T> pieces = new();
            foreach (AbstractPiece piece in boardPieces) {
                if (piece.GetType() == typeof(T))
                    pieces.Add((T)piece);
            }
            return pieces;
        }

        /// <summary>Find this player's king on the board</summary>
        /// <exception cref="Exception"/>
        internal King PlayersKing() {
            foreach (AbstractPiece piece in boardPieces) {
                if (piece.GetType() == typeof(King)) return (King)piece;
            }
            throw new Exception("No king of this player was found, even though it should have already been initialized.");
        }

        /// <summary>
        /// Convert move dictioniaries to a javascript dictionary variable.
        /// </summary>
        internal string JavascriptMoveLists() {
            string text = "var squareMoveDict = {";
            Dictionary<string, List<Square>> dict = moveLists;
            dict = dict.Concat(dropLists).ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<string, List<Square>> entry in dict)
                text += $"{entry.Key}: {JavascriptSquareList(entry.Value)}, ";
            int last = text.LastIndexOf(',');
            if (last >= 0)
                text = text[..last];
            return text + "};";
        }

        private static string JavascriptSquareList(List<Square> squares) {
            int length = squares.Count;
            string text = "[";
            if (length <= 0) return text + "]";
            text += $"\"{squares[0].CoordinateString()}\"";
            for (int i = 1; i < length; i++)
                text += $", \"{squares[i].CoordinateString()}\"";
            return text + "]";
        }
    }
}
