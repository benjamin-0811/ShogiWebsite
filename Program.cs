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
        private static readonly string endStyles = DefaultStyles();
        private static readonly string endFunctions = Functions();
        private static readonly string title = Title("Shogi");

        public static void Main()
        {
            TcpListener listener = new(80);
            listener.Start();
            while (true)
            {
                BetterConsole.Special("Listening");
                TcpClient client = listener.AcceptTcpClient();
                BetterConsole.Special("Connected");
                NetworkStream stream = client.GetStream();

                BetterConsole.Special("Waiting for Request");
                byte[] buffer = new byte[4096];
                stream.Read(buffer, 0, buffer.Length);
                BetterConsole.Special("Reading Request");
                string text = Encoding.UTF8.GetString(buffer);

                action = GetAction(text);
                BetterConsole.Special($"Requested Action: {action}");
                Action(action);

                BetterConsole.Special($"Prepare everything for player {(board.isPlayer1Turn ? 1 : 2)}'s turn.");
                board.PlayerTurn();
                BetterConsole.Special($"Finished preparations");
                BetterConsole.Special($"Sending HTML document.");
                stream.Write(AnswerHTML());
                stream.Close();
            }
        }

        internal static string? GetProjectDir()
        {
            BetterConsole.Action("Find project directory.");
            string d1 = Directory.GetCurrentDirectory();
            DirectoryInfo? d2 = Directory.GetParent(d1);
            if (d2 == null) return null;
            DirectoryInfo? d3 = d2.Parent;
            if (d3 == null) return null;
            DirectoryInfo? d4 = d3.Parent;
            if (d4 == null) return null;
            string dir = d4.ToString();
            BetterConsole.Info($"Found the project directory : \"{dir}\"");
            return dir;
        }

        private static string GetAction(string text)
        {
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
            List<LinesBuilder> builders = new();
            builders.Add(
                new LinesBuilder(3)
                .Line("function finalizeMove(from, to) {")
                .Line("submitForm(`${from}-${to}`);", 1)
                .Line("}"));
            builders.Add(
                new LinesBuilder(3)
                .Line("function finalizePromotionMove(from, to) {")
                .Line("submitForm(`${from}-${to}p`);", 1)
                .Line("}"));
            builders.Add(
                new LinesBuilder(3)
                .Line("function askPromotionFirst(from, to) {")
                .Line("var yesButton = document.getElementById('do-promote');", 1)
                .Line("yesButton.setAttribute('onclick', `finalizePromotionMove('${from}', '${to}')`)", 1)
                .Line("var noButton = document.getElementById('dont-promote');", 1)
                .Line("noButton.setAttribute('onclick', `finalizeMove('${from}', '${to}')`)", 1)
                .Line("document.getElementById('ask-promotion-overlay').style.display = 'block';", 1)
                .Line("}"));
            builders.Add(
                new LinesBuilder(3)
                .Line("function restart() {")
                .Line("submitForm('restart');", 1)
                .Line("}"));
            builders.Add(
                new LinesBuilder(3)
                .Line("function surrender() {")
                .Line("submitForm('surrender');", 1)
                .Line("}"));
            builders.Add(
                new LinesBuilder(3)
                .Line("function submitForm(value) {")
                .Line("var form1 = document.getElementById('moveForm');", 1)
                .Line("var input1 = document.getElementById('move');", 1)
                .Line("input1.setAttribute('value', value);", 1)
                .Line("form1.submit();", 1)
                .Line("}"));
            builders.Add(
                new LinesBuilder(3)
                .Line("function selectMoves(forId) {")
                .Line("resetMoves();", 1)
                .Line("var old = sessionStorage.getItem(\"last\");", 1)
                .Line("if (old == forId) {", 1)
                .Line("sessionStorage.clear();", 2)
                .Line("return;", 2)
                .Line("}", 1)
                .Line("var squares = squareMoveDict[forId];", 1)
                .Line("if (squares !== undefined) {", 1)
                .Line("for (let i = 0; i < squares.length; i++) {", 2)
                .Line("var curId = squares[i];", 3)
                .Line("var elem = document.getElementById(curId);", 3)
                .Line("elem.style.setProperty(\"background-color\", \"#7fffbf\");", 3)
                .Line("if (shouldAskForPromotion(forId, curId)) {", 3)
                .Line("elem.setAttribute(\"onclick\", `askPromotionFirst('${forId}','${curId}')`)", 4)
                .Line("}", 3)
                .Line("else {", 3)
                .Line("elem.setAttribute(\"onclick\", `finalizeMove('${forId}','${curId}')`)", 4)
                .Line("}", 3)
                .Line("}", 2)
                .Line("sessionStorage.setItem(\"last\", forId);", 2)
                .Line("}", 1)
                .Line("}"));
            builders.Add(
                new LinesBuilder(3)
                .Line("function shouldAskForPromotion(from, to) {")
                .Line("var elem = document.getElementById(from);", 1)
                .Line("var isPromotable = elem.classList.contains('promotable');", 1)
                .Line("var forcePromo = false;", 1)
                .Line("var forcePromoteList;", 1)
                .Line("if (elem.classList.contains('forcePromo1')) {", 1)
                .Line("forcePromoteList = pawnLancePromo", 2)
                .Line("}", 1)
                .Line("if (elem.classList.contains('forcePromo2')) {", 1)
                .Line("forcePromoteList = knightPromo", 2)
                .Line("}", 1)
                .Line("if (forcePromoteList !== undefined) {", 1)
                .Line("for (let i = 0; i < forcePromoteList.length; i++) {", 2)
                .Line("if (to.includes(forcePromoteList[i])) {", 3)
                .Line("forcePromo = true;", 4)
                .Line("}", 3)
                .Line("}", 2)
                .Line("}", 1)
                .Line("return isPromotable && !forcePromo && isInPromotionZone(to)")
                .Line("}"));
            builders.Add(
                new LinesBuilder(3)
                .Line("function isInPromotionZone(curId) {")
                .Line("for (let i = 0; i < promotionZone.length; i++) {", 1)
                .Line("if (curId.includes(promotionZone[i])) {", 2)
                .Line("return true;", 3)
                .Line("}", 2)
                .Line("}", 1)
                .Line("return false;", 1)
                .Line("}"));
            builders.Add(
                new LinesBuilder(3)
                .Line("function resetMoves() {")
                .Line("var elems = document.getElementsByClassName(\"square\");", 1)
                .Line("for (let i = 0; i < elems.length; i++) {", 1)
                .Line("var elem = elems[i];", 2)
                .Line("elem.removeAttribute(\"onclick\");", 2)
                .Line("elem.removeAttribute(\"style\");", 2)
                .Line("}", 1)
                .Line("var keys = Object.keys(squareMoveDict);", 1)
                .Line("for (let i = 0; i < keys.length; i++) {", 1)
                .Line("var key = keys[i];", 2)
                .Line("document.getElementById(key).setAttribute(\"onclick\", `selectMoves('${key}')`);", 2)
                .Line("}", 1)
                .Line("}"));
            int length = builders.Count;
            if (length <= 0) return "";
            string text = builders[0].Build();
            for (int i = 1; i < length; i++)
                text += $"\n\n{builders[i].Build()}";
            return text;
        }

        private static string DefaultStyles()
        {
            List<CssBuilder> builders = new();
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
                handWidthS = $"{handWidth}px",
                fontSizeS = $"{fontSize}px",
                marginWidthS = marginWidth.ToString(),
                borderWidthS = $"{borderWidth}px",
                backgroundColorS = backgroundColor.RgbHex(),
                handColorS = handColor.RgbHex(),
                squareColorS = squareColor.RgbHex(),
                squareBorderColorS = squareBorderColor.RgbHex(),
                handPieceAmountColorS = handPieceAmountColor.RgbHex(),
                margin = marginWidthS + " auto",
                squareBorder = borderWidthS + " solid " + squareBorderColorS;
            // Creating builders
            builders.Add(
                new CssBuilder(3)
                .Selector("body")
                .Style("background-color", backgroundColorS));
            builders.Add(
                new CssBuilder(3)
                .Selector(".row")
                .Style("width", rowWidthS)
                .Style("min-width", rowWidthS)
                .Style("max-width", rowWidthS)
                .Style("margin", margin));
            builders.Add(
                new CssBuilder(3)
                .Selector(".hand")
                .Style("width", handWidthS)
                .Style("min-width", handWidthS)
                .Style("max-width", handWidthS)
                .Style("height", squareWidthS)
                .Style("min-height", squareWidthS)
                .Style("max-height", squareWidthS)
                .Style("background-color", handColorS)
                .Style("border", squareBorder)
                .Style("margin", margin));
            builders.Add(
                new CssBuilder(3)
                .Selector(".handPiece")
                .Style("width", squareWidthS)
                .Style("min-width", squareWidthS)
                .Style("max-width", squareWidthS)
                .Style("height", squareWidthS)
                .Style("min-height", squareWidthS)
                .Style("max-height", squareWidthS)
                .Style("border", "none")
                .Style("margin", margin)
                .Style("float", "left")
                .Style("font-size", fontSizeS)
                .Style("color", handPieceAmountColorS)
                .Style("font-family", handPieceAmountFont)
                .Style("font-weight", handPieceAmountWeight.ToString())
                .Style("-webkit-text-stroke-width", $"{handPieceAmountOutlineWidth}px")
                .Style("-webkit-text-stroke-color", squareBorderColorS)
                .Style("text-indent", $"{handPieceAmountIndent}px"));
            builders.Add(
                new CssBuilder(3)
                .Selector(".board")
                .Style("width", rowWidthS)
                .Style("min-width", rowWidthS)
                .Style("max-width", rowWidthS)
                .Style("height", rowWidthS)
                .Style("min-height", rowWidthS)
                .Style("max-height", rowWidthS)
                .Style("background-color", squareColorS)
                .Style("border", squareBorder)
                .Style("margin", margin));
            builders.Add(
                new CssBuilder(3)
                .Selector(".black-piece")
                .Selector(".white-piece")
                .Style("height", squareWidthS)
                .Style("width", squareWidthS));
            builders.Add(
                new CssBuilder(3)
                .Selector(".white-piece")
                .Style("transform", "rotate(180deg)"));
            builders.Add(
                new CssBuilder(3)
                .Selector(".square")
                .Style("background-color", squareColorS)
                .Style("border", squareBorder)
                .Style("height", squareWidthS)
                .Style("width", squareWidthS)
                .Style("float", "left")
                .Style("transition", "background-color .2s"));
            builders.Add(
                new CssBuilder(3)
                .Selector(".log")
                .Style("width", "190px")
                .Style("height", "666px")
                .Style("overflow", "scroll")
                .Style("overflow-x", "hidden")
                .Style("border", squareBorder)
                .Style("background-color", "#ffffff")
                .Style("font-family", handPieceAmountFont)
                .Style("padding", "5px"));
            builders.Add(
                new CssBuilder(3)
                .Selector(".log::-webkit-scrollbar")
                .Style("width", "10px"));
            builders.Add(
                new CssBuilder(3)
                .Selector(".log::-webkit-scrollbar-track")
                .Style("background-color", squareColorS));
            builders.Add(
                new CssBuilder(3)
                .Selector(".log::-webkit-scrollbar-thumb")
                .Style("background-color", handColorS)
                .Style("transition", "background-color .2s"));
            builders.Add(
                new CssBuilder(3)
                .Selector(".log::-webkit-scrollbar-thumb:hover")
                .Style("background-color", "#7fffbf"));
            builders.Add(
                new CssBuilder(3)
                .Selector(".button")
                .Style("width", "200px")
                .Style("height", "32px")
                .Style("border", squareBorder)
                .Style("background-color", "#ffffff")
                .Style("font-family", handPieceAmountFont)
                .Style("text-align", "center")
                .Style("padding", "auto")
                .Style("font-weight", "600")
                .Style("font-size", "24px")
                .Style("transition", "background-color .2s"));
            builders.Add(
                new CssBuilder(3)
                .Selector(".button:hover")
                .Style("background-color", "#7fffbf"));
            builders.Add(
                new CssBuilder(3)
                .Selector("#ask-promotion-overlay")
                .Style("position", "fixed")
                .Style("display", "none")
                .Style("width", "100%")
                .Style("height", "100%")
                .Style("top", "0")
                .Style("left", "0")
                .Style("right", "0")
                .Style("bottom", "0")
                .Style("background-color", "#0000007f")
                .Style("z-index", "2"));
            builders.Add(
                new CssBuilder(3)
                .Selector("#game-end-overlay")
                .Style("position", "fixed")
                .Style("display", "block")
                .Style("width", "100%")
                .Style("height", "100%")
                .Style("top", "0")
                .Style("left", "0")
                .Style("right", "0")
                .Style("bottom", "0")
                .Style("background-color", "#0000007f")
                .Style("z-index", "2"));
            // Using builders
            int length = builders.Count;
            if (length <= 0) return "";
            string text = builders[0].Build();
            for (int i = 1; i < length; i++)
                text += $"\n{builders[i].Build()}";
            return text;
        }

        private static string Title(string text) => $"<title>{text}</title>";

        private static byte[] AnswerHTML()
        {
            LinesBuilder builder = new(0);
            builder
                .Line("HTTP/1.1 200 OK")
                .Line("Content-Type: text/html")
                .EmptyLine()
                .Line("<!DOCTYPE html>")
                .EmptyLine()
                .Line("<html>")
                .Line("<head>", 1)
                .Line("<meta content=\"text/html; charset=ISO-8859-1\" http-equiv=\"content-type\">", 2)
                .Line("<style>", 2)
                .Line(defaultStyles)
                .Line("</style>", 2)
                .Line("<script>", 2)
                .Line("sessionStorage.clear();", 3)
                .EmptyLine()
                .Line(board.PromotionZone(), 3)
                .Line(board.ForcePawnLancePromotion(), 3)
                .Line(board.ForceKnightPromotion(), 3)
                .EmptyLine()
                .Line(board.JavascriptMoveLists())
                .EmptyLine()
                .Line(functions)
                .Line("</script>", 2)
                .Line(title, 2)
                .Line("</head>", 1)
                .EmptyLine()
                .Line("<body>", 1)
                .Line("<main>")

                .Line("<div id=\"ask-promotion-overlay\">")
                .Line("<div style=\"background-color:#ffffff;border:2px solid #000000;width:600px;heigth:300px;\">")
                .Line("<p>Promote?</p>")
                .Line("<div id=\"do-promote\" class=\"button\">")
                .Line("Yes")
                .Line("</div>")
                .Line("<div id=\"dont-promote\" class=\"button\">")
                .Line("No")
                .Line("</div>")
                .Line("</div>")
                .Line("</div>")

                .Line(board.GameEndHtml())

                .Line("<div style=\"width:836px;margin:0 auto;\">")
                .Line("<div id=\"board\" style=\"float:left;\">")
                .Line(board.player2.HtmlHand(), 2)
                .Line(board.ToHtml(), 2)
                .Line(board.player1.HtmlHand(), 2)
                .Line("</div>")
                .Line("<div id=\"side\" style=\"float:left; margin-left:16px;\">")
                .Line(board.LogToHtml())
                .Line("<div id=\"surrender_button\" class=\"button\" onclick=\"surrender();\">")
                .Line("Surrender", 2)
                .Line("</div>")
                .Line("<div id=\"restart_button\" class=\"button\" onclick=\"restart();\">")
                .Line("Restart", 2)
                .Line("</div>")
                .Line("</div>")
                .Line("</div>")
                .Line("</main>")
                .Line("<form id=\"moveForm\" action=\"\" method=\"post\">", 2)
                .Line("<input type=\"hidden\" id=\"move\" name=\"move\" value=\"\">", 3)
                .Line("</form>", 2)
                .Line("</body>", 1)
                .Line("</html>");
            string text = builder.Build();
            return Encoding.UTF8.GetBytes(text);
        }

        /// <summary>Perform the action provided by the <paramref name="actionCode"/>.</summary>
        /// <param name="actionCode">code that contains orders that tells the program what moves to do</param>
        private static void Action(string actionCode)
        {
            bool switchPlayer = false;
            Player player = board.isPlayer1Turn ? board.player1 : board.player2;
            bool promotion = actionCode.Length == 6;
            if (actionCode == "surrender")
            {
                board.EndGame(player.Opponent());
            }
            else if (actionCode == "restart")
            {
                if (!board.isOver) board.EndGame(player.Opponent());
                board = new Board();
            }
            else if (actionCode.Length == 5 || promotion)
            {
                int r1 = Array.IndexOf(Square.rows, actionCode[0]);
                int c1 = Array.IndexOf(Square.columns, actionCode[1]);
                int r2 = Array.IndexOf(Square.rows, actionCode[3]);
                int c2 = Array.IndexOf(Square.columns, actionCode[4]);
                Piece? piece = board.squares[c1, r1].piece;
                if (piece != null)
                {
                    if (board.IsPlayersTurn(piece.player))
                        switchPlayer = piece.Move(board.squares[c2, r2], promotion);
                    else BetterConsole.Error("Wrong player on the move!");
                }
                else BetterConsole.Error("There was no piece to move!");
            }
            else if (actionCode.Length == 4)
            {
                string abbr = $"{actionCode[0]}";
                int r1 = Array.IndexOf(Square.rows, actionCode[2]);
                int c1 = Array.IndexOf(Square.columns, actionCode[3]);
                Piece? piece = player.PieceFromHandByAbbr(abbr);
                if (piece != null)
                    switchPlayer = piece.MoveFromHand(board.squares[c1, r1]);
                else BetterConsole.Error("There was no piece to move!");
            }
            if (switchPlayer)
            {
                if (board.isPlayer1Turn) board.isPlayer1Turn = false;
                else board.isPlayer1Turn = true;
            }
        }
    }
}