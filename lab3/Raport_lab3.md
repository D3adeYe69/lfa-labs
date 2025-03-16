# Lexer & Scanner

## Course: Formal Languages & Finite Automata  
## Author: Chirtoaca Liviu  

## Theory  
Lexical analysis is the process of converting a sequence of characters into a sequence of tokens. Each token represents a logical element in the source code, such as identifiers, keywords, operators, and literals.
The lexer reads the input character by character, identifies patterns, and groups them into tokens. Each token has:

A type (e.g., IDENTIFIER, NUMBER, KEYWORD)
Usually a value (the actual lexeme, like "x" or "42")
Sometimes position information for error reporting

---

## Objectives  
- Understand what lexical analysis [1] is.
- Get familiar with the inner workings of a lexer/scanner/tokenizer.
- Implement a sample lexer and show how it works.

---

## Implementation Description  


### Core Components
- **Token Class**: Stores token type, value, and position (line, column)
- **TokenType Enum**: Defines categories like KEYWORD, IDENTIFIER, INTEGER, etc.
- **Lexer Class**: Main component that processes source code into tokens

## Key Methods
- **Advance()**: Moves to next character and updates position tracking
- **Peek()**: Looks ahead without advancing position
- **GetNextToken()**: Main processing function that identifies next token
- **Tokenize()**: Processes entire source code into a list of tokens

## Token Recognition Process
- Whitespace characters are skipped
- Comments (starting with //) are identified and skipped
- Keywords are recognized by comparing to predefined set
- Identifiers are sequences of letters, digits, and underscores
- Numbers are recognized as integers or floats depending on decimal point
- Strings are recognized as characters between double quotes
- Operators include both single-character (+, -, *) and multi-character (==, !=, >=)
- Delimiters include brackets, semicolons, commas, etc.

## Special Features
- Line and column tracking for error reporting
- Support for multi-character operators (==, !=, <=, >=)
- Escape sequence handling in strings (\n, \t, etc.)
- Comment skipping
- Error reporting for invalid characters and unclosed strings

## Processing Workflow
1. Initialize with source code
2. Process characters one at a time
3. Identify patterns that form tokens
4. Return tokens with their types and values
5. Continue until end of source code is reached

## Demo Program Features
- Tokenizes a sample program with variable declarations, control structures, etc.
- Displays all identified tokens with their types and positions
- Provides statistics on token distribution by type

## Limitations
- Only performs lexical analysis, not syntax validation
- Doesn't build a parse tree or evaluate expressions
- No semantic analysis (type checking, scope validation, etc.)
- No execution capabilities

---

## Code implementation
```csharp
public enum TokenType
{
    IDENTIFIER,   // Variable/function names
    KEYWORD,      // Reserved words like 'if'
    INTEGER,      // Whole numbers
    FLOAT,        // Decimal numbers
    STRING,       // Text in quotes
    OPERATOR,     // +, -, *, /, =, etc.
    DELIMITER,    // (, ), {, }, ;, etc.
    EOF           // End of file marker
}

```
This enum defines all possible token categories the lexer can recognize.


```csharp
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
```
The Advance method:

- Moves to the next character
- Updates position tracking
- Handles line breaks
- Sets _currentChar to null when reaching the end

```csharp
private void SkipWhitespace()
{
    while (_currentChar != null && char.IsWhiteSpace(_currentChar.Value))
    {
        Advance();
    }
}

private void SkipComment()
{

    while (_currentChar != null && _currentChar != '\n')
    {
        Advance();
    }
}
```
Skips all whitespace characters (spaces, tabs, newlines) also everything until the end of line when a comment is found.

```csharp
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
```
The Identifier method:

- Reads a sequence of letters, digits, and underscores
- Determines if it's a keyword or identifier
- Returns the appropriate token

```csharp
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
```
The GetNextToken method is the core of the lexer:

- It skips whitespace and comments
- Based on the current character, it calls the appropriate method to process each token type
- It handles delimiters directly
- It reports errors for invalid characters
- When the end of the source code is reached, it returns an EOF token


## Conclusion  
# Conclusion

Our C# lexer implementation successfully converts source text into a stream of typed tokens—the lexical building blocks like identifiers, operators, and literals that form the foundation of language processing. The character-by-character scanning approach, combined with state tracking and lookahead capabilities, efficiently handles complex patterns such as multi-character operators and string escape sequences while maintaining precise line and column information. While this lexer doesn't tackle parsing or AST construction, it provides a clean API through the `Tokenize()` method that makes integration with downstream components straightforward. The code strikes a nice balance between readability and performance—using StringBuilder for token construction and employing nullable types for cleaner boundary handling—making it both a practical learning tool and a solid starting point for more ambitious language projects.