using ShogiWebsite.Shogi.Pieces;

namespace ShogiWebsite.Shogi
{
    internal class Player
    {
        internal readonly Board board;
        internal readonly bool isPlayer1;
        internal List<Piece> boardPieces;
        // to Dict<Type, int>
        internal Dictionary<Type, int> hand = new();
        internal King king;
        internal bool isCheck;
        internal bool isCheckmate;
        internal Dictionary<string, List<Square>> moveLists;
        internal Dictionary<string, List<Square>> dropLists;

        internal Player(Board board, bool isPlayer1)
        {
            this.board = board;
            this.isPlayer1 = isPlayer1;
            boardPieces = new();
            // hand = new();
            king = new King(this, board);
            isCheck = false;
            isCheckmate = false;
            moveLists = new();
            dropLists = new();
        }

        internal void InitLater()
        {
            boardPieces = PlayersPieces();
            hand = InitHand();
            king = PlayersKing();
        }

        internal string PlayerNumber() => $"Player {(isPlayer1 ? 1 : 2)}";

        /// <summary>
        /// Call after the opponent's turn, before preparing this player's move.
        /// </summary>
        internal void AfterOpponent()
        {
            isCheck = isCheckmate = false;
            BetterConsole.Action($"Setup {PlayerNumber()}'s turn.");
            boardPieces = PlayersPieces();
            Opponent().boardPieces = Opponent().PlayersPieces();
            isCheck = king.IsCheck();
            if (isCheck)
            {
                RemoveDangerousKingMoves();
                isCheckmate = king.IsCheckmate();
                if (isCheckmate)
                {
                    board.EndGame(Opponent());
                    return;
                }
            }
        }

        /// <summary>
        /// Call before taking a turn.<br/>
        /// Don't call if the player is checkmate
        /// </summary>
        internal void PrepareTurn()
        {
            BetterConsole.Action($"Prepare {PlayerNumber()}'s turn.");
            if (isCheckmate)
            {
                board.EndGame(Opponent());
                return;
            }
            if (board.isOver)
                return;
            if (isCheck)
                RemoveDangerousKingMoves();
            else RemoveDangerousMoves();
        }

        private void RemoveDangerousKingMoves()
        {
            List<Square> newMoves = new();
            foreach (Square square in moveLists[king.square.CoordinateString()])
            {
                if (!king.DoesMoveCheckOwnKing(square))
                    newMoves.Add(square);
            }
            moveLists[king.square.CoordinateString()] = newMoves;
        }

        /// <summary>
        /// Remove any move of any piece that would land the king in check.
        /// </summary>
        private void RemoveDangerousMoves()
        {
            BetterConsole.Action("Remove any dangerous moves this player could possibly make.");
            Dictionary<string, List<Square>> newMoves = new();
            foreach (KeyValuePair<string, List<Square>> list in moveLists)
            {
                Square? square = board.GetSquareByCoordinate(list.Key);
                if (square == null)
                    continue;
                Piece? piece = square.piece;
                if (piece == null)
                    continue;
                foreach (Square square1 in list.Value)
                {
                    if (piece is King king && !king.WouldBeCheckAt(square1) || !piece.DoesMoveCheckOwnKing(square1))
                    {
                        string dictKey = list.Key;
                        if (newMoves.ContainsKey(dictKey))
                            newMoves[dictKey].Add(square1);
                        else
                        {
                            var newValue = new KeyValuePair<string, List<Square>>(dictKey, new List<Square> { square1 });
                            newMoves = newMoves.Append(newValue).ToDictionary(x => x.Key, x => x.Value);
                        }
                    }
                }
            }
            moveLists = newMoves;
        }

        internal Player Opponent()
        {
            if (this == board.player1)
                return board.player2;
            else if (this == board.player2)
                return board.player1;
            else
                throw new Exception("Couldn't find opponent for this player. The opponent may not be initialized.");
        }

        private Dictionary<Type, int> InitHand()
        {
            KeyValuePair<Piece, int>[] hand = new KeyValuePair<Piece, int>[7];
            hand[0] = new KeyValuePair<Piece, int>(new Pawn(this, board), 0);
            hand[1] = new KeyValuePair<Piece, int>(new Lance(this, board), 0);
            hand[2] = new KeyValuePair<Piece, int>(new Knight(this, board), 0);
            hand[3] = new KeyValuePair<Piece, int>(new Silver(this, board), 0);
            hand[4] = new KeyValuePair<Piece, int>(new Gold(this, board), 0);
            hand[5] = new KeyValuePair<Piece, int>(new Bishop(this, board), 0);
            hand[6] = new KeyValuePair<Piece, int>(new Rook(this, board), 0);
            return hand;
        }

        private KeyValuePair<Piece, int> NewHandPiece(Piece piece, int newAmount) => piece switch
        {
            Pawn _ => new KeyValuePair<Piece, int>(new Pawn(this, board), newAmount),
            Bishop _ => new KeyValuePair<Piece, int>(new Bishop(this, board), newAmount),
            Rook _ => new KeyValuePair<Piece, int>(new Rook(this, board), newAmount),
            Lance _ => new KeyValuePair<Piece, int>(new Lance(this, board), newAmount),
            Knight _ => new KeyValuePair<Piece, int>(new Knight(this, board), newAmount),
            Silver _ => new KeyValuePair<Piece, int>(new Silver(this, board), newAmount),
            Gold _ => new KeyValuePair<Piece, int>(new Gold(this, board), newAmount),
            _ => new KeyValuePair<Piece, int>(new Pawn(this, board), newAmount),
        };

        internal Dictionary<string, IEnumerable<Square>> GetMoveLists()
        {
            var dict = new Dictionary<string, IEnumerable<Square>>();
            foreach (var piece in boardPieces)
                dict[piece.square.CoordinateString()] = piece.FindMoves();
            return dict;
        }

        internal Dictionary<string, IEnumerable<Square>> GetDropLists()
        {
            var dict = new Dictionary<string, IEnumerable<Square>>();
            foreach (var piece in hand)
            {
                if (piece.Value > 0)
                    dict[Names.Abbreviation(piece.Key)] = piece.Key.FindDrops();
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
        internal void ChangeHandPieceAmount(Piece piece, int change)
        {
            Type pieceType = piece.GetType();
            if (piece is King)
                return;
            int index = -1;
            int amount = 0;
            for (int i = 0; i < hand.Length; i++)
            {
                KeyValuePair<Piece, int> handPiece = hand[i];
                if (handPiece.Key.GetType() == pieceType)
                {
                    index = i;
                    amount = handPiece.Value;
                    break;
                }
            }
            if (index < 0)
                return;
            hand[index] = NewHandPiece(piece, amount + change);
        }

        /// <summary>Find the piece on this player's hand by the abbreviation <paramref name="abbr"/>.</summary>
        /// <param name="abbr"></param>
        internal Piece PieceFromHandByAbbr(string abbr) => abbr switch
        {
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
        internal string HtmlHand()
        {
            string text = "<div class=\"hand\">";
            foreach (KeyValuePair<Piece, int> handPiece in hand)
            {
                Piece piece = handPiece.Key;
                int amount = handPiece.Value;
                string image = Images.Get(piece);
                text += $"<div class=\"handPiece\" style=\"background-image:url('data:image/png;base64,{image}')\"";
                if (amount > 0 && board.IsPlayersTurn(this) && !board.isOver)
                {
                    string abbr = Names.Abbreviation(piece);
                    text += $" id=\"{abbr}\" onclick=\"selectMoves('{abbr}')\"";
                }
                text += $">{amount}</div>";
            }
            return text + "</div>";
        }

        /// <summary>Find all pieces of this player on the board.</summary>
        internal List<Piece> PlayersPieces()
        {
            List<Piece> pieces = new();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Piece? piece = board.squares[i, j].piece;
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
        internal List<T> AllPiecesOfType<T>() where T : Piece
        {
            List<T> pieces = new();
            foreach (Piece piece in boardPieces)
            {
                if (piece is T t)
                    pieces.Add(t);
            }
            return pieces;
        }

        /// <summary>Find this player's king on the board</summary>
        /// <exception cref="Exception"/>
        internal King PlayersKing()
        {
            foreach (Piece piece in boardPieces)
            {
                if (piece is King king)
                    return king;
            }
            throw new Exception("No king of this player was found, even though it should have already been initialized.");
        }

        /// <summary>
        /// Convert move dictioniaries to a javascript dictionary variable.
        /// </summary>
        internal string JavascriptMoveLists()
        {
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

        private static string JavascriptSquareList(List<Square> squares)
        {
            int length = squares.Count;
            string text = "[";
            if (length <= 0)
                return text + "]";
            text += $"\"{squares[0].CoordinateString()}\"";
            for (int i = 1; i < length; i++)
                text += $", \"{squares[i].CoordinateString()}\"";
            return text + "]";
        }
    }
}
