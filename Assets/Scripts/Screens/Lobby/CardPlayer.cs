using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Lobby {
    public class CardPlayer : MonoBehaviour {
        public ScreenLobby owner;
        public int index;
        public Image imgPick;
        public TextMeshPro textButton;
        public Button btnType;
        
        public void Setup(ScreenLobby owner, int index, Sprite sprIcon, string text, bool locked) {
            this.owner = owner;
            this.index = index;
            imgPick.sprite = sprIcon;
            textButton.text = text;
            btnType.enabled = !locked;
        }

        public void OnClick() {
            owner.OnCardButtonClick(index);
        }

        public void OnPrevClick() {
            owner.OnCardPrevHeroClick(index);
        }

        public void OnNextClick() {
            owner.OnCardNextHeroClick(index);
        }
    }
}
