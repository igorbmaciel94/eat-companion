using EatCompanion.Domain.Enums;

namespace EatCompanion.Infrastructure.PdfParsing;

public class IngredientNormalizer
{
    private static readonly Dictionary<string, (string EnglishName, IngredientCategory Category)> Mappings =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Protein
            { "ovo", ("Egg", IngredientCategory.Protein) },
            { "ovos", ("Eggs", IngredientCategory.Protein) },
            { "carne", ("Meat", IngredientCategory.Protein) },
            { "peixe", ("Fish", IngredientCategory.Protein) },
            { "frango", ("Chicken", IngredientCategory.Protein) },
            { "carne/peixe", ("Meat/Fish", IngredientCategory.Protein) },

            // Dairy
            { "queijo flamengo", ("Edam Cheese", IngredientCategory.Dairy) },
            { "queijo cottage", ("Cottage Cheese", IngredientCategory.Dairy) },
            { "queijo creme light", ("Light Cream Cheese", IngredientCategory.Dairy) },
            { "iogurte proteico", ("Protein Yogurt", IngredientCategory.Dairy) },
            { "iogurte sólido ou líquido proteico", ("Protein Yogurt", IngredientCategory.Dairy) },
            { "iogurte sólido ou líquido magro", ("Low-fat Yogurt", IngredientCategory.Dairy) },
            { "pudim proteico", ("Protein Pudding", IngredientCategory.Dairy) },
            { "queijinho vaca que ri light", ("Light Laughing Cow Cheese", IngredientCategory.Dairy) },

            // Grains
            { "pão", ("Bread", IngredientCategory.Grains) },
            { "arroz", ("Rice", IngredientCategory.Grains) },
            { "massa", ("Pasta", IngredientCategory.Grains) },
            { "tapioca", ("Tapioca", IngredientCategory.Grains) },
            { "flocos de aveia", ("Oat Flakes", IngredientCategory.Grains) },
            { "farinha de aveia", ("Oat Flour", IngredientCategory.Grains) },
            { "granola", ("Granola", IngredientCategory.Grains) },
            { "granola 0% açúcar", ("Sugar-free Granola", IngredientCategory.Grains) },
            { "cereais", ("Cereal", IngredientCategory.Grains) },
            { "cereais 0% açúcar", ("Sugar-free Cereal", IngredientCategory.Grains) },
            { "bolachas marinheiras", ("Crackers", IngredientCategory.Grains) },
            { "tortitas de milho", ("Corn Cakes", IngredientCategory.Grains) },
            { "tortitas de arroz", ("Rice Cakes", IngredientCategory.Grains) },
            { "tortitas de milho/arroz", ("Corn/Rice Cakes", IngredientCategory.Grains) },
            { "tortilha grande de wrap", ("Large Wrap Tortilla", IngredientCategory.Grains) },
            { "batata", ("Potato", IngredientCategory.Grains) },
            { "arroz/massa/batata", ("Rice/Pasta/Potato", IngredientCategory.Grains) },

            // Produce
            { "fruta", ("Fruit", IngredientCategory.Produce) },
            { "banana", ("Banana", IngredientCategory.Produce) },
            { "legumes", ("Vegetables", IngredientCategory.Produce) },
            { "salada", ("Salad", IngredientCategory.Produce) },

            // Oils & Condiments
            { "azeite", ("Olive Oil", IngredientCategory.OilsAndCondiments) },
            { "chocolate em pó", ("Cocoa Powder", IngredientCategory.OilsAndCondiments) },
            { "fermento em pó", ("Baking Powder", IngredientCategory.OilsAndCondiments) },

            // Other
            { "café", ("Coffee", IngredientCategory.Other) },
            { "sopa", ("Soup", IngredientCategory.Other) },
        };

    // Partial match mappings for fuzzy matching - checked in order
    private static readonly List<(string Partial, string EnglishName, IngredientCategory Category)> PartialMappings = new()
    {
        ("queijo flamengo", "Edam Cheese", IngredientCategory.Dairy),
        ("queijo cottage", "Cottage Cheese", IngredientCategory.Dairy),
        ("queijo creme", "Light Cream Cheese", IngredientCategory.Dairy),
        ("iogurte", "Yogurt", IngredientCategory.Dairy),
        ("pudim proteico", "Protein Pudding", IngredientCategory.Dairy),
        ("queijinho", "Light Cheese", IngredientCategory.Dairy),
        ("bolachas", "Crackers", IngredientCategory.Grains),
        ("tortitas", "Corn/Rice Cakes", IngredientCategory.Grains),
        ("tortilha", "Wrap Tortilla", IngredientCategory.Grains),
        ("aveia", "Oats", IngredientCategory.Grains),
        ("granola", "Granola", IngredientCategory.Grains),
        ("cereais", "Cereal", IngredientCategory.Grains),
        ("arroz", "Rice", IngredientCategory.Grains),
        ("massa", "Pasta", IngredientCategory.Grains),
        ("batata", "Potato", IngredientCategory.Grains),
        ("pão", "Bread", IngredientCategory.Grains),
        ("frango", "Chicken", IngredientCategory.Protein),
        ("carne", "Meat", IngredientCategory.Protein),
        ("peixe", "Fish", IngredientCategory.Protein),
        ("ovo", "Egg", IngredientCategory.Protein),
        ("fruta", "Fruit", IngredientCategory.Produce),
        ("banana", "Banana", IngredientCategory.Produce),
        ("legumes", "Vegetables", IngredientCategory.Produce),
        ("salada", "Salad", IngredientCategory.Produce),
        ("azeite", "Olive Oil", IngredientCategory.OilsAndCondiments),
        ("chocolate", "Cocoa Powder", IngredientCategory.OilsAndCondiments),
        ("fermento", "Baking Powder", IngredientCategory.OilsAndCondiments),
        ("café", "Coffee", IngredientCategory.Other),
        ("sopa", "Soup", IngredientCategory.Other),
    };

    public (string EnglishName, IngredientCategory Category) Normalize(string portugueseName)
    {
        // Try exact match first
        if (Mappings.TryGetValue(portugueseName.Trim(), out var exact))
        {
            return exact;
        }

        // Try partial/contains match
        var lower = portugueseName.ToLowerInvariant();
        foreach (var (partial, englishName, category) in PartialMappings)
        {
            if (lower.Contains(partial, StringComparison.OrdinalIgnoreCase))
            {
                return (englishName, category);
            }
        }

        // Fallback: capitalize Portuguese name, category Other
        var capitalized = char.ToUpper(portugueseName[0]) + portugueseName.Substring(1);
        return (capitalized, IngredientCategory.Other);
    }
}
