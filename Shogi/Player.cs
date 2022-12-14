using ShogiWebsite.Shogi.Pieces;

namespace ShogiWebsite.Shogi
{
    internal class Player
    {
        internal readonly Board board;
        internal readonly bool isPlayer1;
        // to Dict<Type, int>
        internal Dictionary<Type, int> hand = new();
        internal King king;
        internal bool isCheck;
        internal bool isCheckmate;
        internal Dictionary<string, IEnumerable<Coordinate>> moveLists;
        internal Dictionary<string, IEnumerable<Coordinate>> dropLists;

        internal Player(Board board, bool isPlayer1)
        {
            this.board = board;
            this.isPlayer1 = isPlayer1;
            // hand = new();
            king = new King(this, board);
            isCheck = false;
            isCheckmate = false;
            moveLists = new();
            dropLists = new();
        }

        internal void InitLater()
        {
            hand = InitHand();
            king = PlayersKing();
        }

        internal string PlayerNumber() => $"Player {(isPlayer1 ? 1 : 2)}";

        internal void AfterOpponent()
        {
            isCheck = isCheckmate = false;
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

        internal void PrepareTurn()
        {
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
            List<Coordinate> newMoves = new();
            foreach (Coordinate square in moveLists[board.CoordinateString(king.pos)])
            {
                if (!king.DoesMoveCheckOwnKing(square))
                    newMoves.Add(square);
            }
            moveLists[board.CoordinateString(king.pos)] = newMoves.AsEnumerable();
        }

        private void RemoveDangerousMoves()
        {
            Dictionary<string, IEnumerable<Coordinate>> newMoves = new();
            foreach (KeyValuePair<string, IEnumerable<Coordinate>> list in moveLists)
            {
                Coordinate? pos = board.GetSquareByCoordinate(list.Key);
                if (pos == null)
                    continue;
                Piece? piece = board.PieceAt(pos.Value);
                if (piece == null)
                    continue;
                foreach (Coordinate move in list.Value)
                {
                    if (piece is King king && !king.WouldBeCheckAt(move) || !piece.DoesMoveCheckOwnKing(move))
                        newMoves[list.Key].Append(move);
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

        private static Dictionary<Type, int> InitHand()
        {
            Dictionary<Type, int> hand = new();
            hand[typeof(Pawn)] = 0;
            hand[typeof(Lance)] = 0;
            hand[typeof(Knight)] = 0;
            hand[typeof(Silver)] = 0;
            hand[typeof(Gold)] = 0;
            hand[typeof(Bishop)] = 0;
            hand[typeof(Rook)] = 0;
            return hand;
        }

        private static KeyValuePair<Type, int> NewHandPiece(Piece piece, int newAmount) => piece switch
        {
            Pawn _ => new KeyValuePair<Type, int>(typeof(Pawn), newAmount),
            Bishop _ => new KeyValuePair<Type, int>(typeof(Bishop), newAmount),
            Rook _ => new KeyValuePair<Type, int>(typeof(Rook), newAmount),
            Lance _ => new KeyValuePair<Type, int>(typeof(Lance), newAmount),
            Knight _ => new KeyValuePair<Type, int>(typeof(Knight), newAmount),
            Silver _ => new KeyValuePair<Type, int>(typeof(Silver), newAmount),
            Gold _ => new KeyValuePair<Type, int>(typeof(Gold), newAmount),
            _ => new KeyValuePair<Type, int>(typeof(Pawn), newAmount),
        };

        internal Dictionary<string, IEnumerable<Coordinate>> GetMoveLists()
        {
            var dict = new Dictionary<string, IEnumerable<Coordinate>>();
            foreach (var piece in PlayersPieces())
                dict[board.CoordinateString(piece.pos)] = piece.FindMoves();
            return dict;
        }

        internal Dictionary<string, IEnumerable<Coordinate>> GetDropLists()
        {
            var dict = new Dictionary<string, IEnumerable<Coordinate>>();
            foreach (var piece in hand)
            {
                if (piece.Value > 0)
                    dict[Names.Abbreviation(piece.Key)] = piece.Key.FindDrops();
            }
            return dict;
        }

        internal void ChangeHandPieceAmount(Piece piece, int change) => hand[piece.GetType()] += change;

        internal Piece PieceFromHandByAbbr(string abbr) => abbr switch
        {
            "P" => new Pawn(this, board),
            "L" => new Lance(this, board),
            "N" => new Knight(this, board),
            "S" => new Silver(this, board),
            "G" => new Gold(this, board),
            "B" => new Bishop(this, board),
            "R" => new Rook(this, board),
            _ => new Pawn(this, board)
        };

        internal HtmlBuilder HtmlHand()
        {
            HtmlBuilder builder = new HtmlBuilder().Class("hand");
            foreach (KeyValuePair<Type, int> handPiece in hand)
            {
                Type piece = handPiece.Key;
                int amount = handPiece.Value;
                string image = Images.Get(piece);
                HtmlBuilder htmlHandPiece = new HtmlBuilder().Class("handPiece").Child(amount);
                htmlHandPiece.Style($"background-image:url('data:image/png;base64,{image}')");
                if (amount > 0 && board.IsPlayersTurn(this) && !board.isOver)
                {
                    string abbr = Names.Abbreviation(piece);
                    htmlHandPiece.Id(abbr).Property("onclick", $"selectMoves('{abbr}')");
                }
                builder.Child(htmlHandPiece);
            }
            return builder;
        }

        internal IEnumerable<Piece> PlayersPieces()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Piece? piece = board.pieces[i, j];
                    if (piece != null && this == piece.player)
                        yield return piece;
                }
            }
        }

        internal IEnumerable<T> AllPiecesOfType<T>() where T : Piece
        {
            foreach (Piece piece in PlayersPieces())
            {
                if (piece is T t)
                    yield return t;
            }
        }

        internal King PlayersKing()
        {
            foreach (Piece piece in PlayersPieces())
            {
                if (piece is King king)
                    return king;
            }
            throw new Exception("No king of this player was found, even though it should have already been initialized.");
        }

        internal string JavascriptMoveLists()
        {
            string text = "var squareMoveDict = {";
            Dictionary<string, IEnumerable<Coordinate>> dict = moveLists;
            dict = dict.Concat(dropLists).ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<string, IEnumerable<Coordinate>> entry in dict)
                text += $"{entry.Key}: {JavascriptSquareList(entry.Value)}, ";
            int last = text.LastIndexOf(',');
            if (last >= 0)
                text = text[..last];
            return text + "};";
        }

        private string JavascriptSquareList(IEnumerable<Coordinate> squares)
        {
            int length = squares.Count();
            string text = "[";
            if (length <= 0)
                return text + "]";
            text += $"\"{board.CoordinateString(squares[0])}\"";
            for (int i = 1; i < length; i++)
                text += $", \"{board.CoordinateString(squares[i])}\"";
            return text + "]";
        }
    }
}