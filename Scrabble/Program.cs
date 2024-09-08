namespace Scrabble;

public class Program {
    static void Main(string[] args) {
        int players = args.Length > 0 ? int.Parse(args[0]) : 2;
        int seed = args.Length > 1 ? int.Parse(args[1]) : new Random().Next();
        Random random = new(seed);
        Game game = new(players, random);
        WordList wordList = new();
        int passes = 0;
        while (passes < players) {
            game.board.Print();
            Console.WriteLine($"Player {game.turn + 1} ({game.players[game.turn].score})");
            Console.WriteLine("Your tiles: " + string.Join(" ", game.players[game.turn].tiles));
            string cmd;
            for (;;) {
                Console.Write("Enter row,col,h/v,word or help: ");
                cmd = Console.ReadLine();
                if (cmd is "") {
                    var plays = game.board.GetPossiblePlays(wordList.WordsByLen, wordList.LetterCountsForWordsByLen,
                            game.players[game.turn].tiles)
                        .OrderBy(p => p.score).ThenBy(p => p.word);
                    foreach (var (y, x, h, w, score, extras, underlinePositions) in plays) {
                        string uw = Underline(w, underlinePositions);
                        Console.Write($"{y + 1},{x + 1},{(h ? 'H' : 'V')},{uw}={score - extras.Sum(p => p.score)}");
                        foreach (var (ww, ss) in extras)
                            Console.Write($", {ww}={ss}");
                        Console.WriteLine(extras.Count > 0 ? $", total={score}" : "");
                    }
                    Console.Write("done - ");
                    continue;
                } else if (cmd == "seed") {
                    Console.WriteLine($"Seed={seed}");
                    continue;
                } else if (cmd == "pass") {
                    if (++passes < players) game.turn = (game.turn + 1) % players;
                    break;
                } else if (cmd == "help") {
                    Console.WriteLine("Commands: pass | rack | help | seed | exit");
                    Console.WriteLine("or enter row,col,h/v,word to play a word");
                    continue;
                }
                passes = 0;
                if (cmd == "rack") {
                    game.bag.AddRange(game.players[game.turn].tiles);
                    game.players[game.turn].tiles.Clear();
                    game.ShuffleBag(random);
                    game.players[game.turn].Draw(game.bag);
                    game.turn = (game.turn + 1) % players;
                    break;
                }
                if (cmd is null or "exit" or "quit") break;
                string[] parts = cmd.Split(',');
                int row = int.Parse(parts[0]) - 1, col = int.Parse(parts[1]) - 1;
                bool horizontal = parts[2].ToUpper() == "H";
                string word = parts[3].ToUpper();
                if (!wordList.WordsByLen[word.Length].Contains(word)) {
                    Console.WriteLine("Invalid word");
                    continue;
                }
                int currentPlayer = game.turn, currentScore = game.players[currentPlayer].score;
                string error = game.Play(word, row, col, horizontal, wordList.WordsByLen);
                if (error == null) {
                    int newScore = game.players[currentPlayer].score;
                    Console.WriteLine(
                        $"Player {currentPlayer + 1} adds {newScore - currentScore} to score, new score = {newScore}");
                    break;
                }
                Console.WriteLine(error);
            }
            if (cmd is "exit" or "quit") break;
        }
        game.board.Print();
        Console.WriteLine("Game over");
        int maxScore = game.players.Max(p => p.score);
        for (int i = 0; i < players; i++)
            Console.WriteLine($"Player {i + 1}: {game.players[i].score}" +
                              (game.players[i].score == maxScore ? " (winner)" : ""));
    }
    
    private static string Underline(string word, int positions) {
        for (int i = word.Length; --i >=0;)
            if ((positions & (1 << i)) != 0)
                word = word.Insert(i + 1, Ansi.Reset).Insert(i, Ansi.Underline);
        return word;
    }
}