using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarConverter
{
    public class Rule
    {
        public string LeftSide { get; set; }
        public List<string> RightSide { get; set; }

        public Rule(string leftSide, List<string> rightSide)
        {
            LeftSide = leftSide;
            RightSide = rightSide;
        }

        public override string ToString()
        {
            return $"{LeftSide} → {string.Join("", RightSide)}";
        }

        public Rule Clone()
        {
            return new Rule(LeftSide, new List<string>(RightSide));
        }
    }

    public class Grammar
    {
        public HashSet<string> NonTerminals { get; set; }
        public HashSet<string> Terminals { get; set; }
        public List<Rule> Rules { get; set; }
        public string StartSymbol { get; set; }

        public Grammar(HashSet<string> nonTerminals, HashSet<string> terminals, List<Rule> rules, string startSymbol)
        {
            NonTerminals = nonTerminals;
            Terminals = terminals;
            Rules = rules;
            StartSymbol = startSymbol;
        }

        public Grammar Clone()
        {
            return new Grammar(
                new HashSet<string>(NonTerminals),
                new HashSet<string>(Terminals),
                Rules.Select(r => r.Clone()).ToList(),
                StartSymbol
            );
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("G = (VN, VT, P, S)");
            sb.AppendLine($"VN = {{{string.Join(", ", NonTerminals)}}}");
            sb.AppendLine($"VT = {{{string.Join(", ", Terminals)}}}");
            sb.AppendLine($"S = {StartSymbol}");
            sb.AppendLine("P = {");

            for (int i = 0; i < Rules.Count; i++)
            {
                sb.AppendLine($"    {i + 1}. {Rules[i]}");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }

    public class ChomskyCnfConverter
    {
        private Grammar _grammar;
        private Dictionary<string, HashSet<string>> _nullable;
        private int _newSymbolCounter = 0;

        public ChomskyCnfConverter(Grammar grammar)
        {
            _grammar = grammar.Clone();
            _nullable = new Dictionary<string, HashSet<string>>();
        }

        public Grammar ConvertToCnf()
        {
            Console.WriteLine("Original Grammar:");
            Console.WriteLine(_grammar);

            EliminateEpsilonProductions();
            Console.WriteLine("Grammar after eliminating ε-productions:");
            Console.WriteLine(_grammar);

            EliminateUnitProductions();
            Console.WriteLine("Grammar after eliminating unit productions:");
            Console.WriteLine(_grammar);

            EliminateInaccessibleSymbols();
            Console.WriteLine("Grammar after eliminating inaccessible symbols:");
            Console.WriteLine(_grammar);

            EliminateNonProductiveSymbols();
            Console.WriteLine("Grammar after eliminating non-productive symbols:");
            Console.WriteLine(_grammar);

            ConvertToBinarizedForm();
            Console.WriteLine("Grammar in Chomsky Normal Form:");
            Console.WriteLine(_grammar);

            return _grammar;
        }

        private void EliminateEpsilonProductions()
        {
            HashSet<string> nullableSymbols = FindNullableSymbols();

            List<Rule> newRules = new List<Rule>();
            foreach (var rule in _grammar.Rules)
            {
                if (rule.RightSide.Count == 1 && rule.RightSide[0] == "ε")
                {
                    continue;
                }

                List<int> nullablePositions = new List<int>();
                for (int i = 0; i < rule.RightSide.Count; i++)
                {
                    if (nullableSymbols.Contains(rule.RightSide[i]))
                    {
                        nullablePositions.Add(i);
                    }
                }

                if (nullablePositions.Count > 0)
                {
                    GenerateNullableCombinations(rule, nullablePositions, 0, newRules);
                }
            }

            foreach (var rule in newRules)
            {
                if (rule.RightSide.Count == 0)
                {
                    rule.RightSide.Add("ε");
                }

                if (!_grammar.Rules.Any(r => r.LeftSide == rule.LeftSide &&
                                            r.RightSide.SequenceEqual(rule.RightSide)))
                {
                    _grammar.Rules.Add(rule);
                }
            }

            _grammar.Rules.RemoveAll(r => r.RightSide.Count == 1 && r.RightSide[0] == "ε" && r.LeftSide != _grammar.StartSymbol);
        }

        private HashSet<string> FindNullableSymbols()
        {
            HashSet<string> nullable = new HashSet<string>();
            bool changed;

            foreach (var rule in _grammar.Rules)
            {
                if (rule.RightSide.Count == 1 && rule.RightSide[0] == "ε")
                {
                    nullable.Add(rule.LeftSide);
                }
            }

            do
            {
                changed = false;
                foreach (var rule in _grammar.Rules)
                {
                    if (!nullable.Contains(rule.LeftSide) &&
                        rule.RightSide.All(symbol => nullable.Contains(symbol)))
                    {
                        nullable.Add(rule.LeftSide);
                        changed = true;
                    }
                }
            } while (changed);

            return nullable;
        }

        private void GenerateNullableCombinations(Rule rule, List<int> nullablePositions, int index, List<Rule> newRules)
        {
            if (index >= nullablePositions.Count)
            {
                return;
            }

            var newRule = rule.Clone();
            newRule.RightSide.RemoveAt(nullablePositions[index] - index);

            if (newRule.RightSide.Count > 0 || newRule.LeftSide == _grammar.StartSymbol)
            {
                newRules.Add(newRule);
            }

            GenerateNullableCombinations(rule.Clone(), nullablePositions, index + 1, newRules);
            GenerateNullableCombinations(newRule, nullablePositions, index + 1, newRules);
        }

        private void EliminateUnitProductions()
        {
            Dictionary<string, HashSet<string>> directUnitPairs = new Dictionary<string, HashSet<string>>();

            foreach (var nonTerminal in _grammar.NonTerminals)
            {
                directUnitPairs[nonTerminal] = new HashSet<string>();
                directUnitPairs[nonTerminal].Add(nonTerminal);
            }

            foreach (var rule in _grammar.Rules)
            {
                if (rule.RightSide.Count == 1 && _grammar.NonTerminals.Contains(rule.RightSide[0]))
                {
                    directUnitPairs[rule.LeftSide].Add(rule.RightSide[0]);
                }
            }

            Dictionary<string, HashSet<string>> unitPairs = ComputeTransitiveClosure(directUnitPairs);

            List<Rule> newRules = new List<Rule>();

            foreach (var pair in unitPairs)
            {
                string A = pair.Key;
                foreach (string B in pair.Value)
                {
                    if (A == B) continue;

                    foreach (var rule in _grammar.Rules)
                    {
                        if (rule.LeftSide == B && (rule.RightSide.Count != 1 || !_grammar.NonTerminals.Contains(rule.RightSide[0])))
                        {
                            newRules.Add(new Rule(A, new List<string>(rule.RightSide)));
                        }
                    }
                }
            }

            _grammar.Rules.RemoveAll(r => r.RightSide.Count == 1 && _grammar.NonTerminals.Contains(r.RightSide[0]));
            _grammar.Rules.AddRange(newRules);
        }

        private Dictionary<string, HashSet<string>> ComputeTransitiveClosure(Dictionary<string, HashSet<string>> relation)
        {
            Dictionary<string, HashSet<string>> closure = new Dictionary<string, HashSet<string>>();

            foreach (var pair in relation)
            {
                closure[pair.Key] = new HashSet<string>(pair.Value);
            }

            foreach (string k in _grammar.NonTerminals)
            {
                foreach (string i in _grammar.NonTerminals)
                {
                    if (closure[i].Contains(k))
                    {
                        foreach (string j in _grammar.NonTerminals)
                        {
                            if (closure[k].Contains(j))
                            {
                                closure[i].Add(j);
                            }
                        }
                    }
                }
            }

            return closure;
        }

        private void EliminateInaccessibleSymbols()
        {
            HashSet<string> accessibleSymbols = new HashSet<string>();
            Queue<string> queue = new Queue<string>();

            queue.Enqueue(_grammar.StartSymbol);
            accessibleSymbols.Add(_grammar.StartSymbol);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                foreach (var rule in _grammar.Rules.Where(r => r.LeftSide == current))
                {
                    foreach (string symbol in rule.RightSide)
                    {
                        if (symbol == "ε" || accessibleSymbols.Contains(symbol))
                            continue;

                        accessibleSymbols.Add(symbol);

                        if (_grammar.NonTerminals.Contains(symbol))
                        {
                            queue.Enqueue(symbol);
                        }
                    }
                }
            }

            _grammar.NonTerminals.IntersectWith(accessibleSymbols);
            _grammar.Terminals.IntersectWith(accessibleSymbols);

            _grammar.Rules = _grammar.Rules
                .Where(r => accessibleSymbols.Contains(r.LeftSide) &&
                           r.RightSide.All(s => s == "ε" || accessibleSymbols.Contains(s)))
                .ToList();
        }

        private void EliminateNonProductiveSymbols()
        {
            HashSet<string> productive = new HashSet<string>();
            bool changed;

            foreach (var terminal in _grammar.Terminals)
            {
                productive.Add(terminal);
            }

            do
            {
                changed = false;
                foreach (var rule in _grammar.Rules)
                {
                    if (!productive.Contains(rule.LeftSide) &&
                        (rule.RightSide[0] == "ε" || rule.RightSide.All(symbol => productive.Contains(symbol))))
                    {
                        productive.Add(rule.LeftSide);
                        changed = true;
                    }
                }
            } while (changed);

            _grammar.NonTerminals.IntersectWith(productive);

            _grammar.Rules = _grammar.Rules
                .Where(r => productive.Contains(r.LeftSide) &&
                           (r.RightSide[0] == "ε" || r.RightSide.All(s => productive.Contains(s))))
                .ToList();
        }

        private void ConvertToBinarizedForm()
        {
            List<Rule> newRules = new List<Rule>();
            Dictionary<string, string> terminalToNonTerminal = new Dictionary<string, string>();
            Dictionary<string, string> rightSidePatterns = new Dictionary<string, string>();

            foreach (var rule in _grammar.Rules)
            {
                if (rule.RightSide.Count > 1)
                {
                    List<string> newRightSide = new List<string>();

                    for (int i = 0; i < rule.RightSide.Count; i++)
                    {
                        string symbol = rule.RightSide[i];

                        if (_grammar.Terminals.Contains(symbol))
                        {
                            if (!terminalToNonTerminal.ContainsKey(symbol))
                            {
                                string newNonTerminal = $"T_{symbol}";
                                terminalToNonTerminal[symbol] = newNonTerminal;
                                _grammar.NonTerminals.Add(newNonTerminal);
                                newRules.Add(new Rule(newNonTerminal, new List<string> { symbol }));
                            }

                            newRightSide.Add(terminalToNonTerminal[symbol]);
                        }
                        else
                        {
                            newRightSide.Add(symbol);
                        }
                    }

                    rule.RightSide = newRightSide;
                }
            }

            List<Rule> rulesToBreak = _grammar.Rules.Where(r => r.RightSide.Count > 2).ToList();

            foreach (var rule in rulesToBreak)
            {
                string currentLeft = rule.LeftSide;
                List<string> currentRight = rule.RightSide;

                while (currentRight.Count > 2)
                {
                    string pattern = string.Join("", currentRight.Take(2));
                    string newNonTerminal;

                    if (rightSidePatterns.ContainsKey(pattern))
                    {
                        newNonTerminal = rightSidePatterns[pattern];
                    }
                    else
                    {
                        newNonTerminal = GenerateNewNonTerminal();
                        _grammar.NonTerminals.Add(newNonTerminal);

                        List<string> firstTwo = new List<string> { currentRight[0], currentRight[1] };
                        newRules.Add(new Rule(newNonTerminal, firstTwo));

                        rightSidePatterns[pattern] = newNonTerminal;
                    }

                    currentRight.RemoveRange(0, 2);
                    currentRight.Insert(0, newNonTerminal);
                }

                rule.RightSide = new List<string>(currentRight);
            }

            _grammar.Rules.AddRange(newRules);

            _grammar.Rules = _grammar.Rules
                .GroupBy(r => new { r.LeftSide, RightSide = string.Join("", r.RightSide) })
                .Select(g => g.First())
                .ToList();
        }

        private string GenerateNewNonTerminal()
        {
            return $"X{_newSymbolCounter++}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            HashSet<string> nonTerminals = new HashSet<string> { "S", "A", "B", "C", "E" };
            HashSet<string> terminals = new HashSet<string> { "a", "b" };
            List<Rule> rules = new List<Rule>
            {
                new Rule("S", new List<string> { "b", "A" }),
                new Rule("S", new List<string> { "B" }),
                new Rule("A", new List<string> { "a" }),
                new Rule("A", new List<string> { "a", "S" }),
                new Rule("A", new List<string> { "b", "A","a","A","b" }),
                new Rule("B", new List<string> { "A" , "C" }),
                new Rule("B", new List<string> { "b", "S" }),
                new Rule("B", new List<string> { "a", "A", "a" }),
                new Rule("C", new List<string> { "ε" }),
                new Rule("C", new List<string> { "A", "B" }),
                new Rule("E", new List<string> { "B", "A" }),
            };

            Grammar grammar = new Grammar(nonTerminals, terminals, rules, "S");

            Console.WriteLine("Starting the conversion to Chomsky Normal Form...");

            ChomskyCnfConverter converter = new ChomskyCnfConverter(grammar);
            Grammar cnfGrammar = converter.ConvertToCnf();

            Console.WriteLine("Final Chomsky Normal Form grammar:");
            Console.WriteLine(cnfGrammar);
        }
    }
}