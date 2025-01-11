using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using HarmonyLib;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;


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

            // Color
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
        private GameObject mateChatPage;
        private GameObject rootPageObj;
        private Button backButton;

        private GameObject menuManager;
        private GameObject uniWindowController;

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
                }
            }
        }

        public override void OnGUI()
        {
            foreach (var feature in features)
            {
                if (feature.IsEnabled && feature is ChatFeature chatFeature)
                {
                    chatFeature.DrawGUI();
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            MelonCoroutines.Start(WaitForMenu());
        }

        private IEnumerator WaitForMenu()
        {
            Transform menuParent = null;
            while (menuParent == null)
            {
                menuParent = GameObject.Find("MenuCanvas/MenuParent")?.transform;
                yield return new WaitForSeconds(0.1f);
            }

            MelonLogger.Msg("Found MenuParent");
            CreateMateChatPage(menuParent);
            CreateMenuButton();
        }

        private void InitializeMenuReferences()
        {
            menuManager = GameObject.Find("MenuManager");
            uniWindowController = GameObject.Find("UniWindowController");
            MelonLogger.Msg($"Found MenuManager: {menuManager != null}, UniWindowController: {uniWindowController != null}");
        }

        private void CreateMateChatPage(Transform menuParent)
        {
            InitializeMenuReferences();

            mateChatPage = new GameObject("MateChatSettingsPage");
            mateChatPage.transform.SetParent(menuParent, false);

            // Copy RectTransform settings from RootPage
            Transform rootPage = menuParent.Find("RootPage");
            if (rootPage != null)
            {
                rootPageObj = rootPage.gameObject;
                RectTransform rootRT = rootPage.GetComponent<RectTransform>();
                RectTransform pageRT = mateChatPage.AddComponent<RectTransform>();
                pageRT.anchorMin = rootRT.anchorMin;
                pageRT.anchorMax = rootRT.anchorMax;
                pageRT.pivot = rootRT.pivot;
                pageRT.sizeDelta = rootRT.sizeDelta;
                pageRT.anchoredPosition = rootRT.anchoredPosition;

                CopyComponent(rootPage, "Background", mateChatPage.transform);
                CopyAndSetupButton(rootPage, "BackButton", mateChatPage.transform);
                CopyAndSetupButton(rootPage, "CloseButton", mateChatPage.transform);
                CreateSettingsContent(mateChatPage.transform);
            }

            mateChatPage.SetActive(false);
        }

        private void SwitchToMateChatPage()
        {
            // Try to get animator
            Animator menuAnimator = mateChatPage.GetComponentInParent<Animator>();
            if (menuAnimator != null)
            {
                MelonLogger.Msg("Found animator, playing transition");
                // Reset any existing animations
                menuAnimator.Rebind();
                menuAnimator.Update(0f);

                // Try common animation trigger names
                menuAnimator.SetTrigger("Open");
                menuAnimator.SetTrigger("Show");
                menuAnimator.SetTrigger("Enter");
            }

            rootPageObj.SetActive(false);
            mateChatPage.SetActive(true);
        }

        private void CloseMenu()
        {
            MelonLogger.Msg("Closing menu");

            Animator menuAnimator = mateChatPage.GetComponentInParent<Animator>();
            if (menuAnimator != null)
            {
                MelonLogger.Msg("Found animator, playing close animation");
                // Reset any existing animations
                menuAnimator.Rebind();
                menuAnimator.Update(0f);

                // Try common animation trigger names
                menuAnimator.SetTrigger("Close");
                menuAnimator.SetTrigger("Hide");
                menuAnimator.SetTrigger("Exit");

                // Delay the actual close
                MelonCoroutines.Start(DelayedClose());
            }
            else
            {
                mateChatPage.SetActive(false);
                rootPageObj.SetActive(false);
            }
        }

        private void CopyAndSetupButton(Transform sourcePage, string buttonName, Transform targetParent)
        {
            Transform originalButton = sourcePage.Find(buttonName);
            if (originalButton != null)
            {
                GameObject buttonCopy = GameObject.Instantiate(originalButton.gameObject, targetParent);
                buttonCopy.name = buttonName;

                Button button = buttonCopy.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick = new Button.ButtonClickedEvent();
                    button.onClick.AddListener((UnityEngine.Events.UnityAction)delegate
                    {
                        switch (buttonName)
                        {
                            case "BackButton":
                                if (menuManager != null)
                                {
                                    Component[] components = menuManager.GetComponents<Component>();
                                    if (components.Length > 1)
                                    {
                                        components[1].SendMessage("Back", SendMessageOptions.DontRequireReceiver);
                                    }
                                }
                                mateChatPage.SetActive(false);
                                rootPageObj.SetActive(true);
                                break;

                            case "CloseButton":
                                if (uniWindowController != null)
                                {
                                    Component[] components = uniWindowController.GetComponents<Component>();
                                    if (components.Length > 1)
                                    {
                                        components[1].SendMessage("Close", SendMessageOptions.DontRequireReceiver);
                                    }
                                }
                                MelonCoroutines.Start(DelayedClose());
                                break;
                        }
                    });
                }
            }
        }

        private IEnumerator DelayedClose()
        {
            yield return new WaitForSeconds(0.3f);
            mateChatPage.SetActive(false);
            rootPageObj.SetActive(false);
        }



        private void CopyComponent(Transform sourceParent, string childName, Transform targetParent)
        {
            Transform originalTransform = sourceParent.Find(childName);
            if (originalTransform != null)
            {
                GameObject copiedObject = GameObject.Instantiate(originalTransform.gameObject, targetParent);
                copiedObject.name = childName;
                MelonLogger.Msg($"Copied {childName}");
            }
        }

        private void CreateSettingsContent(Transform parent)
        {
            // Create a container for our settings
            GameObject contentObj = new GameObject("SettingsContent");
            contentObj.transform.SetParent(parent, false);

            // Set up the RectTransform for the content
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(20, 20);
            contentRect.offsetMax = new Vector2(-20, -60);

            // Add example settings buttons
            CreateSettingButton(contentObj.transform, "API Key", 0);
            CreateSettingButton(contentObj.transform, "Toggle Chat", 1);
            CreateSettingButton(contentObj.transform, "Clear History", 2);
        }

        private void CreateSettingButton(Transform parent, string buttonText, int index)
        {
            // Try to find an existing button to copy style from
            Button templateButton = GameObject.Find("MenuCanvas/MenuParent/RootPage")
                ?.GetComponentInChildren<Button>();

            GameObject buttonObj = new GameObject($"Setting_{buttonText}");
            buttonObj.transform.SetParent(parent, false);

            // Add image component
            Image buttonImage = buttonObj.AddComponent<Image>();
            if (templateButton != null && templateButton.GetComponent<Image>() != null)
            {
                // Copy style from template button
                buttonImage.sprite = templateButton.GetComponent<Image>().sprite;
                buttonImage.type = templateButton.GetComponent<Image>().type;
                buttonImage.color = templateButton.GetComponent<Image>().color;
            }
            else
            {
                buttonImage.color = new Color(1f, 1f, 1f, 0.9f);
            }

            // Configure RectTransform
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.sizeDelta = new Vector2(-40, 50);
            rectTransform.anchoredPosition = new Vector2(0, -60 - (index * 60));

            // Add button component
            Button button = buttonObj.AddComponent<Button>();
            if (templateButton != null)
            {
                // Copy colors from template button
                button.colors = templateButton.colors;
            }
            else
            {
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(1f, 1f, 1f, 0.9f);
                colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                button.colors = colors;
            }

            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = buttonText;
            text.alignment = TextAnchor.MiddleCenter;

            if (templateButton != null && templateButton.GetComponentInChildren<Text>() != null)
            {
                // Copy text style from template
                Text templateText = templateButton.GetComponentInChildren<Text>();
                text.font = templateText.font;
                text.fontSize = templateText.fontSize;
                text.color = templateText.color;
            }
            else
            {
                text.color = Color.black;
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.fontSize = 16;
            }

            RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;

            // Add click handler
            button.onClick.AddListener((UnityEngine.Events.UnityAction)delegate
            {
                MelonLogger.Msg($"Settings button clicked: {buttonText}");
                switch (buttonText)
                {
                    case "API Key":
                        // Handle API key setting
                        break;
                    case "Toggle Chat":
                        // Handle chat toggle
                        break;
                    case "Clear History":
                        // Handle history clearing
                        break;
                }
            });
        }
        private void CreateMenuButton()
        {
            try
            {
                Transform contentTransform = GameObject.Find("MenuCanvas/MenuParent/RootPage/Scroll View/Viewport/Content")?.transform;
                if (contentTransform != null)
                {
                    Button existingButton = contentTransform.GetComponentInChildren<Button>();
                    if (existingButton != null)
                    {
                        GameObject buttonObj = GameObject.Instantiate(existingButton.gameObject, contentTransform);
                        buttonObj.name = "MateChatButton";
                        buttonObj.transform.SetSiblingIndex(0);

                        Text[] texts = buttonObj.GetComponentsInChildren<Text>(true);
                        foreach (Text text in texts)
                        {
                            text.text = "MateChat";
                        }

                        Button button = buttonObj.GetComponent<Button>();
                        button.onClick = new Button.ButtonClickedEvent();
                        button.onClick.AddListener((UnityEngine.Events.UnityAction)delegate
                        {
                            if (menuManager != null)
                            {
                                Component[] components = menuManager.GetComponents<Component>();
                                if (components.Length > 1)
                                {
                                    components[1].SendMessage("Open", SendMessageOptions.DontRequireReceiver);
                                }
                            }
                            rootPageObj.SetActive(false);
                            mateChatPage.SetActive(true);
                        });
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error creating button: {ex.Message}\nStack trace: {ex.StackTrace}");
            }
        }
    }
}
