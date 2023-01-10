namespace ShogiWebsite.Shogi.Pieces;

internal class Gold : Piece
{
    internal Gold(Player player, Board board) : base(player, false, board)
    { }


    internal override IEnumerable<Coordinate> FindMoves() => GoldMoves();
}