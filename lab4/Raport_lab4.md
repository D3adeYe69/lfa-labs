# Regular expressions

## Course: Formal Languages & Finite Automata  
## Author: Chirtoaca Liviu  

## Theory  
Regular expressions (regex) are patterns used to match character combinations in strings. A regex defines rules that a string must follow to be considered a match. By interpreting these rules, it's possible to both recognize and generate strings that conform to a particular grammar.

In this laboratory, we focus on the **reverse use of regex**—instead of matching strings, we generate them based on a given pattern. 

---

## Objectives  
- Understand how regular expressions describe string patterns.
- Implement a regex interpreter that generates strings rather than matching them.
- Handle repetition quantifiers and grouped alternatives.
- Ensure consistent interpretation of repetition rules, especially for custom quantifiers like `⁺`, `²`, and `³`.

---

## Implementation Description  

### Core Components
- **Pattern Parser**: Processes the regex string character by character.
- **Repetition Handler**: Applies quantifiers like `*`, `⁺`, `²`, and `³` to characters and groups.
- **Alternative Resolver**: Randomly selects one alternative from grouped patterns like `(A|B|C)`.
- **StringBuilder Result**: Accumulates the final output string based on interpreted rules.

### Key Methods
- **ParseRegex()**: Main parser that walks through the regex pattern.
- **ParseChosen()**: Helper method that interprets selected alternatives.
- **GenerateStringFromRegex()**: Wrapper that initializes the parse and returns the generated string.

### Token Handling Logic
- Parenthesis `()` enclose a group of alternatives.
- Pipe `|` separates choices inside a group.
- Quantifiers include:
  - `⁺` : Repeat one or more times.
  - `*`: Repeat zero or more times.
  - `?`: Optional (zero or one time).
  - `²`: Repeat exactly twice.
  - `³`: Repeat exactly three times.
- For groups, one random alternative is chosen and repeated accordingly.
- For literals, quantifiers modify how many times the literal appears.

### Processing Workflow
1. Read the regex string from left to right.
2. Handle literals and group expressions.
3. Apply quantifiers to the previous element (character or group).
4. Randomize repetition counts where needed.
5. Return a fully constructed string matching the regex.


---

## Code Implementation
```csharp
static string GenerateStringFromRegex(string pattern)
{
    StringBuilder result = new StringBuilder();
    int i = 0;
    ParseRegex(pattern, ref i, result);
    return result.ToString();
}
```
In the code above it is shown how the strings are being generated from the regex patterns. The `GenerateStringFromRegex` method is the main entry point for the regex generation process. It initializes a `StringBuilder` to accumulate the generated string and calls the `ParsRegex`

```csharp
 string chosen = alternatives[random.Next(alternatives.Length)];
for (int rep = 0; rep < repetitions; rep++)
{
    int tempIndex = 0;
    ParseChosen(chosen, ref tempIndex, result);
}
```
This is the part of the code where the chosen alternatives are being repeated according to the quantifiers. We randomly select an alternative and repeat it the specified number of times. The `ParseChosen` method is used to interpret the selected alternative and