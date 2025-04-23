using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Text;
using System;

public class NPCChatbotLia : MonoBehaviour
{
    public NPCChatbotRevi reviChatbot; // Reference to Revi
    public AudioSource audioSource;

    // Dialogflow settings
    public string projectId = "npc-chatbot-448406";
    public string agentId = "c73324d9-d166-4405-8cd7-3d9a03f9a2b1";
    public string location = "global";
    public string languageCode = "en";
    public string accessToken; // OAuth 2.0 token for authentication
    private string sessionId;

    private bool isSpeaking = false;
    private string lastResponse;

    void Start()
    {
        sessionId = Guid.NewGuid().ToString();
        if (reviChatbot == null) Debug.LogError("Revi Chatbot reference is missing!");
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        StartCoroutine(StartConversation());
    }

    private IEnumerator StartConversation()
    {
        // Lia starts the conversation
        string firstMessage = "When is it Friday";
        yield return StartCoroutine(Speak(firstMessage));
        lastResponse = firstMessage;

        // Wait for Lia to finish speaking
        yield return new WaitUntil(() => !isSpeaking);

        // Send Lia's first sentence to Revi's Dialogflow
        yield return StartCoroutine(TriggerReviResponse(lastResponse));

        // Wait for Revi to finish speaking
        yield return new WaitUntil(() => !reviChatbot.IsSpeaking());

        // Start the conversation loop
        while (true)
        {
            // Send Revi's response to Lia's Dialogflow
            yield return StartCoroutine(RespondUsingDialogflow(reviChatbot.GetLastResponse()));
            yield return new WaitUntil(() => !isSpeaking);

            // Send Lia's response to Revi's Dialogflow
            yield return StartCoroutine(TriggerReviResponse(lastResponse));
            yield return new WaitUntil(() => !reviChatbot.IsSpeaking());
        }
    }

    private IEnumerator TriggerReviResponse(string prompt)
    {
        if (string.IsNullOrEmpty(prompt)) Debug.LogError("Lia's prompt to Revi is null or empty!");
        yield return reviChatbot.StartCoroutine(reviChatbot.RespondUsingDialogflow(prompt));
    }

    private IEnumerator Speak(string text)
    {
        Debug.Log("Lia: " + text); // Keep this log for debugging
        isSpeaking = true;
        yield return StartCoroutine(SynthesizeSpeech(text));
        isSpeaking = false;
    }

    private IEnumerator RespondUsingDialogflow(string userQuery)
    {
        string url = $"https://dialogflow.googleapis.com/v3/projects/{projectId}/locations/{location}/agents/{agentId}/sessions/{sessionId}:detectIntent";

        // Create JSON payload
        JObject jsonRequest = new JObject
        {
            ["queryInput"] = new JObject
            {
                ["languageCode"] = languageCode,
                ["text"] = new JObject { ["text"] = userQuery }
            }
        };

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest.ToString());
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Add headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            request.SetRequestHeader("x-goog-user-project", projectId);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Dialogflow Error: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                JObject responseObj = JObject.Parse(jsonResponse);
                string botResponse = responseObj["queryResult"]["responseMessages"][0]["text"]["text"][0]?.ToString();
                lastResponse = botResponse; // Update Lia's last response
                yield return StartCoroutine(Speak(botResponse));
            }
        }
    }

    private IEnumerator SynthesizeSpeech(string text)
    {
        string url = "https://texttospeech.googleapis.com/v1/text:synthesize";
        JObject jsonRequest = new JObject
        {
            ["input"] = new JObject { ["text"] = text },
            ["voice"] = new JObject { ["languageCode"] = "en-US", ["name"] = "en-US-Wavenet-D" },
            ["audioConfig"] = new JObject { ["audioEncoding"] = "MP3" }
        };

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest.ToString());
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Add headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            request.SetRequestHeader("x-goog-user-project", projectId);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("TTS Error: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                JObject responseObj = JObject.Parse(jsonResponse);
                string audioContent = responseObj["audioContent"]?.ToString();
                byte[] audioBytes = Convert.FromBase64String(audioContent);

                yield return StartCoroutine(PlayAudioClip(audioBytes)); // Play the TTS response
            }
        }
    }

    private IEnumerator PlayAudioClip(byte[] audioBytes)
    {
        string path = Application.persistentDataPath + "/tts_lia_audio.mp3";
        System.IO.File.WriteAllBytes(path, audioBytes);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Audio Load Error: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();

                // Wait until the audio finishes playing
                yield return new WaitWhile(() => audioSource.isPlaying);
            }
        }
    }

    public bool IsSpeaking() => isSpeaking;
    public string GetLastResponse() => lastResponse;
}