using System;
using System.Collections.Generic;
using System.IO;

namespace EBE.Core.Evaluation
{
    /// <summary>
    /// Token class used for parsing expression.
    /// </summary>
    internal class Token
    {
        static Dictionary<char, KeyValuePair<TokenType, string>> dict = new Dictionary<char, KeyValuePair<TokenType, string>>()
        {
            {
                '(', new KeyValuePair<TokenType, string>(TokenType.OPEN_PAREN, "(")
            },
            {
                ')', new KeyValuePair<TokenType, string>(TokenType.CLOSE_PAREN, ")")
            },
            {
                ':', new KeyValuePair<TokenType, string>(TokenType.BINARY_OP, "OP")
            },
            {
                '+', new KeyValuePair<TokenType, string>(TokenType.TBINARY_OP, "OP")
            },
            {
                '-', new KeyValuePair<TokenType, string>(TokenType.TBINARY_OP, "OP")
            },
            {
                '*', new KeyValuePair<TokenType, string>(TokenType.TBINARY_OP, "OP")
            },
            {
                '/', new KeyValuePair<TokenType, string>(TokenType.TBINARY_OP, "OP")
            },
            {
                '%', new KeyValuePair<TokenType, string>(TokenType.TBINARY_OP, "OP")
            },
            {
                '<', new KeyValuePair<TokenType, string>(TokenType.TBINARY_OP, "OP")
            },
            {
                '>', new KeyValuePair<TokenType, string>(TokenType.TBINARY_OP, "OP")
            },
            {
                '&', new KeyValuePair<TokenType, string>(TokenType.TBINARY_OP, "OP")
            },
            {
                '|', new KeyValuePair<TokenType, string>(TokenType.TBINARY_OP, "OP")
            },
            {
                '^', new KeyValuePair<TokenType, string>(TokenType.TBINARY_OP, "OP")
            }
        };

        public enum TokenType
        {
            OPEN_PAREN,
            CLOSE_PAREN,
            UNARY_OP,
            BINARY_OP,
            TBINARY_OP,
            LITERAL,
            EXPR_END,
            MALFORMED
        }

        public TokenType type;
        public string value;

        public Token(StringReader s)
        {
            int c = s.Read();
            if (c == -1)
            {
                type = TokenType.EXPR_END;
                value = "";
                return;
            }

            char ch = (char)c;

            string str = String.Empty;

            if(ch == ':')
            {
                str += ch;

                if(s.Peek() == -1 && s.Peek() == ':')
                {
                    type = TokenType.MALFORMED;
                    return;
                }

                do
                {
                    str += (char)s.Read();
                }
                while (s.Peek() != -1 && s.Peek() != ':');

                if(s.Peek() == ':')
                {
                    str += (char)s.Read();
                }

                value = str;

                if(str.Length == 3)
                {
                    type = TokenType.TBINARY_OP;
                }
                else
                {
                    type = TokenType.BINARY_OP;
                }
                return;
            }

            if (dict.ContainsKey(ch))
            {
                type = dict[ch].Key;

                if(type == TokenType.TBINARY_OP)
                {
                    value = ch.ToString();
                }
                else
                {
                    value = dict[ch].Value;
                }
            }
            else
            {
                str = String.Empty;
                str += ch;
                while (s.Peek() != -1 && !dict.ContainsKey((char)s.Peek()))
                {
                    str += (char)s.Read();
                }
                type = TokenType.LITERAL;
                value = str;
            }
        }

        public override string ToString()
        {
            if(type == TokenType.BINARY_OP)
            {
                return "OP";
            }
            else if(type == TokenType.LITERAL)
            {
                return value;
            }
            else
            {
                return value;
            }
        }
    }
}

