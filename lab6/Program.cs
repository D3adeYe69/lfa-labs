using System;
using System.Collections.Generic;
using SimpleLexer;

namespace SimpleParser
{
    // === AST NODES ===
    public abstract class AstNode { }

    public abstract class Statement : AstNode { }

    public class ProgramNode : AstNode
    {
        public List<Statement> Statements { get; }
        public ProgramNode(List<Statement> statements) => Statements = statements;
    }

    public class BlockStatement : Statement
    {
        public List<Statement> Statements { get; }
        public BlockStatement(List<Statement> statements) => Statements = statements;
    }

    public class VariableDeclaration : Statement
    {
        public string Name { get; }
        public Expression Initializer { get; }
        public VariableDeclaration(string name, Expression init)
        {
            Name = name;
            Initializer = init;
        }
    }

    public class IfStatement : Statement
    {
        public Expression Condition { get; }
        public Statement ThenBranch { get; }
        public Statement ElseBranch { get; }
        public IfStatement(Expression cond, Statement thenB, Statement elseB)
        {
            Condition = cond;
            ThenBranch = thenB;
            ElseBranch = elseB;
        }
    }

    public class WhileStatement : Statement
    {
        public Expression Condition { get; }
        public Statement Body { get; }
        public WhileStatement(Expression cond, Statement body)
        {
            Condition = cond;
            Body = body;
        }
    }

    public class ReturnStatement : Statement
    {
        public Expression Value { get; }
        public ReturnStatement(Expression value) => Value = value;
    }

    public class ExpressionStatement : Statement
    {
        public Expression Expr { get; }
        public ExpressionStatement(Expression expr) => Expr = expr;
    }

    // === Expression AST ===
    public abstract class Expression : AstNode { }

    public class AssignmentExpression : Expression
    {
        public string Target { get; }
        public Expression Value { get; }
        public AssignmentExpression(string target, Expression value)
        {
            Target = target;
            Value = value;
        }
    }

    public class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public string Operator { get; }
        public Expression Right { get; }
        public BinaryExpression(Expression left, string op, Expression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }

    public class LiteralExpression : Expression
    {
        public object Value { get; }
        public LiteralExpression(object value) => Value = value;
    }

    public class IdentifierExpression : Expression
    {
        public string Name { get; }
        public IdentifierExpression(string name) => Name = name;
    }

    // === Parser with assignment support ===
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Token Peek(int offset = 0) =>
            (_pos + offset < _tokens.Count) ? _tokens[_pos + offset] : _tokens[^1];

        private bool Match(TokenType type, string value = null)
        {
            if (Peek().Type == type && (value == null || Peek().Value?.ToString() == value))
            {
                _pos++;
                return true;
            }
            return false;
        }

        private Token Consume(TokenType type, string expectDesc)
        {
            var tok = Peek();
            if (tok.Type != type)
                throw new Exception($"Expected {expectDesc}, got {tok.Type} at line {tok.Line}");
            _pos++;
            return tok;
        }

        public ProgramNode ParseProgram()
        {
            var stmts = new List<Statement>();
            while (!Match(TokenType.EOF))
                stmts.Add(ParseStatement());
            return new ProgramNode(stmts);
        }

        private Statement ParseStatement()
        {
            // var declaration
            if (Match(TokenType.KEYWORD, "var"))
            {
                var name = Consume(TokenType.IDENTIFIER, "identifier").Value.ToString();
                Consume(TokenType.OPERATOR, "=");
                var init = ParseExpression();
                Consume(TokenType.DELIMITER, ";");
                return new VariableDeclaration(name, init);
            }

            // if/else
            if (Match(TokenType.KEYWORD, "if"))
            {
                Consume(TokenType.DELIMITER, "(");
                var cond = ParseExpression();
                Consume(TokenType.DELIMITER, ")");
                var thenB = ParseStatement();
                Statement elseB = null;
                if (Match(TokenType.KEYWORD, "else"))
                    elseB = ParseStatement();
                return new IfStatement(cond, thenB, elseB);
            }

            // while
            if (Match(TokenType.KEYWORD, "while"))
            {
                Consume(TokenType.DELIMITER, "(");
                var cond = ParseExpression();
                Consume(TokenType.DELIMITER, ")");
                var body = ParseStatement();
                return new WhileStatement(cond, body);
            }

            // return
            if (Match(TokenType.KEYWORD, "return"))
            {
                var val = ParseExpression();
                Consume(TokenType.DELIMITER, ";");
                return new ReturnStatement(val);
            }

            // block
            if (Match(TokenType.DELIMITER, "{"))
            {
                var list = new List<Statement>();
                while (!Match(TokenType.DELIMITER, "}"))
                    list.Add(ParseStatement());
                return new BlockStatement(list);
            }

            // expression or assignment
            var expr = ParseExpression();
            Consume(TokenType.DELIMITER, ";");
            return new ExpressionStatement(expr);
        }

        private Expression ParseExpression() => ParseAssignment();

        private Expression ParseAssignment()
        {
            var expr = ParseEquality();
            if (Match(TokenType.OPERATOR, "="))
            {
                var right = ParseAssignment();
                if (expr is IdentifierExpression id)
                    return new AssignmentExpression(id.Name, right);
                throw new Exception("Invalid assignment target.");
            }
            return expr;
        }

        private Expression ParseEquality()
        {
            var expr = ParseComparison();
            while (Match(TokenType.OPERATOR, "==") || Match(TokenType.OPERATOR, "!="))
            {
                var op = _tokens[_pos - 1].Value.ToString();
                var right = ParseComparison();
                expr = new BinaryExpression(expr, op, right);
            }
            return expr;
        }

        private Expression ParseComparison()
        {
            var expr = ParseTerm();
            while (Match(TokenType.OPERATOR, "<") || Match(TokenType.OPERATOR, ">") ||
                   Match(TokenType.OPERATOR, "<=") || Match(TokenType.OPERATOR, ">="))
            {
                var op = _tokens[_pos - 1].Value.ToString();
                var right = ParseTerm();
                expr = new BinaryExpression(expr, op, right);
            }
            return expr;
        }

        private Expression ParseTerm()
        {
            var expr = ParseFactor();
            while (Match(TokenType.OPERATOR, "+") || Match(TokenType.OPERATOR, "-"))
            {
                var op = _tokens[_pos - 1].Value.ToString();
                var right = ParseFactor();
                expr = new BinaryExpression(expr, op, right);
            }
            return expr;
        }

        private Expression ParseFactor()
        {
            var expr = ParseUnary();
            while (Match(TokenType.OPERATOR, "*") || Match(TokenType.OPERATOR, "/"))
            {
                var op = _tokens[_pos - 1].Value.ToString();
                var right = ParseUnary();
                expr = new BinaryExpression(expr, op, right);
            }
            return expr;
        }

        private Expression ParseUnary()
        {
            if (Match(TokenType.OPERATOR, "!") || Match(TokenType.OPERATOR, "-"))
            {
                var op = _tokens[_pos - 1].Value.ToString();
                var right = ParseUnary();
                return new BinaryExpression(null, op, right);
            }
            return ParsePrimary();
        }

        private Expression ParsePrimary()
        {
            var tok = Peek();
            if (Match(TokenType.INTEGER) || Match(TokenType.FLOAT) || Match(TokenType.STRING))
                return new LiteralExpression(_tokens[_pos - 1].Value);
            if (Match(TokenType.IDENTIFIER))
                return new IdentifierExpression(_tokens[_pos - 1].Value.ToString());
            if (Match(TokenType.DELIMITER, "("))
            {
                var e = ParseExpression();
                Consume(TokenType.DELIMITER, ")");
                return e;
            }
            throw new Exception($"Unexpected token {tok.Type} at line {tok.Line}");
        }
    }

    // ——— AST Printer ———
    public static class AstPrinter
    {
        public static string Print(AstNode node)
        {
            switch (node)
            {
                case ProgramNode prog:
                    return "Program(" + string.Join(", ", prog.Statements.Select(Print)) + ")";
                case BlockStatement blk:
                    return "Block[" + string.Join(", ", blk.Statements.Select(Print)) + "]";
                case VariableDeclaration vd:
                    return $"VarDecl({vd.Name}, {Print(vd.Initializer)})";
                case IfStatement iff:
                    var elsePart = iff.ElseBranch != null ? Print(iff.ElseBranch) : "null";
                    return $"If({Print(iff.Condition)}, {Print(iff.ThenBranch)}, {elsePart})";
                case WhileStatement w:
                    return $"While({Print(w.Condition)}, {Print(w.Body)})";
                case ReturnStatement r:
                    return $"Return({Print(r.Value)})";
                case ExpressionStatement es:
                    return Print(es.Expr);
                case AssignmentExpression ae:
                    return $"Assign({ae.Target}, {Print(ae.Value)})";
                case BinaryExpression be:
                    var left = be.Left != null ? Print(be.Left) : "";
                    return $"Binary({left}, '{be.Operator}', {Print(be.Right)})";
                case LiteralExpression lit:
                    return $"Literal({lit.Value})";
                case IdentifierExpression id:
                    return $"Ident({id.Name})";
                default:
                    return node.GetType().Name;
            }
        }
    }

    // Demo
    class Program
    {
        static void Main()
        {
            var code = @"
var x = 10;
if (x > 5) { x = x + 1; } else { x = x - 1; }
return x;
";
            // 1) Lex
            var lex = new SimpleLexer.Lexer(code);
            var tokens = lex.Tokenize();
            Console.WriteLine("Tokens: [" + string.Join(", ", tokens) + "]");

            // 2) Parse
            var parser = new Parser(tokens);
            var ast = parser.ParseProgram();

            // 3) Print AST
            Console.WriteLine("AST: " + AstPrinter.Print(ast));
        }
    }
}
