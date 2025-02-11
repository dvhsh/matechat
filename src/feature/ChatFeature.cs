using System.Collections;
using MelonLoader;
using UnityEngine;
using matechat.sdk.Feature;
using matechat.database;

namespace matechat.feature
{
    public class ChatFeature : Feature
    {
        private bool isWaitingForResponse;
        private string inputText = string.Empty;
        private string responseText = string.Empty;
        private bool isChatFocused;
        private Vector2 scrollPosition;
        private GUIStyle textStyle;
        private Rect windowRect;

        private static readonly Color MikuTeal = new Color(0.07f, 0.82f, 0.82f, 0.95f);
        private static readonly Color DarkTeal = new Color(0.05f, 0.4f, 0.4f, 0.95f);
        private static readonly Color WindowBackground = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        private static readonly Color InputBackground = new Color(1, 1, 1, 0.15f);
        private static readonly Color ContentBackground = new Color(1, 1, 1, 0.1f);
        private const int TitleBarHeight = 30;
        private const int InputHeight = 25;
        private const int Padding = 10;
        private const int MaxChatHistory = 50;

        public ChatFeature() : base("Chat", Config.CHAT_KEYBIND.Value)
        {
            textStyle = new GUIStyle
            {
                normal = { textColor = Color.white },
                fontSize = Config.CHAT_WINDOW_FONT_SIZE.Value,
                wordWrap = true
            };
            UpdateWindowRect();
            UpdateSettings();
        }

        public void UpdateWindowRect()
        {
            windowRect = new Rect(
                Config.CHAT_WINDOW_X.Value, Config.CHAT_WINDOW_Y.Value,
                Config.CHAT_WINDOW_WIDTH.Value, Config.CHAT_WINDOW_HEIGHT.Value);
        }

        public void DrawGUI()
        {
            if (!IsEnabled)
                return;
            if (Event.current?.type == EventType.MouseDown)
            {
                isChatFocused = windowRect.Contains(Event.current.mousePosition);
            }
            DrawWindow();
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

        private void DrawChatContent()
        {
            GUI.backgroundColor = ContentBackground;
            Rect contentRect = new Rect(
                windowRect.x + Padding,
                windowRect.y + TitleBarHeight + Padding,
                windowRect.width - (Padding * 2),
                windowRect.height - TitleBarHeight - InputHeight - (Padding * 3));

            GUI.Box(contentRect, string.Empty);

            // Calculate content height
            float contentHeight = textStyle.CalcHeight(new GUIContent(responseText),
                contentRect.width - 10); // 10 for scrollbar width

            // Only allow manual scrolling when not waiting for response
            if (contentRect.Contains(Event.current.mousePosition) && !isWaitingForResponse)
            {
                float scroll = Input.mouseScrollDelta.y * 20f;
                scrollPosition.y = Mathf.Clamp(scrollPosition.y - scroll, 0,
                    Mathf.Max(0, contentHeight - contentRect.height));
            }

            GUI.BeginGroup(contentRect);
            GUI.Label(new Rect(5, -scrollPosition.y, contentRect.width - 10,
                Mathf.Max(contentRect.height, contentHeight)),
                responseText, textStyle);
            GUI.EndGroup();
        }

        private void DrawShadow()
        {
            GUI.backgroundColor = Color.black;
            GUI.Box(new Rect(windowRect.x + 2, windowRect.y + 2, windowRect.width,
                             windowRect.height), string.Empty);
        }

        private void DrawMainWindow()
        {
            GUI.backgroundColor = WindowBackground;
            GUI.Box(windowRect, string.Empty);
        }

        private void DrawTitleBar()
        {
            Rect titleBarRect = new Rect(windowRect.x, windowRect.y, windowRect.width,
                                         TitleBarHeight);
            GUI.backgroundColor = MikuTeal;
            GUI.Box(titleBarRect, string.Empty);
            GUI.Label(new Rect(windowRect.x + 60, windowRect.y + 5,
                               windowRect.width - 120, 20), "✧ Mate Chat ♪ ✧");

            Rect clearButtonRect = new Rect(windowRect.x + windowRect.width - 55,
                                            windowRect.y + 5, 50, 20);
            GUI.backgroundColor =
                clearButtonRect.Contains(Event.current.mousePosition)
                    ? new Color(DarkTeal.r * 1.2f, DarkTeal.g * 1.2f,
                                DarkTeal.b * 1.2f, DarkTeal.a)
                    : DarkTeal;
            if (GUI.Button(clearButtonRect, "Clear"))
            {
                ClearChat();
            }
        }

        private void DrawInputArea()
        {
            GUI.backgroundColor = InputBackground;
            Rect inputRect = new Rect(windowRect.x + Padding,
                                      windowRect.y + windowRect.height - InputHeight - Padding,
                                      windowRect.width - 90, InputHeight);
            GUI.Box(inputRect, string.Empty);
            GUI.Label(inputRect, inputText, textStyle);
            HandleInputEvents();
            DrawSendButton(inputRect);
        }

        private void HandleInputEvents()
        {
            if (!isChatFocused || Event.current?.type != EventType.KeyDown)
                return;

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
            Rect sendButtonRect = new Rect(inputRect.x + inputRect.width + 10,
                                           inputRect.y, 60, InputHeight);
            if (GUI.Button(sendButtonRect, "♪ Send") && !string.IsNullOrEmpty(inputText))
            {
                SendMessage();
            }
        }

        private void SendMessage()
        {
            if (string.IsNullOrEmpty(inputText) || isWaitingForResponse)
                return;

            AppendToChatHistory($"You: {inputText}");
            string userMessage = inputText;
            inputText = string.Empty;
            isWaitingForResponse = true;
            MelonCoroutines.Start(SendMessageCoroutine(userMessage));            
        }


        private IEnumerator SendMessageCoroutine(string userMessage)
        {
            var aiManager = Core.GetAIEngineManager();
            string engineName = Config.ENGINE_TYPE.Value;
            string model = Config.MODEL_NAME.Value;
            string systemprompt = Config.SYSTEM_PROMPT.Value;

            systemprompt += string.Format(" You are Responding to {0}, and your name is {1}", Config.NAME.Value, Config.AI_NAME.Value);

            // Add typing message
            string typingMessage = $"{Config.AI_NAME.Value}: typing...";
            AppendToChatHistory(typingMessage);

            // Send the request asynchronously
            var task = aiManager.SendRequestAsync(userMessage, engineName, model, systemprompt);

            // Wait for the task to complete
            while (!task.IsCompleted)
            {
                yield return null;
            }

            // Remove the typing message
            responseText = responseText.Replace($"\n\n{typingMessage}", "");
            if (responseText.EndsWith(typingMessage))
            {
                responseText = responseText.Substring(0, responseText.Length - typingMessage.Length);
            }

            // Process the result
            if (task.Exception == null && task.IsCompletedSuccessfully)
            {
                string assistantMessage = task.Result;
                // Append the assistant's response to the chat history
                AppendToChatHistory($"{Config.AI_NAME.Value}: {assistantMessage}");

            }
            else
            {
                AppendToChatHistory($"Error: {(task.Exception?.Message ?? "Unknown error occurred.")}");
            }

            // Reset the waiting state
            isWaitingForResponse = false;
            LimitChatHistory();


            // run tts engine
            if (task.Exception == null && task.IsCompletedSuccessfully)
            {
                if (Config.ENABLE_TTS.Value)
                {
                    MelonCoroutines.Start(PlayMessageCoroutine(task.Result));
                }
            }
        }
        private IEnumerator PlayMessageCoroutine(string responseText)
        {
            var audioManager = Core.GetAudioEngine();
            var taskCompletionSource = new TaskCompletionSource<string>();

            MelonLogger.Msg("[TTS] Converting AI response to speech...");

            MelonCoroutines.Start(ProcessAudioCoroutine(responseText, taskCompletionSource));

            while (!taskCompletionSource.Task.IsCompleted)
            {
                yield return null;
            }

            if (taskCompletionSource.Task.Exception != null)
            {
                MelonLogger.Error($"[TTS] Failed to play: {taskCompletionSource.Task.Exception.Message}");
            }
            else
            {
                string audioPath = taskCompletionSource.Task.Result;
                MelonLogger.Msg($"[TTS] Successfully played audio from path: {audioPath}");

                Core.databaseAudioManager.AddAudioPath(responseText, audioPath);
            }
        }
        private IEnumerator ProcessAudioCoroutine(string text, TaskCompletionSource<string> tcs)
        {
            var audioManager = Core.GetAudioEngine();

            var task = audioManager.ProcessAudioAsync(text);

            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.Exception != null)
            {
                tcs.SetException(task.Exception);
            }
            else
            {
                tcs.SetResult(task.Result);
            }
        }



        private void LimitChatHistory()
        {
            string[] lines = responseText.Split('\n');
            if (lines.Length > MaxChatHistory)
            {
                responseText = string.Join("\n", lines.Skip(lines.Length - MaxChatHistory));
            }
        }
        private void ClearChat()
        {
            responseText = string.Empty;
            inputText = string.Empty;
            scrollPosition = Vector2.zero;

            Core.databaseManager.ClearMessages();
        }

        private void AppendToChatHistory(string message)
        {
            if (responseText.Length > 0)
                responseText += "\n\n";

            responseText += message;

            // Auto-scroll to bottom
            float contentHeight = textStyle.CalcHeight(new GUIContent(responseText),
                windowRect.width - (Padding * 2) - 10); 
            scrollPosition.y = Mathf.Max(0, contentHeight);
        }

        public void UpdateSettings()
        {
            textStyle.fontSize = Config.CHAT_WINDOW_FONT_SIZE.Value;
            UpdateWindowRect();
        }
    }
}
