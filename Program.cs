using System.Net;
using System.Net.Sockets;
using System.Text;
using ShogiWebsite.Shogi;
using ShogiWebsite.Shogi.Pieces;


namespace ShogiWebsite
{
    class Program
    {
        internal static readonly string? projectDir = GetProjectDir();
        internal static string action = "";
        private static Board board = new();
        private static readonly HtmlBuilder title = new HtmlBuilder("title").Child("Shogi");
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
                Console.WriteLine($"POST:\t{post}");
                string url = request.RawUrl ?? "";
                byte[] buffer = GetResponseForUrl(url);
                Action(GetAction(post));
                board.PlayerTurn();
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer);
                response.OutputStream.Close();
            }
            httpListener.Close();
        }

        internal static byte[] GetResponseForUrl(string url)
        {
            string ext = Path.GetExtension(url);
            Console.WriteLine($"URL:\t{url}");
            if (ext == ".png" || ext == ".ico")
                return Images.GetBytes(url);
            else if (ext == "")
                return AnswerHTML();
            else if (ext == ".css")
                return GetStyleSheet(url);
            else if (ext == ".js")
                return GetJavaScript(url);
            return Array.Empty<byte>();
        }

        internal static byte[] GetStyleSheet(string url)
        {
            string stylesheet = File.ReadAllText(projectDir + @"\assets\css\" + url);
            return Encoding.UTF8.GetBytes(stylesheet);
        }

        internal static byte[] GetJavaScript(string url)
        {
            string stylesheet = File.ReadAllText(projectDir + @"\assets\js\" + url);
            return Encoding.UTF8.GetBytes(stylesheet);
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

        private static HtmlBuilder HtmlHead()
        {
            HtmlBuilder head = new("head");
            head.Child(new HtmlBuilder("meta").Property("content", "text/html; charset=ISO-8859-1").Property("http-equiv", "content-type"));
            head.Child(new HtmlBuilder("meta").Property("name", "viewport").Property("content", "width=device-width").Property("initial-scale", "1"));
            head.Child(new HtmlBuilder("link").Property("rel", "stylesheet").Property("href", "m_gray.css"));
            head.Child(new HtmlBuilder("link").Property("rel", "stylesheet").Property("href", "c_silver.css"));
            head.Child(new HtmlBuilder("link").Property("rel", "stylesheet").Property("href", "main.css"));
            head.Child(new HtmlBuilder("script").Property("src", "functions.js"));
            // head.Child(new HtmlBuilder("style").Child(defaultStyles));
            HtmlBuilder script = new("script");
            script.Child("sessionStorage.clear();");
            script.Child(board.PromotionZone());
            script.Child(board.ForcePawnLancePromotion());
            script.Child(board.ForceKnightPromotion());
            script.Child(board.JavascriptMoveLists());
            return head.Child(script).Child(new HtmlBuilder("title").Child("Shogi"));
        }

        private static HtmlBuilder HtmlPromotionOverlay()
        {
            HtmlBuilder overlay = new HtmlBuilder().Id("ask-promotion-overlay");
            HtmlBuilder box = new();
            box.Style("background-color:#ffffff;border:2px solid #000000;width:600px;heigth:300px;");
            box.Child(new HtmlBuilder("p").Child("Promote?"));
            box.Child(new HtmlBuilder().Id("do-promote").Class("button").Child("Yes"));
            box.Child(new HtmlBuilder().Id("dont-promote").Class("button").Child("No"));
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
            HtmlBuilder html = new("html");
            html.Child(HtmlHead());
            HtmlBuilder body = new("body");
            body.Child(HtmlMain());
            body.Child(HtmlHiddenForm());
            html.Child(body);
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

        private static bool ChooseSquareOrMove(string actionCode)
        {
            // scenario 1 : chose first piece, highlight squares this piece can move to
            // scenario 2 : chose unhighlighted square, deselect all squares
            // scenario 3 : chose highlighted, move piece
            Coordinate? coord = board.CoordByString(actionCode);
            if (coord == null)
                return false;
            if (board.phase == Board.Phase.ChoosePiece)
            {
                Piece? piece = board.PieceAt(coord.Value);
                board.phase = Board.Phase.SelectTarget;
                return piece != null && board.IsPlayersTurn(piece.player) && piece.
            }
            return true;
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
            else if (actionCode.Length == 2)
                switchPlayer = ChooseSquareOrMove(actionCode);
            /*
            else if (actionCode.Length == 5 || actionCode.Length == 6)
                switchPlayer = Move(actionCode);
            else if (actionCode.Length == 4)
                switchPlayer = Drop(player, actionCode);
            */
            if (switchPlayer)
                board.isPlayer1Turn = !board.isPlayer1Turn;
        }
    }
}