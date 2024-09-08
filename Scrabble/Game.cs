namespace Scrabble;

public class Game {
    public readonly Player[] Players;
    public readonly List<Tile> Bag;
    public readonly Board Board;
    public int Turn;

    public Game(int players, Random random = null) {
        Board = new();
        Bag = new();
        for (char letter = 'A'; letter <= 'Z'; letter++) {
            int points = Tile.PointsForLetter(letter);
            for (int i = 0; i < Tile.CountForLetter(letter); i++)
                Bag.Add(new Tile(letter, points));
        }
        {
            char letter = ' ';
            int points = Tile.PointsForLetter(letter);
            for (int i = 0; i < Tile.CountForLetter(letter); i++)
                Bag.Add(new Tile(letter, points));
        }
        ShuffleBag(random);
        Players = new Player[players];
        for (int i = 0; i < players; i++)
            Players[i] = new Player(Bag);
    }

    public void ShuffleBag(Random random = null) {
        random ??= new();
        // Fisher-Yates shuffle
        for (int i = Bag.Count - 1; i > 0; i--) {
            int j = random.Next(i + 1);
            (Bag[i], Bag[j]) = (Bag[j], Bag[i]);
        }
    }

    public string Play(string word, int row, int col, bool horizontal, string[][] wordsByLen) {
        Player player = Players[Turn];
        (int score, int usedTiles, List<(string word, int score)> extras) =
            Board.GetScore(row, col, horizontal, word, player.Tiles, wordsByLen);
        if (extras != null) score += extras.Sum(e => e.score);
        if (score == 0) return "Invalid play";
        // commit the play
        List<Tile> tilesToUse = player.Tiles.Where((_, i) => (usedTiles & (1 << i)) != 0).ToList();
        foreach (var c in word) {
            if (Board.TheBoard[row, col] == null) {
                Tile t = Board.TheBoard[row, col] = tilesToUse.FirstOrDefault(t => t.Letter == c) ??
                                                 tilesToUse.First(t => t.Letter == ' ');
                if (t.OrgLetter == ' ') t.SetLetter(c);
                tilesToUse.Remove(t);
                player.Tiles.Remove(t);
            }
            if (horizontal) col++;
            else row++;
        }
        player.Draw(Bag);
        player.Score += score;
        Turn = (Turn + 1) % Players.Length;
        return null;
    }
}