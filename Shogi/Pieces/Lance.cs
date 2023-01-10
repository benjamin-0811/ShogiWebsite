namespace ShogiWebsite.Shogi.Pieces;

internal class Lance : Piece
{
    internal Lance(Player player, Board board) : base(player, true, board)
    { }


    internal override IEnumerable<Coordinate> FindMoves() => isPromoted ? GoldMoves() : LanceMoves();


    private IEnumerable<Coordinate> LanceMoves() => RangeMoves(new Func<Coordinate, int, Coordinate?>[] { Front() });


    internal override IEnumerable<Coordinate> FindDrops() => FindDrops(1);


    internal override void ForcePromote() => ForcePromote(1);
}