using System.Text.RegularExpressions;
using EatCompanion.Domain.Enums;

namespace EatCompanion.Infrastructure.PdfParsing;

public class MealPlanParser
{
    public ParsedMealPlan Parse(IReadOnlyList<string> pageTexts)
    {
        var fullText = string.Join("\n", pageTexts);

        // Remove nutritionist watermarks
        fullText = PortugueseMealPatterns.NutritionistName.Replace(fullText, "");

        // Detect section boundaries
        var sections = DetectSections(fullText);

        // Parse each meal section
        var meals = new List<ParsedMeal>();
        foreach (var section in sections)
        {
            if (section.MealType is null)
                continue;

            var options = ParseOptions(section.Text);
            if (options.Count > 0)
            {
                meals.Add(new ParsedMeal(section.MealType.Value, options));
            }
        }

        return new ParsedMealPlan(meals);
    }

    internal List<DetectedSection> DetectSections(string fullText)
    {
        var sectionStarts = new List<(int Index, SectionType Type, MealType? MealType, string HeaderText)>();

        // Find all section headers
        FindHeaders(fullText, PortugueseMealPatterns.BreakfastHeader, SectionType.Breakfast, MealType.Breakfast, sectionStarts);
        FindHeaders(fullText, PortugueseMealPatterns.LunchHeader, SectionType.Lunch, MealType.Lunch, sectionStarts);
        FindHeaders(fullText, PortugueseMealPatterns.SnackHeader, SectionType.Snack, MealType.AfternoonSnack, sectionStarts);
        FindHeaders(fullText, PortugueseMealPatterns.DinnerHeader, SectionType.Dinner, MealType.Dinner, sectionStarts);
        FindHeaders(fullText, PortugueseMealPatterns.RecommendationsHeader, SectionType.Recommendations, null, sectionStarts);
        FindHeaders(fullText, PortugueseMealPatterns.RecipeHeader, SectionType.Recipe, null, sectionStarts);

        // Sort by position in text
        sectionStarts.Sort((a, b) => a.Index.CompareTo(b.Index));

        // Deduplicate: if a "Lanche da tarde" was found, remove any bare "Lanche" at the same position
        // Also deduplicate overlapping headers (e.g., "Pequeno-almoço" contains "Almoço")
        var deduped = new List<(int Index, SectionType Type, MealType? MealType, string HeaderText)>();
        foreach (var item in sectionStarts)
        {
            var dominated = false;
            for (int idx = 0; idx < sectionStarts.Count; idx++)
            {
                var other = sectionStarts[idx];
                if (other.Index == item.Index && other.Type == item.Type && other.HeaderText == item.HeaderText) continue;
                // If another header fully contains this one's position range
                if (other.Index <= item.Index &&
                    other.Index + other.HeaderText.Length >= item.Index + item.HeaderText.Length &&
                    other.HeaderText.Length > item.HeaderText.Length)
                {
                    dominated = true;
                    break;
                }
            }
            if (!dominated)
            {
                deduped.Add(item);
            }
        }

        // Remove "Almoço" matches that are actually part of "Pequeno-almoço"
        deduped = deduped.Where(d =>
        {
            if (d.Type == SectionType.Lunch)
            {
                // Check if there's a breakfast header within a few chars before this
                var lookBack = Math.Max(0, d.Index - 20);
                var prefix = fullText.Substring(lookBack, d.Index - lookBack);
                if (Regex.IsMatch(prefix, @"Pequeno[\s\-]?$", RegexOptions.IgnoreCase))
                    return false;
            }
            return true;
        }).ToList();

        // Build sections from boundaries
        var sections = new List<DetectedSection>();
        for (int i = 0; i < deduped.Count; i++)
        {
            var start = deduped[i].Index;
            var end = i + 1 < deduped.Count ? deduped[i + 1].Index : fullText.Length;
            var sectionText = fullText.Substring(start, end - start).Trim();

            // Remove the header text itself from the section body
            var headerEnd = sectionText.IndexOf('\n');
            if (headerEnd > 0)
                sectionText = sectionText.Substring(headerEnd).Trim();

            sections.Add(new DetectedSection(deduped[i].Type, deduped[i].MealType, sectionText));
        }

        // Merge duplicate dinner sections (Opção 1 and Opção 2)
        var dinnerSections = sections.Where(s => s.Type == SectionType.Dinner).ToList();
        if (dinnerSections.Count > 1)
        {
            var mergedText = string.Join("\nOU\n", dinnerSections.Select(d => d.Text));
            sections.RemoveAll(s => s.Type == SectionType.Dinner);
            sections.Add(new DetectedSection(SectionType.Dinner, MealType.Dinner, mergedText));
        }

        return sections;
    }

    private static void FindHeaders(
        string text,
        Regex pattern,
        SectionType sectionType,
        MealType? mealType,
        List<(int Index, SectionType Type, MealType? MealType, string HeaderText)> results)
    {
        foreach (Match match in pattern.Matches(text))
        {
            results.Add((match.Index, sectionType, mealType, match.Value));
        }
    }

    internal List<ParsedMealOption> ParseOptions(string sectionText)
    {
        // Strip noise lines (nutritionist watermarks, recommendations, reference info)
        sectionText = StripNoiseLines(sectionText);

        // Remove option labels like "Opção 1", "Opção 2"
        sectionText = PortugueseMealPatterns.OptionLabel.Replace(sectionText, "");

        // Split by "OU" on its own line
        var parts = PortugueseMealPatterns.OptionSeparator.Split(sectionText);

        var options = new List<ParsedMealOption>();
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            var ingredients = ParseIngredients(trimmed);
            // Use the first line or trimmed text as description
            var description = GetOptionDescription(trimmed);

            if (!string.IsNullOrWhiteSpace(description))
            {
                options.Add(new ParsedMealOption(description, ingredients));
            }
        }

        return options;
    }

    private static string GetOptionDescription(string optionText)
    {
        // Clean up the text for description: keep as line-separated ingredients
        var lines = optionText.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l));

        return string.Join("\n", lines)
            .Replace("  ", " ")
            .Trim();
    }

    internal List<ParsedIngredient> ParseIngredients(string optionText)
    {
        var ingredients = new List<ParsedIngredient>();
        var matched = new HashSet<string>();

        // Tablespoons: "2 colheres de sopa de tapioca"
        foreach (Match match in PortugueseMealPatterns.QuantityTablespoon.Matches(optionText))
        {
            var amount = decimal.Parse(match.Groups[1].Value);
            var name = CleanIngredientName(match.Groups[2].Value);
            if (!string.IsNullOrWhiteSpace(name))
            {
                ingredients.Add(new ParsedIngredient(name, amount, UnitOfMeasure.Tablespoon));
                matched.Add(match.Value);
            }
        }

        // Teaspoons: "1 colher de chá de ..."
        foreach (Match match in PortugueseMealPatterns.QuantityTeaspoon.Matches(optionText))
        {
            var amount = decimal.Parse(match.Groups[1].Value);
            var name = CleanIngredientName(match.Groups[2].Value);
            if (!string.IsNullOrWhiteSpace(name))
            {
                ingredients.Add(new ParsedIngredient(name, amount, UnitOfMeasure.Teaspoon));
                matched.Add(match.Value);
            }
        }

        // Grams: "60g de pão", "150g de arroz"
        foreach (Match match in PortugueseMealPatterns.QuantityGrams.Matches(optionText))
        {
            var amount = decimal.Parse(match.Groups[1].Value);
            var name = CleanIngredientName(match.Groups[2].Value);
            if (!string.IsNullOrWhiteSpace(name))
            {
                ingredients.Add(new ParsedIngredient(name, amount, UnitOfMeasure.Grams));
                matched.Add(match.Value);
            }
        }

        // Milliliters
        foreach (Match match in PortugueseMealPatterns.QuantityMl.Matches(optionText))
        {
            var amount = decimal.Parse(match.Groups[1].Value);
            var name = CleanIngredientName(match.Groups[2].Value);
            if (!string.IsNullOrWhiteSpace(name))
            {
                ingredients.Add(new ParsedIngredient(name, amount, UnitOfMeasure.Ml));
                matched.Add(match.Value);
            }
        }

        // Slices: "2 fatias de queijo flamengo"
        foreach (Match match in PortugueseMealPatterns.QuantitySlices.Matches(optionText))
        {
            var amount = decimal.Parse(match.Groups[1].Value);
            var name = CleanIngredientName(match.Groups[2].Value);
            if (!string.IsNullOrWhiteSpace(name))
            {
                ingredients.Add(new ParsedIngredient(name, amount, UnitOfMeasure.Slice));
                matched.Add(match.Value);
            }
        }

        // Porção: "1 porção de fruta"
        foreach (Match match in PortugueseMealPatterns.QuantityPorcao.Matches(optionText))
        {
            var amount = decimal.Parse(match.Groups[1].Value);
            var name = CleanIngredientName(match.Groups[2].Value);
            if (!string.IsNullOrWhiteSpace(name))
            {
                ingredients.Add(new ParsedIngredient(name, amount, UnitOfMeasure.Units));
                matched.Add(match.Value);
            }
        }

        // Units: "1 ovo", "3 ovos"
        foreach (Match match in PortugueseMealPatterns.QuantityUnits.Matches(optionText))
        {
            // Avoid double-matching things already matched as tablespoons/slices/etc.
            if (matched.Any(m => m.Contains(match.Value)))
                continue;

            var amount = decimal.Parse(match.Groups[1].Value);
            var name = CleanIngredientName(match.Groups[2].Value);
            if (!string.IsNullOrWhiteSpace(name))
            {
                ingredients.Add(new ParsedIngredient(name, amount, UnitOfMeasure.Units));
            }
        }

        // Look for "café sem açúcar" as a standalone ingredient
        if (Regex.IsMatch(optionText, @"caf[ée]\s+sem\s+a[çc][úu]car", RegexOptions.IgnoreCase))
        {
            if (!ingredients.Any(i => i.NamePt.Contains("café", StringComparison.OrdinalIgnoreCase)))
            {
                ingredients.Add(new ParsedIngredient("café", 1, UnitOfMeasure.Units));
            }
        }

        // Look for "sopa" and "salada/legumes" as generic items
        if (Regex.IsMatch(optionText, @"\bsopa\b", RegexOptions.IgnoreCase))
        {
            if (!ingredients.Any(i => i.NamePt.Contains("sopa", StringComparison.OrdinalIgnoreCase)))
            {
                ingredients.Add(new ParsedIngredient("sopa", 1, UnitOfMeasure.Units));
            }
        }

        if (Regex.IsMatch(optionText, @"\bsalada\b", RegexOptions.IgnoreCase))
        {
            if (!ingredients.Any(i => i.NamePt.Contains("salada", StringComparison.OrdinalIgnoreCase)))
            {
                ingredients.Add(new ParsedIngredient("salada", 1, UnitOfMeasure.Units));
            }
        }

        if (Regex.IsMatch(optionText, @"\blegumes\b", RegexOptions.IgnoreCase))
        {
            if (!ingredients.Any(i => i.NamePt.Contains("legumes", StringComparison.OrdinalIgnoreCase)))
            {
                ingredients.Add(new ParsedIngredient("legumes", 1, UnitOfMeasure.Units));
            }
        }

        if (Regex.IsMatch(optionText, @"\bazeite\b", RegexOptions.IgnoreCase))
        {
            if (!ingredients.Any(i => i.NamePt.Contains("azeite", StringComparison.OrdinalIgnoreCase)))
            {
                ingredients.Add(new ParsedIngredient("azeite", 1, UnitOfMeasure.Units));
            }
        }

        return ingredients;
    }

    private static string CleanIngredientName(string name)
    {
        // Remove trailing descriptors like "- 50% gordura", "(entre 15 a 20g proteína)"
        name = Regex.Replace(name, @"\s*[-–]\s*\d+%.*$", "", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, @"\s*\(.*?\)\s*", " ", RegexOptions.IgnoreCase);

        // Remove "cozinhados", "cozinhado" and similar cooking descriptors
        name = Regex.Replace(name, @"\s+cozinhad[oa]s?\b", "", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, @"\s+sem\s+gordura\s+adicionada\b", "", RegexOptions.IgnoreCase);

        // Clean up separators: "arroz/massa/batata" stays as-is for now
        name = name.Trim().Trim('+', ' ', '\t', '\n', '\r');

        return name;
    }

    private static string StripNoiseLines(string text)
    {
        var lines = text.Split('\n');
        var cleaned = lines.Where(line =>
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) return true;
            // Remove lines with nutritionist name/number patterns (e.g., "Teresa 5235N", "Teresa Valadar 5235N")
            if (Regex.IsMatch(trimmed, @"Teresa\b", RegexOptions.IgnoreCase)) return false;
            if (Regex.IsMatch(trimmed, @"^\d+N\s*$")) return false;
            // Remove recommendation-style numbered items (e.g., "1- Beber pelo menos 2L...")
            if (Regex.IsMatch(trimmed, @"^\d+\s*[-–]\s+\w", RegexOptions.IgnoreCase)) return false;
            // Remove lines with nutritional reference info (e.g., "150g de peixe: corresponde a...")
            if (trimmed.Contains("corresponde a", StringComparison.OrdinalIgnoreCase)) return false;
            // Remove advisory text patterns
            if (Regex.IsMatch(trimmed, @"\bimportante\s+manter\b", RegexOptions.IgnoreCase)) return false;
            if (Regex.IsMatch(trimmed, @"\bobrigat[óo]rio\b", RegexOptions.IgnoreCase)) return false;
            if (Regex.IsMatch(trimmed, @"\bApesar\s+a\s+sopa\b", RegexOptions.IgnoreCase)) return false;
            if (Regex.IsMatch(trimmed, @"\bsempre\s+na\s+Valadar\b", RegexOptions.IgnoreCase)) return false;
            return true;
        });
        return string.Join("\n", cleaned);
    }
}
