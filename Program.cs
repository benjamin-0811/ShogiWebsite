using System.Net;
using System.Net.Sockets;
using System.Text;
using ShogiWebsite.Shogi;


namespace ShogiWebsite
{
    class Program
    {
        internal static readonly string? projectDir = GetProjectDir();
        internal static string action = "";
        private static Board board = new();
        private static readonly string defaultStyles = DefaultStyles();
        private static readonly string functions = Functions();
        private static readonly HtmlBuilder title = new HtmlBuilder("title").Child("Shogi");
        private static bool run = true;

        public static void Main()
        {
            // HTTP Server
            HttpListener httpListener = new();
            httpListener.Prefixes.Add("http:localhost:8001/");
            // TcpListener listener = new(80);
            // listener.Start();
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
                Action(GetAction(request.RawUrl));
                board.PlayerTurn();
                byte[] buffer = AnswerHTML();
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(AnswerHTML());
                response.OutputStream.Close();
            }
            httpListener.Close();
        }

        internal static string? GetProjectDir()
        {
            string d1 = Directory.GetCurrentDirectory();
            DirectoryInfo? d2 = Directory.GetParent(d1);
            if (d2 == null)
                return null;
            DirectoryInfo? d3 = d2.Parent;
            if (d3 == null)
                return null;
            DirectoryInfo? d4 = d3.Parent;
            if (d4 == null)
                return null;
            string dir = d4.ToString();
            return dir;
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

        private static string Functions()
        {
            string location = projectDir + @"\assets\js\functions.js";
            return File.ReadAllText(location);
        }

        private static string DefaultStyles()
        {
            // Explicit Variables, only make changes here
            int squareWidth = 64,
                borderWidth = 2,
                boardSize = 9,
                handSize = 7,
                fontSize = 32,
                marginWidth = 0,
                handPieceAmountIndent = 5,
                handPieceAmountOutlineWidth = 2,
                handPieceAmountWeight = 600;
            Color backgroundColor = new(169, 204, 247),
                handColor = new(143, 175, 239),
                squareColor = new(191, 191, 191),
                squareBorderColor = new(47, 47, 47),
                handPieceAmountColor = new(199, 255, 239);
            string handPieceAmountFont = "Arial";
            // Implicit Variables based on the Expicit ones mentioned above
            int rowWidth = boardSize * (squareWidth + borderWidth * 2),
                handWidth = squareWidth * handSize;
            string squareWidthS = $"{squareWidth}px",
                rowWidthS = $"{rowWidth}px",
                handColorS = handColor.RgbHex(),
                squareColorS = squareColor.RgbHex(),
                squareBorderColorS = squareBorderColor.RgbHex(),
                margin = marginWidth.ToString() + " auto",
                squareBorder = $"{borderWidth}px" + " solid " + squareBorderColorS;

            string location = projectDir + @"\assets\css\shogi.css";
            StringBuilder builder1 = new(File.ReadAllText(location));
            builder1.Replace("$bgc", backgroundColor.RgbHex());
            builder1.Replace("$rw", rowWidthS)
                .Replace("$rw", margin);
            builder1.Replace("$hw", $"{handWidth}px")
                .Replace("$hh", squareWidthS)
                .Replace("$hbgc", handColorS)
                .Replace("$hb", squareBorder)
                .Replace("$hm", margin);
            builder1.Replace("$hpw", squareWidthS)
                .Replace("$hph", squareWidthS)
                .Replace("$hpm", margin)
                .Replace("$hpfs", $"{fontSize}px")
                .Replace("$hpfc", handPieceAmountColor.RgbHex())
                .Replace("$hpff", handPieceAmountFont)
                .Replace("$hpfw", handPieceAmountWeight.ToString())
                .Replace("$hpow", $"{handPieceAmountOutlineWidth}px")
                .Replace("$hpoc", squareBorderColorS)
                .Replace("$hpi", $"{handPieceAmountIndent}px");
            builder1.Replace("$bw", rowWidthS)
                .Replace("$bh", rowWidthS)
                .Replace("$bbgc", squareColorS)
                .Replace("$bb", squareBorder)
                .Replace("$bm", margin);
            builder1.Replace("$pw", squareWidthS);
            builder1.Replace("$sbgc", squareColorS)
                .Replace("$sb", squareBorder)
                .Replace("$sh", squareWidthS)
                .Replace("$sw", squareWidthS);
            builder1.Replace("$lw", "190px")
                .Replace("$lh", "666px")
                .Replace("$lb", squareBorder)
                .Replace("$lbgc", "#ffffff")
                .Replace("$lff", handPieceAmountFont)
                .Replace("$lp", "5px");
            builder1.Replace("$lsbw", "10px");
            builder1.Replace("$lsbtrbgc", squareColorS);
            builder1.Replace("$lsbthbgc", handColorS);
            builder1.Replace("$lsbthhbgc", "#7fffbf");
            builder1.Replace("$buw", "200px")
                .Replace("$buh", "32px")
                .Replace("$bub", squareBorder)
                .Replace("$bubgc", "#ffffff")
                .Replace("$buff", handPieceAmountFont)
                .Replace("$bufw", "600")
                .Replace("$bufs", "24px");
            builder1.Replace("$buhbgc", "#7fffbf");
            return builder1.ToString();
        }

        private static HtmlBuilder HtmlHead()
        {
            HtmlBuilder head = new("head");
            HtmlBuilder meta = new("meta");
            meta.Property("content", "text/html; charset=ISO-8859-1").Property("http-equiv", "content-type");
            head.Child(meta);
            head.Child(new HtmlBuilder("style").Child(defaultStyles));
            HtmlBuilder script = new("script");
            script.Child("sessionStorage.clear();");
            script.Child(board.PromotionZone());
            script.Child(board.ForcePawnLancePromotion());
            script.Child(board.ForceKnightPromotion());
            script.Child(board.JavascriptMoveLists());
            script.Child(functions);
            return head.Child(script).Child(new HtmlBuilder("title").Child("Shogi"));
        }

        private static HtmlBuilder HtmlPromotionOverlay()
        {
            HtmlBuilder overlay = new HtmlBuilder().Id("ask-promotion-overlay");
            HtmlBuilder box = new();
            box.Style("background-color:#ffffff;border:2px solid #000000;width:600px;heigth:300px;");
            box.Child(new HtmlBuilder("p").Child("Promote?"));
            box.Child(new HtmlBuilder().Id("do-promote").Class("button").Child("Yes"));
            box.Child(new HtmlBuilder().Id("do-promote").Class("button").Child("No"));
            return overlay.Child(box);
        }

        private static HtmlBuilder HtmlBoard()
        {
            HtmlBuilder board1 = new HtmlBuilder().Id("board").Style("float:left;");
            board1.Child(board.player2.HtmlHand());
            board1.Child(board.ToHtml());
            return board1.Child(board.player1.HtmlHand());
        }

        private static HtmlBuilder HtmlLog()
        {
            HtmlBuilder side = new HtmlBuilder().Id("side").Style("float:left; margin-left:16px;");
            side.Child(board.LogToHtml());
            HtmlBuilder surrender = new HtmlBuilder().Id("surrender_button").Class("button");
            surrender.Property("onclick", "surrender();").Child("Surrender");
            HtmlBuilder restart = new HtmlBuilder().Id("restart_button").Class("button");
            restart.Property("onclick", "restart();").Child("Restart");
            return side.Child(surrender).Child(restart);
        }

        private static HtmlBuilder HtmlMain()
        {
            HtmlBuilder main = new("main");
            main.Child(HtmlPromotionOverlay());
            main.Child(board.GameEndHtml());
            HtmlBuilder gameArea = new HtmlBuilder().Style("width:836px;margin:0 auto;");
            gameArea.Child(HtmlBoard());
            gameArea.Child(HtmlLog());
            return main.Child(gameArea);
        }

        private static HtmlBuilder HtmlHiddenForm()
        {
            HtmlBuilder form = new HtmlBuilder("form").Id("moveForm").Property("action", "").Property("method", "post");
            return form.Child(new HtmlBuilder("input").Property("type", "hidden").Id("move").Property("name", "move").Property("value", ""));
        }

        private static byte[] AnswerHTML()
        {
            HtmlBuilder builder = new HtmlBuilder("!DOCTYPE").Property("html");
            HtmlBuilder html = new("html");
            html.Child(HtmlHead());
            HtmlBuilder body = new("body");
            body.Child(HtmlMain());
            body.Child(HtmlHiddenForm());
            html.Child(body);
            string text = builder.Build();
            return Encoding.UTF8.GetBytes(text);
        }

        private static void Surrender(Player player) => board.EndGame(player.Opponent());

        private static void Restart(Player player)
        {
            if (!board.isOver)
                board.EndGame(player.Opponent());
            board = new Board();
        }

        private static bool Move(string actionCode)
        {
            bool promote = actionCode.Length == 6 && actionCode[5] == '+';
            int r1 = Array.IndexOf(board.rows, actionCode[0]);
            int c1 = Array.IndexOf(board.columns, actionCode[1]);
            int r2 = Array.IndexOf(board.rows, actionCode[3]);
            int c2 = Array.IndexOf(board.columns, actionCode[4]);
            Piece? piece = board.pieces[c1, r1];
            return piece != null && board.IsPlayersTurn(piece.player) && piece.Move(new Coordinate(c2, r2), promote);
        }

        private static bool Drop(Player player, string actionCode)
        {
            string abbr = $"{actionCode[0]}";
            int r1 = Array.IndexOf(board.rows, actionCode[2]);
            int c1 = Array.IndexOf(board.columns, actionCode[3]);
            Piece? piece = player.PieceFromHandByAbbr(abbr);
            return piece != null && piece.MoveFromHand(new Coordinate(c1, r1));
        }

        private static void Action(string actionCode)
        {
            bool switchPlayer = false;
            Player player = board.isPlayer1Turn ? board.player1 : board.player2;
            if (actionCode == "surrender")
                Surrender(player);
            else if (actionCode == "restart")
                Restart(player);
            else if (actionCode == "stop")
                run = false;
            else if (actionCode.Length == 5 || actionCode.Length == 6)
                switchPlayer = Move(actionCode);
            else if (actionCode.Length == 4)
                switchPlayer = Drop(player, actionCode);
            if (switchPlayer)
                board.isPlayer1Turn = !board.isPlayer1Turn;
        }
    }
}