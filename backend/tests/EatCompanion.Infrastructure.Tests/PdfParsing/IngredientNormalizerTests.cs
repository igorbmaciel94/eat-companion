using EatCompanion.Domain.Enums;
using EatCompanion.Infrastructure.PdfParsing;

namespace EatCompanion.Infrastructure.Tests.PdfParsing;

public class IngredientNormalizerTests
{
    private readonly IngredientNormalizer _normalizer = new();

    [Theory]
    [InlineData("pão", "Bread", IngredientCategory.Grains)]
    [InlineData("arroz", "Rice", IngredientCategory.Grains)]
    [InlineData("massa", "Pasta", IngredientCategory.Grains)]
    [InlineData("ovo", "Egg", IngredientCategory.Protein)]
    [InlineData("ovos", "Eggs", IngredientCategory.Protein)]
    [InlineData("fruta", "Fruit", IngredientCategory.Produce)]
    [InlineData("café", "Coffee", IngredientCategory.Other)]
    [InlineData("azeite", "Olive Oil", IngredientCategory.OilsAndCondiments)]
    public void Normalize_KnownMappings_ReturnsCorrectValues(
        string portugueseName, string expectedEnglish, IngredientCategory expectedCategory)
    {
        var (englishName, category) = _normalizer.Normalize(portugueseName);

        Assert.Equal(expectedEnglish, englishName);
        Assert.Equal(expectedCategory, category);
    }

    [Theory]
    [InlineData("queijo flamengo", "Edam Cheese", IngredientCategory.Dairy)]
    [InlineData("queijo cottage", "Cottage Cheese", IngredientCategory.Dairy)]
    [InlineData("iogurte sólido ou líquido proteico", "Protein Yogurt", IngredientCategory.Dairy)]
    [InlineData("flocos de aveia", "Oat Flakes", IngredientCategory.Grains)]
    public void Normalize_MultiWordMappings_ReturnsCorrectValues(
        string portugueseName, string expectedEnglish, IngredientCategory expectedCategory)
    {
        var (englishName, category) = _normalizer.Normalize(portugueseName);

        Assert.Equal(expectedEnglish, englishName);
        Assert.Equal(expectedCategory, category);
    }

    [Fact]
    public void Normalize_FuzzyMatch_IogurteVariant_ReturnsDairy()
    {
        var (_, category) = _normalizer.Normalize("iogurte qualquer tipo");

        Assert.Equal(IngredientCategory.Dairy, category);
    }

    [Fact]
    public void Normalize_FuzzyMatch_QueijoCreme_ReturnsDairy()
    {
        var (_, category) = _normalizer.Normalize("queijo creme especial");

        Assert.Equal(IngredientCategory.Dairy, category);
    }

    [Fact]
    public void Normalize_UnknownIngredient_FallsBackToCapitalizedPortuguese()
    {
        var (englishName, category) = _normalizer.Normalize("temperos especiais");

        Assert.Equal("Temperos especiais", englishName);
        Assert.Equal(IngredientCategory.Other, category);
    }

    [Theory]
    [InlineData("Pão")]
    [InlineData("PÃO")]
    [InlineData("pão")]
    public void Normalize_CaseInsensitive_MatchesRegardlessOfCase(string input)
    {
        var (englishName, _) = _normalizer.Normalize(input);

        Assert.Equal("Bread", englishName);
    }

    [Fact]
    public void Normalize_PartialMatch_BolachasVariant_ReturnsGrains()
    {
        var (_, category) = _normalizer.Normalize("bolachas integrais");

        Assert.Equal(IngredientCategory.Grains, category);
    }

    [Fact]
    public void Normalize_PartialMatch_TortitasVariant_ReturnsGrains()
    {
        var (_, category) = _normalizer.Normalize("tortitas de milho e arroz");

        Assert.Equal(IngredientCategory.Grains, category);
    }
}
