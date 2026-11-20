using System;
using System.Collections.Generic;
using TimeMachine.Data;

namespace TimeMachine.Education
{
    public class GeneratedTaskInstance
    {
        public string templateId;
        public string subject;
        public string fgosTopic;
        public string questionText;
        public string correctAnswer;
    }

    public class TaskGenerator
    {
        private readonly Dictionary<string, int> _lastContextByTemplate = new Dictionary<string, int>(StringComparer.Ordinal);

        public GeneratedTaskInstance GenerateFromTemplate(TemplateData template, int seed)
        {
            if (template == null) return null;

            var rng = new Random(seed);
            int ctxIndex = 0;
            if (template.contexts != null && template.contexts.Length > 0)
            {
                ctxIndex = rng.Next(0, template.contexts.Length);
                if (template.contexts.Length > 1 &&
                    _lastContextByTemplate.TryGetValue(template.id, out int prev) &&
                    prev == ctxIndex)
                {
                    ctxIndex = (ctxIndex + 1) % template.contexts.Length;
                }
                _lastContextByTemplate[template.id] = ctxIndex;
            }

            string rawCtx = (template.contexts != null && template.contexts.Length > 0)
                ? template.contexts[ctxIndex]
                : (template.fgosTopic ?? "Реши задачу 4 класса.");

            int a = rng.Next(3, 9);
            int b = rng.Next(4, 12);
            int t = rng.Next(3, 8);
            int v = rng.Next(40, 75);
            int s = v * t;
            int denom = rng.Next(3, 6);
            int total = denom * rng.Next(5, 14);
            int part = rng.Next(3, 7);
            int q = rng.Next(4, 9);
            int r = rng.Next(1, part);
            int totalRem = part * q + r;
            int kg = rng.Next(2, 6);
            int g = rng.Next(150, 850);
            int h = rng.Next(1, 4);
            int m = rng.Next(10, 50);
            int y1 = 1901;
            int y2 = 1954;
            int c = rng.Next(5, 25);

            string question = rawCtx
                .Replace("{S}", s.ToString())
                .Replace("{t}", t.ToString())
                .Replace("{v}", v.ToString())
                .Replace("{a}", a.ToString())
                .Replace("{b}", b.ToString())
                .Replace("{c}", c.ToString())
                .Replace("{denom}", denom.ToString())
                .Replace("{total}", (template.formula != null && template.formula.Contains("ост.") ? totalRem : total).ToString())
                .Replace("{part}", part.ToString())
                .Replace("{kg}", kg.ToString())
                .Replace("{g}", g.ToString())
                .Replace("{h}", h.ToString())
                .Replace("{m}", m.ToString())
                .Replace("{y1}", y1.ToString())
                .Replace("{y2}", y2.ToString());

            string answer = ComputeAnswer(template.formula, a, b, c, s, t, v, total, denom, q, r, kg, g, h, m, y1, y2);
            if (!string.IsNullOrWhiteSpace(template.unit) && !answer.Contains(template.unit))
            {
                answer = $"{answer} {template.unit}".Trim();
            }

            return new GeneratedTaskInstance
            {
                templateId = template.id,
                subject = template.subject,
                fgosTopic = template.fgosTopic,
                questionText = question,
                correctAnswer = answer
            };
        }

        private static string ComputeAnswer(
            string formula, int a, int b, int c, int s, int t, int v,
            int total, int denom, int q, int r, int kg, int g, int h, int m, int y1, int y2)
        {
            if (string.IsNullOrWhiteSpace(formula)) return "0";
            string f = formula.Trim();
            if (f == "S / t") return (s / t).ToString();
            if (f == "v * t") return (v * t).ToString();
            if (f == "a * b") return (a * b).ToString();
            if (f == "(a + b) * 2") return ((a + b) * 2).ToString();
            if (f == "total / denom") return (total / denom).ToString();
            if (f == "{q} (ост. {r})") return $"{q} (ост. {r})";
            if (f == "kg * 1000 + g") return (kg * 1000 + g).ToString();
            if (f == "h * 60 + m") return (h * 60 + m).ToString();
            if (f == "y2 - y1") return (y2 - y1).ToString();
            if (f == "a * b + c") return (a * b + c).ToString();
            return f;
        }
    }
}
