using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows.Speech;
using Newtonsoft.Json.Linq;

public class DialogflowChatbotM : MonoBehaviour
{
    public string projectId = "npc-chatbot-448406";
    public string agentId = "4ce28c31-c841-474c-a6cd-8002f59e1c4b";
    public string location = "global"; // Location of your agent
    public string languageCode = "en"; // Default language
    public string accessToken; // Use OAuth 2.0 token for secure authentication
    private string sessionId;
    private DictationRecognizer dictationRecognizer;
    private bool isDictating = false;
    private bool isPlayerInTrigger = false;
    private AudioSource audioSource;

    // Added headers
    private const string ContentTypeHeader = "application/json";
    private const string XGoogUserProjectHeader = "x-goog-user-project";
    private const string AuthorizationHeader = "Authorization";

    void Start()
    {
        sessionId = GenerateSessionId();
        audioSource = GetComponent<AudioSource>();

        // Initialize DictationRecognizer
        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += OnDictationResult;
        dictationRecognizer.DictationComplete += OnDictationComplete;
        dictationRecognizer.DictationError += OnDictationError;

        dictationRecognizer.InitialSilenceTimeoutSeconds = 10;

        // Pre-initialize the recognizer to reduce delay
        dictationRecognizer.Start();
        dictationRecognizer.Stop(); // Stop it immediately to keep it ready
    }

    private string GenerateSessionId()
    {
        return Guid.NewGuid().ToString();
    }

    private void Update()
    {
        // Check if the player is in the trigger area and presses/releases the Spacebar
        if (isPlayerInTrigger)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartDictation();
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                StopDictation();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;

            // Stop dictation and NPC speech when the player leaves the trigger area
            if (isDictating)
            {
                StopDictation();
            }
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private void StartDictation()
    {
        if (isDictating)
        {
            return;
        }

        isDictating = true;
        dictationRecognizer.Start();
    }

    private void StopDictation()
    {
        if (!isDictating || dictationRecognizer.Status != SpeechSystemStatus.Running)
        {
            return;
        }

        dictationRecognizer.Stop();
        isDictating = false;
    }

    private void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        // Send the user's input to Dialogflow
        StartCoroutine(SendToDialogflow(text));
    }

    private void OnDictationComplete(DictationCompletionCause cause)
    {
        if (cause != DictationCompletionCause.Complete)
        {
            Debug.LogWarning("Dictation completed unsuccessfully: " + cause);
        }
        isDictating = false; // Ensure isDictating is reset
    }

    private void OnDictationError(string error, int hresult)
    {
        Debug.LogError("Dictation error: " + error);
        isDictating = false; // Ensure isDictating is reset
    }

    private IEnumerator SendToDialogflow(string userQuery)
    {
        string url = $"https://dialogflow.googleapis.com/v3/projects/{projectId}/locations/{location}/agents/{agentId}/sessions/{sessionId}:detectIntent";

        // Create JSON payload
        JObject jsonRequest = new JObject
        {
            ["queryInput"] = new JObject
            {
                ["languageCode"] = languageCode,
                ["text"] = new JObject
                {
                    ["text"] = userQuery,
                }
            }
        };

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest.ToString());
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Add headers
        request.SetRequestHeader("Content-Type", ContentTypeHeader);
        request.SetRequestHeader(AuthorizationHeader, $"Bearer {accessToken}");
        request.SetRequestHeader(XGoogUserProjectHeader, projectId);

        // Send request and yield
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Dialogflow Error: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            JObject responseObj = JObject.Parse(jsonResponse);
            string botResponse = responseObj["queryResult"]["responseMessages"][0]["text"]["text"][0]?.ToString();

            // Synthesize speech in the detected language
            StartCoroutine(SynthesizeSpeech(botResponse, languageCode));
        }
    }

    private IEnumerator SynthesizeSpeech(string text, string languageCode)
    {
        string voiceName = GetVoiceNameForLanguage(languageCode);

        // Create JSON request body for Google Text-to-Speech
        JObject jsonRequest = new JObject
        {
            ["input"] = new JObject { ["text"] = text },
            ["voice"] = new JObject { ["languageCode"] = languageCode, ["name"] = voiceName },
            ["audioConfig"] = new JObject { ["audioEncoding"] = "MP3" }
        };

        string url = "https://texttospeech.googleapis.com/v1/text:synthesize";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest.ToString());
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Add headers
        request.SetRequestHeader("Content-Type", ContentTypeHeader);
        request.SetRequestHeader(AuthorizationHeader, $"Bearer {accessToken}");
        request.SetRequestHeader(XGoogUserProjectHeader, projectId);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Text-to-Speech Error: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            JObject responseObj = JObject.Parse(jsonResponse);
            string audioContent = responseObj["audioContent"]?.ToString();
            byte[] audioBytes = Convert.FromBase64String(audioContent);

            PlayAudioClip(audioBytes); // Play the TTS response
        }
    }

    private string GetVoiceNameForLanguage(string languageCode)
    {
        string voiceName;
        switch (languageCode)
        {
            case "en":
                voiceName = "te-IN-Standard-B"; // English (US)
                break;
            case "hi":
                voiceName = "hi-IN-Standard-C"; // Hindi (India)
                break;
            case "te":
                voiceName = "te-IN-Standard-C"; // Telugu (India)
                break;
            case "ta":
                voiceName = "ta-IN-Standard-A"; // Tamil (India)
                break;
            default:
                voiceName = "en-US-Standard-C"; // Default to English
                break;
        }

        return voiceName;
    }

    private void PlayAudioClip(byte[] audioBytes)
    {
        string path = Application.persistentDataPath + "/tts_audio.mp3";
        System.IO.File.WriteAllBytes(path, audioBytes);

        StartCoroutine(LoadAndPlayAudio(path));
    }

    private IEnumerator LoadAndPlayAudio(string path)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Audio Load Error: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }
}

