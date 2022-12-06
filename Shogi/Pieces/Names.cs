namespace ShogiWebsite.Shogi.Pieces
{
    internal static class Names
    {
        private static readonly string pawn = "Pawn";
        private static readonly string promotedPawn = "Promoted Pawn";
        private static readonly string bishop = "Bishop";
        private static readonly string horse = "Horse";
        private static readonly string rook = "Rook";
        private static readonly string dragon = "Dragon";
        private static readonly string lance = "Lance";
        private static readonly string promotedLance = "Promoted Lance";
        private static readonly string knight = "Knight";
        private static readonly string promotedKnight = "Promoted Knight";
        private static readonly string silverGeneral = "Silver General";
        private static readonly string promotedSilver = "Promoted Silver";
        private static readonly string goldGeneral = "Gold General";
        private static readonly string king = "King";

        private static readonly string abbrPawn = "P";
        private static readonly string abbrBishop = "B";
        private static readonly string abbrRook = "R";
        private static readonly string abbrLance = "L";
        private static readonly string abbrKnight = "N";
        private static readonly string abbrSilverGeneral = "S";
        private static readonly string abbrGoldGeneral = "G";
        private static readonly string abbrKing = "K";

        internal static string Get(Piece? piece) => piece switch
        {
            Pawn _ => piece.isPromoted ? promotedPawn : pawn,
            Bishop _ => piece.isPromoted ? horse : bishop,
            Rook _ => piece.isPromoted ? dragon : rook,
            Lance _ => piece.isPromoted ? promotedLance : lance,
            Knight _ => piece.isPromoted ? promotedKnight : knight,
            Silver _ => piece.isPromoted ? promotedSilver : silverGeneral,
            Gold _ => goldGeneral,
            King _ => king,
            _ => ""
        };

        /// <summary>Get abbreviation</summary>
        internal static string Abbr(Piece? piece)
        {
            string abbr = piece switch
            {
                Pawn _ => abbrPawn,
                Bishop _ => abbrBishop,
                Rook _ => abbrRook,
                Lance _ => abbrLance,
                Knight _ => abbrKnight,
                Silver _ => abbrSilverGeneral,
                Gold _ => abbrGoldGeneral,
                King _ => abbrKing,
                _ => ""
            };
            if (piece != null && piece.canPromote && piece.isPromoted) abbr = "+" + abbr;
            return abbr;
        }
    }
}
