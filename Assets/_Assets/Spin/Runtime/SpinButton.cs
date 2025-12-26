using UnityEngine;
using UnityEngine.UI;

public class SpinButton : MonoBehaviour
{
    [SerializeField] private Button button;

    public void HandleButtonStartSpinning()
    {
        this.button.interactable = false;
    }

    public void HandleButtonWhileSpinning()
    {
        
    }

    public void HandleButtonStopSpinning()
    {
        this.button.interactable = true;
    }
}
