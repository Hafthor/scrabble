namespace ScrabbleHelper;

public class Program {
    static void Main(string[] args) {
        int players = args.Length > 0 ? int.Parse(args[0]) : 2;
        Random random = args.Length > 1 ? new(int.Parse(args[1])) : new(0);
        Game game = new(players, random);
        WordList wordList = new();
        int passes = 0;
        while (passes < players) {
            game.board.Print();
            Console.WriteLine($"Player {game.turn + 1} ({game.players[game.turn].score})");
            Console.WriteLine("Your tiles: " + string.Join(" ", game.players[game.turn].tiles));
            string cmd;
            for (;;) {
                Console.Write("Enter row,col,h/v,word: ");
                cmd = Console.ReadLine();
                if (cmd is "") {
                    var plays = game.board.GetPossiblePlays(wordList.WordsByLen, wordList.DorwsByLen,
                            wordList.LetterCountsForWordsByLen, game.players[game.turn].tiles)
                        .OrderBy(p => p.score).ThenBy(p => p.word);
                    foreach (var (y, x, h, w, score, extras) in plays) {
                        Console.Write($"{y + 1},{x + 1},{(h ? 'H' : 'V')},{w}={score - extras.Sum(p => p.score)}");
                        foreach (var (ww, ss) in extras)
                            Console.Write($", {ww}={ss}");
                        Console.WriteLine(extras.Count > 0 ? $", total={score}" : "");
                    }
                    Console.Write("done - ");
                    continue;
                }
                if (cmd == "pass") {
                    if (++passes < players) game.turn = (game.turn + 1) % players;
                    break;
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
                if (!wordList.Words.Contains(word)) {
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
        for (int i = 0; i < players; i++)
            Console.WriteLine($"Player {i + 1}: {game.players[i].score}");
        int maxScore = game.players.Max(p => p.score);
        List<int> winners = game.players.Where(p => p.score == maxScore).Select((_, i) => i + 1).ToList();
        Console.WriteLine($"Winner{(winners.Count > 1 ? "s" : "")}: {string.Join(", ", winners)}");
    }
}