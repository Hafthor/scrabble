namespace Scrabble;

public class Tile {
    public readonly int points;
    public readonly char orgLetter;
    public char Letter { get; private set; }

    public Tile(char letter, int points) {
        Letter = orgLetter = letter;
        this.points = points;
    }

    public void SetLetter(char letter) {
        if (orgLetter != ' ')
            throw new InvalidOperationException("Cannot change the letter of a non-blank tile");
        Letter = letter;
    }

    public override string ToString() => (Letter == ' ' ? '_' : Letter) + "" + (points == 10 ? "$" : points);

    public static int CountForLetter(char letter) => letter switch {
        'E' => 12,
        'A' or 'I' => 9,
        'O' => 8,
        'N' or 'R' or 'T' => 6,
        'L' or 'S' or 'U' or 'D' => 4,
        'G' => 3,
        'K' or 'J' or 'X' or 'Q' or 'Z' => 1,
        _ => 2,
    };

    public static int PointsForLetter(char letter) => letter switch {
        ' ' => 0,
        'D' or 'G' => 2,
        'B' or 'C' or 'M' or 'P' => 3,
        'F' or 'H' or 'V' or 'W' or 'Y' => 4,
        'K' => 5,
        'J' or 'X' => 8,
        'Q' or 'Z' => 10,
        _ => 1,
    };
}