using ShogiWebsite.Shogi;
using ShogiWebsite.Shogi.Pieces;
using System.Drawing;
using System.Net;
using System.Text;


namespace ShogiWebsite;

class Program
{
    internal static readonly string projectDir = GetProjectDir();
    internal static string action = "";
    private static Board board = new();
    private static bool run = true;


    public static void Main()
    {
        HttpListener httpListener = new();
        httpListener.Prefixes.Add("http://localhost:8001/");
        httpListener.Start();
        Console.WriteLine("Listening on Port 8001...");
        while (run)
        {
            // do different stuff for different urls, ie. send pics for .png at the end and stuff.
            HttpListenerContext context = httpListener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.StatusDescription = "Status OK";
            string post = new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
            string url = request.RawUrl ?? "";
            Action(GetAction(post));
            byte[] buffer = GetResponseForFileName(url);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer);
            response.OutputStream.Close();
        }
        httpListener.Close();
    }


    internal static byte[] GetResponseForFileName(string fileName)
    {
        string ext = Path.GetExtension(fileName);
        return ext switch
        {
            ".png" or ".ico" => Images.GetBytes(fileName),
            "" => AnswerHTML(),
            ".css" => GetStyleSheet(fileName),
            ".js" => GetJavaScript(fileName),
            _ => Array.Empty<byte>(),
        };
    }


    internal static byte[] GetStyleSheet(string fileName) => Helper.GetFileContentBytes($@"{projectDir}\assets\css\{fileName}");


    internal static byte[] GetJavaScript(string fileName) => Helper.GetFileContentBytes($@"{projectDir}\assets\js\{fileName}");


    internal static string GetProjectDir()
    {
        string d1 = Directory.GetCurrentDirectory();

        DirectoryInfo? d2 = Directory.GetParent(d1);
        Helper.AssertNotNull(d2);

        DirectoryInfo? d3 = d2.Parent;
        Helper.AssertNotNull(d3);

        DirectoryInfo? d4 = d3.Parent;
        Helper.AssertNotNull(d4);

        return d4.ToString();
    }


    private static string GetAction(string? text)
    {
        if (text == null)
            return "";

        StringReader reader = new(text);
        string? line = reader.ReadLine();
        while (line != null)
        {
            if (line.Contains("move="))
            {
                int start = line.LastIndexOf('=') + 1;
                string action = line[start..];
                return action.Replace("\0", string.Empty);
            }
            line = reader.ReadLine();
        }
        return "";
    }


    private static HtmlBuilder HtmlHead()
    {
        HtmlBuilder charset = new HtmlBuilder("meta")
            .Property("content", "text/html; charset=ISO-8859-1")
            .Property("http-equiv", "content-type");
        HtmlBuilder viewport = new HtmlBuilder("meta")
            .Property("name", "viewport")
            .Property("content", "width=device-width")
            .Property("initial-scale", "1");
        HtmlBuilder mainColor = new HtmlBuilder("link")
            .Property("rel", "stylesheet")
            .Property("href", "m_gray.css");
        HtmlBuilder contrastColor = new HtmlBuilder("link")
            .Property("rel", "stylesheet")
            .Property("href", "c_azure.css");
        HtmlBuilder style = new HtmlBuilder("link")
            .Property("rel", "stylesheet")
            .Property("href", "main.css");
        HtmlBuilder scriptFile = new HtmlBuilder("script")
            .Property("src", "functions.js");
        HtmlBuilder script = new HtmlBuilder("script")
            .Child("sessionStorage.clear();");
        HtmlBuilder title = new HtmlBuilder("title")
            .Child("Shogi");

        HtmlBuilder head = new HtmlBuilder("head")
            .Child(charset)
            .Child(viewport)
            .Child(mainColor)
            .Child(contrastColor)
            .Child(style)
            .Child(scriptFile)
            .Child(script)
            .Child(title);
        return head;
    }


    private static HtmlBuilder HtmlPromotionOverlay()
    {
        HtmlBuilder overlay = new HtmlBuilder()
            .Id("ask-promotion-overlay");
        HtmlBuilder promoteButton = new HtmlBuilder()
            .Id("do-promote")
            .Class("button")
            .Child("Yes");
        HtmlBuilder dontPromoteButton = new HtmlBuilder()
            .Id("dont-promote")
            .Class("button")
            .Child("No");
        HtmlBuilder box = new HtmlBuilder()
            .Style("background-color:#ffffff;border:2px solid #000000;width:600px;heigth:300px;")
            .Child(new HtmlBuilder("p").Child("Promote?"))
            .Child(promoteButton)
            .Child(dontPromoteButton);
        return overlay.Child(box);
    }


    private static HtmlBuilder HtmlBoard()
    {
        HtmlBuilder board1 = new HtmlBuilder()
            .Id("board")
            .Style("float:left;")
            .Child(board.player2.HtmlHand())
            .Child(board.ToHtml())
            .Child(board.player1.HtmlHand());
        return board1;
    }


    private static HtmlBuilder HtmlLog()
    {
        HtmlBuilder surrender = new HtmlBuilder()
            .Id("surrender_button")
            .Class("button")
            .Property("onclick", "surrender();")
            .Child("Surrender");
        HtmlBuilder restart = new HtmlBuilder()
            .Id("restart_button")
            .Class("button")
            .Property("onclick", "restart();")
            .Child("Restart");
        HtmlBuilder side = new HtmlBuilder()
            .Id("side")
            .Style("float:left; margin-left:16px;")
            .Child(board.LogToHtml())
            .Child(surrender)
            .Child(restart);
        return side;
    }


    private static HtmlBuilder HtmlMain()
    {
        HtmlBuilder gameArea = new HtmlBuilder()
            .Style("width:836px;margin:0 auto;")
            .Child(HtmlBoard())
            .Child(HtmlLog());
        HtmlBuilder main = new HtmlBuilder("main")
            .Child(HtmlPromotionOverlay())
            .Child(board.GameEndHtml())
            .Child(gameArea);
        return main;
    }


    private static HtmlBuilder HtmlHiddenForm()
    {
        HtmlBuilder input = new HtmlBuilder("input")
            .Property("type", "hidden")
            .Id("move")
            .Property("name", "move")
            .Property("value", "");
        HtmlBuilder form = new HtmlBuilder("form")
            .Id("moveForm")
            .Property("action", "")
            .Property("method", "post")
            .Child(input);
        return form;
    }


    private static byte[] AnswerHTML()
    {
        HtmlBuilder body = new HtmlBuilder("body")
            .Child(HtmlMain())
            .Child(HtmlHiddenForm());
        HtmlBuilder html = new HtmlBuilder("html")
            .Child(HtmlHead())
            .Child(body);

        string text = html.Build();
        return Encoding.UTF8.GetBytes(text);
    }


    private static void Surrender(Player player) => board.EndGame(player.Opponent());


    private static void Restart(Player player)
    {
        if (!board.isOver)
            board.EndGame(player.Opponent());
        board = new Board();
    }


    private static void Action(string actionCode)
    {
        bool switchPlayer = false;
        bool codeIsCoord = board.CoordByString(actionCode).HasValue;
        bool codeIsHandPiece = StringPieceConverter.PIECE_TABLE.ContainsKey(actionCode.ToLower());
        Player player = board.CurrentPlayer();
        if (actionCode == "surrender")
            Surrender(player);
        else if (actionCode == "restart")
            Restart(player);
        else if (actionCode == "stop")
        {
            run = false;
            return;
        }
        else if (actionCode == "promote")
        {
        }
        else if (codeIsCoord)
        {
            Coordinate coord = new(board.ColumnIndex(actionCode[1]), board.RowIndex(actionCode[0]));
            Piece? piece = board.PieceAt(coord);
            bool selectedIsHandPiece = board.SelectedIsHandPiece();
            bool selectedIsOnBoard = board.SelectedIsOnBoard();
            if (!(selectedIsHandPiece || selectedIsOnBoard))
            {
                if (piece != null && piece.player == board.CurrentPlayer())
                    board.selected ??= board.PieceAt(coord);
            }
            else
            {
                Helper.AssertNotNull(board.selected);
                if (piece != null && !piece.DifferentPlayer(board.selected))
                    board.selected = piece;
                else
                {
                    if (selectedIsOnBoard)
                        switchPlayer = board.selected.Move(coord, false);
                    else if (selectedIsHandPiece)
                        switchPlayer = board.selected.MoveFromHand(coord);
                    board.selected = null;
                }
            }
        }
        else if (codeIsHandPiece)
            board.selected = player.PieceFromHandByAbbr(actionCode);

        if (switchPlayer)
            board.isPlayer1Turn = !board.isPlayer1Turn;
    }
}