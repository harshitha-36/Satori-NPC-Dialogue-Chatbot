using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Text;
using System;

public class NPCChatbotRevi : MonoBehaviour
{
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
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public IEnumerator RespondUsingDialogflow(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
        {
            Debug.LogError("Revi received an empty or null prompt!");
            yield break; // Exit the coroutine if the input is invalid
        }

        Debug.Log("Revi's input: " + prompt); // Keep this log for debugging

        string url = $"https://dialogflow.googleapis.com/v3/projects/{projectId}/locations/{location}/agents/{agentId}/sessions/{sessionId}:detectIntent";

        // Create JSON payload
        JObject jsonRequest = new JObject
        {
            ["queryInput"] = new JObject
            {
                ["languageCode"] = languageCode,
                ["text"] = new JObject { ["text"] = prompt }
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
                lastResponse = botResponse; // Update Revi's last response
                yield return StartCoroutine(Speak(botResponse));
            }
        }
    }

    private IEnumerator Speak(string text)
    {
        Debug.Log("Revi: " + text); // Keep this log for debugging
        isSpeaking = true;
        yield return StartCoroutine(SynthesizeSpeech(text));
        isSpeaking = false;
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
        string path = Application.persistentDataPath + "/tts_revi_audio.mp3";
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