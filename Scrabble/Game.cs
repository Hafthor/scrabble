namespace Scrabble;

public class Game {
    public readonly Player[] Players;
    public readonly List<Tile> Bag = [];
    public readonly Board Board = new();
    public int Turn;
    private readonly Random _random;

    public Game(int players, Random random) {
        _random = random;
        for (char letter = 'A'; letter <= 'Z'; letter++)
            for (int points = Tile.PointsForLetter(letter), i = 0; i < Tile.CountForLetter(letter); i++)
                Bag.Add(new Tile(letter, points));
        for (int points = Tile.PointsForLetter(' '), i = 0; i < Tile.CountForLetter(' '); i++)
            Bag.Add(new Tile(' ', points));
        Players = new Player[players];
        for (int i = 0; i < players; i++) {
            Players[i] = new Player();
            Players[i].Draw(Bag, random);
        }
    }

    public string Play(string word, int row, int col, bool horizontal, string[][] wordsByLen) {
        Player player = Players[Turn];
        (string error, int score, int usedTiles, List<(string word, int score)> extras) =
            Board.Score(row, col, horizontal, word, player.Tiles, wordsByLen);
        if (error != null) return error;
        score += extras.Sum(e => e.score);
        // commit the play
        List<Tile> tilesToUse = player.Tiles.Where((_, i) => (usedTiles & (1 << i)) != 0).ToList();
        foreach (var c in word) {
            if (Board.Tiles[row, col] == null) {
                Tile t = Board.Tiles[row, col] = tilesToUse.FirstOrDefault(t => t.Letter == c) ??
                                                 tilesToUse.First(t => t.Letter == ' ');
                if (t.OrgLetter == ' ') t.SetLetter(c);
                tilesToUse.Remove(t);
                player.Tiles.Remove(t);
            }
            if (horizontal) col++;
            else row++;
        }
        player.Score += score;
        player.Draw(Bag, _random);
        NextPlayer();
        return null;
    }

    public void NextPlayer() => Turn = (Turn + 1) % Players.Length;
}