namespace ShogiWebsite.Shogi.Pieces;

internal class Bishop : Piece
{
    internal Bishop(Player player, Board board) : base(player, true, board)
    { }

    internal override IEnumerable<Coordinate> FindMoves()
    {
        Func<Coordinate, int, Coordinate?>[] diagonal = new[]
        {
            this.board.NE, this.board.NW, this.board.SE, this.board.SW
        };
        IEnumerable<Coordinate> moves = this.RangeMoves(diagonal);
        if (this.isPromoted)
        {
            Func<Coordinate, int, Coordinate?>[] straight = new[]
            {
                this.board.N, this.board.E, this.board.S, this.board.W
            };
            moves = moves.Concat(this.ListMoves(straight));
        }
        return moves;
    }
}