using System.Collections;
using System.Text;

using MelonLoader;

using UnityEngine;
using UnityEngine.Networking;

using matechat.sdk.Feature;
using matechat.util;

namespace matechat.feature
{
    public class ChatFeature : Feature
    {
        private bool isWaitingForResponse;
        private string inputText = string.Empty;
        private string responseText = string.Empty;
        private bool isChatFocused;
        private Vector2 scrollPosition;

        private readonly Rect windowRect = new Rect(
            20, 
            20,                
            400,             
            500               
        );

        private static readonly Color MikuTeal = new Color(0.07f, 0.82f, 0.82f, 0.95f);
        private static readonly Color DarkTeal = new Color(0.05f, 0.4f, 0.4f, 0.95f);
        private static readonly Color WindowBackground = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        private static readonly Color InputBackground = new Color(1, 1, 1, 0.15f);
        private static readonly Color ContentBackground = new Color(1, 1, 1, 0.1f);

        private const int TitleBarHeight = 30;
        private const int InputHeight = 25;
        private const int Padding = 10;
        private const int MaxChatHistory = 50;

        public ChatFeature() : base("Chat", Config.CHAT_KEYBIND.Value) { }


        public void DrawGUI()
        {
            if (!IsEnabled) return;

            if (Event.current?.type == EventType.MouseDown)
            {
                isChatFocused = windowRect.Contains(Event.current.mousePosition);
            }

            DrawWindow();
        }

        private void HandleFocusEvents()
        {
            Event current = Event.current;
            if (current?.type == EventType.MouseDown)
            {
                isChatFocused = windowRect.Contains(current.mousePosition);
            }
        }

        private void DrawWindow()
        {
            Color originalBgColor = GUI.backgroundColor;

            DrawShadow();
            DrawMainWindow();
            DrawTitleBar();
            DrawChatContent();
            DrawInputArea();

            GUI.backgroundColor = originalBgColor;
        }

        private void DrawShadow()
        {
            GUI.backgroundColor = Color.black;
            GUI.Box(new Rect(windowRect.x + 2, windowRect.y + 2, windowRect.width, windowRect.height), string.Empty);
        }

        private void DrawMainWindow()
        {
            GUI.backgroundColor = WindowBackground;
            GUI.Box(windowRect, string.Empty);
        }

        private void DrawTitleBar()
        {
            Rect titleBarRect = new Rect(windowRect.x, windowRect.y, windowRect.width, TitleBarHeight);
            GUI.backgroundColor = MikuTeal;
            GUI.Box(titleBarRect, string.Empty);

            GUI.Label(new Rect(windowRect.x + 60, windowRect.y + 5, windowRect.width - 120, 20), "✧ Mate Chat ♪ ✧");

            Rect clearButtonRect = new Rect(windowRect.x + windowRect.width - 55, windowRect.y + 5, 50, 20);
            GUI.backgroundColor = clearButtonRect.Contains(Event.current.mousePosition)
                ? new Color(DarkTeal.r * 1.2f, DarkTeal.g * 1.2f, DarkTeal.b * 1.2f, DarkTeal.a)
                : DarkTeal;

            if (GUI.Button(clearButtonRect, "Clear"))
            {
                ClearChat();
            }
        }

        private void DrawChatContent()
        {
            GUI.backgroundColor = ContentBackground;
            Rect contentRect = new Rect(
                windowRect.x + Padding,
                windowRect.y + TitleBarHeight + Padding,
                windowRect.width - (Padding * 2),
                windowRect.height - TitleBarHeight - InputHeight - (Padding * 3)
            );

            GUI.Box(contentRect, string.Empty);

            // Handle mouse scroll wheel
            if (contentRect.Contains(Event.current.mousePosition))
            {
                float scroll = Input.mouseScrollDelta.y * 20f;
                scrollPosition.y = Mathf.Clamp(scrollPosition.y - scroll, 0, Mathf.Max(0, responseText.Length * 2 - contentRect.height));
            }

            // Create a clipped area for the text
            GUI.BeginGroup(contentRect);

            // Draw the text offset by the scroll position
            GUI.Label(new Rect(5, -scrollPosition.y, contentRect.width - 10, Mathf.Max(contentRect.height, responseText.Length * 2)),
                responseText);

            GUI.EndGroup();
        }


        private void DrawInputArea()
        {
            GUI.backgroundColor = InputBackground;
            Rect inputRect = new Rect(windowRect.x + Padding, windowRect.y + windowRect.height - InputHeight - Padding,
                                    windowRect.width - 90, InputHeight);

            GUI.Box(inputRect, string.Empty);
            GUI.Label(inputRect, inputText);

            HandleInputEvents();
            DrawSendButton(inputRect);
        }

        private void HandleInputEvents()
        {
            if (!isChatFocused || Event.current?.type != EventType.KeyDown) return;

            switch (Event.current.keyCode)
            {
                case KeyCode.Return when !string.IsNullOrEmpty(inputText):
                    SendMessage();
                    Event.current.Use();
                    break;
                case KeyCode.Backspace when inputText.Length > 0:
                    inputText = inputText[..^1];
                    Event.current.Use();
                    break;
                default:
                    if (!char.IsControl(Event.current.character))
                    {
                        inputText += Event.current.character;
                        Event.current.Use();
                    }
                    break;
            }
        }

        private void DrawSendButton(Rect inputRect)
        {
            GUI.backgroundColor = MikuTeal;
            Rect sendButtonRect = new Rect(inputRect.x + inputRect.width + 10, inputRect.y, 60, InputHeight);

            if (GUI.Button(sendButtonRect, "♪ Send") && !string.IsNullOrEmpty(inputText))
            {
                SendMessage();
            }
        }

        private void SendMessage()
        {
            if (string.IsNullOrEmpty(inputText) || isWaitingForResponse) return;

            AppendToChatHistory($"You: {inputText}");
            string userMessage = inputText;
            inputText = string.Empty;

            AppendToChatHistory("Mate: typing...");
            isWaitingForResponse = true;

            MelonCoroutines.Start(GetAIResponse(userMessage));
        }

        private void AppendToChatHistory(string message)
        {
            if (responseText.Length > 0)
                responseText += "\n\n";
            responseText += message;
            scrollPosition.y = float.MaxValue;
        }

        private void ClearChat()
        {
            responseText = string.Empty;
            inputText = string.Empty;
            scrollPosition = Vector2.zero;
        }

        private IEnumerator GetAIResponse(string userMessage)
        {
            string jsonRequest = $"{{\"messages\":[{{\"role\":\"system\",\"content\":\"{JsonUtil.EscapeJsonString(Config.SYSTEM_PROMPT.Value)}\"}},{{\"role\":\"user\",\"content\":\"{JsonUtil.EscapeJsonString(userMessage)}\"}}]}}";

            UnityWebRequest webRequest = new UnityWebRequest(Config.API_URL.Value, "POST");
            try
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonRequest);
                webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {Config.API_KEY.Value}");

                yield return webRequest.SendWebRequest();

                ProcessApiResponse(webRequest);
            }
            finally
            {
                if (webRequest.uploadHandler != null)
                    webRequest.uploadHandler.Dispose();
                if (webRequest.downloadHandler != null)
                    webRequest.downloadHandler.Dispose();
                webRequest.Dispose();

                isWaitingForResponse = false;
                LimitChatHistory();
            }
        }

        private void ProcessApiResponse(UnityWebRequest webRequest)
        {
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Melon<Core>.Logger.Error($"API request failed: {webRequest.error}");
                UpdateTypingMessage("Sorry, I couldn't connect to llm right now.");
                return;
            }

            try
            {
                string response = webRequest.downloadHandler.text;
                int startIndex = response.IndexOf("\"response\":\"") + 11;
                int endIndex = response.IndexOf("\"}", startIndex);

                if (startIndex != -1 && endIndex != -1)
                {
                    string aiResponse = response[startIndex..endIndex];
                    UpdateTypingMessage(aiResponse);
                }
                else
                {
                    Melon<Core>.Logger.Error($"Could not find response in: {response}");
                    UpdateTypingMessage("Sorry, I received an invalid response format.");
                }
            }
            catch (System.Exception ex)
            {
                Melon<Core>.Logger.Error($"Failed to parse API response: {ex.Message}");
                UpdateTypingMessage("Sorry, I encountered an error while processing your message.");
            }
        }

        private void UpdateTypingMessage(string newMessage)
        {
            responseText = responseText.Replace("Mate: typing...", $"Mate: {newMessage}");
        }

        private void LimitChatHistory()
        {
            string[] lines = responseText.Split('\n');
            if (lines.Length > MaxChatHistory)
            {
                responseText = string.Join("\n", lines.Skip(lines.Length - MaxChatHistory));
            }
        }
    }
}
