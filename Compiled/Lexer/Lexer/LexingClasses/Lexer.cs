using System;
using System.Collections.Generic;
using System.Linq;

namespace WHILE_Lexer
{
    public class Lexer
    {
        public static int pos = 0;
        public List<Token> tokens;

        public Lexer(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Lexer()
        {
            tokens = new List<Token>();
        }

        public Lexer(List<Pair<string, string>> pairs)
        {
            tokens = new List<Token>();
            pairs.ForEach(x => tokens.Add(x));
        }

        private static Rexp CharListToRexp(List<char> s)
        {
            if (s.Count == 0)
            {
                return new One();
            }
            else if (s.Count == 1)
            {
                char c = s[0];
                s.RemoveAt(0);
                return new Char(c);
            }

            char ch = s[0];
            s.RemoveAt(0);
            return new Seq(new Char(ch), Lexer.CharListToRexp(s));
        }

        public List<Token> RemoveCommentsAndWhitespaces()
        {
            return tokens.Where(token => token.id != "c" && token.id != "w").ToList();
        }

        public void Display()
        {
            foreach (var token in tokens)
            {
                Console.WriteLine(GetTokenById(token.id) + " : " + token.value);
            }
        }

        public static string GetTokenById(string id)
        {
            return id switch
            {
                "k" => "Keyword",
                "i" => "Id",
                "o" => "Operation",
                "n" => "Number",
                "s" => "Semicolon",
                "str" => "String",
                "p" => "Paren",
                "w" => "Whitespace",
                "c" => "Comment",
                _ => ""
            };
        }

        public static Rexp StringToRexp(string s)
        {
            var chars = s.ToCharArray().ToList();
            return Lexer.CharListToRexp(chars);
        }

        private static bool IsNullable(Rexp r)
        {
            if ((r is Zero))
            {
                return false;
            }
            else if ((r is One))
            {
                return true;
            }
            else if ((r is Char))
            {
                return false;
            }
            else if ((r is Alt alt))
            {
                return (Lexer.IsNullable(alt.r1) || Lexer.IsNullable(alt.r2));
            }
            else if ((r is Seq seq))
            {
                return (Lexer.IsNullable(seq.r1) && Lexer.IsNullable(seq.r2));
            }
            else if ((r is Star))
            {
                return true;
            }
            else if ((r is Recd recd))
            {
                return Lexer.IsNullable(recd.r);
            }
            else
            {
                return false;
            }

        }

        private static Rexp Der(char c, Rexp r)
        {
            return r switch
            {
                Zero => new Zero(),
                One => new Zero(),
                Char ch1 when (c == ch1.c) => new One(),
                Char => new Zero(),
                Alt alt => new Alt(Lexer.Der(c, alt.r1), Lexer.Der(c, alt.r2)),
                Seq seq when Lexer.IsNullable(seq.r1) => new Alt(
                    new Seq(Lexer.Der(c, seq.r1), seq.r2), Lexer.Der(c, seq.r2)),
                Seq seq => new Seq(Lexer.Der(c, seq.r1), seq.r2),
                Star star => new Seq(Lexer.Der(c, star.r), new Star(star.r)),
                _ => Lexer.Der(c, ((Recd)(r)).r)
            };
        }

        public static Rexp Ders(Rexp r, List<char> list)
        {
            while (true)
            {
                if ((list.Count == 0))
                {
                    return r;
                }

                char ch = list[0];
                list.RemoveAt(0);
                r = Lexer.Der(ch, r);
            }
        }

        private static string Flatten(Val v)
        {
            if (v is Stars stars)
            {
                List<Val> inputList = new();
                inputList.AddRange(stars.vs);
                return inputList.Aggregate("", (current, obj) => (current + Lexer.Flatten(obj)));
            }

            return v switch
            {
                Empty => "",
                Chr chr => char.ToString(chr.c),
                Left left => Lexer.Flatten(left.v),
                Right right => Lexer.Flatten(right.v),
                Sequ sequ => (Lexer.Flatten(sequ.v1) + Lexer.Flatten(sequ.v2)),
                Rec rec => Lexer.Flatten(rec.v),
                _ => ""
            };
        }

        private static List<Pair<string, string>> Env(Val v)
        {
            switch (v)
            {
                case Empty:
                    return new List<Pair<string, string>>();
                case Chr:
                    return new List<Pair<string, string>>();
                case Left left:
                    return Lexer.Env(left.v);
                case Right right:
                    return Lexer.Env(right.v);
                case Sequ sequ:
                {
                    List<Pair<string, string>> list1 = Lexer.Env(sequ.v1);
                    List<Pair<string, string>> list2 = Lexer.Env(sequ.v2);
                    List<Pair<string, string>> res = new();
                    res.AddRange(list1);
                    res.AddRange(list2);
                    return res;
                }
                case Stars stars:
                {
                    List<Val> inp = stars.vs;
                    List<Pair<string, string>> res = new();
                    foreach (Val obj in inp)
                    {
                        res.AddRange(Lexer.Env(obj));
                    }

                    return res;
                }
                case Rec rec:
                {
                    List<Pair<string, string>> list = new();
                    list.AddRange(Lexer.Env(rec.v));
                    list.Insert(0, new Pair<string, string>(rec.x, Lexer.Flatten(rec.v)));
                    return list;
                }
                default:
                    return new List<Pair<string, string>>();
            }
        }

        private static Val Mkeps(Rexp r)
        {
            return r switch
            {
                One => new Empty(),
                Alt alt when Lexer.IsNullable(alt.r1) => new Left(Lexer.Mkeps(alt.r1)),
                Alt alt => new Right(Lexer.Mkeps(alt.r2)),
                Seq seq => new Sequ(Lexer.Mkeps(seq.r1), Lexer.Mkeps(seq.r2)),
                Star => new Stars(new List<Val>()),
                Recd recd => new Rec(recd.x, Lexer.Mkeps(recd.r)),
                _ => new Empty()
            };
        }

        private static Val Inj(Rexp r, char c, Val v)
        {
            switch (r)
            {
                case Star star when (v as Sequ)?.v2 is Stars:
                {
                    List<Val> list = new();
                    list.Add(Lexer.Inj(star.r, c, ((Sequ)(v)).v1));
                    list.AddRange(((Stars)(((Sequ)(v)).v2)).vs);
                    return new Stars(list);
                }
                case Seq seq when (v is Sequ sequ):
                    return new Sequ(Lexer.Inj(seq.r1, c, sequ.v1), sequ.v2);
                case Seq seq when (v as Left)?.v is Sequ:
                    return new Sequ(Lexer.Inj(seq.r1, c, ((Sequ)(((Left)(v)).v)).v1), ((Sequ)(((Left)(v)).v)).v2);
            }

            if ((r is Seq seq1) && (v is Right right))
            {
                return new Sequ(Lexer.Mkeps(seq1.r1), Lexer.Inj(seq1.r2, c, right.v));
            }

            if ((r is Alt alt) && (v is Left left))
            {
                return new Left(Lexer.Inj(alt.r1, c, left.v));
            }

            if ((r is Alt alt1) && (v is Right right1))
            {
                return new Right(Lexer.Inj(alt1.r2, c, right1.v));
            }

            if ((r is Char) && (v is Empty))
            {
                return new Chr(c);
            }

            if (r is Recd recd)
            {
                return new Rec(recd.x, Lexer.Inj(recd.r, c, v));
            }

            return null;
        }

        public static Pair<Rexp, Func<Val, Val>> Simp(Rexp r)
        {
            switch (r)
            {
                case Alt alt:
                {
                    Pair<Rexp, Func<Val, Val>> pair1 = Lexer.Simp(alt.r1);
                    Pair<Rexp, Func<Val, Val>> pair2 = Lexer.Simp(alt.r2);
                    if (pair1.first is Zero)
                    {
                        return new Pair<Rexp, Func<Val, Val>>(pair2.first, Rectification.F_RIGHT(pair2.second));
                    }
                    else if (pair2.first is Zero)
                    {
                        return new Pair<Rexp, Func<Val, Val>>(pair1.first, Rectification.F_LEFT(pair1.second));
                    }
                    else
                    {
                        //may throw exception in an unknown case
                        return new Pair<Rexp, Func<Val, Val>>(new Alt(pair1.first, pair2.first), Rectification.F_ALT(pair1.second, pair2.second));
                    }
                }
                case Seq seq:
                {
                    Pair<Rexp, Func<Val, Val>> pair1 = Lexer.Simp(seq.r1);
                    Pair<Rexp, Func<Val, Val>> pair2 = Lexer.Simp(seq.r2);
                    if (pair1.first is Zero)
                    {
                        return new Pair<Rexp, Func<Val, Val>>(new Zero(), Rectification.F_ERROR());
                    }
                    else if (pair2.first is Zero)
                    {
                        return new Pair<Rexp, Func<Val, Val>>(new Zero(), Rectification.F_ERROR());
                    }
                    else if (pair1.first is One)
                    {
                        return new Pair<Rexp, Func<Val, Val>>(pair2.first, Rectification.F_SEQ_EMPTY1(pair1.second, pair2.second));
                    }
                    else if (pair2.first is One)
                    {
                        return new Pair<Rexp, Func<Val, Val>>(pair1.first, Rectification.F_SEQ_EMPTY2(pair1.second, pair2.second));
                    }
                    else
                    {
                        return new Pair<Rexp, Func<Val, Val>>(new Seq(pair1.first, pair2.first), Rectification.F_SEQ(pair1.second, pair2.second));
                    }
                }
                default:
                    return new Pair<Rexp, Func<Val, Val>>(r, Rectification.F_ID);
            }
        }

        private static Val LexSimp(Rexp r, List<char> s)
        {
            if (s.Count == 0)
            {
                if (Lexer.IsNullable(r))
                {
                    return Lexer.Mkeps(r);
                }
                else
                {
                    throw new Exception($"lexing error occured in '{r}' type");
                }
            }

            char c = (char)s[0];
            s.RemoveAt(0);
            return Inj(r, c, Lexer.LexSimp(Lexer.Der(c, r), s));

        }

        private static List<Pair<string, string>> LexingSimp(Rexp r, string s)
        {
            List<char> list = new();
            list.AddRange(s);

            return Lexer.Env(Lexer.LexSimp(r, list));
        }

        public void Tokenizer(string line)
        {
            var pairs = Lexer.LexingSimp(LexingRules.WHILE_REGS(), line);
            this.tokens.AddRange(pairs.PairsToTokens());
        }
    }

}
