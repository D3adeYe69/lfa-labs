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

### Why is the Grammar Type-3?  
A Type-3 grammar (regular grammar) follows strict rules where each production rule is of the form:  
1. `A → aB` (Right-linear) or `A → Ba` (Left-linear)  
2. `A → a` (Terminal-only)  

Examining the given grammar, the production rules mostly conform to right-linear form (`S → aD`, `D → bE`, etc.), which makes it a **Regular Grammar (Type-3)** according to the Chomsky hierarchy.  

### Finite Automaton Implementation  
The `FiniteAutomaton` class consists of:  
- **States**: `{q0, q1, q2, q3}`  
- **Alphabet**: `{a, b}`  
- **Start State**: `q0`  
- **Final State**: `{q3}`  
- **Transition Rules**:  
  - δ(q0, a) = q1  
  - δ(q1, b) = q2  
  - δ(q2, b) = q3, q2  
  - δ(q3, a) = q1  
  - δ(q1, a) = q1  

### Why is the Automaton an NDFA?  
An automaton is **non-deterministic** (NDFA) if:  
- A state has multiple possible transitions for the same input symbol.  
- There exists at least one transition to multiple states.  
  
In this case, the transition δ(q2, b) = {q3, q2} means that on input `b`, the automaton can move to either `q3` or stay in `q2`. This confirms that the given automaton is **non-deterministic**.  

---

## Code Implementation  
```csharp
using System;
using System.Collections.Generic;
using System.Linq;

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

    public string ClassifyGrammar()
    {
        bool isRegular = P.All(rule => rule.Value.All(prod => prod.Length <= 2 && (prod.Length == 1 || char.IsLower(prod[0]))));
        if (isRegular) return "Regular Grammar (Type-3)";
        return "More complex than Regular Grammar";
    }

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
    private HashSet<string> States = new HashSet<string> { "q0", "q1", "q2", "q3" };
    private HashSet<char> Alphabet = new HashSet<char> { 'a', 'b' };
    private string StartState = "q0";
    private HashSet<string> FinalStates = new HashSet<string> { "q3" };
    private Dictionary<(string, char), List<string>> Transitions = new Dictionary<(string, char), List<string>>
    {
        { ("q0", 'a'), new List<string> { "q1" } },
        { ("q1", 'b'), new List<string> { "q2" } },
        { ("q2", 'b'), new List<string> { "q3", "q2" } },
        { ("q3", 'a'), new List<string> { "q1" } },
        { ("q1", 'a'), new List<string> { "q1" } }
    };

    public bool IsDeterministic()
    {
        return Transitions.All(t => t.Value.Count == 1);
    }

    public Dictionary<string, List<string>> ConvertToRegularGrammar()
    {
        var grammar = new Dictionary<string, List<string>>();
        foreach (var transition in Transitions)
        {
            string fromState = transition.Key.Item1;
            char symbol = transition.Key.Item2;
            List<string> toStates = transition.Value;

            if (!grammar.ContainsKey(fromState))
                grammar[fromState] = new List<string>();

            foreach (var toState in toStates)
            {
                grammar[fromState].Add(symbol + toState);
            }
        }
        return grammar;
    }

    public Dictionary<(string, char), string> ConvertNDFAtoDFA()
    {
        var dfaTransitions = new Dictionary<(string, char), string>();
        var newStates = new Queue<HashSet<string>>();
        var processedStates = new HashSet<string>();
        var initialState = new HashSet<string> { StartState };
        newStates.Enqueue(initialState);
        while (newStates.Count > 0)
        {
            var currentSet = newStates.Dequeue();
            string stateName = string.Join("", currentSet.OrderBy(s => s));
            if (processedStates.Contains(stateName)) continue;
            processedStates.Add(stateName);
            foreach (char symbol in Alphabet)
            {
                var newStateSet = new HashSet<string>();
                foreach (var state in currentSet)
                {
                    if (Transitions.TryGetValue((state, symbol), out var nextStates))
                    {
                        newStateSet.UnionWith(nextStates);
                    }
                }
                if (newStateSet.Count > 0)
                {
                    string newStateName = string.Join("", newStateSet.OrderBy(s => s));
                    dfaTransitions[(stateName, symbol)] = newStateName;
                    if (!processedStates.Contains(newStateName))
                    {
                        newStates.Enqueue(newStateSet);
                    }
                }
            }
        }
        return dfaTransitions;
    }
}

public class Program
{
    public static void Main()
    {
        Grammar grammar = new Grammar();
        Console.WriteLine("Grammar Classification: " + grammar.ClassifyGrammar());

        Console.WriteLine("Generated strings:");
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine(grammar.GenerateString());
        }

        FiniteAutomaton fa = new FiniteAutomaton();
        Console.WriteLine("The FA is " + (fa.IsDeterministic() ? "Deterministic" : "Non-Deterministic"));

        Console.WriteLine("Regular Grammar Representation:");
        var regularGrammar = fa.ConvertToRegularGrammar();
        foreach (var rule in regularGrammar)
        {
            Console.WriteLine(rule.Key + " -> " + string.Join(" | ", rule.Value));
        }

        Console.WriteLine("DFA Representation (Converted from NDFA):");
        var dfa = fa.ConvertNDFAtoDFA();
        foreach (var transition in dfa)
        {
            Console.WriteLine($"({transition.Key.Item1}, {transition.Key.Item2}) -> {transition.Value}");
        }


    }
}

```
![Console results](/Images/Console2.png)
---

## Conclusion  
This project demonstrates how formal languages and finite automata work together to define structured word formation and validation. By implementing a grammar-based generator and a basic automaton, we can see how theoretical concepts translate into actual computation.
