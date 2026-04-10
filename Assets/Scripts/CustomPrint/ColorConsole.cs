namespace MyPrint
{
    public enum ColorConsole
    {
        Red,
        Blue,
        Green,
        Yellow,
        Orange,
        Pink,
        Purple,
        Black,
        Grey,
        Cyan,
        White
    }

    public enum ConsoleStyle
    {
        Bold,
        Italic,
        Underline,
    }

    public class ConsoleOption
    {
        public static string GetColor(ColorConsole color)
        {
            switch (color)
            {
                case ColorConsole.Red: return "<color=#ff4040>";
                case ColorConsole.Blue: return "<color=#5e9cff>";
                case ColorConsole.Green: return "<color=#32821a>";
                case ColorConsole.Yellow: return "<color=#eddf42>";
                case ColorConsole.Orange: return "<color=orange>";
                case ColorConsole.Pink: return "<color=#FFC0CB>";
                case ColorConsole.Purple: return "<color=purple>";
                case ColorConsole.Black: return "<color=black>";
                case ColorConsole.Grey: return "<color=grey>";
                case ColorConsole.Cyan: return "<color=#00FFFF>";
                case ColorConsole.White:
                default: return "<color=white>";
            }
        }

        public static (string, string) GetStyle(ConsoleStyle style)
        {
            switch (style)
            {
                case ConsoleStyle.Bold: return ("<b>", "</b>");
                case ConsoleStyle.Italic: return ("<i>", "</i>");
                default: return ("", "");
            }
        }
    }
}
