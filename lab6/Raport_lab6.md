
# Parsing and Abstract Syntax Trees

## Course: Formal Languages & Finite Automata  
## Author: Chirtoaca Liviu

---

## Theory

Parsing is a fundamental step in language processing, where a sequence of tokens is analyzed according to the grammatical rules of a language. The main goal of parsing is to uncover the syntactic structure of a given input, which is often represented as an Abstract Syntax Tree (AST).

### Key Concepts

- **Lexer**: Converts raw input into a list of tokens, each categorized by a type (e.g., identifier, keyword, symbol).
- **Parser**: Takes these tokens and constructs a syntax tree that reflects the nested and hierarchical nature of the language.
- **AST (Abstract Syntax Tree)**: A tree structure representing the syntactic structure of the code, omitting unnecessary syntactic details (like parentheses or semicolons) and focusing on meaningful structure (e.g., statements, expressions, blocks).

Parsing is essential for interpreting or compiling code, as it translates linear text into structured data the computer can work with more easily.

---

## Implementation Description

### Token Type Definition

The `TokenType` enum categorizes tokens for lexical analysis. It is used to identify keywords, identifiers, literals, operators, and symbols using regular expressions during tokenization.

```csharp
public enum TokenType
{
    Identifier,
    Number,
    Keyword,
    Symbol,
    Operator,
    EOF
}
```

Regular expressions are used in the lexer (from `SimpleLexer`) to assign types to each token in the input text.

---

### AST Node Structure

The AST is modeled using an object-oriented hierarchy. Each syntactic construct in the language corresponds to a specific class:

#### Program Structure

```csharp
public class ProgramNode : AstNode
{
    public List<Statement> Statements { get; }
}
```

Represents the root of the syntax tree containing all top-level statements.

#### Statement Variants

- **BlockStatement**: A sequence of nested statements.
- **VariableDeclaration**: Declaration of a variable with an optional initializer.
- **IfStatement**: Conditional execution with optional else branch.
- **WhileStatement**: Loop construct.
- **ReturnStatement**: Return from a function.
- **ExpressionStatement**: Expression treated as a standalone statement.

#### Expressions

- **LiteralExpression**: Represents literal values like numbers or strings.
- **IdentifierExpression**: Variable or function identifiers.
- **BinaryExpression**: Arithmetic or logical binary operations.
- **AssignmentExpression**: Assignment of values to variables.

---

### Parser Implementation

The parser processes a list of tokens and constructs the AST. It uses recursive descent parsing techniques with helper methods to match and consume expected tokens:

```csharp
private Token Peek(int offset = 0) => ...;
private Token Consume(TokenType type, string expectedMessage) => ...;
```

Each syntactic category (statement, expression, etc.) is parsed by a dedicated method. For example:

```csharp
private Statement ParseStatement()
{
    if (Match(TokenType.Keyword, "if"))
        return ParseIfStatement();
    if (Match(TokenType.Keyword, "while"))
        return ParseWhileStatement();
    if (Match(TokenType.Keyword, "return"))
        return ParseReturnStatement();
    return ParseExpressionStatement();
}
```

This modular design makes the parser extensible and easier to maintain.

---

## Results and Analysis

Given an input like:

```text
var x = 10;
if (x > 5) {
    return x;
}
```

The parser produces an AST with the following structure:

- ProgramNode
- VariableDeclaration ("x", LiteralExpression(10))
- IfStatement
- Condition: BinaryExpression(Identifier("x"), ">", Literal(5))
- ThenBranch: BlockStatement
- ReturnStatement(Identifier("x"))

Each node in the AST directly maps to a syntactic feature in the input source code, making it suitable for interpretation, optimization, or code generation.

---

## Conclusions

Building a parser and an AST highlights the crucial relationship between syntax and structure in programming languages. The abstraction provided by the AST simplifies further language processing tasks and creates a strong foundation for interpreters or compilers.

This lab provided practical experience with:
- Token classification using regular expressions
- Recursive descent parsing techniques
- Hierarchical AST representation
- Mapping flat token streams to structured syntax trees

