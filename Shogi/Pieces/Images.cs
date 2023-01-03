using System.Drawing.Imaging;
using System.Drawing;

namespace ShogiWebsite.Shogi.Pieces;

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

    internal static byte[] GetBytes(string imageName)
    {
        return GetBytes(imageName, Path.GetExtension(imageName));
    }

    internal static byte[] GetBytes(string imageName, string extension)
    {
        if (OperatingSystem.IsWindows())
        {
            string path = @$"{Program.projectDir}\assets\img\{imageName}";
            Image image = Image.FromFile(path);
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

    internal static string Get(Piece? piece) => piece switch
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

    internal static string Get(Type type) => type switch
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