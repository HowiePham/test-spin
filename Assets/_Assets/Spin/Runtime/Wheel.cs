using System;
using System.Collections.Generic;
using DG.Tweening;
using EasyUI.PickerWheelUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Wheel : MonoBehaviour
{
    [Header("References :")] [SerializeField]
    private GameObject linePrefab;

    [SerializeField] private Transform linesParent;

    [Space] [SerializeField] private Transform pickerWheelTransform;
    [SerializeField] private Transform wheelCircle;
    [SerializeField] private GameObject wheelPiecePrefab;
    [SerializeField] private Transform wheelPiecesParent;

    [Space] [SerializeField] private WheelIndicator wheelIndicator;
    [SerializeField] private SpinButton spinButton;
    [SerializeField] private SpinResultPanel spinResultPanel;

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

    public UnityEvent OnSpinStartEvent;
    public UnityEvent OnPassingSegment;
    public UnityEvent OnSpinEndEvent;
    public UnityEvent<WheelPiece> OnRewardEvent;
    private float currentLerpRotationTime;
    private WheelPiece piece;
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
        this.pieceAngle = 360 / this.wheelPieces.Length;
        this.halfPieceAngle = this.pieceAngle / 2f;
        this.halfPieceAngleWithPaddings = this.halfPieceAngle - (this.halfPieceAngle / 10f);

        Generate();

        CalculateWeightsAndIndices();
        if (this.nonZeroChancesIndices.Count == 0)
            Debug.LogError("You can't set all pieces chance to zero");
    }

    private void OnEnable()
    {
        ListenEvent();
    }

    private void OnDisable()
    {
        StopListeningEvent();
    }

    private void StopListeningEvent()
    {
        this.OnPassingSegment.RemoveAllListeners();
        this.OnSpinStartEvent.RemoveAllListeners();
        this.OnSpinEndEvent.RemoveAllListeners();
        this.OnRewardEvent.RemoveAllListeners();
    }

    private void ListenEvent()
    {
        if (this.wheelIndicator != null)
        {
            this.OnPassingSegment.AddListener(this.wheelIndicator.ShakeIndicator);
        }

        if (this.spinButton != null)
        {
            this.OnSpinStartEvent.AddListener(this.spinButton.HandleButtonStartSpinning);
            this.OnSpinEndEvent.AddListener(this.spinButton.HandleButtonStopSpinning);
        }

        if (this.spinResultPanel != null)
        {
            this.OnRewardEvent.AddListener(this.spinResultPanel.ShowResult);
        }
    }

    private void Generate()
    {
        this.wheelPiecePrefab = InstantiatePiece();

        var rt = this.wheelPiecePrefab.transform.GetChild(0).GetComponent<RectTransform>();
        float pieceWidth = Mathf.Lerp(this.pieceMinSize.x, this.pieceMaxSize.x, 1f - Mathf.InverseLerp(this.piecesMin, this.piecesMax, this.wheelPieces.Length));
        float pieceHeight = Mathf.Lerp(this.pieceMinSize.y, this.pieceMaxSize.y, 1f - Mathf.InverseLerp(this.piecesMin, this.piecesMax, this.wheelPieces.Length));
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pieceWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pieceHeight);

        for (int i = 0; i < this.wheelPieces.Length; i++)
            DrawPiece(i);

        Destroy(this.wheelPiecePrefab);
    }

    private void DrawPiece(int index)
    {
        WheelPiece piece = this.wheelPieces[index];
        Transform pieceTransform = InstantiatePiece().transform.GetChild(0);

        pieceTransform.GetChild(0).GetComponent<Image>().sprite = piece.icon;
        pieceTransform.GetChild(1).GetComponent<Text>().text = piece.label;
        pieceTransform.GetChild(2).GetComponent<Text>().text = piece.Amount.ToString();

        Transform lineTransform = Instantiate(this.linePrefab, this.linesParent.position, Quaternion.identity, this.linesParent).transform;
        lineTransform.RotateAround(this.wheelPiecesParent.position, Vector3.back, (this.pieceAngle * index) + this.halfPieceAngle);

        pieceTransform.RotateAround(this.wheelPiecesParent.position, Vector3.back, this.pieceAngle * index);
    }

    private GameObject InstantiatePiece()
    {
        return Instantiate(this.wheelPiecePrefab, this.wheelPiecesParent.position, Quaternion.identity, this.wheelPiecesParent);
    }


    public void Spin()
    {
        if (this.OnSpinStartEvent != null)
            this.OnSpinStartEvent.Invoke();

        int index = GetRandomPieceIndex();
        this.piece = wheelPieces[index];

        if (this.piece.Chance == 0 && nonZeroChancesIndices.Count != 0)
        {
            index = nonZeroChancesIndices[Random.Range(0, this.nonZeroChancesIndices.Count)];
            this.piece = this.wheelPieces[index];
        }

        float angle = -(this.pieceAngle * index);

        float rightOffset = (angle - this.halfPieceAngleWithPaddings) % 360;
        float leftOffset = (angle + this.halfPieceAngleWithPaddings) % 360;

        float randomAngle = Random.Range(leftOffset, rightOffset);

        Vector3 targetRotation = Vector3.back * (randomAngle + this.numberOfRound * 360 * this.spinDuration);
        this.finalAngle = targetRotation.z;
        this.startAngle = this.wheelCircle.eulerAngles.z;

        this.currentLerpRotationTime = 0f;
        this.isSpinning = true;
    }

    private void Update()
    {
        if (!this.isSpinning)
        {
            return;
        }

        this.currentLerpRotationTime += Time.deltaTime;

        if (this.currentLerpRotationTime > this.spinDuration)
        {
            this.currentLerpRotationTime = this.spinDuration;
            this.isSpinning = false;
            this.OnSpinEndEvent?.Invoke();
            this.OnRewardEvent?.Invoke(this.piece);
        }
        else
        {
            float t = this.currentLerpRotationTime / this.spinDuration;
            float easedT = this.animationCurve.Evaluate(t);
            float angle = Mathf.Lerp(this.startAngle, this.finalAngle, easedT);
            this.wheelCircle.transform.eulerAngles = new Vector3(0, 0, angle);
            CheckPassSegment();
        }
    }

    private void CheckPassSegment()
    {
        float currentAngle = this.wheelCircle.eulerAngles.z;
        float diff = Mathf.Abs(this.prevAngle - currentAngle);
        if (diff >= this.halfPieceAngle)
        {
            if (this.isIndicatorOnTheLine)
            {
                this.OnPassingSegment?.Invoke();
            }

            this.prevAngle = currentAngle;
            this.isIndicatorOnTheLine = !this.isIndicatorOnTheLine;
        }
    }

    private int GetRandomPieceIndex()
    {
        double r = this.rand.NextDouble() * this.accumulatedWeight;

        for (var i = 0; i < this.wheelPieces.Length; i++)
            if (this.wheelPieces[i].Weight >= r)
                return i;

        return 0;
    }

    private void CalculateWeightsAndIndices()
    {
        for (int i = 0; i < wheelPieces.Length; i++)
        {
            WheelPiece piece = this.wheelPieces[i];

            this.accumulatedWeight += piece.Chance;
            piece.Weight = this.accumulatedWeight;

            piece.Index = i;

            if (piece.Chance > 0)
                this.nonZeroChancesIndices.Add(i);
        }
    }


    private void OnValidate()
    {
        if (this.pickerWheelTransform != null)
            this.pickerWheelTransform.localScale = new Vector3(this.wheelSize, this.wheelSize, 1f);

        if (this.wheelPieces.Length > this.piecesMax || this.wheelPieces.Length < this.piecesMin)
            Debug.LogError("[ PickerWheelwheel ]  pieces length must be between " + this.piecesMin + " and " + this.piecesMax);
    }

    [SerializeField] private int targetFPS;

    [ContextMenu("Change FPS")]
    private void ChangeFPS()
    {
        Application.targetFrameRate = this.targetFPS;
    }
}