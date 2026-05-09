using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GeminiManager : MonoBehaviour
{
    public GameObject chatPanel;
    public InputField inputField;
    public Transform contentParent; 
    public GameObject chatTextPrefab; 

    private string geminiApiKey = "";
    private string apiUrl;

    private void Start()
    {
        apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={geminiApiKey}";
        chatPanel.SetActive(false);
    }

private void Update()
{
    if (SceneManager.GetActiveScene().name == "Scene01_HealingZone")
    {
        bool isTyping = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == inputField.gameObject;

        if (!isTyping && Input.GetKeyDown(KeyCode.T))
        {
            bool isActive = !chatPanel.activeSelf;
            chatPanel.SetActive(isActive);

            if (isActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                inputField.ActivateInputField();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

            if (chatPanel.activeSelf && Input.GetKeyDown(KeyCode.Return))
            {
                if (!string.IsNullOrEmpty(inputField.text))
                {
                    string userText = inputField.text;
                    inputField.text = "";
                    inputField.ActivateInputField();

                    AddChatMessage($"나: {userText}", true); // ⭐ 즉시 스크롤
                    StartCoroutine(SendMessageToGemini(userText));
                }
            }


        }
    }


    private void AddChatMessage(string message, bool immediateScroll = false)
    {
        GameObject newTextObj = Instantiate(chatTextPrefab, contentParent);
        Text newText = newTextObj.GetComponent<Text>();
        newText.text = message;

        if (immediateScroll)
        {
            ScrollRect scrollRect = contentParent.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
        else
        {
            StartCoroutine(ScrollToBottomSmooth());
        }
    }




    private IEnumerator ScrollToBottomSmooth()
    {
        ScrollRect scrollRect = contentParent.GetComponentInParent<ScrollRect>();
        if (scrollRect == null) yield break;

        yield return null; /

        float duration = 0.5f; 
        float elapsed = 0f;
        float startPosition = scrollRect.verticalNormalizedPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPosition, 0f, t);
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = 0f; 
    }


    private IEnumerator SendMessageToGemini(string userInput)
    {
        string requestBody = CreateRequestJson(userInput);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            string aiMessage = ParseGeminiResponse(responseText);

            if (string.IsNullOrEmpty(aiMessage))
                aiMessage = "[AI 응답 없음]";

            AddChatMessage($"AI: {aiMessage}");
        }

    }


    private string CreateRequestJson(string userInput)
    {
        string prompt = $"{userInput}\n\n(짧게 답변해주세요)";
        return $"{{\"contents\":[{{\"parts\":[{{\"text\":\"{prompt}\"}}]}}]}}";
    }


    private string ParseGeminiResponse(string json)
    {
        GeminiResponse parsed = JsonUtility.FromJson<GeminiResponse>(json);

        if (parsed != null && parsed.candidates.Length > 0)
        {
            return parsed.candidates[0].content.parts[0].text;
        }
        return "";
    }

    [System.Serializable]
    private class GeminiResponse
    {
        public Candidate[] candidates;
    }

    [System.Serializable]
    private class Candidate
    {
        public Content content;
    }

    [System.Serializable]
    private class Content
    {
        public Part[] parts;
    }

    [System.Serializable]
    private class Part
    {
        public string text;
    }
}

