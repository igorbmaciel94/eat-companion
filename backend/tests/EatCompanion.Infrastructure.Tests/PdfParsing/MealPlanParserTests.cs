using EatCompanion.Domain.Enums;
using EatCompanion.Infrastructure.PdfParsing;

namespace EatCompanion.Infrastructure.Tests.PdfParsing;

public class MealPlanParserTests
{
    private readonly MealPlanParser _parser = new();

    private static readonly string BreakfastText = @"Pequeno-almoço
Crepioca (1 ovo + 2 colheres de sopa de tapioca + temperos a gosto - é só bater
tudo com um garfo e levar a uma frigideira bem quente de um lado e do outro) + 2
fatias de queijo flamengo - 50% gordura + 1 iogurte sólido ou líquido proteico (entre
15 a 20g proteína) + 1 porção de fruta + café sem açúcar
OU
Café sem açúcar + 60g de pão + 2 ovos mexidos/estrelados/omelete sem gordura
adicionada + 1 fatia de queijo flamengo -50% gordura ou queijo creme light
(camada fina) ou 1 queijinho vaca que ri light + 1 porção de fruta
OU
Café sem açúcar + 60g de pão + 2 ovos mexidos/estrelados/omelete sem gordura
adicionada + 30g de queijo cottage + 1 porção de fruta
OU
Bolo da caneca de chocolate (receita no fim do plano) + 1 iogurte sólido ou líquido
proteico (15 a 20g proteína) + café sem açúcar adicionado";

    private static readonly string LunchText = @"Almoço
Salada/ legumes a gosto e/ou sopa
150g de arroz/massa/batata cozinhados
150g de carne/peixe cozinhados ou 3 ovos
Utilizar pouco azeite para cozinhar/temperar
1 porção de fruta";

    private static readonly string SnackText = @"Lanche da tarde
1 pudim proteico (20g proteína) + 1 porção de fruta + 2 bolachas marinheiras ou 2
tortitas de milho/arroz
OU
1 iogurte sólido ou líquido proteico (15 a 20g proteína) + 1 porção de fruta + 20g de
flocos de aveia ou 20g de granola 0% açúcar ou 20g de cereais 0% açúcar
OU
1 iogurte sólido ou líquido magro + 60g de pão + 30g de queijo cottage + 1 porção
de fruta
OU
Bolo da caneca de chocolate (receita no fim do plano) + 1 iogurte sólido ou líquido
magro";

    private static readonly string DinnerText = @"Jantar
Salada/ legumes a gosto e/ou sopa
150g de arroz/massa/batata cozinhados
150g de carne/peixe cozinhados ou 3 ovos
Utilizar pouco azeite para cozinhar/temperar
Opção 1

Jantar
Salada/ legumes a gosto e/ou sopa
70g de pão ou 1 tortilha grande de wrap
150g de carne/peixe cozinhados ou 3 ovos
Utilizar pouco azeite para cozinhar/temperar
Opção 2";

    private static readonly string RecommendationsText = @"Recomendações
1- Beber pelo menos 2L de água durante o dia
150g de peixe: corresponde a 1 posta de peixe media ou 1 bife médio
150g de arroz cozido: corresponde a 8 colheres de sopa bem rasas";

    [Fact]
    public void Parse_BreakfastSection_FindsFourOptions()
    {
        var pages = new List<string> { BreakfastText + "\n" + RecommendationsText };
        var result = _parser.Parse(pages);

        var breakfast = result.Meals.FirstOrDefault(m => m.Type == MealType.Breakfast);
        Assert.NotNull(breakfast);
        Assert.Equal(4, breakfast.Options.Count);
    }

    [Fact]
    public void Parse_LunchSection_FindsOneOptionWithIngredients()
    {
        var pages = new List<string> { LunchText + "\n" + RecommendationsText };
        var result = _parser.Parse(pages);

        var lunch = result.Meals.FirstOrDefault(m => m.Type == MealType.Lunch);
        Assert.NotNull(lunch);
        Assert.Single(lunch.Options);
        Assert.True(lunch.Options[0].Ingredients.Count >= 3,
            $"Expected at least 3 ingredients but found {lunch.Options[0].Ingredients.Count}");
    }

    [Fact]
    public void Parse_DinnerSection_FindsTwoOptions()
    {
        var pages = new List<string> { DinnerText + "\n" + RecommendationsText };
        var result = _parser.Parse(pages);

        var dinner = result.Meals.FirstOrDefault(m => m.Type == MealType.Dinner);
        Assert.NotNull(dinner);
        Assert.Equal(2, dinner.Options.Count);
    }

    [Fact]
    public void Parse_SnackSection_FindsFourOptions()
    {
        var pages = new List<string> { SnackText + "\n" + RecommendationsText };
        var result = _parser.Parse(pages);

        var snack = result.Meals.FirstOrDefault(m => m.Type == MealType.AfternoonSnack);
        Assert.NotNull(snack);
        Assert.Equal(4, snack.Options.Count);
    }

    [Fact]
    public void ParseIngredients_GramsQuantity_ExtractsCorrectly()
    {
        var text = "150g de arroz/massa/batata cozinhados";
        var ingredients = _parser.ParseIngredients(text);

        var arroz = ingredients.FirstOrDefault(i => i.NamePt.Contains("arroz"));
        Assert.NotNull(arroz);
        Assert.Equal(150m, arroz.Amount);
        Assert.Equal(UnitOfMeasure.Grams, arroz.Unit);
    }

    [Fact]
    public void ParseIngredients_TablespoonQuantity_ExtractsCorrectly()
    {
        var text = "2 colheres de sopa de tapioca";
        var ingredients = _parser.ParseIngredients(text);

        var tapioca = ingredients.FirstOrDefault(i => i.NamePt.Contains("tapioca"));
        Assert.NotNull(tapioca);
        Assert.Equal(2m, tapioca.Amount);
        Assert.Equal(UnitOfMeasure.Tablespoon, tapioca.Unit);
    }

    [Fact]
    public void ParseIngredients_SliceQuantity_ExtractsCorrectly()
    {
        var text = "2 fatias de queijo flamengo - 50% gordura";
        var ingredients = _parser.ParseIngredients(text);

        var queijo = ingredients.FirstOrDefault(i => i.NamePt.Contains("queijo"));
        Assert.NotNull(queijo);
        Assert.Equal(2m, queijo.Amount);
        Assert.Equal(UnitOfMeasure.Slice, queijo.Unit);
    }

    [Fact]
    public void ParseIngredients_Porcao_ExtractsCorrectly()
    {
        var text = "1 porção de fruta";
        var ingredients = _parser.ParseIngredients(text);

        var fruta = ingredients.FirstOrDefault(i => i.NamePt.Contains("fruta"));
        Assert.NotNull(fruta);
        Assert.Equal(1m, fruta.Amount);
        Assert.Equal(UnitOfMeasure.Units, fruta.Unit);
    }

    [Fact]
    public void ParseIngredients_UnitQuantity_Eggs()
    {
        var text = "3 ovos";
        var ingredients = _parser.ParseIngredients(text);

        var ovos = ingredients.FirstOrDefault(i => i.NamePt.Contains("ovo"));
        Assert.NotNull(ovos);
        Assert.Equal(3m, ovos.Amount);
        Assert.Equal(UnitOfMeasure.Units, ovos.Unit);
    }

    [Fact]
    public void ParseIngredients_CafeSemAcucar_FoundAsIngredient()
    {
        var text = "café sem açúcar + 60g de pão";
        var ingredients = _parser.ParseIngredients(text);

        Assert.Contains(ingredients, i => i.NamePt.Contains("café"));
    }

    [Fact]
    public void Parse_FullMealPlan_AllSectionsFound()
    {
        var fullText = string.Join("\n", BreakfastText, LunchText, SnackText, DinnerText, RecommendationsText);
        var pages = new List<string> { fullText };
        var result = _parser.Parse(pages);

        Assert.Contains(result.Meals, m => m.Type == MealType.Breakfast);
        Assert.Contains(result.Meals, m => m.Type == MealType.Lunch);
        Assert.Contains(result.Meals, m => m.Type == MealType.AfternoonSnack);
        Assert.Contains(result.Meals, m => m.Type == MealType.Dinner);
        Assert.Equal(4, result.Meals.Count);
    }

    [Fact]
    public void Parse_RecommendationsSection_NotIncludedAsMeal()
    {
        var pages = new List<string> { BreakfastText + "\n" + RecommendationsText };
        var result = _parser.Parse(pages);

        Assert.DoesNotContain(result.Meals, m =>
            m.Type != MealType.Breakfast &&
            m.Type != MealType.Lunch &&
            m.Type != MealType.AfternoonSnack &&
            m.Type != MealType.Dinner);
    }

    [Fact]
    public void Parse_OuSeparator_SplitsOptions()
    {
        var text = @"Lanche da tarde
option 1 text
OU
option 2 text
OU
option 3 text";

        var pages = new List<string> { text };
        var result = _parser.Parse(pages);

        var snack = result.Meals.FirstOrDefault(m => m.Type == MealType.AfternoonSnack);
        Assert.NotNull(snack);
        Assert.Equal(3, snack.Options.Count);
    }

    [Fact]
    public void Parse_HandlesExtraWhitespace()
    {
        var text = @"  Pequeno-almoço
  1 ovo + café sem açúcar
  OU
  60g de pão + café sem açúcar
Recomendações
some text";

        var pages = new List<string> { text };
        var result = _parser.Parse(pages);

        var breakfast = result.Meals.FirstOrDefault(m => m.Type == MealType.Breakfast);
        Assert.NotNull(breakfast);
        Assert.Equal(2, breakfast.Options.Count);
    }

    [Fact]
    public void Parse_MissingSections_HandlesGracefully()
    {
        var pages = new List<string> { "Some random text with no meal headers" };
        var result = _parser.Parse(pages);

        Assert.Empty(result.Meals);
    }
}
