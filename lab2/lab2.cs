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
