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


## Components and Methods
The lexer is built on fundamental classes and methods. It includes a Token Class to store each token’s type, value, and position, along with a TokenType Enum that categorizes tokens (such as KEYWORD, IDENTIFIER, INTEGER, etc.). The Lexer Class processes the source code using key methods like Advance()—which moves to the next character and updates position tracking—and Peek(), which looks ahead without advancing. The GetNextToken() method identifies the next token based on context, while Tokenize() loops through the source code until all tokens are formed. Additional features include precise line and column tracking for error reporting, handling multi-character operators through lookahead, and processing escape sequences in strings.

## Token Recognition and Workflow
During tokenization, the lexer skips over whitespace and comments, distinguishes keywords from identifiers, and parses numbers as integers or floats. It also processes strings (handling escape sequences) and identifies both single-character and multi-character operators, as well as delimiters like brackets and semicolons. The overall workflow starts with initializing the lexer with the source code, then analyzing the input character by character to form tokens, each annotated with type, value, and positional data, until the end of the source is reached.

## Demo Program and Limitations
A demo program might showcase this lexer by processing a sample program with variable declarations and control structures, displaying the tokens with their types and positions, and providing basic statistics on token distribution. However, the lexer is limited to lexical analysis only; it does not validate syntax, build a parse tree, perform semantic analysis like type checking or scope validation, nor does it execute code.

---

## Code implementation
```csharp
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
        
        if (char.IsWhiteSpace(_currentChar.Value))
        {
            SkipWhitespace();
            continue;
        }

        if (_currentChar == '/' && Peek() == '/')
        {
            Advance();  
            Advance();  
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

---
## Results
![Consola](/Images/token.png)
![Consola2](/Images/summary.png)
---  
# Conclusion

This C# lexer implementation successfully converts source text into a stream of typed tokens—the lexical building blocks like identifiers, operators, and literals that form the foundation of language processing. The character-by-character scanning approach, combined with state tracking and lookahead capabilities, efficiently handles complex patterns such as multi-character operators and string escape sequences while maintaining precise line and column information. While this lexer doesn't tackle parsing or AST construction, it provides a clean API through the `Tokenize()` method that makes integration with downstream components straightforward. The code strikes a nice balance between readability and performance—using StringBuilder for token construction and employing nullable types for cleaner boundary handling—making it both a practical learning tool and a solid starting point for more ambitious language projects.
