using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public GameObject QuestionScreen;
    public Slider killsSlider;
    public TextMeshProUGUI QuestionText;

    [Header("Player Settings")]
    [Tooltip("Drag your Player object or FPS Controller script component here")]
    public MonoBehaviour fpsControllerScript;

    [Header("Lerp Settings")]
    public float lerpSpeed = 5f;

    [Header("Local AI Settings")]
    public string localModelName = "llama3";
    private string localOllamaUrl = "http://localhost:11434/api/generate";

    private int kills = 0;
    private bool currentCorrectAnswer;
    private bool awaitingAIResponse = false;

    [System.Serializable]
    public class TriviaQuestion
    {
        public string question;
        public bool answer;
    }

    [System.Serializable]
    private class OllamaRequestData
    {
        public string model;
        public string prompt;
        public bool stream;
        public string format;
    }

    [System.Serializable]
    private class OllamaResponseData
    {
        public string response;
    }

    void Start()
    {
        killsSlider.value = 0f;
        QuestionScreen.SetActive(false);
    }

    void Update()
    {
        float targetValue = (float)kills / 3f;
        killsSlider.value = Mathf.Lerp(killsSlider.value, targetValue, Time.deltaTime * lerpSpeed);

        if (kills >= 3 && killsSlider.value >= 0.99f && !awaitingAIResponse)
        {
            killsSlider.value = 1f;
            GiveQuestion();
            kills = 0;
        }
    }

    public void AddKill()
    {
        kills++;
    }

    void GiveQuestion()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;

        if (fpsControllerScript != null)
        {
            fpsControllerScript.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        QuestionScreen.SetActive(true);

        string currentPlanet = SceneManager.GetActiveScene().name.Replace(" Terrain", "");
        StartCoroutine(FetchAIQuestion(currentPlanet));
    }

    IEnumerator FetchAIQuestion(string planet)
    {
        awaitingAIResponse = true;
        QuestionText.text = "Generating question via local AI...";

        string systemPrompt = $"Generate one unique, accurate True or False trivia question about the planet {planet}. " +
                             "The question must be basic knowledge. You must respond ONLY using this raw JSON structure: " +
                             "{\"question\": \"Your question here\", \"answer\": true} or {\"question\": \"Your question here\", \"answer\": false}. " +
                             "Do not include any introductory remarks, markdown formatting, or trailing text.";

        OllamaRequestData requestBody = new OllamaRequestData
        {
            model = localModelName,
            prompt = systemPrompt,
            stream = false,
            format = "json"
        };

        string jsonPayload = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest webRequest = new UnityWebRequest(localOllamaUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                OllamaResponseData outerContent = JsonUtility.FromJson<OllamaResponseData>(webRequest.downloadHandler.text);
                TriviaQuestion generatedQuestion = JsonUtility.FromJson<TriviaQuestion>(outerContent.response);

                QuestionText.text = generatedQuestion.question;
                currentCorrectAnswer = generatedQuestion.answer;
            }
            else
            {
                QuestionText.text = "Failed to connect to local AI engine.";
                currentCorrectAnswer = true;
            }
        }
        awaitingAIResponse = false;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;

        if (fpsControllerScript != null)
        {
            fpsControllerScript.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        QuestionScreen.SetActive(false);
    }

    public void TrueClicked()
    {
        EvaluateAnswer(true);
    }

    public void FalseClicked()
    {
        EvaluateAnswer(false);
    }

    private void EvaluateAnswer(bool playerChoice)
    {
        if (playerChoice == currentCorrectAnswer)
        {
            Debug.Log("Correct Answer!");
        }
        else
        {
            Debug.Log("Wrong Answer!");
        }
        ResumeGame();
    }
}