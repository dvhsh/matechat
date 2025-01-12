using MelonLoader;

using UnityEngine.UI;
using UnityEngine;

using System.Collections;

namespace matechat.ui
{
    public class MenuManager
    {
        private GameObject mateChatPage;
        private GameObject rootPageObj;

        private GameObject menuManager;
        private GameObject uniWindowController;

        public IEnumerator WaitForMenu()
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

        public void InitializeMenuReferences()
        {
            menuManager = GameObject.Find("MenuManager");
            uniWindowController = GameObject.Find("UniWindowController");
            MelonLogger.Msg($"Found MenuManager: {menuManager != null}, UniWindowController: {uniWindowController != null}");
        }

        public void CreateMateChatPage(Transform menuParent)
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

        public void CopyAndSetupButton(Transform sourcePage, string buttonName, Transform targetParent)
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

        public IEnumerator DelayedClose()
        {
            yield return new WaitForSeconds(0.3f);
            mateChatPage.SetActive(false);
            rootPageObj.SetActive(false);
        }


        public void CopyComponent(Transform sourceParent, string childName, Transform targetParent)
        {
            Transform originalTransform = sourceParent.Find(childName);
            if (originalTransform != null)
            {
                GameObject copiedObject = GameObject.Instantiate(originalTransform.gameObject, targetParent);
                copiedObject.name = childName;
                MelonLogger.Msg($"Copied {childName}");
            }
        }

        public void CreateSettingsContent(Transform parent)
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

        public void CreateSettingButton(Transform parent, string buttonText, int index)
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
        public void CreateMenuButton()
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
