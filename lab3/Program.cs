using System;
using System.Collections.Generic;
using SimpleLexer;

namespace LexerDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Sample code to tokenize
            string sampleCode = @"
// This is a sample program
var x = 10;
var y = 20.5;
var name = ""John"";

if (x > 5) {
    while (x > 0) { 
        x = x - 1;
    }
    return x + y;
} else {
    return ""Hello "" + name;
}
";

            Console.WriteLine("Sample Code:");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine(sampleCode);
            Console.WriteLine("-----------------------------------\n");

            // Create our lexer and tokenize the sample code
            Lexer lexer = new Lexer(sampleCode);
            List<Token> tokens = lexer.Tokenize();

            // Print all tokens
            Console.WriteLine("Tokens:");
            Console.WriteLine("-----------------------------------");
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
            Console.WriteLine("-----------------------------------\n");

            // Group tokens by type for statistics
            Dictionary<TokenType, int> tokenCounts = new Dictionary<TokenType, int>();
            foreach (var token in tokens)
            {
                if (tokenCounts.ContainsKey(token.Type))
                {
                    tokenCounts[token.Type]++;
                }
                else
                {
                    tokenCounts[token.Type] = 1;
                }
            }

            // Print token statistics
            Console.WriteLine("Token Statistics:");
            Console.WriteLine("-----------------------------------");
            foreach (var kvp in tokenCounts)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value} tokens");
            }
            Console.WriteLine("-----------------------------------");

            Console.ReadLine();
        }
    }
}