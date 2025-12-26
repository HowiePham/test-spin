using EasyUI.PickerWheelUI;
using UnityEngine;
using UnityEngine.UI;

public class SpinResultPanel : MonoBehaviour
{
    [SerializeField] private Image resultImage;
    [SerializeField] private Text resultText;
    [SerializeField] private float delayTime;
    
    public void ShowResult(WheelPiece piece)
    {
        this.gameObject.SetActive(true);
        this.resultImage.sprite = piece.icon;
        this.resultText.text = piece.Amount.ToString();

        Invoke(nameof(Hide), this.delayTime);
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }
}