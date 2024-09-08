namespace Scrabble;

public class Player {
    public readonly List<Tile> Tiles = [];
    public int Score = 0;

    public Player(List<Tile> bag) => Draw(bag);

    public void Draw(List<Tile> bag) {
        int count = Math.Min(7 - Tiles.Count, bag.Count);
        Tiles.AddRange(bag[^count..]);
        bag.RemoveRange(bag.Count - count, count);
    }
}