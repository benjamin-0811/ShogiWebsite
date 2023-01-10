using System.Reflection;

namespace ShogiWebsite.Shogi.Pieces;

internal class StringPieceConverter
{
    internal static readonly Dictionary<string, Type> PIECE_TABLE = new()
    {
        ["p"] = typeof(Pawn),
        ["l"] = typeof(Lance),
        ["n"] = typeof(Knight),
        ["s"] = typeof(Silver),
        ["g"] = typeof(Gold),
        ["b"] = typeof(Bishop),
        ["r"] = typeof(Rook),
        ["k"] = typeof(King)
        // ["st"] = new Stone(),
        // ["i"] = new Iron(),
        // ["c"] = new Copper(),
        // ["rc"] = new ReverseChariot(),
        // ["cs"] = new CatSword(),
        // ["fl"] = new FerociousLeopard(),
        // ["bt"] = new BlindTiger(),
        // ["de"] = new DrunkElephant(),
        // ["vo"] = new ViolentOx(),
        // ["ab"] = new AngryBoar(),
        // ["ew"] = new EvilWolf(),
        // ["ph"] = new Phoenix(),
        // ["ln"] = new Lion(),
        // ["kr"] = new Kirin(),
        // ["fd"] = new FlyingDragon(),
        // ["sm"] = new SideMover(),
        // ["vm"] = new VerticalMover(),
        // ["dh"] = new DragonHorse(),
        // ["dk"] = new DragonKing(),
        // ["q"] = new Queen(),
        // ["gb"] = new Gobetween()
    };


    internal static Piece? GetPiece(Board board, string abbreviation)
    {
        if (abbreviation == "_")
            return null;
        string lowerAbbr = abbreviation.ToLower();
        Player player = abbreviation == lowerAbbr ? board.player2 : board.player1;
        Type type = PIECE_TABLE[lowerAbbr];
        BindingFlags bFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type[] paramTypes = new[] { typeof(Player), typeof(Board) };
        ConstructorInfo? constructor = type.GetConstructor(bFlags, null, paramTypes, null);
        return constructor == null ? null : (Piece)constructor.Invoke(new object[] { player, board });
    }
}
