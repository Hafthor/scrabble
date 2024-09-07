namespace ScrabbleHelper;

public static class Ansi {
    public static readonly string Reset = "\u001b[0m";
    public static readonly string ClearScreen = "\u001b[2J\u001b[0;0H";
    public static readonly string ClearLine = "\u001b[2K";
    public static readonly string HideCursor = "\u001b[?25l";
    public static readonly string ShowCursor = "\u001b[?25h";
    public static string MoveCursor(int row, int col) => $"\u001b[{row};{col}H";
    public static readonly string SaveCursor = "\u001b[s";
    public static readonly string RestoreCursor = "\u001b[u";
    public static readonly string Bold = "\u001b[1m";
    public static readonly string Underline = "\u001b[4m";
    public static readonly string Inverse = "\u001b[7m";
    
    public static readonly string Black = "\u001b[30m";
    public static readonly string Red = "\u001b[31m";
    public static readonly string Green = "\u001b[32m";
    public static readonly string Brown = "\u001b[33m";
    public static readonly string Blue = "\u001b[34m";
    public static readonly string Magenta = "\u001b[35m";
    public static readonly string Cyan = "\u001b[36m";
    public static readonly string White = "\u001b[37m";
    
    public static readonly string BlackBg = "\u001b[40m";
    public static readonly string RedBg = "\u001b[41m";
    public static readonly string GreenBg = "\u001b[42m";
    public static readonly string BrownBg = "\u001b[43m";
    public static readonly string BlueBg = "\u001b[44m";
    public static readonly string MagentaBg = "\u001b[45m";
    public static readonly string CyanBg = "\u001b[46m";
    public static readonly string WhiteBg = "\u001b[47m";
    
    public static readonly string BBlack = "\u001b[90m";
    public static readonly string BRed = "\u001b[91m";
    public static readonly string BGreen = "\u001b[92m";
    public static readonly string Yellow = "\u001b[93m";
    public static readonly string BBlue = "\u001b[94m";
    public static readonly string Pink = "\u001b[95m";
    public static readonly string BCyan = "\u001b[96m";
    public static readonly string BWhite = "\u001b[97m";
    
    public static readonly string BBlackBg = "\u001b[100m";
    public static readonly string BRedBg = "\u001b[101m";
    public static readonly string BGreenBg = "\u001b[102m";
    public static readonly string YellowBg = "\u001b[103m";
    public static readonly string BBlueBg = "\u001b[104m";
    public static readonly string PinkBg = "\u001b[105m";
    public static readonly string BCyanBg = "\u001b[106m";
    public static readonly string BWhiteBg = "\u001b[107m";
}