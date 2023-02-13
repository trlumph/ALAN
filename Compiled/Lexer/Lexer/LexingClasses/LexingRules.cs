using System;
using System.Collections.Generic;
using System.Linq;

namespace WHILE_Lexer
{
    public class LexingRules
    {
        private static Seq Plus(Rexp r)
        {
            return RexpOps.tildeOP(r, RexpOps.percentOP(r));
        }

        private static Rexp Range(List<char> s)
        {
            if (s.Count == 0)
            {
                return new Zero();
            }
            
            char ch = (char)s[0];

            if (s.Count == 1)
            {
                s.RemoveAt(0);
                return new Char(ch);
            }
            
            s.RemoveAt(0);
            return new Alt(new Char(ch), LexingRules.Range(s));
        }

        private static Rexp Range(string s)
        {
            var chars = s.ToCharArray().ToList();
            return LexingRules.Range(chars);
        }

        private static Rexp Letter()
        {
            return LexingRules.Range("ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz_");
        }

        private static Rexp SYM()
        {
            return RexpOps.verticalBarOP(Letter(), Range(" .><=;,:"));
        }

        private static Rexp Digit()
        {
            return LexingRules.Range("0123456789");
        }

        private static Rexp Id()
        {
            return RexpOps.tildeOP(LexingRules.Letter(), RexpOps.percentOP(RexpOps.verticalBarOP(LexingRules.Letter(), LexingRules.Digit())));
        }

        private static Rexp Num()
        {
            return LexingRules.Plus(LexingRules.Digit());
        }

        private static Rexp Keyword()
        {
            string[] str = {
                "skip",
                "while",
                "do",
                "if",
                "then",
                "else",
                "read",
                "write",
                "for",
                "upto",
                "true",
                "false"
            };
            
            Rexp res = StringOps.verticalBarOP(str[0], str[1]);
            for (var i = 2; i < str.Length; i++)
            {
                res = StringOps.verticalBarOP(str[i], res);
            }

            return res;
        }

        private static readonly Rexp Semicolon = new Char(';'); //PP not sure

        private static Rexp Operator()
        {
            string[] str =  {
                "" +
                "=",
                "==",
                "-",
                "+",
                "*",
                "!=",
                "<",
                ">",
                "%",
                "&&",
                "||",
                "<=",
                ">="
            };
            
            Rexp res = StringOps.verticalBarOP(str[0], str[1]);
            for (var i = 2; i < str.Length; i++)
            {
                res = StringOps.verticalBarOP(str[i], res);
            }

            return res;
        }

        private static Rexp Whitespace()
        {
            string[] str = {
                " ",
                "\n",
                "\t",
                "\r"
            };
            Rexp res = StringOps.verticalBarOP(str[0], str[1]);
            for (var i = 2; (i < str.Length); i++)
            {
                res = StringOps.verticalBarOP(str[i], res);
            }

            return LexingRules.Plus(res);
        }


        private static Rexp Rparen()
        {
            return StringOps.verticalBarOP("{", "(");
        }

        private static Rexp Lparen()
        {
            return StringOps.verticalBarOP("}", ")");
        }

        //Check if order is correct
        private static Rexp STRING()
        {
            Rexp res = RexpOps.tildeOP(new Char('\"'), RexpOps.percentOP(LexingRules.Letter()));
            return RexpOps.tildeOP(res, new Char('\"'));
        }

        public static Rexp COMMENT()
        {
            return RexpOps.tildeOP(RexpOps.powOP(new Char('/'),times: 2), RexpOps.percentOP(RexpOps.verticalBarOP(SYM(), Digit())));
        }

        public static Rexp WHILE_REGS()
        {
            Rexp rexp = StringOps.dollarOP("k", LexingRules.Keyword());
            rexp = RexpOps.verticalBarOP(rexp, StringOps.dollarOP("i", LexingRules.Id()));
            rexp = RexpOps.verticalBarOP(rexp, StringOps.dollarOP("o", LexingRules.Operator()));
            rexp = RexpOps.verticalBarOP(rexp, StringOps.dollarOP("n", LexingRules.Num()));
            rexp = RexpOps.verticalBarOP(rexp, StringOps.dollarOP("s", LexingRules.Semicolon));
            rexp = RexpOps.verticalBarOP(rexp, StringOps.dollarOP("str", LexingRules.STRING()));
            rexp = RexpOps.verticalBarOP(rexp, StringOps.dollarOP("p", RexpOps.verticalBarOP(LexingRules.Lparen(), LexingRules.Rparen())));
            rexp = RexpOps.verticalBarOP(rexp, StringOps.dollarOP("w", LexingRules.Whitespace()));
            rexp = RexpOps.verticalBarOP(rexp, StringOps.dollarOP("c", LexingRules.COMMENT()));
            return RexpOps.percentOP(rexp);
        }
    }
}