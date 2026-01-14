using System.Globalization;
using System.Text;

namespace Extensions
{
    public static class TmpExtensions
    {
        public static string RemoveDiacritics(string text) 
        {
            if (text.IsNullOrEmpty())
                return string.Empty;
            
            // Разделяет буквы и их акценты (например, 'á' станет 'a' + '´')
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                // Оставляем только базовые буквы, игнорируя символы акцентов (NonSpacingMark)
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            
            var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // Дополнительная ручная замена для символов, которые не убираются нормализацией
            var clearedText = result.Replace("¿", "").Replace("¡", "").Replace("ñ", "n").Replace("Ñ", "N");
            
            return clearedText.IsNullOrEmpty() ? text : clearedText;
        }
    }
}