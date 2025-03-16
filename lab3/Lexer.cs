using System.Text;

namespace SimpleLexer
{
    public enum TokenType
    {
        IDENTIFIER,
        KEYWORD,
        INTEGER,
        FLOAT,
        STRING,
        OPERATOR,
        DELIMITER,
        EOF
    }

    public class Token
    {
        public TokenType Type { get; }
        public object Value { get; }
        public int Line { get; }
        public int Column { get; }

        public Token(TokenType type, object value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"Token({Type}, '{Value}', line:{Line}, col:{Column})";
        }
    }

    public class Lexer
    {
        private readonly string _source;
        private int _position;
        private int _line;
        private int _column;
        private char? _currentChar;

        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "if", "else", "while", "for", "function",
            "return", "var", "true", "false", "null"
        };

        public Lexer(string sourceCode)
        {
            _source = sourceCode;
            _position = 0;
            _line = 1;
            _column = 1;
            _currentChar = _source.Length > 0 ? _source[0] : (char?)null;
        }

        private void Error()
        {
            throw new Exception($"Invalid character '{_currentChar}' at line {_line}, column {_column}");
        }

        private void Advance()
        {
            _position++;
            _column++;

            if (_position >= _source.Length)
            {
                _currentChar = null;
            }
            else
            {
                if (_currentChar == '\n')
                {
                    _line++;
                    _column = 1;
                }
                _currentChar = _source[_position];
            }
        }

        private char? Peek()
        {
            int peekPos = _position + 1;
            if (peekPos >= _source.Length)
            {
                return null;
            }
            return _source[peekPos];
        }

        private void SkipWhitespace()
        {
            while (_currentChar != null && char.IsWhiteSpace(_currentChar.Value))
            {
                Advance();
            }
        }

        private void SkipComment()
        {
            // Skip single-line comments starting with //
            while (_currentChar != null && _currentChar != '\n')
            {
                Advance();
            }
        }

        private Token Identifier()
        {
            int startCol = _column;
            StringBuilder result = new StringBuilder();

            while (_currentChar != null && (char.IsLetterOrDigit(_currentChar.Value) || _currentChar == '_'))
            {
                result.Append(_currentChar);
                Advance();
            }

            string value = result.ToString();
            TokenType tokenType = Keywords.Contains(value) ? TokenType.KEYWORD : TokenType.IDENTIFIER;
            return new Token(tokenType, value, _line, startCol);
        }

        private Token Number()
        {
            int startCol = _column;
            StringBuilder result = new StringBuilder();
            bool isFloat = false;

            while (_currentChar != null && (char.IsDigit(_currentChar.Value) || _currentChar == '.'))
            {
                if (_currentChar == '.')
                {
                    if (isFloat)  // If we've already seen a decimal point
                    {
                        break;
                    }
                    isFloat = true;
                }
                result.Append(_currentChar);
                Advance();
            }

            string resultStr = result.ToString();
            TokenType tokenType = isFloat ? TokenType.FLOAT : TokenType.INTEGER;
            object value = isFloat ? float.Parse(resultStr) : int.Parse(resultStr);
            return new Token(tokenType, value, _line, startCol);
        }

        private Token String()
        {
            int startCol = _column;
            StringBuilder result = new StringBuilder();
            Advance();  // Skip opening quote

            while (_currentChar != null && _currentChar != '"')
            {
                // Handle escape sequences
                if (_currentChar == '\\' && Peek() != null)
                {
                    Advance();  // Skip backslash
                    switch (_currentChar)
                    {
                        case 'n': result.Append('\n'); break;
                        case 't': result.Append('\t'); break;
                        case 'r': result.Append('\r'); break;
                        case '\\': result.Append('\\'); break;
                        case '"': result.Append('"'); break;
                        default: result.Append(_currentChar); break;
                    }
                }
                else
                {
                    result.Append(_currentChar);
                }
                Advance();
            }

            if (_currentChar == null)
            {
                throw new Exception($"Unclosed string at line {_line}, column {startCol}");
            }

            Advance();  // Skip closing quote
            return new Token(TokenType.STRING, result.ToString(), _line, startCol);
        }

        private Token Operator()
        {
            int startCol = _column;
            string op = _currentChar.ToString();

            // Check for double-character operators like ==, !=, <=, >=
            if (_currentChar is char c && Peek() is char nextChar)
            {
                string possibleOp = new string(new[] { c, nextChar });
                if (possibleOp == "==" || possibleOp == "!=" ||
                    possibleOp == "<=" || possibleOp == ">=" ||
                    possibleOp == "&&" || possibleOp == "||")
                {
                    Advance();  // Move to the second character
                    op = possibleOp;
                }
            }

            Advance();  // Move past the operator
            return new Token(TokenType.OPERATOR, op, _line, startCol);
        }

        public Token GetNextToken()
        {
            while (_currentChar != null)
            {
                // Skip whitespace
                if (char.IsWhiteSpace(_currentChar.Value))
                {
                    SkipWhitespace();
                    continue;
                }

                // Skip comments
                if (_currentChar == '/' && Peek() == '/')
                {
                    Advance();  // Skip first '/'
                    Advance();  // Skip second '/'
                    SkipComment();
                    continue;
                }

                // Identifiers or keywords
                if (char.IsLetter(_currentChar.Value) || _currentChar == '_')
                {
                    return Identifier();
                }

                // Numbers
                if (char.IsDigit(_currentChar.Value))
                {
                    return Number();
                }

                // Strings
                if (_currentChar == '"')
                {
                    return String();
                }

                // Operators
                if (_currentChar == '+' || _currentChar == '-' ||
                    _currentChar == '*' || _currentChar == '/' ||
                    _currentChar == '=' || _currentChar == '<' ||
                    _currentChar == '>' || _currentChar == '!' ||
                    _currentChar == '&' || _currentChar == '|')
                {
                    return Operator();
                }

                // Delimiters
                if (_currentChar == '(' || _currentChar == ')' ||
                    _currentChar == '{' || _currentChar == '}' ||
                    _currentChar == '[' || _currentChar == ']' ||
                    _currentChar == ';' || _currentChar == ',' ||
                    _currentChar == ':')
                {
                    char delimiter = _currentChar.Value;
                    int colPos = _column;
                    Advance();
                    return new Token(TokenType.DELIMITER, delimiter, _line, colPos);
                }

                Error();
            }

            return new Token(TokenType.EOF, null, _line, _column);
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();
            Token token;

            do
            {
                token = GetNextToken();
                tokens.Add(token);
            } while (token.Type != TokenType.EOF);

            return tokens;
        }
    }
}