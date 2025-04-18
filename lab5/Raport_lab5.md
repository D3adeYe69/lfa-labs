# Chomsky Normal Form

## Course: Formal Languages & Finite Automata  
## Author: Chirtoaca Liviu  

## Theory  
Context-free grammars (CFGs) are powerful tools for describing languages with nested or recursive structures. Chomsky Normal Form (CNF) is a standardized form for context-free grammars where all production rules follow specific patterns. A grammar in CNF has all its rules in one of these formats:

1. A → BC (where A, B, and C are non-terminal symbols)
2. A → a (where A is a non-terminal symbol and a is a terminal symbol)
3. S → ε (only allowed if S is the start symbol and it doesn't appear on the right side of any rule)

Converting a grammar to CNF simplifies many algorithms in language processing, such as the CYK parsing algorithm, which relies on the binary structure of CNF rules.

The conversion process involves several steps:
1. Eliminating ε-productions (except possibly S → ε)
2. Eliminating unit productions (like A → B)
3. Eliminating inaccessible symbols
4. Eliminating non-productive symbols
5. Converting remaining rules to CNF form

---

## Implementation Description  

### Grammar Representation
The implementation uses three main classes to represent and manipulate context-free grammars:

- **Rule**: Encapsulates a production rule with a left-side non-terminal and a list of right-side symbols
- **Grammar**: Represents the complete grammar with sets of non-terminals, terminals, production rules, and a start symbol
- **ChomskyCnfConverter**: Contains the algorithms for each transformation step


### Step 1: Eliminating ε-Productions
This step identifies all nullable symbols (those that can derive ε) and generates new rules that consider all combinations of including or excluding these nullable symbols.

```csharp
private void EliminateEpsilonProductions()
{
    HashSet<string> nullableSymbols = FindNullableSymbols();
    List<Rule> newRules = new List<Rule>();
    _grammar.Rules.RemoveAll(r => r.RightSide.Count == 1 && r.RightSide[0] == "ε" && r.LeftSide != _grammar.StartSymbol);
}
```

The algorithm first identifies all symbols that can derive ε either directly or indirectly. Then, for each rule containing nullable symbols, it generates all possible combinations of rules where these nullable symbols are either kept or removed.

### Step 2: Eliminating Unit Productions
Unit productions (also called renaming rules) are rules of the form A → B, where both A and B are non-terminals. The elimination process replaces these with direct derivations:

```csharp
private void EliminateUnitProductions()
{
    Dictionary<string, HashSet<string>> unitPairs = ComputeTransitiveClosure(directUnitPairs);

    List<Rule> newRules = new List<Rule>();
    foreach (var pair in unitPairs)
    {
        string A = pair.Key;
        foreach (string B in pair.Value)
        {
            if (A == B) continue;
        }
    }
    
    _grammar.Rules.RemoveAll(r => r.RightSide.Count == 1 &&  _grammar.NonTerminals.Contains(r.RightSide[0]));
    _grammar.Rules.AddRange(newRules);
}
```

This step ensures that no rule in the grammar has a single non-terminal on the right side, which is a requirement for CNF.

### Step 3: Eliminating Inaccessible Symbols
Inaccessible symbols are those that cannot be reached from the start symbol in any derivation. These symbols are identified using a breadth-first search algorithm:

```csharp
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
}
```

This step simplifies the grammar by removing symbols and rules that don't contribute to the language.

### Step 4: Eliminating Non-Productive Symbols
Non-productive symbols are those that cannot derive any string of terminals. The implementation finds productive symbols through an iterative process:

```csharp
private void EliminateNonProductiveSymbols()
{
    HashSet<string> productive = new HashSet<string>();
    
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

```

This fixed-point iteration continues until no new productive symbols are found, ensuring that all symbols in the final grammar contribute to generating actual strings in the language.

### Step 5: Converting to Chomsky Normal Form
The final step transforms the remaining rules into the CNF format by:
1. Replacing terminals within longer rules with new non-terminals
2. Breaking down rules with more than two right-side symbols into binary rules

```csharp
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
    }
    
```
Second part will break rules with non-terminals more than 2
```csharp
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

``` 

The implementation optimizes this process by reusing non-terminals for identical patterns, which reduces redundancy in the final grammar.

---

## Results and Analysis

When running the implementation on the provided grammar:

```
G = (VN, VT, P, S)  
VN = {S, A, B, C, E}
VT = {a, b}
S = S
P = {
    1. S → bA       
    2. S → B        
    3. A → a        
    4. A → aS       
    5. A → bAaAb
    6. B → AC
    7. B → bS
    8. B → aAa
    9. C → ε
    10. C → AB
    11. E → BA
}

```

1. **After eliminating ε-productions**: Rules with C → ε are removed, and new rules are generated to account for C's potential absence.
![Epsilon](/Images/image.png)
2. **After eliminating unit productions**: Direct derivations replace rules like A → B.
![Unit](/Images/image-1.png)
3. **After eliminating inaccessible symbols**: Symbol E is removed as it's not reachable from S.
![Inaccesible](/Images/image-2.png)
4. **After eliminating non-productive symbols**: All remaining symbols are productive.
![Non-productive](/Images/image-3.png)
5. **Final CNF form**: Rules are in proper A → BC or A → a format.
![Final](/Images/image-4.png)

The implementation successfully handles any context-free grammar input, making it a flexible tool for grammar normalization.

---

## Conclusions

Implementing the conversion to Chomsky Normal Form provides valuable insights into grammar transformations and formal language theory. The step-by-step approach ensures that each transformation preserves the language generated by the grammar while moving closer to the required normal form.
The implementation demonstrates how complex grammar transformations can be broken down into manageable steps, each with a specific purpose in achieving the final normal form.
