using MelonLoader;

using UnityEngine;
using UnityEngine.UI;
using MelonLoader.Utils;

using System.Collections;

namespace matechat.ui.Pages
{
    public class SettingsPage
    {
        private readonly GameObject pageObject;
        private readonly GameObject contentObject;
        private readonly GameObject rootPageObj;
        private readonly GameObject menuManager;
        private readonly GameObject uniWindowController;

        public SettingsPage(Transform menuParent, GameObject rootPage,
                            GameObject menuManager,
                            GameObject uniWindowController)
        {
            this.rootPageObj = rootPage;
            this.menuManager = menuManager;
            this.uniWindowController = uniWindowController;

            pageObject = CreatePageObject(menuParent, rootPage);
            contentObject = CreateContentObject();
            InitializeButtons();
            CreateVersionInfo();
        }

        private GameObject CreatePageObject(Transform menuParent,
                                            GameObject rootPage)
        {
            var page = new GameObject("MateChatSettingsPage");
            page.transform.SetParent(menuParent, false);

            var rootRT = rootPage.GetComponent<RectTransform>();
            var pageRT = page.AddComponent<RectTransform>();
            pageRT.anchorMin = rootRT.anchorMin;
            pageRT.anchorMax = rootRT.anchorMax;
            pageRT.pivot = rootRT.pivot;
            pageRT.sizeDelta = rootRT.sizeDelta;
            pageRT.anchoredPosition = rootRT.anchoredPosition;

            CopyComponent(rootPage.transform, "Background", page.transform);
            SetupNavigationButtons(rootPage.transform, page.transform);

            page.SetActive(false);
            return page;
        }

        private void SetupNavigationButtons(Transform sourcePage,
                                            Transform targetParent)
        {
            SetupButton(sourcePage, "BackButton", targetParent,
                        (UnityEngine.Events.UnityAction)delegate {
                            if (menuManager != null)
                            {
                                Component[] components =
                              menuManager.GetComponents<Component>();
                                if (components.Length > 1)
                                {
                                    components[1].SendMessage(
                                  "Back", SendMessageOptions.DontRequireReceiver);
                                }
                            }
                            Hide();
                            rootPageObj.SetActive(true);
                        });

            SetupButton(sourcePage, "CloseButton", targetParent,
                        (UnityEngine.Events.UnityAction)delegate {
                            if (uniWindowController != null)
                            {
                                Component[] components =
                              uniWindowController.GetComponents<Component>();
                                if (components.Length > 1)
                                {
                                    components[1].SendMessage(
                                  "Close", SendMessageOptions.DontRequireReceiver);
                                }
                            }
                            MelonCoroutines.Start(DelayedClose());
                        });
        }

        private void SetupButton(Transform sourcePage, string buttonName,
                                 Transform targetParent,
                                 UnityEngine.Events.UnityAction action)
        {
            Transform originalButton = sourcePage.Find(buttonName);
            if (originalButton != null)
            {
                GameObject buttonCopy =
                    GameObject.Instantiate(originalButton.gameObject, targetParent);
                buttonCopy.name = buttonName;
                Button button = buttonCopy.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick = new Button.ButtonClickedEvent();
                    button.onClick.AddListener(action);
                }
            }
        }

        private GameObject CreateContentObject()
        {
            GameObject contentObj = new GameObject("SettingsContent");
            contentObj.transform.SetParent(pageObject.transform, false);

            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(20, 20);
            contentRect.offsetMax = new Vector2(-20, -60);

            return contentObj;
        }

        private void InitializeButtons()
        {
            Transform contentTransform =
                GameObject.Find(Constants.UI.CONTENT_PATH)?.transform;
            Button templateButton =
                contentTransform?.GetComponentInChildren<Button>();

            if (templateButton != null)
            {
                CreateConfigButton(templateButton);
                CreateReloadButton(templateButton);
            }
        }

        private void CreateConfigButton(Button template)
        {
            var button =
                CreateSettingsButton(template, "ConfigButton",
                                     EmbeddedAssets.EditButton, new Vector2(0, 0));
            button.onClick.AddListener((UnityEngine.Events.UnityAction)delegate {
                string configPath =
                    Path.Combine(MelonEnvironment.UserDataDirectory, "MateChat.cfg");
                try
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "notepad.exe",
                        Arguments = $"\"{configPath}\"",
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(startInfo);
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Error($"Failed to open config: {ex.Message}");
                }
            });
        }

        private void CreateReloadButton(Button template)
        {
            var button = CreateSettingsButton(template, "ReloadButton",
                                              EmbeddedAssets.ReloadButton,
                                              new Vector2(0, -60));
            button.onClick.AddListener(
                (UnityEngine.Events.UnityAction)delegate { Config.ReloadConfig(); });
        }

        private Button CreateSettingsButton(Button template, string name,
                                            byte[] spriteData, Vector2 position)
        {
            GameObject buttonObj =
                GameObject.Instantiate(template.gameObject, contentObject.transform);
            buttonObj.name = name;

            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = EmbeddedAssets.LoadButtonSprite(spriteData);
                buttonImage.type = Image.Type.Simple;
                buttonImage.preserveAspect = true;
                buttonImage.color = Color.white;
            }

            foreach (Text text in buttonObj.GetComponentsInChildren<Text>())
            {
                GameObject.Destroy(text.gameObject);
            }

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = template.GetComponent<RectTransform>().sizeDelta;
            rect.anchoredPosition = position;

            return buttonObj.GetComponent<Button>();
        }

        private void CreateVersionInfo()
        {
            CreateInfoText(contentObject.transform, $"MateChat v{Constants.VERSION}",
                           2);
        }

        private void CreateInfoText(Transform parent, string text, int index)
        {
            GameObject textObj = new GameObject("InfoText");
            textObj.transform.SetParent(parent, false);

            Text uiText = textObj.AddComponent<Text>();
            uiText.text = text;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            uiText.fontSize = 16;

            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.sizeDelta = new Vector2(-40, 30);
            rectTransform.anchoredPosition = new Vector2(0, 20);
        }

        private void CopyComponent(Transform sourceParent, string childName,
                                   Transform targetParent)
        {
            Transform originalTransform = sourceParent.Find(childName);
            if (originalTransform != null)
            {
                GameObject copiedObject =
                    GameObject.Instantiate(originalTransform.gameObject, targetParent);
                copiedObject.name = childName;
            }
        }

        private IEnumerator DelayedClose()
        {
            yield return new WaitForSeconds(0.3f);
            Hide();
            rootPageObj.SetActive(false);
        }

        public void Show()
        {
            pageObject.SetActive(true);
        }

        public void Hide()
        {
            pageObject.SetActive(false);
        }
    }
}
