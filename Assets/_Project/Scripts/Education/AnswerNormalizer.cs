using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TimeMachine.Education
{
    public static class AnswerNormalizer
    {
        private static readonly Dictionary<string, int> NumberWords = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            { "ноль", 0 }, { "нуль", 0 },
            { "один", 1 }, { "одна", 1 }, { "одно", 1 },
            { "два", 2 }, { "две", 2 },
            { "три", 3 }, { "четыре", 4 }, { "пять", 5 },
            { "шесть", 6 }, { "семь", 7 }, { "восемь", 8 }, { "девять", 9 },
            { "десять", 10 }, { "одиннадцать", 11 }, { "двенадцать", 12 },
            { "тринадцать", 13 }, { "четырнадцать", 14 }, { "пятнадцать", 15 },
            { "шестнадцать", 16 }, { "семнадцать", 17 }, { "восемнадцать", 18 }, { "девятнадцать", 19 },
            { "двадцать", 20 }, { "тридцать", 30 }, { "сорок", 40 },
            { "пятьдесят", 50 }, { "шестьдесят", 60 }, { "семьдесят", 70 },
            { "восемьдесят", 80 }, { "девяносто", 90 },
            { "сто", 100 }, { "двести", 200 }, { "триста", 300 }, { "четыреста", 400 },
            { "пятьсот", 500 }, { "шестьсот", 600 }, { "семьсот", 700 }, { "восемьсот", 800 }, { "девятьсот", 900 },
            { "тысяча", 1000 }
        };

        private static readonly string[] Units =
        {
            "кв. дм", "кв дм", "дм²", "дм2",
            "кв. м", "кв м", "м²", "м2",
            "кв. см", "кв см", "см²", "см2",
            "м/мин", "км/ч",
            "метров", "метра", "метр", "м",
            "сантиметров", "сантиметра", "сантиметр", "см",
            "дециметров", "дециметра", "дециметр", "дм",
            "километров", "километра", "километр", "км",
            "килограммов", "килограмма", "килограмм", "кг",
            "граммов", "грамма", "грамм", "г",
            "оборотов", "оборота", "оборот", "об",
            "коробок", "коробки", "коробка",
            "минут", "минуты", "минута", "мин",
            "часов", "часа", "час", "ч",
            "лет", "года", "год",
            "раз", "раза", "шт"
        };

        private static readonly Regex SpacesRegex = new Regex(@"\s+", RegexOptions.Compiled);

        public static string Normalize(string rawInput, string answerType = "text")
        {
            if (string.IsNullOrWhiteSpace(rawInput)) return string.Empty;

            if (answerType == "stress")
            {
                return rawInput.Trim().Replace("́", string.Empty).Replace('ё', 'е').Replace('Ё', 'Е');
            }

            string lower = rawInput.Trim().ToLowerInvariant().Replace('ё', 'е');
            lower = SpacesRegex.Replace(lower, " ");

            if (answerType == "sequence")
            {
                string[] seqParts = lower.Split(new[] { ',', ';', '-', '>', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return string.Join(",", seqParts);
            }

            if (answerType == "wordAssembly")
            {
                return lower.Replace("-", string.Empty).Replace(" ", string.Empty);
            }

            if (answerType == "numberWithUnit")
            {
                string stripped = StripUnit(lower).Replace(',', '.').Trim();
                if (stripped == "½" || stripped == "1/2") return "0.5";
                if (stripped == "¼" || stripped == "1/4") return "0.25";
                if (stripped == "¾" || stripped == "3/4") return "0.75";

                string compact = stripped.Replace(" ", string.Empty);
                if (double.TryParse(compact, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
                {
                    return num.ToString(CultureInfo.InvariantCulture);
                }

                string[] words = stripped.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int sum = 0;
                bool allWords = words.Length > 0;
                for (int i = 0; i < words.Length; i++)
                {
                    if (NumberWords.TryGetValue(words[i], out int val)) sum += val;
                    else { allWords = false; break; }
                }
                if (allWords) return sum.ToString(CultureInfo.InvariantCulture);

                return stripped;
            }

            return lower;
        }

        private static string StripUnit(string text)
        {
            string res = text.TrimEnd('.', ' ');
            for (int i = 0; i < Units.Length; i++)
            {
                string u = Units[i];
                if (res.EndsWith(u, StringComparison.Ordinal))
                {
                    string prefix = res.Substring(0, res.Length - u.Length).Trim();
                    if (!string.IsNullOrEmpty(prefix)) return prefix;
                }
            }
            return res;
        }
    }
}
