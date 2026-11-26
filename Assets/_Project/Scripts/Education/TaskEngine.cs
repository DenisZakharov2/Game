using System;
using System.Collections.Generic;
using UnityEngine;
using TimeMachine.Data;

namespace TimeMachine.Education
{
    public class TaskEngine : MonoBehaviour
    {
        public static TaskEngine Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool ValidateAnswer(string taskKey, string userAnswer, string expectedAnswer)
        {
            bool isCorrect = AnswerNormalizer.CompareAnswers(userAnswer, expectedAnswer);
            
            RecordAttempt(taskKey, userAnswer, isCorrect);

            if (isCorrect && GameState.Instance != null)
            {
                GameState.Instance.AddXP(10);
            }

            return isCorrect;
        }

        private void RecordAttempt(string taskKey, string answer, bool isCorrect)
        {
            if (GameState.Instance == null || GameState.Instance.CurrentData == null) return;

            string record = $"{DateTime.UtcNow:o}|{taskKey}|{answer}|{isCorrect}";
            GameState.Instance.CurrentData.attempts.Add(record);
            GameState.Instance.SaveCurrentState();
            Debug.Log($"[TimeMachine] [TaskEngine] Записана попытка решения задачи [{taskKey}]: {(isCorrect ? "Успешно" : "Ошибка")}");
        }
    }
}
