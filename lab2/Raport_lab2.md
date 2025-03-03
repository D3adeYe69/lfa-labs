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

### Converting NDFA to DFA
The ConvertNDFAtoDFA() function follows these steps:
1. Initialize the DFA state set with the NDFA start state.
2. Process each DFA state, checking its possible transitions.
3. Create new DFA states for any set of NDFA states reached via a transition.
4. Continue until all reachable states are processed.
5. Ensure that final states include any state that contains the original NDFA final state.

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
```c#
string result = "S";
while (result.IndexOfAny(VN.ToArray()) != -1)
{
    foreach (char nonTerminal in VN)
    {
        if (result.Contains(nonTerminal))
        {
            string replacement = P[nonTerminal][rand.Next(P[nonTerminal].Count)];
            result = result.Replace(nonTerminal.ToString(), replacement);
        }
    }
}
return result;
```
Starts with S and replaces non-terminals with random productions until only terminals remain, generating valid strings.

```csharp
var dfaTransitions = new Dictionary<(string, char), string>();
var newStates = new Queue<HashSet<string>>();
var initialState = new HashSet<string> { StartState };
newStates.Enqueue(initialState);

while (newStates.Count > 0)
{
    var currentSet = newStates.Dequeue();
    string stateName = string.Join("", currentSet.OrderBy(s => s));
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
            newStates.Enqueue(newStateSet);
        }
    }
}
return dfaTransitions;
```
Converts a Non-Deterministic Finite Automaton (NDFA) to a Deterministic Finite Automaton (DFA) by creating new states that represent combinations of NDFA states and defining transitions between them.




![Console results](/Images/Console2.png)
---

## Conclusion  
This project demonstrates how formal languages and finite automata work together to define structured word formation and validation. By implementing a grammar-based generator and a basic automaton, we can see how theoretical concepts translate into actual computation.
