namespace ShogiWebsite.Shogi.Pieces;

internal class Knight : Piece
{
    internal Knight(Player player, Board board) : base(player, true, board)
    { }


    internal override IEnumerable<Coordinate> FindMoves() => isPromoted ? GoldMoves() : KnightMoves();


    private IEnumerable<Coordinate> KnightMoves()
    {
        Coordinate? left = Knight(true);
        if (IsAvailableSquare(left))
        {
            Helper.AssertNotNull(left);
            yield return left.Value;
        }
        Coordinate? right = Knight(false);
        if (IsAvailableSquare(right))
        {
            Helper.AssertNotNull(right);
            yield return right.Value;
        }
    }


    internal override IEnumerable<Coordinate> FindDrops() => FindDrops(2);


    internal override void ForcePromote() => ForcePromote(2);
}