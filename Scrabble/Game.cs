namespace ScrabbleHelper;

public class Game {
    public readonly Player[] players;
    public int turn = 0;
    public readonly List<Tile> bag;
    public readonly Board board;

    public Game(int players, Random random = null) {
        board = new();
        bag = new();
        foreach (var letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ ") {
            int points = Tile.PointsForLetter(letter);
            for (int i = 0; i < Tile.CountForLetter(letter); i++)
                bag.Add(new Tile(letter, points));
        }
        ShuffleBag(random);
        this.players = new Player[players];
        for (int i = 0; i < players; i++)
            this.players[i] = new Player(bag);
    }

    public void ShuffleBag(Random random = null) {
        random ??= new();
        // Fisher-Yates shuffle
        for (int i = bag.Count - 1; i > 0; i--) {
            int j = random.Next(i + 1);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }
    }

    public string Play(string word, int row, int col, bool horizontal, string[][] wordsByLen) {
        Player player = players[turn];
        (int score, int usedTiles, List<(string word, int score)> perpendiculars) = board.GetScore(row, col, horizontal, word, player.tiles, wordsByLen);
        if (score == 0) return "Invalid play";
        // commit the play
        List<Tile> tilesToUse = player.tiles.Where((_, i) => (usedTiles & (1 << i)) != 0).ToList();
        foreach (var c in word) {
            if (board.board[row, col] == null) {
                Tile t = board.board[row, col] = tilesToUse.FirstOrDefault(t => t.Letter == c) ??
                                                 tilesToUse.First(t => t.Letter == ' ');
                if (t.orgLetter == ' ') t.SetLetter(c);
                tilesToUse.Remove(t);
                player.tiles.Remove(t);
            }
            if (horizontal) col++;
            else row++;
        }
        player.Draw(bag);
        player.score += score;
        turn = (turn + 1) % players.Length;
        return null;
    }
}