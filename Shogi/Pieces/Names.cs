namespace ShogiWebsite.Shogi.Pieces
{
    internal static class Names
    {
        private static readonly string pawn = "Pawn";
        private static readonly string promoted = "Promoted Pawn";
        private static readonly string bishop = "Bishop";
        private static readonly string horse = "Horse";
        private static readonly string rook = "Rook";
        private static readonly string dragon = "Dragon";
        private static readonly string lance = "Lance";
        private static readonly string promotedLance = "Promoted Lance";
        private static readonly string knight = "Knight";
        private static readonly string promotedKnight = "Promoted Knight";
        private static readonly string silver = "Silver General";
        private static readonly string promotedSilver = "Promoted Silver";
        private static readonly string gold = "Gold General";
        private static readonly string king = "King";

        private static readonly string abbreviationPawn = "P";
        private static readonly string abbreviationBishop = "B";
        private static readonly string abbreviationRook = "R";
        private static readonly string abbreviationLance = "L";
        private static readonly string abbreviationKnight = "N";
        private static readonly string abbreviationSilver = "S";
        private static readonly string abbreviationGold = "G";
        private static readonly string abbreviationKing = "K";

        internal static string Get(Piece? piece) => piece switch
        {
            Pawn _ => piece.IsPromoted() ? promoted : pawn,
            Bishop _ => piece.IsPromoted() ? horse : bishop,
            Rook _ => piece.IsPromoted() ? dragon : rook,
            Lance _ => piece.IsPromoted() ? promotedLance : lance,
            Knight _ => piece.IsPromoted() ? promotedKnight : knight,
            Silver _ => piece.IsPromoted() ? promotedSilver : silver,
            Gold _ => gold,
            King _ => king,
            _ => ""
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
            Type _ when type == typeof(King) => king,
            _ => ""
        };

        internal static string Abbreviation(Piece? piece)
        {
            string abbreviation = piece != null ? Abbreviation(piece.GetType()) : "";
            if (piece != null && piece.IsPromoted())
                abbreviation = "+" + abbreviation;
            return abbreviation;
        }

        internal static string Abbreviation(Type type) => type switch
        {
            Type _ when type == typeof(Pawn) => abbreviationPawn,
            Type _ when type == typeof(Bishop) => abbreviationBishop,
            Type _ when type == typeof(Rook) => abbreviationRook,
            Type _ when type == typeof(Lance) => abbreviationLance,
            Type _ when type == typeof(Knight) => abbreviationKnight,
            Type _ when type == typeof(Silver) => abbreviationSilver,
            Type _ when type == typeof(Gold) => abbreviationGold,
            Type _ when type == typeof(King) => abbreviationKing,
            _ => ""
        };
    }
}