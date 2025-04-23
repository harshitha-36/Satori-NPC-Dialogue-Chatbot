# AI-Driven NPC Dialogue Chatbot using Dialogflow CX and Vertex AI


<!-- Briefly describe the project in one sentence. --> 

---

## About


This project is an AI-powered NPC interaction system that enables natural, dynamic, and context-aware voice conversations in games. It addresses the limitations of traditional NPCs that rely on static dialogue trees by leveraging Dialogflow CX and Google Vertex AI. The main objective is to enhance immersion, engagement, and multilingual accessibility in gaming environments. It supports real-time, unscripted conversations and offers cross-platform integration using Unity, Firebase, and cloud APIs.

- Demo Video: [Questionarre_Chatbot](https://youtu.be/7aSThXTeyU0);
[NPC_Tour](https://youtu.be/ObTkR59b23E);
[Multilingual_Chatbot](https://youtu.be/tsYQXvI0EHQ);
[NPC-to-NPC_Communication](https://youtu.be/3cr8nwjyxWc)
- R&D Document: `./RND.docx`
- Knowledge Transfer Document: `./KnowledgeTransfer.docx`

---

## Quick Start

1. Prerequisites:
   - Unity Editor: Version `2022.3.48f1`
   - Google Cloud Dialogflow, Vertex AI, Ready Player Me, Firebase
   - Git with LFS

   Note: To install Git LFS, run `winget install GitHub.GitLFS`, then in your project directory run `git lfs install`. <!-- Git lfs is important to sync large files with git -->

2. Clone the Repository:

   ```bash
   git clone https://github.com/harshitha-36/Satori_npc_chatbot
   cd Satori_npc_chatbot
   ```

3. Open the cloned folder using Unity Hub.

4. Run:
   - Open the main scene: `Assets/Chatbot/Scenes/NoonA.unity`
   - Click the Play button ▶️ in the Unity Editor.

---

## Features

🔊 Voice-Based NPC Interactions: Supports real-time speech recognition and responses.

🧠 Context-Aware Dialogue Generation: Powered by Dialogflow CX and Vertex AI.

🌐 Multilingual Support: Interacts in Hindi, Tamil, and Telugu for inclusive gameplay.

🔁 Two-Way Conversations: Enables back-and-forth NPC-player and NPC-NPC dialogue.

🧩 Role-Specific AI Personalities: Domain-specific sales agent NPCs with goal-driven behavior.

⚙️ Cross-Platform Integration: Works seamlessly across devices using Unity + Firebase.

## Dependencies <!-- (Extra Tools/Frameworks/Packages) -->

- Unity: High Definition 3d, Unity Japan Office, Unity Dictation Recognizer
- External: Newtonsoft, Google Dialogflow, Vertex AI, Google Speech-to-text API, Google text-to-speech API, Ready me player, Google Service account (accesstoken)


---

## Project Structure Overview

```
Satori_npc_chatbot/          # Root directory
├── Assets/                  # Core Unity assets
│   ├── Animations/          # All Animations of NPCs
│   ├── Chatbot/             # directory for chatbot related files
        ├── Scenes/          # All Scenes
        ├── Scripts/         # C# scripts
│   ├── Ready Player Me/     # Ready Me Player Files
│   ├── Unity Japan Office/  # Files Related to office Asset

├── Builds/                    # Compiled game builds
├── Demo.mp4                   # Demo videos
├── RND.docx                   # R&D document
├── KnowledgeTransfer.docx     # Knowledge-Transfer document
├── Packages/                  # Unity package dependencies and manifests
├── ProjectSettings/           # Unity configuration files
├── .gitignore                 # Git ignore rules
└── README.md                  # Project overview and setup instructions (This file)
```

---

## Configuration

<!-- List any important settings that can be adjusted or need to be modified. -->
<!-- remove / add more if needed -->

| Setting               | Location                   | Description                                                  | Default Value            |
|------------------------|----------------------------|--------------------------------------------------------------|--------------------------|
| Setting                    | Location                               | Description                                                                                                        | Default Value            |

| Player Speed               | `PlayerController` Script              | Adjusts the movement speed of the player.                                                                         | `5.0`                    |
| Access Token (Dialogflow CX) | `Assets/Chatbot/DialogflowChatbotM.cs` | Handles multilingual chatbot interactions by connecting to Dialogflow CX for intent recognition, Google STT for voice input processing, and Google TTS for voice output. Token refreshes automatically every 20 minutes. |                          |
| Access Token (NPC-Lia)     | `Assets/Chatbot/NPCChatbotLia.cs`      | Connects Lia NPC to Dialogflow CX for conversation logic. Uses STT for interpreting player or NPC speech input and TTS to generate Lia's voice response. Access token refreshes every 20 minutes. |                          |
| Access Token (NPC-Revi)    | `Assets/Chatbot/NPCChatbotRevi.cs`     | Connects Revi NPC to Dialogflow CX and integrates Google STT and TTS for full voice-based NPC-to-NPC interactions. Token refresh is handled automatically every 20 minutes. |                          |

---

## Contact

- **Intern:** [Kambala Sree Harshitha](hhttps://www.linkedin.com/in/sreeharshitha/)
  - Email: [harshithakambala@gmail.com](mailto:harshithakambala@gmail.com)
  - Phone: +91 81243 84732    <!-- Phone is optional -->
- **Mentor:** Puneeth

