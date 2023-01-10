using ShogiWebsite.Shogi.Pieces;

namespace ShogiWebsite.Shogi;

internal class Player
{
    internal readonly Board board;
    internal readonly bool isPlayer1;
    internal Dictionary<Type, int> hand;
    internal King king;
    internal bool isCheck;
    internal bool isCheckmate;
    internal Dictionary<string, IEnumerable<Coordinate>> moveLists;
    internal Dictionary<string, IEnumerable<Coordinate>> dropLists;


    internal Player(Board board, bool isPlayer1)
    {
        this.board = board;
        this.isPlayer1 = isPlayer1;
        hand = new();
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
        {
            RemoveDangerousKingMoves();
        }
        else
        {
            RemoveDangerousMoves();
        }

    }


    private void RemoveDangerousKingMoves()
    {
        List<Coordinate> newMoves = new();
        string kingPos = board.CoordinateString(king.pos);
        IEnumerable<Coordinate> kingMoves = moveLists[kingPos];
        foreach (Coordinate square in kingMoves)
        {
            if (!king.DoesMoveCheckOwnKing(square))
                newMoves.Add(square);
        }
        moveLists[kingPos] = newMoves.AsEnumerable();
    }


    private void RemoveDangerousMoves()
    {
        Dictionary<string, IEnumerable<Coordinate>> newMoves = new();
        foreach (KeyValuePair<string, IEnumerable<Coordinate>> list in moveLists)
        {
            Coordinate? pos = board.CoordByString(list.Key);
            if (pos == null)
                continue;
            Piece? piece = board.PieceAt(pos.Value);
            if (piece == null)
                continue;
            foreach (Coordinate move in list.Value)
            {
                bool safeKingMove = piece is King king && !king.WouldBeCheckAt(move);
                bool noSelfCheck = !piece.DoesMoveCheckOwnKing(move);
                if (safeKingMove || noSelfCheck)
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
            throw new Exception("Couldn't find opponent for this player.");
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
        foreach (Piece piece in PlayersPieces())
            dict[board.CoordinateString(piece.pos)] = piece.FindMoves();
        return dict;
    }


    internal Dictionary<string, IEnumerable<Coordinate>> GetDropLists()
    {
        Dictionary<string, IEnumerable<Coordinate>> dict = new();
        foreach (KeyValuePair<Type, int> piece in hand)
        {
            if (piece.Value > 0)
            {
                object[] parameters = new object[] { this, board };
                Piece? tempPiece = (Piece?)Activator.CreateInstance(piece.Key, parameters);
                if (tempPiece != null)
                    dict[Names.Abbreviation(piece.Key)] = tempPiece.FindDrops();
            }
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
            // string image = Images.Get(piece);
            HtmlBuilder htmlHandPiece = new HtmlBuilder()
                .Class("handPiece")
                .Child(amount)
                .Style($"background-image:url('{Images.Get(piece)}')");
            if (amount > 0 && board.IsPlayersTurn(this) && !board.isOver)
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
                Piece? piece = board.pieces[i, j];
                if (piece != null && this == piece.player)
                    yield return piece;
            }
        }
    }


    internal King PlayersKing()
    {
        foreach (Piece piece in PlayersPieces())
        {
            if (piece is King king)
                return king;
        }
        throw new Exception("No king of this player was found, even though it should have.");
    }
}