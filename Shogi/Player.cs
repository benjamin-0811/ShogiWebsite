using ShogiWebsite.Shogi.Pieces;

namespace ShogiWebsite.Shogi;

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
        this.king = new King(this, board);
        this.isCheck = false;
        this.isCheckmate = false;
        this.moveLists = new();
        this.dropLists = new();
    }

    internal void InitLater()
    {
        this.hand = InitHand();
        this.king = this.PlayersKing();
    }

    internal string PlayerNumber()
    {
        return $"Player {(this.isPlayer1 ? 1 : 2)}";
    }

    internal void AfterOpponent()
    {
        this.isCheck = this.isCheckmate = false;
        this.isCheck = this.king.IsCheck();
        if (this.isCheck)
        {
            this.RemoveDangerousKingMoves();
            this.isCheckmate = this.king.IsCheckmate();
            if (this.isCheckmate)
            {
                this.board.EndGame(this.Opponent());
                return;
            }
        }
    }

    internal void PrepareTurn()
    {
        if (this.isCheckmate)
        {
            this.board.EndGame(this.Opponent());
            return;
        }
        if (this.board.isOver)
        {
            return;
        }
        if (this.isCheck)
        {
            this.RemoveDangerousKingMoves();
        }
        else
        {
            this.RemoveDangerousMoves();
        }
            
    }

    private void RemoveDangerousKingMoves()
    {
        List<Coordinate> newMoves = new();
        string kingPos = this.board.CoordinateString(this.king.pos);
        IEnumerable<Coordinate> kingMoves = this.moveLists[kingPos];
        foreach (Coordinate square in kingMoves)
        {
            if (!this.king.DoesMoveCheckOwnKing(square))
            {
                newMoves.Add(square);
            }
        }
        this.moveLists[kingPos] = newMoves.AsEnumerable();
    }

    private void RemoveDangerousMoves()
    {
        Dictionary<string, IEnumerable<Coordinate>> newMoves = new();
        foreach (KeyValuePair<string, IEnumerable<Coordinate>> list in this.moveLists)
        {
            Coordinate? pos = this.board.CoordByString(list.Key);
            if (pos == null)
            {
                continue;
            }
            Piece? piece = this.board.PieceAt(pos.Value);
            if (piece == null)
            {
                continue;
            }
            foreach (Coordinate move in list.Value)
            {
                bool safeKingMove = piece is King king && !king.WouldBeCheckAt(move);
                bool noSelfCheck = !piece.DoesMoveCheckOwnKing(move);
                if (safeKingMove || noSelfCheck)
                {
                    newMoves[list.Key].Append(move);
                }
            }
        }
        this.moveLists = newMoves;
    }

    internal Player Opponent()
    {
        if (this == this.board.player1)
        {
            return this.board.player2;
        }
        else if (this == this.board.player2)
        {
            return this.board.player1;
        }
        else
        {
            throw new Exception("Couldn't find opponent for this player.");
        }
    }

    private static Dictionary<Type, int> InitHand()
    {
        return new Dictionary<Type, int>()
        {
            [typeof(Pawn)] = 0,
            [typeof(Lance)] = 0,
            [typeof(Knight)] = 0,
            [typeof(Silver)] = 0,
            [typeof(Gold)] = 0,
            [typeof(Bishop)] = 0,
            [typeof(Rook)] = 0
        };
    }

    internal Dictionary<string, IEnumerable<Coordinate>> GetMoveLists()
    {
        Dictionary<string, IEnumerable<Coordinate>> dict = new();
        foreach (Piece piece in this.PlayersPieces())
        {
            dict[this.board.CoordinateString(piece.pos)] = piece.FindMoves();
        }
        return dict;
    }

    internal Dictionary<string, IEnumerable<Coordinate>> GetDropLists()
    {
        Dictionary<string, IEnumerable<Coordinate>> dict = new();
        foreach (KeyValuePair<Type, int> piece in this.hand)
        {
            if (piece.Value > 0)
            {
                object[] parameters = new object[] { this, this.board };
                Piece? tempPiece = (Piece?)Activator.CreateInstance(piece.Key, parameters);
                if (tempPiece != null)
                {
                    dict[Names.Abbreviation(piece.Key)] = tempPiece.FindDrops();
                }
            }
        }
        return dict;
    }

    internal void ChangeHandPieceAmount(Piece piece, int change)
    {
        this.hand[piece.GetType()] += change;
    }

    internal Piece PieceFromHandByAbbr(string abbr) => abbr switch
    {
        "P" => new Pawn(this, this.board),
        "L" => new Lance(this, this.board),
        "N" => new Knight(this, this.board),
        "S" => new Silver(this, this.board),
        "G" => new Gold(this, this.board),
        "B" => new Bishop(this, this.board),
        "R" => new Rook(this, this.board),
        _ => new Pawn(this, this.board)
    };

    internal HtmlBuilder HtmlHand()
    {
        HtmlBuilder builder = new HtmlBuilder().Class("hand");
        foreach (KeyValuePair<Type, int> handPiece in this.hand)
        {
            Type piece = handPiece.Key;
            int amount = handPiece.Value;
            // string image = Images.Get(piece);
            HtmlBuilder htmlHandPiece = new HtmlBuilder()
                .Class("handPiece")
                .Child(amount)
                .Style($"background-image:url('{Images.Get(piece)}')");
            if (amount > 0 && this.board.IsPlayersTurn(this) && !this.board.isOver)
            {
                string abbr = Names.Abbreviation(piece);
                htmlHandPiece.Id(abbr)
                    .Property("onclick", $"selectMoves('{abbr}')");
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
                Piece? piece = this.board.pieces[i, j];
                if (piece != null && this == piece.player)
                {
                    yield return piece;
                }
            }
        }
    }

    internal King PlayersKing()
    {
        foreach (Piece piece in this.PlayersPieces())
        {
            if (piece is King king)
            {
                return king;
            }
        }
        throw new Exception("No king of this player was found, even though it should have.");
    }

    internal string JavascriptMoveLists()
    {
        string text = "var squareMoveDict = {";
        Dictionary<string, IEnumerable<Coordinate>> dict = this.moveLists;
        dict = dict.Concat(this.dropLists).ToDictionary(x => x.Key, x => x.Value);
        foreach (KeyValuePair<string, IEnumerable<Coordinate>> entry in dict)
        {
            text += $"{entry.Key}: {this.JavascriptSquareList(entry.Value)}, ";
        }
        int last = text.LastIndexOf(',');
        if (last >= 0)
        {
            text = text[..last];
        }
        return text + "};";
    }

    private string JavascriptSquareList(IEnumerable<Coordinate> squares)
    {
        return "[]";
        /*
        int length = squares.Count();
        string text = "[";
        if (length <= 0)
            return text + "]";
        text += $"\"{board.CoordinateString(squares[0])}\"";
        for (int i = 1; i < length; i++)
            text += $", \"{board.CoordinateString(squares[i])}\"";
        return text + "]";
        */
    }
}