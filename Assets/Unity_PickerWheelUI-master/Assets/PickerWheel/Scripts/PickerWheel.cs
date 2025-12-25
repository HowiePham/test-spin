using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace EasyUI.PickerWheelUI
{
    public class PickerWheel : MonoBehaviour
    {
        [Header("References :")] [SerializeField]
        private GameObject linePrefab;

        [SerializeField] private Transform linesParent;

        [Space] [SerializeField] private Transform PickerWheelTransform;
        [SerializeField] private Transform wheelCircle;
        [SerializeField] private GameObject wheelPiecePrefab;
        [SerializeField] private Transform wheelPiecesParent;
        [SerializeField] private Transform wheelIndicator;

        [Space] [Header("Sounds :")] [SerializeField]
        private AudioSource audioSource;

        [SerializeField] private AudioClip tickAudioClip;
        [SerializeField] [Range(0f, 1f)] private float volume = .5f;
        [SerializeField] [Range(-3f, 3f)] private float pitch = 1f;

        [Space] [Header("Picker wheel settings :")] [SerializeField]
        private Vector2 pieceMinSize = new Vector2(81f, 146f);

        [SerializeField] private Vector2 pieceMaxSize = new Vector2(144f, 213f);
        [SerializeField] private int piecesMin = 2;
        [SerializeField] private int piecesMax = 12;
        [Range(1, 20)] public int spinDuration = 8;
        [SerializeField] [Range(1f, 100f)] private int numberOfRound = 1;
        [SerializeField] [Range(.2f, 2f)] private float wheelSize = 1f;
        [SerializeField] private bool isSpinning;
        [SerializeField] private AnimationCurve animationCurve;

        [Space] [Header("Picker wheel pieces :")]
        public WheelPiece[] wheelPieces;

        [Space] [Header("Wheel Indicator :")] [SerializeField]
        private float shakeAmplitude;

        [SerializeField] private float shakeDuration;

        private UnityAction onSpinStartEvent;
        private UnityAction<WheelPiece> onSpinEndEvent;
        private float _currentLerpRotationTime;
        private WheelPiece piece;
        private Tween _indicatorTween;
        private float startAngle;
        private float finalAngle;
        private float prevAngle;
        private bool isIndicatorOnTheLine;
        private float pieceAngle;
        private float halfPieceAngle;
        private float halfPieceAngleWithPaddings;
        private double accumulatedWeight;
        private System.Random rand = new System.Random();

        private List<int> nonZeroChancesIndices = new List<int>();

        private void Start()
        {
            ChangeFPS();
            pieceAngle = 360 / wheelPieces.Length;
            halfPieceAngle = pieceAngle / 2f;
            halfPieceAngleWithPaddings = halfPieceAngle - (halfPieceAngle / 10f);

            Generate();

            CalculateWeightsAndIndices();
            if (nonZeroChancesIndices.Count == 0)
                Debug.LogError("You can't set all pieces chance to zero");


            SetupAudio();
        }

        private void SetupAudio()
        {
            audioSource.clip = tickAudioClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
        }

        private void Generate()
        {
            wheelPiecePrefab = InstantiatePiece();

            RectTransform rt = wheelPiecePrefab.transform.GetChild(0).GetComponent<RectTransform>();
            float pieceWidth = Mathf.Lerp(pieceMinSize.x, pieceMaxSize.x, 1f - Mathf.InverseLerp(piecesMin, piecesMax, wheelPieces.Length));
            float pieceHeight = Mathf.Lerp(pieceMinSize.y, pieceMaxSize.y, 1f - Mathf.InverseLerp(piecesMin, piecesMax, wheelPieces.Length));
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pieceWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pieceHeight);

            for (int i = 0; i < wheelPieces.Length; i++)
                DrawPiece(i);

            Destroy(wheelPiecePrefab);
        }

        private void DrawPiece(int index)
        {
            WheelPiece piece = wheelPieces[index];
            Transform pieceTrns = InstantiatePiece().transform.GetChild(0);

            pieceTrns.GetChild(0).GetComponent<Image>().sprite = piece.Icon;
            pieceTrns.GetChild(1).GetComponent<Text>().text = piece.Label;
            pieceTrns.GetChild(2).GetComponent<Text>().text = piece.Amount.ToString();

            //Line
            Transform lineTrns = Instantiate(linePrefab, linesParent.position, Quaternion.identity, linesParent).transform;
            lineTrns.RotateAround(wheelPiecesParent.position, Vector3.back, (pieceAngle * index) + halfPieceAngle);

            pieceTrns.RotateAround(wheelPiecesParent.position, Vector3.back, pieceAngle * index);
        }

        private GameObject InstantiatePiece()
        {
            return Instantiate(wheelPiecePrefab, wheelPiecesParent.position, Quaternion.identity, wheelPiecesParent);
        }


        private void ShakeIndicator()
        {
            if (_indicatorTween != null && _indicatorTween.IsActive())
            {
                wheelIndicator.localEulerAngles = Vector3.zero;
                _indicatorTween.Kill();
            }

            _indicatorTween = this.wheelIndicator
                .DOLocalRotate(new Vector3(0, 0, shakeAmplitude), shakeDuration / 2)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad);
        }

        public void Spin()
        {
            if (onSpinStartEvent != null)
                onSpinStartEvent.Invoke();

            int index = GetRandomPieceIndex();
            piece = wheelPieces[index];

            if (piece.Chance == 0 && nonZeroChancesIndices.Count != 0)
            {
                index = nonZeroChancesIndices[Random.Range(0, nonZeroChancesIndices.Count)];
                piece = wheelPieces[index];
            }

            float angle = -(pieceAngle * index);

            float rightOffset = (angle - halfPieceAngleWithPaddings) % 360;
            float leftOffset = (angle + halfPieceAngleWithPaddings) % 360;

            float randomAngle = Random.Range(leftOffset, rightOffset);

            Vector3 targetRotation = Vector3.back * (randomAngle + this.numberOfRound * 360 * spinDuration);
            this.finalAngle = targetRotation.z;
            this.startAngle = this.wheelCircle.eulerAngles.z;

            _currentLerpRotationTime = 0f;
            this.isSpinning = true;
        }

        private void Update()
        {
            if (!this.isSpinning)
            {
                return;
            }

            _currentLerpRotationTime += Time.deltaTime;

            if (_currentLerpRotationTime > this.spinDuration)
            {
                _currentLerpRotationTime = this.spinDuration;
                isSpinning = false;
                onSpinEndEvent?.Invoke(this.piece);
            }
            else
            {
                float t = _currentLerpRotationTime / this.spinDuration;
                float easedT = this.animationCurve.Evaluate(t);
                float angle = Mathf.Lerp(startAngle, finalAngle, easedT);
                wheelCircle.transform.eulerAngles = new Vector3(0, 0, angle);
                CheckPassSegment();
            }
        }

        private void CheckPassSegment()
        {
            var currentAngle = wheelCircle.eulerAngles.z;
            float diff = Mathf.Abs(prevAngle - currentAngle);
            if (diff >= halfPieceAngle)
            {
                if (isIndicatorOnTheLine)
                {
                    audioSource.PlayOneShot(audioSource.clip);
                    ShakeIndicator();
                }

                // Debug.Log($"--- (ROTATION) Feedback at angle: {currentAngle}");
                prevAngle = currentAngle;
                isIndicatorOnTheLine = !isIndicatorOnTheLine;
            }
        }

        public void OnSpinStart(UnityAction action)
        {
            onSpinStartEvent = action;
        }

        public void OnSpinEnd(UnityAction<WheelPiece> action)
        {
            onSpinEndEvent = action;
        }


        private int GetRandomPieceIndex()
        {
            double r = rand.NextDouble() * accumulatedWeight;

            for (int i = 0; i < wheelPieces.Length; i++)
                if (wheelPieces[i]._weight >= r)
                    return i;

            return 0;
        }

        private void CalculateWeightsAndIndices()
        {
            for (int i = 0; i < wheelPieces.Length; i++)
            {
                WheelPiece piece = wheelPieces[i];

                //add weights:
                accumulatedWeight += piece.Chance;
                piece._weight = accumulatedWeight;

                //add index :
                piece.Index = i;

                //save non zero chance indices:
                if (piece.Chance > 0)
                    nonZeroChancesIndices.Add(i);
            }
        }


        private void OnValidate()
        {
            if (PickerWheelTransform != null)
                PickerWheelTransform.localScale = new Vector3(wheelSize, wheelSize, 1f);

            if (wheelPieces.Length > piecesMax || wheelPieces.Length < piecesMin)
                Debug.LogError("[ PickerWheelwheel ]  pieces length must be between " + piecesMin + " and " + piecesMax);
        }

        [SerializeField] private int targetFPS;

        [ContextMenu("Change FPS")]
        private void ChangeFPS()
        {
            Application.targetFrameRate = this.targetFPS;
        }
    }
}