using System.Drawing.Imaging;
using System.Drawing;

namespace ShogiWebsite.Shogi.Pieces
{
    internal static class Images
    {
        private static readonly string p = "p.png";
        private static readonly string pp = "+p.png";
        private static readonly string b = "b.png";
        private static readonly string pb = "+b.png";
        private static readonly string r = "r.png";
        private static readonly string pr = "+r.png";
        private static readonly string l = "l.png";
        private static readonly string pl = "+l.png";
        private static readonly string n = "n.png";
        private static readonly string pn = "+n.png";
        private static readonly string s = "s.png";
        private static readonly string ps = "+s.png";
        private static readonly string g = "g.png";
        private static readonly string bk = "k.png";
        private static readonly string wk = "k-.png";
        private static readonly string nul = "null.png";

        private static readonly string pawn = ImageToBase64(p);
        private static readonly string promotedPawn = ImageToBase64(pp);
        private static readonly string bishop = ImageToBase64(b);
        private static readonly string horse = ImageToBase64(pb);
        private static readonly string rook = ImageToBase64(r);
        private static readonly string dragon = ImageToBase64(pr);
        private static readonly string lance = ImageToBase64(l);
        private static readonly string promotedLance = ImageToBase64(pl);
        private static readonly string knight = ImageToBase64(n);
        private static readonly string promotedKnight = ImageToBase64(pn);
        private static readonly string silver = ImageToBase64(s);
        private static readonly string promotedSilver = ImageToBase64(ps);
        private static readonly string gold = ImageToBase64(g);
        private static readonly string blackKing = ImageToBase64(bk);
        private static readonly string whiteKing = ImageToBase64(wk);
        private static readonly string nullPiece = ImageToBase64(nul);

        internal static byte[] GetBytes(string imageName) => GetBytes(imageName, Path.GetExtension(imageName));

        internal static byte[] GetBytes(string imageName, string extension)
        {
            if (OperatingSystem.IsWindows())
            {
                Image image = Image.FromFile(@$"{Program.projectDir}\assets\img\{imageName}");
                MemoryStream stream = new();
                image.Save(stream, GetImageFormat(extension));
                stream.Position = 0;
                return stream.ToArray();
            }
            return Array.Empty<byte>();
        }

        internal static ImageFormat GetImageFormat(string extension)
        {
            if (OperatingSystem.IsWindows())
            {
                return extension switch
                {
                    ".jpg" => ImageFormat.Jpeg,
                    ".jpeg" => ImageFormat.Jpeg,
                    ".jpe" => ImageFormat.Jpeg,
                    ".jif" => ImageFormat.Jpeg,
                    ".jfif" => ImageFormat.Jpeg,
                    ".jfi" => ImageFormat.Jpeg,
                    ".png" => ImageFormat.Png,
                    ".gif" => ImageFormat.Gif,
                    ".ico" => ImageFormat.Png,
                    ".bmp" => ImageFormat.Bmp,
                    ".dib" => ImageFormat.Bmp,
                    ".tiff" => ImageFormat.Tiff,
                    ".tif" => ImageFormat.Tiff,
                    ".emf" => ImageFormat.Emf,
                    ".emz" => ImageFormat.Emf,
                    ".wmf" => ImageFormat.Wmf,
                    _ => ImageFormat.MemoryBmp
                };
            }
            throw new Exception("This part of the program is only supported on Windows, please run this program under a Windows OS.");
        }
        

        private static string ImageToBase64(string imageName) => Convert.ToBase64String(GetBytes(imageName));

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

        internal static string GetUrl(Piece? piece) => piece switch
        {
            Pawn _ => piece.IsPromoted() ? pp : p,
            Bishop _ => piece.IsPromoted() ? pb : b,
            Rook _ => piece.IsPromoted() ? pr : r,
            Lance _ => piece.IsPromoted() ? pl : l,
            Knight _ => piece.IsPromoted() ? pn : n,
            Silver _ => piece.IsPromoted() ? ps : s,
            Gold _ => g,
            King _ => piece.player.isPlayer1 ? bk : wk,
            _ => nul
        };

        internal static string GetUrl(Type type) => type switch
        {
            Type _ when type == typeof(Pawn) => p,
            Type _ when type == typeof(Bishop) => b,
            Type _ when type == typeof(Rook) => r,
            Type _ when type == typeof(Lance) => l,
            Type _ when type == typeof(Knight) => n,
            Type _ when type == typeof(Silver) => s,
            Type _ when type == typeof(Gold) => g,
            Type _ when type == typeof(King) => bk,
            _ => nul
        };
    }
}