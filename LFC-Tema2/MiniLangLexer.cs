using System;
using System.Collections;

namespace LFC_Tema2
{
    public class Token
    {
        public string Type { get; set; }
        public string Lexeme { get; set; }
        public int LineNumber { get; set; }

        public Token(string type, string lexeme, int lineNumber)
        {
            Type = type;
            Lexeme = lexeme;
            LineNumber = lineNumber;
        }

        public override string ToString()
        {
            return $"<{Type}, {Lexeme}, {LineNumber}>";
        }
    }

    // analizator lexical
    public class MiniLangLexer
    {
        private readonly string input;
        private int position;
        private int line;
        private int column;
        private ArrayList tokens;
        private ArrayList lexicalErrors;

        private static readonly string[] Keywords = new string[]
        {
            "int", "float", "double", "string", "void",
            "const", "if", "else", "for", "while", "return"
        };

        public MiniLangLexer(string input)
        {
            this.input = input;
            this.position = 0;
            this.line = 1;
            this.column = 1;
            this.tokens = new ArrayList();
            this.lexicalErrors = new ArrayList();
        }

        public ArrayList Tokenize()
        {
            while (position < input.Length)
            {
                SkipWhitespaceAndComments();
                if (position >= input.Length)
                    break;

                char current = input[position];

                if (char.IsLetter(current) || current == '_')
                {
                    ScanIdentifierOrKeyword();
                }
                else if (char.IsDigit(current))
                {
                    ScanNumber();
                }
                else if (current == '"')
                {
                    ScanString();
                }
                else if (IsOperatorChar(current))
                {
                    ScanOperator();
                }
                else if (IsDelimiter(current))
                {
                    ScanDelimiter();
                }
                else
                {
                    // caracter necunoscut
                    lexicalErrors.Add($"Lexical error at line {line}, column {column}: Unknown character '{current}'");
                    position++;
                    column++;
                }
            }

            return tokens;
        }

        public ArrayList GetLexicalErrors()
        {
            ArrayList result = new ArrayList();
            for (int i = 0; i < lexicalErrors.Count; i++)
                result.Add(lexicalErrors[i]);
            return result;
        }

        private void SkipWhitespaceAndComments()
        {
            while (position < input.Length)
            {
                char current = input[position];

                if (char.IsWhiteSpace(current))
                {
                    if (current == '\n')
                    {
                        line++;
                        column = 1;
                    }
                    else
                    {
                        column++;
                    }
                    position++;
                }
                else if (position + 1 < input.Length && current == '/' && input[position + 1] == '/')
                {
                    // comentariu pe o linie
                    while (position < input.Length && input[position] != '\n')
                        position++;
                }
                else if (position + 1 < input.Length && current == '/' && input[position + 1] == '*')
                {
                    // bloc de comentariu
                    int startLine = line;
                    position += 2;
                    column += 2;
                    bool closed = false;
                    while (position + 1 < input.Length)
                    {
                        if (input[position] == '*' && input[position + 1] == '/')
                        {
                            position += 2;
                            column += 2;
                            closed = true;
                            break;
                        }
                        if (input[position] == '\n')
                        {
                            line++;
                            column = 1;
                        }
                        else
                        {
                            column++;
                        }
                        position++;
                    }
                    if (!closed)
                    {
                        lexicalErrors.Add($"Lexical error at line {startLine}: Unterminated block comment");
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void ScanIdentifierOrKeyword()
        {
            int startPos = position;
            int startLine = line;

            while (position < input.Length && (char.IsLetterOrDigit(input[position]) || input[position] == '_'))
            {
                position++;
                column++;
            }

            // extrage lexema si determina tipul token-ului (keyword sau identifier)
            string lexeme = input.Substring(startPos, position - startPos);
            string tokenType = IsKeyword(lexeme) ? lexeme.ToUpper() : "IDENTIFIER";

            tokens.Add(new Token(tokenType, lexeme, startLine));
        }

        private bool IsKeyword(string word)
        {
            for (int i = 0; i < Keywords.Length; i++)
            {
                if (Keywords[i] == word)
                    return true;
            }
            return false;
        }

        private void ScanNumber()
        {
            int startPos = position;
            int startLine = line;

            while (position < input.Length && char.IsDigit(input[position]))
            {
                position++;
                column++;
            }

            //nr real
            if (position < input.Length && input[position] == '.' && position + 1 < input.Length && char.IsDigit(input[position + 1]))
            {
                position++;
                column++;
                while (position < input.Length && char.IsDigit(input[position]))
                {
                    position++;
                    column++;
                }
            }


            string lexeme = input.Substring(startPos, position - startPos);
            tokens.Add(new Token("NUMBER", lexeme, startLine));
        }

        private void ScanString()
        {
            int startLine = line;
            int startCol = column;
            position++; // Skip primele "
            column++;
            int startPos = position;
            bool isValid = true;

            while (position < input.Length && input[position] != '"')
            {
                if (input[position] == '\\' && position + 1 < input.Length)
                {
                    position += 2;
                    column += 2;
                }
                else if (input[position] == '\n')
                {
          
                    lexicalErrors.Add($"Lexical error at line {line}, column {startCol}: String literal cannot span multiple lines");
                    isValid = false;
                    line++;
                    column = 1;
                    position++;
                }
                else
                {
                    position++;
                    column++;
                }
            }

            if (position >= input.Length)
            {
                // string neinchis
                lexicalErrors.Add($"Lexical error at line {startLine}, column {startCol}: Unterminated string literal");
                isValid = false;
            }
            else if (isValid)
            {
                string lexeme = input.Substring(startPos, position - startPos);
                tokens.Add(new Token("STRING", lexeme, startLine));
                position++;
                column++;
            }
        }

        //operator cu 1 sau 2 caractere
        private void ScanOperator()
        {
            int startLine = line;

            char first = input[position];
            position++;
            column++;

            if (position < input.Length)
            {
                char second = input[position];
                string twoChar = first.ToString() + second;

                if (IsOperator(twoChar))
                {
                    position++;
                    column++;
                    string tokenType = GetOperatorTokenType(twoChar);
                    tokens.Add(new Token(tokenType, twoChar, startLine));
                    return;
                }
            }

            string tokenType1 = GetOperatorTokenType(first.ToString());
            if (tokenType1 != null)
            {
                tokens.Add(new Token(tokenType1, first.ToString(), startLine));
            }
        }

        private void ScanDelimiter()
        {
            int startLine = line;
            char current = input[position];
            string tokenType = GetDelimiterTokenType(current);

            if (tokenType != null)
            {
                tokens.Add(new Token(tokenType, current.ToString(), startLine));
            }

            position++;
            column++;
        }

        private bool IsOperatorChar(char c)
        {
            return "+-*/%<>=!&|".Contains(c.ToString());
        }

        private bool IsDelimiter(char c)
        {
            return "(){}.,;".Contains(c.ToString());
        }

        private bool IsOperator(string op)
        {
            string[] operators = new string[] { "++", "--", "+=", "-=", "*=", "/=", "%=", "==", "!=", "<=", ">=", "&&", "||" };
            for (int i = 0; i < operators.Length; i++)
            {
                if (operators[i] == op)
                    return true;
            }
            return false;
        }

        private string GetOperatorTokenType(string op)
        {
            switch (op)
            {
                case "+": return "PLUS";
                case "-": return "MINUS";
                case "*": return "MUL";
                case "/": return "DIV";
                case "%": return "MOD";
                case "=": return "ASSIGN";
                case "+=": return "PLUS_ASSIGN";
                case "-=": return "MINUS_ASSIGN";
                case "*=": return "MUL_ASSIGN";
                case "/=": return "DIV_ASSIGN";
                case "%=": return "MOD_ASSIGN";
                case "<": return "LT";
                case ">": return "GT";
                case "<=": return "LTE";
                case ">=": return "GTE";
                case "==": return "EQ";
                case "!=": return "NEQ";
                case "&&": return "LOGICAL_AND";
                case "||": return "LOGICAL_OR";
                case "++": return "INCREMENT";
                case "--": return "DECREMENT";
                case "!": return "NOT";
                default: return null;
            }
        }

        private string GetDelimiterTokenType(char c)
        {
            switch (c)
            {
                case '(': return "LPAREN";
                case ')': return "RPAREN";
                case '{': return "LBRACE";
                case '}': return "RBRACE";
                case ',': return "COMMA";
                case ';': return "SEMICOLON";
                case '.': return "DOT";
                default: return null;
            }
        }
    }
}
