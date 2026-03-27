using System.Text;
using UglyToad.PdfPig;

namespace EatCompanion.Infrastructure.PdfParsing;

public class PdfTextExtractor
{
    public IReadOnlyList<string> ExtractPages(Stream pdfStream)
    {
        var pages = new List<string>();

        using var document = PdfDocument.Open(pdfStream);

        foreach (var page in document.GetPages())
        {
            // Use the basic word extraction available in this version
            var words = page.GetWords();
            var sb = new StringBuilder();
            double? lastY = null;

            foreach (var word in words.OrderBy(w => -w.BoundingBox.Top).ThenBy(w => w.BoundingBox.Left))
            {
                var currentY = Math.Round(word.BoundingBox.Top, 1);

                if (lastY.HasValue && Math.Abs(currentY - lastY.Value) > 2)
                {
                    sb.AppendLine();
                }
                else if (sb.Length > 0 && !sb.ToString().EndsWith(' ') && !sb.ToString().EndsWith('\n'))
                {
                    sb.Append(' ');
                }

                sb.Append(word.Text);
                lastY = currentY;
            }

            pages.Add(sb.ToString());
        }

        return pages;
    }
}
