namespace ScrabbleHelper;

public class Player {
    public readonly List<Tile> tiles = new();
    public int score = 0;

    public Player(List<Tile> bag) => Draw(bag);

    public void Draw(List<Tile> bag) {
        int count = Math.Min(7 - tiles.Count, bag.Count);
        tiles.AddRange(bag[^count..]);
        bag.RemoveRange(bag.Count - count, count);
    }
}