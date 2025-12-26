using DG.Tweening;
using UnityEngine;

public class WheelIndicator : MonoBehaviour
{
    [SerializeField] private WheelEffect wheelEffect;
    [SerializeField] private float shakeAmplitude;
    [SerializeField] private float shakeDuration;
    [SerializeField] private bool reverseShakingDirection;
    private Tween indicatorTween;

    public void ShakeIndicator()
    {
        if (this.indicatorTween != null && this.indicatorTween.IsActive())
        {
            this.transform.localEulerAngles = Vector3.zero;
            this.indicatorTween.Kill();
        }

        if (this.wheelEffect != null)
        {
            this.wheelEffect.PlayAudio();
        }

        float shakeAmplitude = this.shakeAmplitude;

        if (this.reverseShakingDirection)
        {
            shakeAmplitude *= -1;
        }

        this.indicatorTween = this.transform
            .DOLocalRotate(new Vector3(0, 0, shakeAmplitude), this.shakeDuration / 2)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad);
    }
}