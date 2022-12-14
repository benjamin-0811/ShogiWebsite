using System.Drawing.Imaging;
using System.Drawing;

namespace ShogiWebsite.Shogi.Pieces
{
    internal static class Images
    {
        private static readonly string pawn = ImageToBase64("p.png");
        private static readonly string promotedPawn = ImageToBase64("+p.png");
        private static readonly string bishop = ImageToBase64("b.png");
        private static readonly string horse = ImageToBase64("+b.png");
        private static readonly string rook = ImageToBase64("r.png");
        private static readonly string dragon = ImageToBase64("+r.png");
        private static readonly string lance = ImageToBase64("l.png");
        private static readonly string promotedLance = ImageToBase64("+l.png");
        private static readonly string knight = ImageToBase64("n.png");
        private static readonly string promotedKnight = ImageToBase64("+n.png");
        private static readonly string silver = ImageToBase64("s.png");
        private static readonly string promotedSilver = ImageToBase64("+s.png");
        private static readonly string gold = ImageToBase64("g.png");
        private static readonly string blackKing = ImageToBase64("k.png");
        private static readonly string whiteKing = ImageToBase64("k-.png");
        private static readonly string nullPiece = ImageToBase64("null.png");

        private static string ImageToBase64(string imageName)
        {
            if (OperatingSystem.IsWindows())
            {
                Image image = Image.FromFile(@$"{Program.projectDir}\assets\img\{imageName}");
                MemoryStream stream = new();
                image.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                byte[] bytes = stream.ToArray();
                return Convert.ToBase64String(bytes);
            }
            else
                return "";
        }

        internal static string Get(Piece? piece) => piece switch
        {
            Pawn _ => piece.IsPromoted() ? promotedPawn : pawn,
            Bishop _ => piece.IsPromoted() ? horse : bishop,
            Rook _ => piece.IsPromoted() ? dragon : rook,
            Lance _ => piece.IsPromoted() ? promotedLance : lance,
            Knight _ => piece.IsPromoted() ? promotedKnight : knight,
            Silver _ => piece.IsPromoted() ? promotedSilver : silver,
            Gold _ => gold,
            King _ => piece.player.isPlayer1 ? blackKing : whiteKing,
            _ => nullPiece
        };

        internal static string Get(Type type) => type switch
        {
            Type _ when type == typeof(Pawn) => pawn,
            Type _ when type == typeof(Bishop) => bishop,
            Type _ when type == typeof(Rook) => rook,
            Type _ when type == typeof(Lance) => lance,
            Type _ when type == typeof(Knight) => knight,
            Type _ when type == typeof(Silver) => silver,
            Type _ when type == typeof(Gold) => gold,
            Type _ when type == typeof(King) => whiteKing,
            _ => nullPiece
        };
    }
}