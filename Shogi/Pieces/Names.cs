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

        /// <summary>Get abbreviation</summary>
        internal static string Abbreviation(Piece? piece)
        {
            string abbreviation = piece switch
            {
                Pawn _ => abbreviationPawn,
                Bishop _ => abbreviationBishop,
                Rook _ => abbreviationRook,
                Lance _ => abbreviationLance,
                Knight _ => abbreviationKnight,
                Silver _ => abbreviationSilver,
                Gold _ => abbreviationGold,
                King _ => abbreviationKing,
                _ => ""
            };
            if (piece != null && piece.IsPromoted()) abbreviation = "+" + abbreviation;
            return abbreviation;
        }
    }
}
