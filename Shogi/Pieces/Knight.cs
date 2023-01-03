namespace ShogiWebsite.Shogi.Pieces;

internal class Knight : Piece
{
    internal Knight(Player player, Board board) : base(player, true, board)
    { }

    internal override IEnumerable<Coordinate> FindMoves()
    {
        return this.isPromoted ? this.GoldMoves() : this.KnightMoves();
    }

    private IEnumerable<Coordinate> KnightMoves()
    {
        Coordinate? left = this.Knight(true);
        if (this.IsAvailableSquare(left))
        {
            Helper.AssertNotNull(left);
            yield return left.Value;
        }
        Coordinate? right = this.Knight(false);
        if (this.IsAvailableSquare(right))
        {
            Helper.AssertNotNull(right);
            yield return right.Value;
        }
    }

    internal override IEnumerable<Coordinate> FindDrops()
    {
        return this.FindDrops(2);
    }

    internal override void ForcePromote()
    {
        this.ForcePromote(2);
    }
}