namespace WHILE_Lexer
{
    public class StringOps
    {

        // operator |
        public static Alt verticalBarOP(string s, Rexp r)
        {
            return new Alt(Lexer.StringToRexp(s), r);
        }

        public static Alt verticalBarOP(string s, string r)
        {
            return new Alt(Lexer.StringToRexp(s), Lexer.StringToRexp(r));
        }

        // operator %
        public static Star percentOP(string s)
        {
            return new Star(Lexer.StringToRexp(s));
        }

        // operator ~
        public static Seq tildeOP(string s, Rexp r)
        {
            return new Seq(Lexer.StringToRexp(s), r);
        }

        public static Seq tildeOP(string s, string r)
        {
            return new Seq(Lexer.StringToRexp(s), Lexer.StringToRexp(r));
        }

        // operator $
        public static Recd dollarOP(string s, Rexp r)
        {
            return new Recd(s, r);
        }
    }
}