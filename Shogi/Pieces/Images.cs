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
        private static readonly string silverGeneral = ImageToBase64("s.png");
        private static readonly string promotedSilver = ImageToBase64("+s.png");
        private static readonly string goldGeneral = ImageToBase64("g.png");
        private static readonly string blackKing = ImageToBase64("k.png");
        private static readonly string whiteKing = ImageToBase64("k-.png");
        private static readonly string nullPiece = ImageToBase64("null.png");

        private static string ImageToBase64(string imageName)
        {
            if (OperatingSystem.IsWindows())
            {
                Image image = Image.FromFile(@$"{Program.Program.projectDir}\assets\img\{imageName}");
                MemoryStream stream = new();
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                var bytes = stream.ToArray();
                return Convert.ToBase64String(bytes);
            }
            else return "";
        }

        internal static string Get(Piece? piece) => piece switch
        {
            Pawn _ => piece.isPromoted ? promotedPawn : pawn,
            Bishop _ => piece.isPromoted ? horse : bishop,
            Rook _ => piece.isPromoted ? dragon : rook,
            Lance _ => piece.isPromoted ? promotedLance : lance,
            Knight _ => piece.isPromoted ? promotedKnight : knight,
            Silver _ => piece.isPromoted ? promotedSilver : silverGeneral,
            Gold _ => goldGeneral,
            King _ => piece.player.isPlayer1 ? blackKing : whiteKing,
            _ => nullPiece
        };
    }
}
