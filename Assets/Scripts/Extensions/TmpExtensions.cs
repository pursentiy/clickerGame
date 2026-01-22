using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Extensions
{
    public static class TmpExtensions
    {
        // Словарь для жесткой замены, если нормализация FormD не сработала
        private static readonly Dictionary<char, char> SpanishFallbackMap = new Dictionary<char, char>
        {
            {'á', 'a'}, {'é', 'e'}, {'í', 'i'}, {'ó', 'o'}, {'ú', 'u'},
            {'Á', 'A'}, {'É', 'E'}, {'Í', 'I'}, {'Ó', 'O'}, {'Ú', 'U'},
            {'ñ', 'n'}, {'Ñ', 'N'}
        };

        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder stringBuilder = new StringBuilder();

            // 1. Сначала проверяем через словарь (быстрее и надежнее для испанского)
            foreach (char c in text)
            {
                if (SpanishFallbackMap.TryGetValue(c, out char replaced))
                {
                    stringBuilder.Append(replaced);
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            string processedText = stringBuilder.ToString();
            stringBuilder.Clear();

            // 2. Теперь применяем FormD для очистки всего остального (другие диакритики)
            string normalizedString = processedText.Normalize(NormalizationForm.FormD);

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
                else
                {
                    // Сохраняем только Й (U+0306) и Ё (U+0308)
                    if (c == '\u0306' || c == '\u0308')
                    {
                        stringBuilder.Append(c);
                    }
                }
            }

            string result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        
            return result.Replace("¿", "").Replace("¡", "");
        }
    }
}