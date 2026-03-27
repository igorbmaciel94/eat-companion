using System.Text.RegularExpressions;

namespace EatCompanion.Infrastructure.PdfParsing;

public static class PortugueseMealPatterns
{
    // Section headers
    public static readonly Regex BreakfastHeader = new(@"Pequeno[\s\-]?almo[çc]o", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex LunchHeader = new(@"\bAlmo[çc]o\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex SnackHeader = new(@"Lanche(?:\s+da\s+tarde)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex DinnerHeader = new(@"\bJantar\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex RecommendationsHeader = new(@"Recomenda[çc][õo]es|RECOMENDA", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    // Only match "Bolo da caneca" as a section header when it appears on its own line (not inline in an option)
    // Match "Bolo da caneca de chocolate\nIngredientes:" or standalone "Receita" header
    public static readonly Regex RecipeHeader = new(@"^Bolo\s+da\s+caneca\s+de\s+chocolate\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

    // Option separator - "OU" on its own line
    public static readonly Regex OptionSeparator = new(@"^\s*OU\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    // Option label (e.g., "Opção 1", "Opção 2")
    public static readonly Regex OptionLabel = new(@"Op[çc][ãa]o\s+(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Ingredient patterns - ordered from most specific to least specific
    public static readonly Regex QuantityTablespoon = new(@"(\d+)\s+colher(?:es)?\s+de\s+sopa\s+de\s+(.+?)(?:\s*[+\n]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex QuantityTeaspoon = new(@"(\d+)\s+colher(?:es)?\s+de\s+ch[áa]\s+de\s+(.+?)(?:\s*[+\n]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex QuantityGrams = new(@"(\d+)\s*g\s+de\s+(.+?)(?:\s*[+\n]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex QuantityMl = new(@"(\d+)\s*ml\s+de\s+(.+?)(?:\s*[+\n]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex QuantitySlices = new(@"(\d+)\s+fatia(?:s)?\s+de\s+(.+?)(?:\s*[+\n]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex QuantityUnits = new(@"(\d+)\s+(ovo|ovos|porç[ãa]o\s+de\s+\w+|iogurte[^+\n]*|pudim[^+\n]*|queijinho[^+\n]*|bolachas?\s+\w+|tortitas?\s+de\s+\w+(?:/\w+)?|tortilha[^+\n]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex QuantityPorcao = new(@"(\d+)\s+porç[ãa]o\s+de\s+(.+?)(?:\s*[+\n]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Noise patterns to remove
    public static readonly Regex NutritionistName = new(@"Teresa\s+(?:Valadar\s+)?\d+N?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // General food item with "de" connector
    public static readonly Regex GeneralDe = new(@"(\d+)\s+(?:de\s+)?(.+?)(?:\s*[+\n]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
}
