namespace ShogiWebsite.Shogi.Pieces;

internal class Rook : Piece
{
    internal Rook(Player player, Board board) : base(player, true, board)
    { }

    internal override IEnumerable<Coordinate> FindMoves()
    {
        Func<Coordinate, int, Coordinate?>[] straightDirections = new[]
        {
            this.board.N, this.board.E, this.board.S, this.board.W
        };
        IEnumerable<Coordinate> moves = this.RangeMoves(straightDirections);
        if (this.isPromoted)
        {
            Func<Coordinate, int, Coordinate?>[] diagonalDirections = new[]
            {
                this.board.NE, this.board.NW, this.board.SE, this.board.SW
            };
            moves = moves.Concat(this.ListMoves(diagonalDirections));
        }
        return moves;
    }
}