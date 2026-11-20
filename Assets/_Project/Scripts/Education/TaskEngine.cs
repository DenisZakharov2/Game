using System;
using TimeMachine.Data;

namespace TimeMachine.Education
{
    public struct TaskEvaluationResult
    {
        public bool isCorrect;
        public string feedback;
        public string explanation;
    }

    public class TaskEngine
    {
        private readonly GameState _state;

        public TaskEngine(GameState state)
        {
            _state = state;
        }

        public TaskEvaluationResult Evaluate(TaskData task, string rawAnswer, int hintsUsed, bool lessonFreeMode)
        {
            if (task == null)
            {
                return new TaskEvaluationResult { isCorrect = false, feedback = "Задача не найдена.", explanation = string.Empty };
            }

            bool matched = lessonFreeMode || CheckAnswerMatch(task, rawAnswer);

            if (_state != null && _state.Data != null)
            {
                _state.Data.attempts.Add(new AttemptRecord
                {
                    taskId = task.id,
                    fgosTopic = task.fgosTopic ?? string.Empty,
                    subject = task.subject ?? string.Empty,
                    submittedAnswer = rawAnswer ?? string.Empty,
                    isCorrect = matched,
                    hintsUsed = hintsUsed,
                    lessonFreeMode = lessonFreeMode
                });
            }

            return new TaskEvaluationResult
            {
                isCorrect = matched,
                feedback = matched ? "Отлично! Ответ верный." : "Попробуй ещё раз или открой подсказку!",
                explanation = task.explanation ?? string.Empty
            };
        }

        private static bool CheckAnswerMatch(TaskData task, string rawAnswer)
        {
            if (string.IsNullOrWhiteSpace(rawAnswer)) return false;

            string mode = string.IsNullOrWhiteSpace(task.answerType) ? "text" : task.answerType;
            string normUser = AnswerNormalizer.Normalize(rawAnswer, mode);
            string normExpected = AnswerNormalizer.Normalize(task.correctAnswer, mode);

            if (!string.IsNullOrEmpty(normUser) && string.Equals(normUser, normExpected, StringComparison.Ordinal))
            {
                return true;
            }

            if (mode == "choice" && normUser.Length == 1 && normExpected.StartsWith(normUser + ")", StringComparison.Ordinal))
            {
                return true;
            }

            if (task.acceptedVariants != null)
            {
                for (int i = 0; i < task.acceptedVariants.Length; i++)
                {
                    string normVar = AnswerNormalizer.Normalize(task.acceptedVariants[i], mode);
                    if (!string.IsNullOrEmpty(normUser) && string.Equals(normUser, normVar, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
