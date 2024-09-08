namespace Scrabble;

public static class Program {
    public static void Main(string[] args) {
        int players = args.Length > 0 ? int.Parse(args[0]) : 2;
        int seed = args.Length > 1 ? int.Parse(args[1]) : new Random().Next();
        Random random = new(seed);
        Game game = new(players, random);
        WordList wordList = new();
        int passes = 0;
        while (passes < players) {
            game.Board.Print();
            var curPlayer = game.Players[game.Turn];
            Console.Write($"Player {game.Turn + 1} ({curPlayer.Score}) tiles: ");
            Console.WriteLine(string.Join(" ", curPlayer.Tiles));
            string cmd;
            for (;;) {
                Console.Write("Enter row,col,h/v,word or help: ");
                cmd = Console.ReadLine();
                if (cmd is "") {
                    var plays = game.Board.GetPossiblePlays(wordList.WordsByLen, wordList.LetterCountsForWordsByLen,
                            curPlayer.Tiles)
                        .OrderBy(p => p.score + p.extras.Sum(e => e.score)).ThenBy(p => p.word);
                    foreach (var (y, x, h, w, score, extras, underlinePositions) in plays) {
                        string uw = Underline(w, underlinePositions);
                        Console.Write($"{y + 1},{x + 1},{(h ? 'H' : 'V')},{uw}={score}");
                        foreach (var (ww, ss) in extras)
                            Console.Write($", {ww}={ss}");
                        Console.WriteLine(extras.Count > 0 ? $", total={score + extras.Sum(p => p.score)}" : "");
                    }
                    Console.Write("done - ");
                    continue;
                } else if (cmd == "seed") {
                    Console.WriteLine($"Seed={seed}");
                    continue;
                } else if (cmd == "pass") {
                    if (++passes < players) game.Turn = (game.Turn + 1) % players;
                    break;
                } else if (cmd == "help") {
                    Console.WriteLine("Commands: pass | rack | help | seed | exit");
                    Console.WriteLine("or enter row,col,h/v,word to play a word");
                    continue;
                }
                passes = 0;
                if (cmd == "rack") {
                    game.Bag.AddRange(curPlayer.Tiles);
                    curPlayer.Tiles.Clear();
                    game.ShuffleBag(random);
                    curPlayer.Draw(game.Bag);
                    game.Turn = (game.Turn + 1) % players;
                    break;
                }
                if (cmd is null or "exit" or "quit") break;
                string[] cmdParts = cmd.Split(',');
                int row = int.Parse(cmdParts[0]) - 1, col = int.Parse(cmdParts[1]) - 1;
                bool horizontal = cmdParts[2].ToUpper() == "H";
                string word = cmdParts[3].ToUpper();
                if (!wordList.WordsByLen[word.Length].Contains(word)) {
                    Console.WriteLine("Invalid word");
                    continue;
                }
                int currentPlayerTurn = game.Turn, currentScore = curPlayer.Score;
                string error = game.Play(word, row, col, horizontal, wordList.WordsByLen);
                if (error == null) {
                    int newScore = curPlayer.Score;
                    Console.WriteLine(
                        $"Player {currentPlayerTurn + 1} adds {newScore - currentScore} to score, new score = {newScore}");
                    break;
                }
                Console.WriteLine(error);
            }
            if (cmd is "exit" or "quit") break;
        }
        game.Board.Print();
        Console.WriteLine("Game over");
        int maxScore = game.Players.Max(p => p.Score);
        for (int i = 0; i < players; i++) {
            var p = game.Players[i];
            Console.WriteLine($"Player {i + 1}: {p.Score}" + (p.Score == maxScore ? " (winner)" : ""));
        }
    }

    private static string Underline(string word, int positions) {
        for (int i = word.Length; --i >= 0;)
            if ((positions & (1 << i)) != 0)
                word = word.Insert(i + 1, Ansi.Reset).Insert(i, Ansi.Underline);
        return word;
    }
}