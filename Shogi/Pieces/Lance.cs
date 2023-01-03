namespace ShogiWebsite.Shogi.Pieces;

internal class Lance : Piece
{
    internal Lance(Player player, Board board) : base(player, true, board)
    { }

    internal override IEnumerable<Coordinate> FindMoves()
    {
        return this.isPromoted ? this.GoldMoves() : this.LanceMoves();
    }

    private IEnumerable<Coordinate> LanceMoves()
    {
        return this.RangeMoves(new Func<Coordinate, int, Coordinate?>[] { this.Front() });
    }

    internal override IEnumerable<Coordinate> FindDrops()
    {
        return this.FindDrops(1);
    }

    internal override void ForcePromote()
    {
        this.ForcePromote(1);
    }
}