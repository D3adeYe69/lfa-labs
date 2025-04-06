using System;
using System.Collections.Generic;
using System.Text;

namespace RegexGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Regular Expression String Generator");
            Console.WriteLine("===================================");

            string[] patterns = new string[]
            {
                "O(P|Q|R)⁺2(3|4)",
                "A*B(C|D|E)F(G|H|I)²",
                "J⁺K(L|M|N)*O?(P|Q)³"
            };

            foreach (var pattern in patterns)
            {
                Console.WriteLine($"\nRegex Pattern: {pattern}");
                Console.WriteLine("Generated examples:");

                for (int i = 0; i < 5; i++)
                {
                    string generated = GenerateStringFromRegex(pattern);
                    Console.WriteLine($"  {i + 1}. {generated}");
                }
            }
        }

        static string GenerateStringFromRegex(string pattern)
        {
            StringBuilder result = new StringBuilder();
            int i = 0;
            ParseRegex(pattern, ref i, result);
            return result.ToString();
        }

        static void ParseRegex(string pattern, ref int i, StringBuilder result)
        {
            Random random = new Random();

            while (i < pattern.Length)
            {
                char c = pattern[i];
                i++;

                if (c == '(')
                {
                    StringBuilder groupContent = new StringBuilder();
                    int nestedLevel = 1;

                    while (i < pattern.Length && nestedLevel > 0)
                    {
                        char gc = pattern[i];
                        i++;

                        if (gc == '(')
                            nestedLevel++;
                        else if (gc == ')')
                            nestedLevel--;

                        if (nestedLevel > 0)
                            groupContent.Append(gc);
                    }

                    string[] alternatives = groupContent.ToString().Split('|');

                    int repetitions = 1;

                    if (i < pattern.Length)
                    {
                        char quantifier = pattern[i];

                        if (quantifier == '⁺' || quantifier == '+')
                        {
                            repetitions = random.Next(1, 6);
                            i++;
                        }
                        else if (quantifier == '*')
                        {
                            repetitions = random.Next(0, 6);
                            i++;
                        }
                        else if (quantifier == '?')
                        {
                            repetitions = random.Next(0, 2);
                            i++;
                        }
                        else if (quantifier == '²')
                        {
                            repetitions = 2;
                            i++;
                        }
                        else if (quantifier == '³')
                        {
                            repetitions = 3;
                            i++;
                        }
                    }

                    string chosen = alternatives[random.Next(alternatives.Length)];
                    for (int rep = 0; rep < repetitions; rep++)
                    {
                        int tempIndex = 0;
                        ParseChosen(chosen, ref tempIndex, result);
                    }
                }
                else if (c != ')')
                {
                    result.Append(c);

                    if (i < pattern.Length)
                    {
                        char next = pattern[i];

                        if (next == '⁺' || next == '+')
                        {
                            int additionalReps = random.Next(0, 5);
                            for (int rep = 0; rep < additionalReps; rep++)
                            {
                                result.Append(c);
                            }
                            i++;
                        }
                        else if (next == '*')
                        {
                            int totalReps = random.Next(0, 6);
                            if (totalReps == 0)
                            {
                                result.Length--;
                            }
                            else
                            {
                                for (int rep = 1; rep < totalReps; rep++)
                                {
                                    result.Append(c);
                                }
                            }
                            i++;
                        }
                        else if (next == '?')
                        {
                            if (random.Next(2) == 0)
                            {
                                result.Length--;
                            }
                            i++;
                        }
                    }
                }
            }
        }

        private static void ParseChosen(string chosen, ref int index, StringBuilder result)
        {
            while (index < chosen.Length)
            {
                char c = chosen[index];
                index++;

                if (c == '(')
                {
                    StringBuilder nestedContent = new StringBuilder();
                    int nestedLevel = 1;

                    while (index < chosen.Length && nestedLevel > 0)
                    {
                        char nc = chosen[index];
                        index++;

                        if (nc == '(')
                            nestedLevel++;
                        else if (nc == ')')
                            nestedLevel--;

                        if (nestedLevel > 0)
                            nestedContent.Append(nc);
                    }

                    string[] nestedAlternatives = nestedContent.ToString().Split('|');
                    string nestedChosen = nestedAlternatives[new Random().Next(nestedAlternatives.Length)];

                    int tempIndex = 0;
                    ParseChosen(nestedChosen, ref tempIndex, result);
                }
                else if (c != ')')
                {
                    result.Append(c);
                }
            }
        }
    }
}
