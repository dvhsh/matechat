using MelonLoader;

using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using matechat.ui.Pages;

namespace matechat.ui
{
    public class MenuManager
    {
        private SettingsPage settingsPage;
        private GameObject rootPageObj;
        private GameObject menuManager;
        private GameObject uniWindowController;

        public IEnumerator WaitForMenu()
        {
            Transform menuParent = null;
            while (menuParent == null)
            {
                menuParent = GameObject.Find(Constants.UI.MENU_CANVAS_PATH)?.transform;
                yield return new WaitForSeconds(0.1f);
            }

            InitializeMenuReferences();
            CreateSettingsPage(menuParent);
            CreateMenuButton();
        }

        private void InitializeMenuReferences()
        {
            menuManager = GameObject.Find("MenuManager");
            uniWindowController = GameObject.Find("UniWindowController");
        }

        private void CreateSettingsPage(Transform menuParent)
        {
            Transform rootPage = menuParent.Find("RootPage");
            if (rootPage != null)
            {
                rootPageObj = rootPage.gameObject;
                settingsPage = new SettingsPage(menuParent, rootPageObj, menuManager,
                                                uniWindowController);
            }
        }
        private void CreateMenuButton()
        {
            try
            {
                Transform contentTransform =
                    GameObject.Find(Constants.UI.CONTENT_PATH)?.transform;
                if (contentTransform != null)
                {
                    Button existingButton =
                        contentTransform.GetComponentInChildren<Button>();
                    if (existingButton != null)
                    {
                        GameObject buttonObj = GameObject.Instantiate(
                            existingButton.gameObject, contentTransform);
                        buttonObj.name = "MateChatButton";
                        buttonObj.transform.SetSiblingIndex(0);

                        Image buttonImage = buttonObj.GetComponent<Image>();
                        if (buttonImage != null)
                        {
                            buttonImage.sprite = EmbeddedAssets.LoadButtonSprite(Resource1.MateChat_Vocaloid);
                            buttonImage.type = Image.Type.Simple;
                            buttonImage.preserveAspect = true;
                        }

                        foreach (Text text in buttonObj.GetComponentsInChildren<Text>(
                                     true))
                        {
                            text.text = "MateChat";
                        }

                        Button button = buttonObj.GetComponent<Button>();
                        button.onClick = new Button.ButtonClickedEvent();
                        button.onClick.AddListener((
                            UnityEngine.Events.UnityAction)delegate {
                                if (menuManager != null)
                                {
                                    Component[] components = menuManager.GetComponents<Component>();
                                    if (components.Length > 1)
                                    {
                                        components[1].SendMessage(
                                "Open", SendMessageOptions.DontRequireReceiver);
                                    }
                                }
                                rootPageObj.SetActive(false);
                                settingsPage.Show();
                            });
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error(
                    $"Error creating button: {ex.Message}\nStack trace: {ex.StackTrace}");
            }
        }
    }
}
