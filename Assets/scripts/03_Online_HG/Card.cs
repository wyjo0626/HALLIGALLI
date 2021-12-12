using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;

public class Card : MonoBehaviourPun
{
    #region Field

    public bool interactable = false;    // 상호작용 여부
    public bool pageDragging = false;  // 드래깅 여부
    public bool fliped = false;     // 뒤집어진 여부
    public bool moved = false;

    public CardInfo info;

    public Image FrontPage;
    public Image BackPage;
    public Image ClippingPlane;

    public Canvas Canvas;
    public RectTransform CardPanel;

    float width;
    float height;
    float half;

    public Vector3 MovedPR;

    Vector3 top;
    Vector3 bottom;
    Vector3 f;      // 팔로잉 포인트

    private GameManager GM = null;
    private CardProperty CP = null;

    #endregion

    #region MonoBehaviour

    private void Awake() {
        GM = GameManager.Instance;
        CP = CardProperty.Instance;
        Canvas = GM.Canvas;
        transform.SetParent(GM.Panel.transform);

        if (!Canvas) Canvas = GetComponentInParent<Canvas>();
        if (!Canvas) Debug.LogError("카드는 캔버스의 자식이어야 합니다.");

        CalcCurlCriticalPoints();
    }

    void Update() {
        if (pageDragging && interactable) {
            UpdateCard();
        }
    }

    #endregion

    #region Methods

    public void CalcCurlCriticalPoints() {
        width = CardPanel.rect.width;
        height = CardPanel.rect.height;
        half = height / 2;
        top = new Vector3(0, half, 0);
        bottom = new Vector3(0, -half, 0);
    }



    // 마우스 포인트로 카드 넘기기
    public void UpdateCard() {
        f = Vector3.Lerp(f, transformPoint(Input.mousePosition), Time.deltaTime * 10);
        if (f.y <= half && f.y >= -half)
            UpdateCardToPoint(f);
    }

    
    // 카드 넘기기
    public void UpdateCardToPoint(Vector3 followLocation) {
        f = followLocation;
        BackPage.transform.localPosition = 
            new Vector3(0, half + followLocation.y, 0);
        FrontPage.transform.localPosition =
            new Vector3(0, half - followLocation.y, 0);
    }


    // 마우스 포인트 지점 반환
    public Vector3 transformPoint(Vector3 mouseScreenPos) {
        if (Canvas.renderMode == RenderMode.ScreenSpaceCamera) {
            Vector3 mouseWorldPos = Canvas.worldCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Canvas.planeDistance));
            Vector2 localPos = CardPanel.InverseTransformPoint(mouseWorldPos);

            return localPos;
        } else if (Canvas.renderMode == RenderMode.WorldSpace) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 globalEBR = transform.TransformPoint(new Vector3(width, height));
            Vector3 globalEBL = transform.TransformPoint(new Vector3(0, height));
            Vector3 globalSt = transform.TransformPoint(new Vector3(0, 0));
            Plane p = new Plane(globalEBR, globalEBL, globalSt);
            float distance;
            p.Raycast(ray, out distance);
            Vector2 localPos = CardPanel.InverseTransformPoint(ray.GetPoint(distance));
            return localPos;
        } else {
            //Screen Space Overlay
            Vector2 localPos = CardPanel.InverseTransformPoint(mouseScreenPos);
            return localPos;
        }
    }

    // 드래그 시작
    public void OnMouseDragBegin() {
        if (interactable) {
            pageDragging = true;
            f = transformPoint(Input.mousePosition);
        }
    }

    // 드래그 끝냈을 때
    public void OnMouseDragRelease() {
        if (interactable && pageDragging) {
            pageDragging = false;

            if (f.y >= half || f.y <= -half)
                f = new Vector3(0, BackPage.transform.localPosition.y - half);

            if (f.y <= 0)
                FlipForward();
            else
                FlipBack();

        }
    }

    #endregion

    #region Coroutines

    Coroutine currentCoroutine;
    
    // 드래그를 반틈 넘어서 땠을 때 자동으로 뒤로 넘기기
    public void FlipForward() {
        interactable = false;

        currentCoroutine = StartCoroutine(TweenTo(bottom, CP.FlipS, () => {
            pageDragging = false;
            fliped = true;
        }));
    }
    
    // 강제로 포워드 시키기
    public void FlipForceForward() {
        pageDragging = false;

        f = new Vector3(0, BackPage.transform.localPosition.y - half);
        FlipForward();
    }

    // 드래그를 반틈도 안되서 땠을 때 다시 뒤로 넘기기
    public void FlipBack() {
        currentCoroutine = StartCoroutine(TweenTo(top, CP.FlipS, () => {
            pageDragging = false;
        }));
    }

    // 위 또는 아래로 카드를 되돌림
    public IEnumerator TweenTo(Vector3 to, float duration, System.Action onFinish) {
        int steps = (int)(duration / CP.StepS);
        Vector3 displacement = (to - f) / steps;
        for(int i = 0; i < steps; i++) {
            UpdateCardToPoint(f + displacement);

            yield return CP.StepWS;
        }

        if (onFinish != null)
            onFinish();

    }

    public void MoveForward() {
        currentCoroutine = StartCoroutine(MoveTo(0.15f, () => {
            interactable = true;
            moved = true;
        }));
    }

    // 카드를 옮겨질 장소(카드를 내는 위치)로 움직임
    public IEnumerator MoveTo(float duration, System.Action onFinish) {
        int steps = (int)(duration / CP.StepS);

        float randomAngle = Random.Range(-5f, 5f);
        float angleSteps = randomAngle / steps;

        Vector3 displacement = (MovedPR - transform.localPosition) / steps;

        for (int i = 0; i < steps; i++) {
            transform.localPosition += displacement;
            transform.localRotation = transform.localRotation * Quaternion.Euler(new Vector3(0, 0, angleSteps));

            yield return CP.StepWS;
        }

        if (onFinish != null)
            onFinish();
    }

    public void MoveToPlayer(int player, System.Action action) {
        currentCoroutine = StartCoroutine(MoveToPlayerPos(player, 0.25f, action));
    }

    // 카드를 특정 플레이어 위치로 이동
    public IEnumerator MoveToPlayerPos(int player, float duration, System.Action onFinish) {
        int steps = (int)(duration / CP.StepS);

        Vector3 P_Pos = CP.InitPos[player];
        Vector3 P_Ang = CP.InitAngle[player];

        float My_Ang = GameManager.Instance.IntRound(transform.localEulerAngles.z, -1);

        float temp = Mathf.Abs(My_Ang + P_Ang.z);
        bool same_Ang = temp == 300 || temp == 420 || My_Ang == P_Ang.z;

        if (!same_Ang) {
            if (My_Ang == 240 && My_Ang == 60) {
                P_Ang = new Vector3(0, 0, My_Ang - 60);
            } else {
                P_Ang = new Vector3(0, 0, My_Ang + 60);
            }
        }

        Vector3 displacement = (P_Pos - transform.localPosition) / steps;
        Vector3 rotation = (P_Ang - transform.localEulerAngles) / steps;

        for(int i = 0; i < steps; i++) {
            transform.localPosition += displacement;
            transform.localEulerAngles += rotation;

            yield return CP.StepWS;
        }

        Destroy(gameObject);

        if (onFinish != null)
            onFinish();

    }

    #endregion
}
