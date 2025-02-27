# RegularGrammars

## Course: Formal Languages & Finite Automata
## Author: Chirtoaca Liviu

## Theory

A formal language is a structured way of defining how strings are formed and manipulated within a given set of rules. It consists of an alphabet (valid symbols), a vocabulary (valid words), and a grammar (rules for forming words). Finite automata are abstract machines that help determine whether a string belongs to a given language by transitioning through defined states.

The purpose of this project is to define a simple grammar, generate valid strings, and implement a finite automaton to verify membership within the language.

---

## Objectives

- Understand the concept of formal languages and finite automata.
- Implement a grammar-based generator to create valid words.
- Convert the grammar into a finite automaton that can check string membership.
- Provide a simple C# implementation demonstrating these concepts.

---

## Implementation Description

### Grammar Implementation
The grammar is defined using:
- **Non-terminals (VN)**: `{S, D, E, F, L}` – These are symbols that can be replaced.
- **Terminals (VT)**: `{a, b, c, d}` – These are symbols that cannot be replaced.
- **Production Rules (P)**: Define how symbols transition.
- **Start Symbol (S)**: The entry point of the language.

The `Grammar` class includes:
- A dictionary storing production rules.
- A method `GenerateString()` that starts with `S` and repeatedly replaces non-terminals using predefined rules until only terminals remain.

### Finite Automaton Implementation
The `FiniteAutomaton` class is a simplified model that determines whether a given string is valid by checking if it contains the terminal symbol `c` (as per the grammar rules).

### Main Program
- Generating five valid strings from the grammar.
- Checking if a given string belongs to the language using the finite automaton.

---

## Code Implementation

```csharp
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
            Console.WriteLine("\nEnter a string to check if it's valid (or type 'exit' to quit):");
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

![Screenshot of the code](/Images/Console1.png)
 
---

## Conclusion

This project demonstrates how formal languages and finite automata work together to define structured word formation and validation. By implementing a grammar-based generator and a basic automaton, we can see how theoretical concepts translate into actual computation.

