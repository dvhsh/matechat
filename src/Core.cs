using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

[assembly: MelonInfo(typeof(matechat.Core), "MateChat", "1.0.0", "dvhsh", null)]
[assembly: MelonGame("infiniteloop", "DesktopMate")]

namespace matechat
{
    public abstract class Feature
    {
        public string Name { get; protected set; }
        public KeyCode Keybind { get; protected set; }
        public bool IsEnabled { get; protected set; }

        protected Feature(string name, KeyCode keybind)
        {
            Name = name;
            Keybind = keybind;
            IsEnabled = false;
        }

        public virtual void Toggle()
        {
            IsEnabled = !IsEnabled;
        }
    }

    public class ChatFeature : Feature
    {
        private const string API_KEY = "XXX";
        private const string API_URL = "https://api.cloudflare.com/client/v4/accounts/YYY/ai/run/@cf/meta/llama-3-8b-instruct";
        private bool isWaitingForResponse = false;

        private string inputText = "";
        private string responseText = "";
        private bool isChatFocused = false;

        private Rect windowRect = new Rect(10, 10, 400, 400);

        public ChatFeature() : base("Chat", KeyCode.F8) { }

        public void DrawGUI()
        {
            if (!IsEnabled) return;

            Event current = Event.current;
            if (current != null)
            {
                Vector2 mousePos = current.mousePosition;
                Rect titleBarRect = new Rect(windowRect.x, windowRect.y, windowRect.width, 30);

                // Focus handling
                if (current.type == EventType.MouseDown)
                {
                    isChatFocused = windowRect.Contains(mousePos);
                }
            }

            // Colors
            Color mikuTeal = new Color(0.07f, 0.82f, 0.82f, 0.95f);
            Color darkTeal = new Color(0.05f, 0.4f, 0.4f, 0.95f);
            Color originalBgColor = GUI.backgroundColor;

            // Main window with shadow effect
            GUI.backgroundColor = Color.black;
            GUI.Box(new Rect(windowRect.x + 2, windowRect.y + 2, windowRect.width, windowRect.height), "");

            // Main window
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            GUI.Box(windowRect, "");

            // Title bar
            GUI.backgroundColor = mikuTeal;
            GUI.Box(new Rect(windowRect.x, windowRect.y, windowRect.width, 30), "");

            // Title (centered)
            GUI.Label(new Rect(windowRect.x + 60, windowRect.y + 5, windowRect.width - 120, 20), "✧ Miku Chat ♪ ✧");

            // Clear button (moved to right side)
            GUI.backgroundColor = darkTeal;
            bool clearClicked = GUI.Button(new Rect(windowRect.x + windowRect.width - 55, windowRect.y + 5, 50, 20), "Clear");
            if (clearClicked)
            {
                responseText = "";
                inputText = "";
            }

            // Chat history area
            GUI.backgroundColor = new Color(1, 1, 1, 0.1f);
            Rect contentRect = new Rect(windowRect.x + 10, windowRect.y + 40, windowRect.width - 20, windowRect.height - 80);
            GUI.Box(contentRect, "");

            // Display chat history
            GUI.Label(new Rect(contentRect.x + 5, contentRect.y + 5, contentRect.width - 10, contentRect.height - 10), responseText);

            // Input area
            GUI.backgroundColor = new Color(1, 1, 1, 0.15f);
            Rect inputRect = new Rect(windowRect.x + 10, windowRect.y + windowRect.height - 35, windowRect.width - 90, 25);
            GUI.Box(inputRect, "");
            GUI.Label(inputRect, inputText);

            // Handle input
            if (isChatFocused && current != null && current.type == EventType.KeyDown)
            {
                if (current.keyCode == KeyCode.Return)
                {
                    if (!string.IsNullOrEmpty(inputText))
                    {
                        SendMessage();
                    }
                    current.Use();
                }
                else if (current.keyCode == KeyCode.Backspace && inputText.Length > 0)
                {
                    inputText = inputText.Substring(0, inputText.Length - 1);
                    current.Use();
                }
                else if (!char.IsControl(current.character))
                {
                    inputText += current.character;
                    current.Use();
                }
            }

            // Send button
            GUI.backgroundColor = mikuTeal;
            Rect sendButtonRect = new Rect(windowRect.x + windowRect.width - 70, windowRect.y + windowRect.height - 35, 60, 25);
            bool sendClicked = GUI.Button(sendButtonRect, "♪ Send");
            if (sendClicked && !string.IsNullOrEmpty(inputText))
            {
                SendMessage();
            }

            GUI.backgroundColor = originalBgColor;
        }

        private bool IsOverButtons(Vector2 mousePos)
        {
            Rect clearButtonRect = new Rect(windowRect.x + windowRect.width - 55, windowRect.y + 5, 50, 20);
            Rect sendButtonRect = new Rect(windowRect.x + windowRect.width - 70, windowRect.y + windowRect.height - 35, 60, 25);
            return clearButtonRect.Contains(mousePos) || sendButtonRect.Contains(mousePos);
        }

        private void SendMessage()
        {
            if (string.IsNullOrEmpty(inputText) || isWaitingForResponse) return;

            // Melon<Core>.Logger.Msg("Message sent: " + inputText);

            if (responseText.Length > 0)
                responseText += "\n\n";

            responseText += "You: " + inputText;
            string userMessage = inputText;
            inputText = "";

            // Show typing indicator
            responseText += "\nMate: typing...";
            isWaitingForResponse = true;

            // Start coroutine for API call
            MelonCoroutines.Start(GetAIResponse(userMessage));
        }
        private IEnumerator GetAIResponse(string userMessage)
        {
            string jsonRequest = "{\"messages\":[" +
                "{\"role\":\"system\",\"content\":\"You are a cute and helpful desktop mate assistant. Keep your responses concise and friendly.\"}," +
                "{\"role\":\"user\",\"content\":\"" + EscapeJsonString(userMessage) + "\"}]}";

            var webRequest = new UnityWebRequest(API_URL, "POST");
            byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonRequest);

            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + API_KEY);

            yield return webRequest.SendWebRequest();

            // @TODO : redo error handling completely
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string response = webRequest.downloadHandler.text;

                    // @TODO : replace with real JSON handling??
                    // Find the response text between "response":"" and "},"success"
                    int startIndex = response.IndexOf("\"response\":\"") + 11;
                    int endIndex = response.IndexOf("\"}", startIndex);

                    if (startIndex != -1 && endIndex != -1)
                    {
                        string aiResponse = response.Substring(startIndex, endIndex - startIndex);
                       // Melon<Core>.Logger.Msg("Parsed AI response: " + aiResponse);
                        responseText = responseText.Replace("Mate: typing...", "Mate: " + aiResponse);
                    }
                    else
                    {
                        Melon<Core>.Logger.Error("Could not find response in: " + response);
                        responseText = responseText.Replace("Mate: typing...", "Mate: Sorry, I received an invalid response format.");
                    }
                }
                catch (System.Exception ex)
                {
                    Melon<Core>.Logger.Error("Failed to parse API response: " + ex.Message);
                    responseText = responseText.Replace("Mate: typing...", "Mate: Sorry, I encountered an error while processing your message.");
                }
            }
            else
            {
                Melon<Core>.Logger.Error($"API request failed: {webRequest.error}");
                responseText = responseText.Replace("Mate: typing...", "Mate: Sorry, I couldn't connect to llm right now.");
            }

            // Cleanup
            webRequest.uploadHandler.Dispose();
            webRequest.downloadHandler.Dispose();
            webRequest.Dispose();

            isWaitingForResponse = false;

            // Limit chat history
            const int maxLines = 10;
            string[] lines = responseText.Split('\n');
            if (lines.Length > maxLines)
            {
                responseText = string.Join("\n", lines.Skip(lines.Length - maxLines));
            }
        }

        // @TODO : ?_?
        private string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Replace("\"", "\\\"")
                     .Replace("\n", "\\n")
                     .Replace("\r", "\\r")
                     .Replace("\t", "\\t")
                     .Replace("\\", "\\\\");
        }
    }

    public class Core : MelonMod
    {
        private List<Feature> features;

        public override void OnEarlyInitializeMelon()
        {
            features = new List<Feature>();
            features.Add(new ChatFeature());
        }

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnLateUpdate()
        {
            foreach (var feature in features)
            {
                if (Input.GetKeyDown(feature.Keybind))
                {
                    feature.Toggle();
                 //   LoggerInstance.Msg($"{feature.Name} {(feature.IsEnabled ? "enabled" : "disabled")}");
                }
            }
        }

        public override void OnGUI()
        {
            foreach (var feature in features)
            {
                if (feature.IsEnabled)
                {
                    if (feature is ChatFeature chatFeature)
                    {
                        chatFeature.DrawGUI();
                    }
                }
            }
        }
    }
}
