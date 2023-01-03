namespace ShogiWebsite.Shogi.Pieces;

internal class Silver : Piece
{
    internal Silver(Player player, Board board) : base(player, true, board)
    { }

    internal override IEnumerable<Coordinate> FindMoves()
    {
        return this.isPromoted ? this.GoldMoves() : this.SilverMoves();
    }

    private IEnumerable<Coordinate> SilverMoves()
    {
        Func<Coordinate, int, Coordinate?>[] directions = new[]
        {
            this.Front(), this.FrontLeft(), this.FrontRight(),
            this.BackLeft(), this.BackRight()
        };
        return this.ListMoves(directions);
    }
}