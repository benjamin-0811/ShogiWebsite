using ShogiWebsite.Shogi.Pieces;
using System.Text;

namespace ShogiWebsite.Shogi;

internal struct Coordinate
{
    internal int Column { get; set; }
    internal int Row { get; set; }
    internal static Coordinate nullPos = new(-1, -1);


    internal Coordinate(int column, int row)
    {
        Column = column;
        Row = row;
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
    internal Piece? selected;

    internal readonly string[] columns;
    internal readonly string[] rows;


    internal Board()
    {
        string path = $@"{Program.projectDir}\assets\layouts\standard.txt";
        List<string> layout = File.ReadLines(path).ToList();
        string dimensions = layout[0];
        layout.RemoveAt(0);
        string[] colAndRow = dimensions.Split('x', 2);

        width = int.Parse(colAndRow[0]);
        height = int.Parse(colAndRow[1]);
        columns = Columns().ToArray();
        rows = Rows().ToArray();
        // Square [ column , row ]
        pieces = new Piece?[width, height];
        player1 = new Player(this, true);
        player2 = new Player(this, false);
        InitBoard(layout);

        isPlayer1Turn = true;
        isOver = false;
        winner = null;
        selected = null;
        player1.InitLater();
        player2.InitLater();

        log = new HtmlBuilder().Id("log").Class("log");
    }


    internal bool SelectedIsHandPiece() => selected != null && selected.pos.Equals(Coordinate.nullPos);


    internal bool SelectedIsOnBoard() => selected != null && IsOnBoard(selected.pos);


    internal void Log(string newEntry) => log.Child(new HtmlBuilder("p").Child(newEntry));


    private IEnumerable<string> Columns()
    {
        for (int i = 0; i < width; i++)
            yield return $"{width - i}";
    }


    internal int ColumnIndex(char character) => Array.IndexOf(columns, $"{character}");


    private IEnumerable<string> Rows()
    {
        for (int i = 0; i < height; i++)
            yield return $"{(char)('a' + i)}";
    }


    internal int RowIndex(char character) => Array.IndexOf(rows, $"{character}");


    internal void SetPiece(Piece? piece, Coordinate pos)
    {
        if (!IsOnBoard(pos))
            return;

        pieces[pos.Column, pos.Row] = piece;
        if (piece != null)
            piece.pos = pos;
    }


    internal void SetPiece(Piece? piece, int column, int row) => SetPiece(piece, new Coordinate(column, row));


    internal string CoordinateString(Coordinate pos) => IsOnBoard(pos) ? $"{rows[pos.Row]}{columns[pos.Column]}" : "hand";


    internal bool IsOnBoard(Coordinate pos)
    {
        bool columnInBound = 0 <= pos.Column && pos.Column < width;
        bool rowInBound = 0 <= pos.Row && pos.Row < height;
        return columnInBound && rowInBound;
    }


    internal Piece? PieceAt(Coordinate pos) => IsOnBoard(pos) ? pieces[pos.Column, pos.Row] : null;


    internal Coordinate? CoordByString(string pos)
    {
        if (pos.Length != 2)
            return null;
        int row = RowIndex(pos[0]);
        int column = ColumnIndex(pos[1]);
        return new Coordinate(column, row);
    }


    internal Piece? PieceByCoordString(string pos)
    {
        Coordinate? coord = CoordByString(pos);
        return coord == null ? null : PieceAt(coord.Value);
    }


    internal Coordinate? N(Coordinate pos, int distance = 1)
    {
        pos.Row -= distance;
        return IsOnBoard(pos) ? pos : null;
    }


    internal Coordinate? S(Coordinate pos, int distance = 1) => N(pos, -distance);


    internal Coordinate? E(Coordinate pos, int distance = 1)
    {
        pos.Column += distance;
        return IsOnBoard(pos) ? pos : null;
    }


    internal Coordinate? W(Coordinate pos, int distance = 1) => E(pos, -distance);


    internal Coordinate? NE(Coordinate pos, int distance = 1)
    {
        Coordinate? n = N(pos, distance);
        return n != null ? E(n.Value, distance) : null;
    }


    internal Coordinate? NW(Coordinate pos, int distance = 1)
    {
        Coordinate? n = N(pos, distance);
        return n != null ? W(n.Value, distance) : null;
    }


    internal Coordinate? SE(Coordinate pos, int distance = 1) => NW(pos, -distance);


    internal Coordinate? SW(Coordinate pos, int distance = 1) => NE(pos, -distance);


    private void InitBoard(List<string> layout)
    {
        int rowAmount = layout.Count;
        for (int row = 0; row < rowAmount; row++)
        {
            IEnumerable<string> pieces = layout[row].Split(',');
            int columnAmount = pieces.Count();
            for (int column = 0; column < columnAmount; column++)
            {
                string pieceString = pieces.ElementAt(column);
                Piece? piece = StringPieceConverter.GetPiece(this, pieceString.Trim());
                SetPiece(piece, column, row);
            }
        }
    }


    internal HtmlBuilder ToHtml()
    {
        HtmlBuilder builder = new HtmlBuilder().Class("board");
        for (int i = 0; i < height; i++)
        {
            HtmlBuilder row = new HtmlBuilder().Class("row");
            for (int j = 0; j < width; j++)
                row.Child(SquareHtml(new Coordinate(j, i)));
            builder.Child(row);
        }
        return builder;
    }


    internal Player CurrentPlayer() => isPlayer1Turn ? player1 : player2; 


    internal bool IsPlayersTurn(Player player)
    {
        bool isPlayer1Turn = player.isPlayer1 && this.isPlayer1Turn;
        bool isPlayer2Turn = !(player.isPlayer1 || this.isPlayer1Turn);
        return isPlayer1Turn || isPlayer2Turn;
    }


    internal HtmlBuilder LogToHtml() => log;


    internal void EndGame(Player winner)
    {
        isOver = true;
        this.winner = winner;
    }


    internal void PlayerTurn()
    {
        Player player = CurrentPlayer();
        player.AfterOpponent();
        if (isOver)
            return;
        player.PrepareTurn();
    }


    internal HtmlBuilder GameEndHtml()
    {
        HtmlBuilder builder = new();
        if (!isOver)
            return builder;
        HtmlBuilder button = new HtmlBuilder()
            .Class("button")
            .Property("onclick", "restart()")
            .Child("New Game");
        string p = $"The Winner is: Player {(winner == player1 ? "1" : "2")}!";
        HtmlBuilder message = new HtmlBuilder("p").Child(p);
        HtmlBuilder box = new HtmlBuilder()
            .Class("overlay-box")
            .Child(button)
            .Child(message);
        return builder.Id("game-end-overlay").Child(box);
    }


    internal HtmlBuilder SquareHtml(Coordinate pos)
    {
        string posString = CoordinateString(pos);
        IEnumerable<Coordinate> highlighted = GetHighlightedSquares();
        string cl = highlighted.Contains(pos) ? "highlightedSquare" : "square";
        StringBuilder squareClass = new(cl);
        HtmlBuilder builder = new HtmlBuilder().Id(posString);
        Piece? piece = PieceAt(pos);
        builder.Property("onclick", $"submitForm('{posString}');");
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


    internal IEnumerable<Coordinate> GetHighlightedSquares()
    {
        if (selected == null || !IsPlayersTurn(selected.player))
            return Enumerable.Empty<Coordinate>();
        return IsOnBoard(selected.pos) ? selected.FindMoves() : selected.FindDrops();
    }
}