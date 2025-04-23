// using System;
// using System.Collections;
// using System.Text;
// using UnityEngine;
// using UnityEngine.Networking;
// using UnityEngine.Windows.Speech;
// using Newtonsoft.Json.Linq;

// public class NPCChatbotTrial : MonoBehaviour
// {
//     public string projectId = "npc-chatbot-448406";
//     public string agentId = "cf12527b-8d14-4b91-ac30-078fdc6bdd31";
//     public string location = "global"; // Location of your agent
//     public string languageCode = "en";
//     public string accessToken; // Use OAuth 2.0 token for secure authentication
//     private string sessionId;
//     private DictationRecognizer dictationRecognizer;
//     private bool isDictating = false;
//     private bool isPlayerInTrigger = false;
//     private AudioSource audioSource;

//     // Reference to the NPCAnimationController
//     private NPCAnimationController animationController;

//     // Added headers
//     private const string ContentTypeHeader = "application/json";
//     private const string XGoogUserProjectHeader = "x-goog-user-project";
//     private const string AuthorizationHeader = "Authorization";

//     void Start()
//     {
//         sessionId = GenerateSessionId();
//         audioSource = GetComponent<AudioSource>();

//         // Get reference to the NPCAnimationController
//         animationController = GetComponent<NPCAnimationController>();
//         if (animationController == null)
//         {
//             Debug.LogError("NPCAnimationController component not found on NPC!");
//         }

//         // Initialize DictationRecognizer
//         dictationRecognizer = new DictationRecognizer();
//         dictationRecognizer.DictationResult += OnDictationResult;
//         dictationRecognizer.DictationComplete += OnDictationComplete;
//         dictationRecognizer.DictationError += OnDictationError;

//         dictationRecognizer.InitialSilenceTimeoutSeconds = 10;

//         // Pre-initialize the recognizer to reduce delay
//         dictationRecognizer.Start();
//         dictationRecognizer.Stop(); // Stop it immediately to keep it ready
//     }

//     private string GenerateSessionId()
//     {
//         return Guid.NewGuid().ToString();
//     }

//     private void Update()
//     {
//         // Check if the player is in the trigger area and presses/releases the Spacebar
//         if (isPlayerInTrigger)
//         {
//             if (Input.GetKeyDown(KeyCode.Space))
//             {
//                 StartDictation();
//             }
//             else if (Input.GetKeyUp(KeyCode.Space))
//             {
//                 StopDictation();
//             }
//         }
//     }

//     public void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             isPlayerInTrigger = true;
//             Debug.Log("Player entered trigger area.");

//             // Notify the animation controller that the player has entered
//             if (animationController != null)
//             {
//                 animationController.OnTriggerEnter(other);
//             }
//         }
//     }

//     public void OnTriggerExit(Collider other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             isPlayerInTrigger = false;
//             Debug.Log("Player exited trigger area.");

//             // Stop dictation and NPC speech when the player leaves the trigger area
//             if (isDictating)
//             {
//                 StopDictation();
//             }
//             if (audioSource.isPlaying)
//             {
//                 audioSource.Stop();
//             }

//             // Notify the animation controller that the player has exited
//             if (animationController != null)
//             {
//                 animationController.OnTriggerExit(other);
//             }
//         }
//     }

//     public void StartDictation()
//     {
//         if (isDictating)
//         {
//             Debug.LogWarning("Already dictating.");
//             return;
//         }

//         isDictating = true;
//         dictationRecognizer.Start();
//         Debug.Log("Dictation started.");
//     }

//     public void StopDictation()
//     {
//         if (!isDictating)
//         {
//             Debug.LogWarning("Dictation is not currently active.");
//             return;
//         }

//         dictationRecognizer.Stop();
//         isDictating = false;
//         Debug.Log("Dictation stopped.");
//     }

//     private void OnDictationResult(string text, ConfidenceLevel confidence)
//     {
//         Debug.Log("Dictation Result: " + text);
//         StartCoroutine(SendToDialogflow(text));
//     }

//     private void OnDictationComplete(DictationCompletionCause cause)
//     {
//         if (cause != DictationCompletionCause.Complete)
//         {
//             Debug.LogWarning("Dictation completed unsuccessfully: " + cause);
//         }
//     }

//     private void OnDictationError(string error, int hresult)
//     {
//         Debug.LogError("Dictation error: " + error);
//     }

//     private IEnumerator SendToDialogflow(string userQuery)
//     {
//         Debug.Log("Sending to Dialogflow: " + userQuery);
//         string url = $"https://dialogflow.googleapis.com/v3/projects/{projectId}/locations/{location}/agents/{agentId}/sessions/{sessionId}:detectIntent";

//         // Create JSON payload
//         JObject jsonRequest = new JObject
//         {
//             ["queryInput"] = new JObject
//             {
//                 ["languageCode"] = languageCode,
//                 ["text"] = new JObject
//                 {
//                     ["text"] = userQuery,
//                 }
//             }
//         };

//         UnityWebRequest request = new UnityWebRequest(url, "POST");
//         byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest.ToString());
//         request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//         request.downloadHandler = new DownloadHandlerBuffer();

//         // Add headers
//         request.SetRequestHeader("Content-Type", ContentTypeHeader);
//         request.SetRequestHeader(AuthorizationHeader, $"Bearer {accessToken}");
//         request.SetRequestHeader(XGoogUserProjectHeader, projectId);

//         // Send request and yield
//         yield return request.SendWebRequest();

//         if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
//         {
//             Debug.LogError("Dialogflow Error: " + request.error);
//         }
//         else
//         {
//             string jsonResponse = request.downloadHandler.text;
//             Debug.Log("Dialogflow Response: " + jsonResponse);
//             JObject responseObj = JObject.Parse(jsonResponse);
//             string botResponse = responseObj["queryResult"]["responseMessages"][0]["text"]["text"][0]?.ToString();
//             Debug.Log("Bot Response: " + botResponse);

//             StartCoroutine(SynthesizeSpeech(botResponse)); // Pass the response to TTS
//         }
//     }

//     public IEnumerator SynthesizeSpeech(string text)
//     {
//         Debug.Log("Synthesizing Speech: " + text);
//         string url = "https://texttospeech.googleapis.com/v1/text:synthesize";
//         JObject jsonRequest = new JObject
//         {
//             ["input"] = new JObject { ["text"] = text },
//             ["voice"] = new JObject { ["languageCode"] = "en-US", ["name"] = "te-IN-Standard-C" },
//             ["audioConfig"] = new JObject { ["audioEncoding"] = "MP3" }
//         };

//         UnityWebRequest request = new UnityWebRequest(url, "POST");
//         byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest.ToString());
//         request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//         request.downloadHandler = new DownloadHandlerBuffer();

//         // Add headers
//         request.SetRequestHeader("Content-Type", ContentTypeHeader);
//         request.SetRequestHeader(AuthorizationHeader, $"Bearer {accessToken}");
//         request.SetRequestHeader(XGoogUserProjectHeader, projectId);

//         yield return request.SendWebRequest();

//         if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
//         {
//             Debug.LogError("TTS Error: " + request.error);
//         }
//         else
//         {
//             string jsonResponse = request.downloadHandler.text;
//             Debug.Log("TTS Response: " + jsonResponse);
//             JObject responseObj = JObject.Parse(jsonResponse);
//             string audioContent = responseObj["audioContent"]?.ToString();
//             byte[] audioBytes = Convert.FromBase64String(audioContent);

//             PlayAudioClip(audioBytes); // Play the TTS response
//         }
//     }

//     private void PlayAudioClip(byte[] audioBytes)
//     {
//         Debug.Log("Playing Audio Clip");
//         string path = Application.persistentDataPath + "/tts_audio.mp3";
//         System.IO.File.WriteAllBytes(path, audioBytes);

//         StartCoroutine(LoadAndPlayAudio(path));
//     }

//     private IEnumerator LoadAndPlayAudio(string path)
//     {
//         Debug.Log("Loading Audio Clip: " + path);
//         using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
//         {
//             yield return www.SendWebRequest();

//             if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
//             {
//                 Debug.LogError("Audio Load Error: " + www.error);
//             }
//             else
//             {
//                 Debug.Log("Audio Loaded Successfully");
//                 AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
//                 audioSource.clip = clip;
//                 audioSource.Play();

//                 // Notify the animation controller that the NPC is talking
//                 if (animationController != null)
//                 {
//                     animationController.SetTalkingState(true);
//                 }

//                 // Wait for the audio to finish playing
//                 yield return new WaitForSeconds(clip.length);

//                 // Notify the animation controller that the NPC has stopped talking
//                 if (animationController != null)
//                 {
//                     animationController.SetTalkingState(false);
//                 }
//             }
//         }
//     }
// }




using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows.Speech;
using Newtonsoft.Json.Linq;


public class NPCChatbotTrial : MonoBehaviour
{
    public string projectId = "npc-chatbot-448406";
    public string agentId ="4ce28c31-c841-474c-a6cd-8002f59e1c4b";
    public string location = "global"; // Location of your agent
    public string languageCode = "en";
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
            Debug.Log("Player entered trigger area.");
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            Debug.Log("Player exited trigger area.");


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
            Debug.LogWarning("Already dictating.");
            return;
        }


        isDictating = true;
        dictationRecognizer.Start();
        Debug.Log("Dictation started.");
    }


    private void StopDictation()
    {
        if (!isDictating)
        {
            Debug.LogWarning("Dictation is not currently active.");
            return;
        }


        dictationRecognizer.Stop();
        isDictating = false;
        Debug.Log("Dictation stopped.");
    }


    private void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log("Dictation Result: " + text);
        StartCoroutine(SendToDialogflow(text));
    }


    private void OnDictationComplete(DictationCompletionCause cause)
    {
        if (cause != DictationCompletionCause.Complete)
        {
            Debug.LogWarning("Dictation completed unsuccessfully: " + cause);
        }
    }


    private void OnDictationError(string error, int hresult)
    {
        Debug.LogError("Dictation error: " + error);
    }


    private IEnumerator SendToDialogflow(string userQuery)
    {
        Debug.Log("Sending to Dialogflow: " + userQuery);
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
            Debug.Log("Dialogflow Response: " + jsonResponse);
            JObject responseObj = JObject.Parse(jsonResponse);
            string botResponse = responseObj["queryResult"]["responseMessages"][0]["text"]["text"][0]?.ToString();
            Debug.Log("Bot Response: " + botResponse);


            StartCoroutine(SynthesizeSpeech(botResponse)); // Pass the response to TTS
        }
    }


    private IEnumerator SynthesizeSpeech(string text)
    {
        Debug.Log("Synthesizing Speech: " + text);
        string url = "https://texttospeech.googleapis.com/v1/text:synthesize";
        JObject jsonRequest = new JObject
        {
            ["input"] = new JObject { ["text"] = text },
            ["voice"] = new JObject { ["languageCode"] = "en-US", ["name"] = "te-IN-Standard-B" },
            ["audioConfig"] = new JObject { ["audioEncoding"] = "MP3" }
        };


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
            Debug.LogError("TTS Error: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("TTS Response: " + jsonResponse);
            JObject responseObj = JObject.Parse(jsonResponse);
            string audioContent = responseObj["audioContent"]?.ToString();
            byte[] audioBytes = Convert.FromBase64String(audioContent);


            PlayAudioClip(audioBytes); // Play the TTS response
        }
    }


    private void PlayAudioClip(byte[] audioBytes)
    {
        Debug.Log("Playing Audio Clip");
        string path = Application.persistentDataPath + "/tts_audio.mp3";
        System.IO.File.WriteAllBytes(path, audioBytes);


        StartCoroutine(LoadAndPlayAudio(path));
    }


    private IEnumerator LoadAndPlayAudio(string path)
    {
        Debug.Log("Loading Audio Clip: " + path);
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();


            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Audio Load Error: " + www.error);
            }
            else
            {
                Debug.Log("Audio Loaded Successfully");
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }
}

