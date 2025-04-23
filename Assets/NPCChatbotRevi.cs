// using System;
// using System.Collections;
// using System.Text;
// using UnityEngine;
// using UnityEngine.Networking;
// using UnityEngine.Windows.Speech;
// using Newtonsoft.Json.Linq;

// public class NPCChatbotRevi : MonoBehaviour
// {
//     public string projectId = "npc-chatbot-448406";
//     public string agentId = "cf12527b-8d14-4b91-ac30-078fdc6bdd31";
//     public string location = "global"; // Location of your agent
//     public string languageCode = "en";
//     public string accessToken; // Use OAuth 2.0 token for secure authentication
//     private string sessionId;
//     private DictationRecognizer dictationRecognizer;
//     private bool isDictating = false;
//     private AudioSource audioSource;

//     // Added headers
//     private const string ContentTypeHeader = "application/json";
//     private const string XGoogUserProjectHeader = "x-goog-user-project";
//     private const string AuthorizationHeader = "Authorization";

//     void Start()
//     {
//         sessionId = GenerateSessionId(); // Generate session ID once
//         audioSource = GetComponent<AudioSource>();

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

//     private IEnumerator SynthesizeSpeech(string text)
//     {
//         Debug.Log("Synthesizing Speech: " + text);
//         string url = "https://texttospeech.googleapis.com/v1/text:synthesize";
//         JObject jsonRequest = new JObject
//         {
//             ["input"] = new JObject { ["text"] = text },
//             ["voice"] = new JObject { ["languageCode"] = "en-US", ["name"] = "en-US-Wavenet-D" },
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

//             PlayAudioClip(audioBytes); // Play the TTS response directly
//         }
//     }

//     private void PlayAudioClip(byte[] audioBytes)
//     {
//         Debug.Log("Playing Audio Clip");
//         AudioClip clip = AudioClip.Create("TTSAudio", audioBytes.Length / 2, 1, 44100, false);
//         clip.SetData(ConvertByteArrayToFloatArray(audioBytes), 0);
//         audioSource.clip = clip;
//         audioSource.Play();
//     }

//     private float[] ConvertByteArrayToFloatArray(byte[] byteArray)
//     {
//         float[] floatArray = new float[byteArray.Length / 2];
//         for (int i = 0; i < floatArray.Length; i++)
//         {
//             floatArray[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768.0f;
//         }
//         return floatArray;
//     }
// }