using ShogiWebsite.Shogi.Pieces;
using System.Data.Common;
using System.Text;

namespace ShogiWebsite.Shogi;

internal struct Coordinate
{
    private int column;
    private int row;

    internal int Column { get => this.column; set => this.column = value; }
    internal int Row { get => this.row; set => this.row = value; }

    internal Coordinate(int column, int row)
    {
        this.column = column;
        this.row = row;
    }
}

internal class Board
{
    internal readonly int width;
    internal readonly int height;
    internal Piece?[,] pieces;
    internal bool isPlayer1Turn;
    internal Player player1;
    internal Player player2;
    internal HtmlBuilder log;
    internal bool isOver;
    internal Player? winner;
    internal Phase phase;

    internal readonly string[] columns;
    internal readonly string[] rows;

    internal Board()
    {
        string path = $@"{Program.projectDir}\assets\layouts\standard.txt";
        List<string> layout = File.ReadLines(path).ToList();
        string dimensions = layout[0];
        layout.RemoveAt(0);
        string[] colAndRow = dimensions.Split('x', 2);
        this.width = int.Parse(colAndRow[0]);
        this.height = int.Parse(colAndRow[1]);
        this.columns = this.Columns().ToArray();
        this.rows = this.Rows().ToArray();
        // Square [ column , row ]
        this.pieces = new Piece?[this.width, this.height];
        this.player1 = new Player(this, true);
        this.player2 = new Player(this, false);
        this.InitBoard(layout);
        //InitBoard();
        //InitCheckMate();
        this.isPlayer1Turn = true;
        this.log = new HtmlBuilder().Id("log").Class("log");
        this.isOver = false;
        this.winner = null;
        this.phase = Phase.ChoosePiece;
        this.player1.InitLater();
        this.player2.InitLater();
    }

    internal void Log(string newEntry)
    {
        this.log.Child(new HtmlBuilder("p").Child(newEntry));
    }

    private IEnumerable<string> Columns()
    {
        for (int i = 0; i < this.width; i++)
        {
            yield return $"{this.width - i}";
        }
    }

    private IEnumerable<string> Rows()
    {
        for (int i = 0; i < this.height; i++)
        {
            yield return $"{(char)('a' + i)}";
        }
    }

    internal void SetPiece(Piece? piece, Coordinate pos)
    {
        if (!this.IsOnBoard(pos))
        {
            return;
        }
        this.pieces[pos.Column, pos.Row] = piece;
        if (piece != null)
        {
            piece.pos = pos;
        }
    }

    internal void SetPiece(Piece? piece, int column, int row)
    {
        SetPiece(piece, new Coordinate(column, row));
    }

    internal string CoordinateString(Coordinate pos)
    {
        return this.IsOnBoard(pos) ? $"{this.rows[pos.Row]}{this.columns[pos.Column]}" : "hand";
    }

    internal bool IsOnBoard(Coordinate pos)
    {
        bool columnInBound = 0 <= pos.Column && pos.Column < this.width;
        bool rowInBound = 0 <= pos.Row && pos.Row < this.height;
        return columnInBound && rowInBound;
    }

    internal Piece? PieceAt(Coordinate pos)
    {
        return this.IsOnBoard(pos) ? this.pieces[pos.Column, pos.Row] : null;
    }

    internal enum Phase
    {
        ChoosePiece,
        AskForPromotion,
        SelectTarget
    }

    internal Coordinate? CoordByString(string pos)
    {
        if (pos.Length != 2)
        {
            return null;
        }
        int row = Array.IndexOf(this.rows, pos[0]);
        int column = Array.IndexOf(this.columns, pos[1]);
        return new Coordinate(column, row);
    }

    internal Piece? PieceByCoordString(string pos)
    {
        Coordinate? coord = this.CoordByString(pos);
        return coord == null ? null : this.PieceAt(coord.Value);
    }

    internal Coordinate? N(Coordinate pos, int distance = 1)
    {
        pos.Row -= distance;
        return this.IsOnBoard(pos) ? pos : null;
    }

    internal Coordinate? S(Coordinate pos, int distance = 1)
    {
        return this.N(pos, -distance);
    }

    internal Coordinate? E(Coordinate pos, int distance = 1)
    {
        pos.Column += distance;
        return this.IsOnBoard(pos) ? pos : null;
    }

    internal Coordinate? W(Coordinate pos, int distance = 1)
    {
        return this.E(pos, -distance);
    }

    internal Coordinate? NE(Coordinate pos, int distance = 1)
    {
        Coordinate? n = this.N(pos, distance);
        return n != null ? this.E(n.Value, distance) : null;
    }

    internal Coordinate? NW(Coordinate pos, int distance = 1)
    {
        Coordinate? n = this.N(pos, distance);
        return n != null ? this.W(n.Value, distance) : null;
    }

    internal Coordinate? SE(Coordinate pos, int distance = 1)
    {
        return this.NW(pos, -distance);
    }

    internal Coordinate? SW(Coordinate pos, int distance = 1)
    {
        return this.NE(pos, -distance);
    }

    private void InitBoard(List<string> layout)
    {
        int rowAmount = layout.Count;
        for (int row = 0; row < rowAmount; row++)
        {
            IEnumerable<string> pieces = layout[row].Split(',');
            int columnAmount = pieces.Count();
            for (int column = 0; column < columnAmount; column++)
            {
                Piece? piece = StringPieceConverter.GetPiece(this, pieces.ElementAt(column).Trim());
                this.SetPiece(piece, column, row);
            }
        }
    }

    internal HtmlBuilder ToHtml()
    {
        HtmlBuilder builder = new HtmlBuilder().Class("board");
        for (int i = 0; i < this.height; i++)
        {
            HtmlBuilder row = new HtmlBuilder().Class("row");
            for (int j = 0; j < this.width; j++)
            {
                row.Child(this.SquareHtml(new Coordinate(j, i)));
            }
            builder.Child(row);
        }
        return builder;
    }

    internal string JavascriptMoveLists()
    {
        Player currentPlayer = this.isPlayer1Turn ? this.player1 : this.player2;
        return currentPlayer.JavascriptMoveLists();
    }

    internal bool IsPlayersTurn(Player player)
    {
        bool isPlayer1Turn = player.isPlayer1 && this.isPlayer1Turn;
        bool isPlayer2Turn = !(player.isPlayer1 || this.isPlayer1Turn);
        return isPlayer1Turn || isPlayer2Turn;
    }

    internal HtmlBuilder LogToHtml()
    {
        return this.log;
    }

    internal void EndGame(Player winner)
    {
        this.isOver = true;
        this.winner = winner;
    }

    internal void PlayerTurn()
    {
        Player player = this.isPlayer1Turn ? this.player1 : this.player2;
        player.AfterOpponent();
        if (this.isOver)
        {
            return;
        }
        player.PrepareTurn();
    }

    internal string PromotionZone()
    {
        string promotionZone = this.isPlayer1Turn ? "['a', 'b', 'c']" : "['g', 'h', 'i']";
        return $"var promotionZone = {promotionZone};";
    }

    internal string ForcePawnLancePromotion()
    {
        string pawnLancePromo = this.isPlayer1Turn ? "'a'" : "'i'";
        return $"var pawnLancePromo = {pawnLancePromo};";
    }

    internal string ForceKnightPromotion()
    {
        string knightPromo = this.isPlayer1Turn ? "['a', 'b']" : "['h', 'i']";
        return $"var knightPromo = {knightPromo};";
    }

    internal HtmlBuilder GameEndHtml()
    {
        HtmlBuilder builder = new();
        if (!this.isOver)
        {
            return builder;
        }
        HtmlBuilder button = new HtmlBuilder()
            .Class("button")
            .Property("onclick", "restart()")
            .Child("New Game");
        string p = $"The Winner is: Player {(this.winner == this.player1 ? "1" : "2")}!";
        HtmlBuilder message = new HtmlBuilder("p").Child(p);
        HtmlBuilder box = new HtmlBuilder()
            .Class("overlay-box")
            .Child(button)
            .Child(message);
        return builder.Id("game-end-overlay").Child(box);
    }

    internal HtmlBuilder SquareHtml(Coordinate pos)
    {
        string posString = this.CoordinateString(pos);
        IEnumerable<Coordinate> highlighted = this.GetHighlightedSquares(pos);
        string cl = highlighted.Contains(pos) ? "highlightedSquare" : "square";
        StringBuilder squareClass = new(cl);
        HtmlBuilder builder = new HtmlBuilder().Id(posString);
        Piece? piece = this.PieceAt(pos);
        if (piece != null && this.IsPlayersTurn(piece.player) && !this.isOver)
        {
            if (piece.canPromote && !piece.IsPromoted())
            {
                squareClass.Append(" promotable");
                builder.Class("promotable");
                if (piece is Pawn or Lance)
                {
                    squareClass.Append(" forcePromo1");
                }
                else if (piece is Knight)
                {
                    squareClass.Append(" forcePromo2");
                }
            }
            builder.Property("onclick", $"submitForm('{posString}');");
        }
        return builder.Class(squareClass.ToString()).Child(HtmlPieceImage(piece));
    }

    private static HtmlBuilder HtmlPieceImage(Piece? piece)
    {
        HtmlBuilder builder = new("img");
        if (piece != null)
        {
            string player = piece.player.isPlayer1 ? "black" : "white";
            builder.Class($"{player}-piece");
        }
        builder.Property("src", Images.Get(piece));
        builder.Property("alt", Names.Get(piece));
        return builder;
    }

    internal IEnumerable<Coordinate> GetHighlightedSquares(Coordinate pos)
    {
        Piece? piece = this.PieceAt(pos);
        if (piece == null)
        {
            return Enumerable.Empty<Coordinate>();
        }
        return this.IsOnBoard(piece.pos) ? piece.FindMoves() : piece.FindDrops();
    }
}