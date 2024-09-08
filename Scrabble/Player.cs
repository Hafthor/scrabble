namespace Scrabble;

public class Player {
    private const int MaxTiles = 7;
    public readonly List<Tile> Tiles = [];
    public int Score = 0;

    public void Draw(List<Tile> bag, Random random) {
        int count = Math.Min(MaxTiles - Tiles.Count, bag.Count);
        for (int c = 0; c < count; c++) {
            int i = random.Next(bag.Count);
            Tiles.Add(bag[i]);
            bag.RemoveAt(i);
        }
    }

    public void Rack(List<Tile> bag, Random random) {
        bag.AddRange(Tiles);
        Tiles.Clear();
        Draw(bag, random);
    }
}