using System;
using System.Collections.Generic;

public class Grammar
{
    private HashSet<char> VN = new HashSet<char> { 'S', 'D', 'E', 'F', 'L' };
    private HashSet<char> VT = new HashSet<char> { 'a', 'b', 'c', 'd' };
    private Dictionary<char, List<string>> P = new Dictionary<char, List<string>>
    {
        { 'S', new List<string> { "aD" } },
        { 'D', new List<string> { "bE" } },
        { 'E', new List<string> { "cF", "dL" } },
        { 'F', new List<string> { "dD" } },
        { 'L', new List<string> { "aL", "bL", "c" } }
    };
    private char S = 'S';

    public string GenerateString()
    {
        Random rand = new Random();
        string result = "S";
        while (result.IndexOfAny(VN.ToArray()) != -1)
        {
            foreach (char nonTerminal in VN)
            {
                if (result.Contains(nonTerminal))
                {
                    string replacement = P[nonTerminal][rand.Next(P[nonTerminal].Count)];
                    int index = result.IndexOf(nonTerminal);
                    if (index != -1)
                    {
                        result = result.Substring(0, index) + replacement + result.Substring(index + 1);
                    }
                }
            }
        }
        return result;
    }
}

public class FiniteAutomaton
{
    public bool StringBelongsToLanguage(string input)
    {
        if (!input.StartsWith("a"))
        {
            Console.WriteLine($"'{input}' is NOT a valid word because it does not start with 'a'.");
            return false;
        }

        if (!input.EndsWith("c"))
        {
            Console.WriteLine($"'{input}' is NOT a valid word because it does not end with 'c'.");
            return false;
        }

        string currentState = "S";
        foreach (char symbol in input)
        {
            switch (currentState)
            {
                case "S":
                    if (symbol == 'a') currentState = "D";
                    else return false;
                    break;
                case "D":
                    if (symbol == 'b') currentState = "E";
                    else return false;
                    break;
                case "E":
                    if (symbol == 'c') currentState = "F";
                    else if (symbol == 'd') currentState = "L";
                    else return false;
                    break;
                case "F":
                    if (symbol == 'd') currentState = "D";
                    else return false;
                    break;
                case "L":
                    if (symbol == 'a' || symbol == 'b') currentState = "L";
                    else if (symbol == 'c') return true;
                    else return false;
                    break;
                default:
                    return false;
            }
        }
        return currentState == "L";
    }
}

public class Program
{
    public static void Main()
    {
        Grammar grammar = new Grammar();
        Console.WriteLine("Generated strings:");
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine(grammar.GenerateString());
        }

        FiniteAutomaton fa = new FiniteAutomaton();
        while (true)
        {
            Console.WriteLine("\nEnter a string to check if it's valid ('exit' to quit):");
            string userInput = Console.ReadLine();

            if (userInput.ToLower() == "exit")
                break;

            if (fa.StringBelongsToLanguage(userInput))
            {
                Console.WriteLine($"'{userInput}' is a valid word.");
            }
            else
            {
                Console.WriteLine($"'{userInput}' is NOT a valid word.");
            }
        }
    }
}
