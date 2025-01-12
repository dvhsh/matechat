using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

namespace matechat.ui.Components
{
    public class UIButton
    {
        private readonly GameObject buttonObject;
        private readonly Button button;
        private readonly Image buttonImage;

        public UIButton(GameObject template, Transform parent, string name)
        {
            buttonObject = GameObject.Instantiate(template, parent);
            buttonObject.name = name;
            button = buttonObject.GetComponent<Button>();
            buttonImage = buttonObject.GetComponent<Image>();
        }

        public void SetSprite(Sprite sprite)
        {
            if (buttonImage != null)
            {
                buttonImage.sprite = sprite;
                buttonImage.type = Image.Type.Simple;
                buttonImage.preserveAspect = true;
                buttonImage.color = Color.white;
            }
        }

        public void SetClickHandler(UnityAction action)
        {
            if (button != null)
            {
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(action);
            }
        }

        public void SetPosition(Vector2 position)
        {
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.pivot = new Vector2(0.5f, 1);
                rect.anchoredPosition = position;
            }
        }

        public void ClearText()
        {
            foreach (Text text in buttonObject.GetComponentsInChildren<Text>())
            {
                GameObject.Destroy(text.gameObject);
            }
        }
    }
}
