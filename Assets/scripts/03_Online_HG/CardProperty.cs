using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CardKind {
    BANANA,
    LIME,
    PLUM,
    STRAWBERRY
}

public class CardProperty
{
    //TODO 카드 크기에 따른 포지션 값 설정으로 변경

    #region Create Singleton

    private static CardProperty _instance;

    public static CardProperty Instance {
        get {
            if (_instance == null) {
                _instance = new CardProperty();
            }

            return _instance;
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// 초기 포지션
    /// </summary>
    private Vector3[] _initPos = new Vector3[] {
        new Vector3(-495, 246),
        new Vector3(495, 246),
        new Vector3(495, -246),
        new Vector3(-495, -246),
    };

    public Vector3[] InitPos {
        get {
            return _initPos;
        }
    }

    /// <summary>
    /// 옮겨질 포지션
    /// </summary>
    private Vector3[] _movedPos = new Vector3[] {
        new Vector3(-195, 125),
        new Vector3(195, 125),
        new Vector3(195, -125),
        new Vector3(-195, -125),
    };

    public Vector3[] MovedPos {
        get {
            return _movedPos;
        }
    }

    /// <summary>
    /// 각 포지션의 각도
    /// </summary>
    private Vector3[] _initAngle = new Vector3[] {
        new Vector3(0, 0, 240),
        new Vector3(0, 0, 120),
        new Vector3(0, 0, 60),
        new Vector3(0, 0, 300),
    };

    public Vector3[] InitAngle {
        get {
            return _initAngle;
        }
    }

    /// <summary>
    /// 과일 이미지 로컬 포지션
    /// </summary>
    private Vector3[] _fruitPos = new Vector3[] {
        new Vector3(-31.25f, 48.75f),
        new Vector3(31.25f, 48.75f),
        new Vector3(0, 0),
        new Vector3(-31.25f, -48.75f),
        new Vector3(31.25f, 48.75f),
    };

    public Vector3[] FruitPos {
        get {
            return _fruitPos;
        }
    }

    /// <summary>
    /// 카드 개수에 따른 위치 인덱스 값
    /// </summary>
    private int[][] _fruitByNum = new int[][] {
        new int[] { 2 },
        new int[] { 0, 4 },
        new int[] { 0, 2, 4 },
        new int[] { 0, 1, 3, 4 },
        new int[] { 0, 1, 2, 3, 4 },
    };

    public int[][] FruitByNum {
        get {
            return _fruitByNum;
        }
    }

    private float _flipS = 0.15f;

    public float FlipS {
        get {
            return _flipS;
        }
    }

    private WaitForSeconds _flipWS = new WaitForSeconds(0.15f);

    public WaitForSeconds FlipWS {
        get {
            return _flipWS;
        }
    }

    private float _stepS = 0.0125f;

    public float StepS {
        get {
            return _stepS;
        }
    }

    private WaitForSeconds _stepWS = new WaitForSeconds(0.0125f);

    public WaitForSeconds StepWS {
        get {
            return _stepWS;
        }
    }


    /// <summary>
    /// 이미지 리소스 
    /// </summary>
    Sprite Banana = Resources.Load<Sprite>("Sprites/Fruit/Banana");
    Sprite Lime = Resources.Load<Sprite>("Sprites/Fruit/Lime");
    Sprite Plum = Resources.Load<Sprite>("Sprites/Fruit/Plum");
    Sprite Strawberry = Resources.Load<Sprite>("Sprites/Fruit/Strawberry");

    #endregion

    #region Card Setting

    /// <summary>
    /// Properties 로 카드 초기 위치값, 과일 이미지, 과일 개수 설정
    /// </summary>
    /// <param name="card"></param>
    /// <param name="info"></param>
    /// <param name="playerOrd"></param>
    public void InitCard(GameObject card, CardInfo info, int playerOrd) {
        // 카드 위치 및 각도 설정
        card.transform.localPosition = _initPos[playerOrd];
        card.transform.localEulerAngles = _initAngle[playerOrd];

        // 백 페이지에 카드 정보에 따른 이미지 설정하기
        Transform backPage = card.transform.GetChild(0).GetChild(1);
        
        for(int i = 0; i < _fruitByNum[info.num].Length; i++) {
            Image image = backPage.GetChild(_fruitByNum[info.num][i]).GetComponent<Image>();

            switch (info.kind) {
                case CardKind.BANANA:
                    image.sprite = Banana;
                    break;
                case CardKind.LIME:
                    image.sprite = Lime;
                    break;
                case CardKind.PLUM:
                    image.sprite = Plum;
                    break;
                case CardKind.STRAWBERRY:
                    image.sprite = Strawberry;
                    break;
            }
        }

        Card cardScript = card.GetComponent<Card>();
        cardScript.MovedPR = _movedPos[playerOrd];
        cardScript.info = info;
    }

    /// <summary>
    /// Properties 로 카드 초기 위치값 설정
    /// 카드 과일 정보를 함께 초기화 시킬 필요가 없을 때를 위해 오버로드
    /// </summary>
    /// <param name="card"></param>
    /// <param name="playerOrd"></param>
    public void InitCard(GameObject card, int playerOrd) {
        // 카드 위치 및 각도 설정
        card.transform.localPosition = _initPos[playerOrd];
        card.transform.localEulerAngles = _initAngle[playerOrd];
    }

    #endregion
}
