using System.Globalization;
using System.Text;

namespace Extensions
{
    public static class TmpExtensions
    {
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Нормализуем строку (разбиваем символы на базовую букву и ударение)
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                // Проверяем, является ли символ частью кириллицы
                // Диапазон кириллицы в Unicode: 0400–04FF
                bool isCyrillic = c >= '\u0400' && c <= '\u04FF';

                if (isCyrillic)
                {
                    // Если это кириллица (включая Й и Ё в разложенном виде), 
                    // мы просто оставляем символ как есть
                    stringBuilder.Append(c);
                }
                else
                {
                    // Для латиницы (испанского и т.д.) применяем фильтр диакритики
                    UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    {
                        stringBuilder.Append(c);
                    }
                }
            }

            // Возвращаем в исходную форму (склеиваем обратно И + кратка в Й)
            string result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        
            // Ручная замена специфических испанских знаков, которые не являются диакритикой
            return result.Replace("¿", "").Replace("¡", "");
        }
    }
}