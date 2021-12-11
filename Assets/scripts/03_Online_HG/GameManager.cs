using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public struct CardInfo {
    public CardKind kind;
    public int num;

    public CardInfo(CardKind kind, int num) {
        this.kind = kind;
        this.num = num;
    }
}

public class GameManager : MonoBehaviourPunCallbacks
{

    #region Singleton

    private static GameManager _instance;

    public static GameManager Instance {
        get {
            if(_instance == null)
                _instance = FindObjectOfType(typeof(GameManager)) as GameManager;

            return _instance;
        }
    }

    #endregion

    #region SerialField

    [SerializeField]
    public GameObject Card; // 카드 프리팹
    [SerializeField]
    public Canvas Canvas;   // 카드 부모 캔버스
    [SerializeField]
    public Image Panel;     // 카드 부모 패널
    [SerializeField]
    public Text State;      // 상태 텍스트
    [SerializeField]
    private List<GameObject> Players;   // 플레이어

    #endregion

    #region Objects For Game Playing

    // 카드 56장 게임 매니저에서 초기화 밑 섞어 플레이어들에게 나누어줌
    private List<CardInfo> CardInfos;

    // 각 플레이어가 낸 카드들을 저장
    public Stack<GameObject> PlayedCards;

    // CardInfos 에 필요한 세팅들
    // 할리갈리 카드 종류(과일 및 과일 개수)
    // 과일 종류 - 바나나, 딸기, 라임(키위), 자두(체리)
    // 1개 - 5장, 2개 - 3장, 3개 - 3장, 4개 - 2장, 5개 - 1장
    private CardKind[] CardKinds = new CardKind[] { CardKind.BANANA, CardKind.LIME, CardKind.PLUM, CardKind.STRAWBERRY };
    private int[] CardNum = new int[] { 5, 3, 3, 2, 1 };

    // 현재 턴 플레이어
    int turn = 0;
    // 임시적으로 현재 조작하는 플레이어
    int player = 0;

    // 타이머 코루틴
    private Coroutine TimerRoutine;

    #endregion

    #region MonoBehaviour

    void Awake() {
        if (_instance == null)
            _instance = this;
        else if(_instance != this)
            Destroy(gameObject);

        Init();
    }

    void Start() {
        print("게임 시작");

        GameStart();
    }

    #endregion

    #region Methods For Game Playing

    /// <summary>
    /// 카드 초기화
    /// </summary>
    private void Init() {
        // CardInfos 초기화
        CardInfos = new List<CardInfo>();
        PlayedCards = new Stack<GameObject>();

        // 종류 및 개수로 초기화
        for (int i = 0; i < CardKinds.Length; i++) {
            for (int j = 0; j < CardNum.Length; j++) {
                for (int k = 0; k < CardNum[j]; k++) {
                    CardInfo card = new CardInfo(CardKinds[i], j);
                    CardInfos.Add(card);
                }
            }
        }
    }

    /// <summary>
    /// 게임 시작
    /// 1. 카드를 섞음
    /// 2. 카드를 각 플레이어 4명에게 14장씩 나누어줌
    /// 3. 각 플레이어는 카드를 추가함
    /// 4. 게임시작 루틴 시작
    /// </summary>
    private void GameStart() {
        Shuffle<CardInfo>(CardInfos);

        for(int i = 0; i < CardInfos.Count; i++) {
            Players[i % 4].GetComponent<Player>().PlayerCards.Enqueue(CardInfos[i]);
        }

        Players.ForEach(x => { x.GetComponent<Player>().AddPlayerCard(); x.GetComponent<Player>().ChangeState(); });

        StartCoroutine(GameStartRoutine());
    }

    /// <summary>
    /// 2초 후 게임
    /// 타이머 루틴 시작
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameStartRoutine() {
        State.text = "게임 스타트!";

        yield return new WaitForSeconds(2f);

        TimerRoutine = StartCoroutine("TimerStart");
    }

    /// <summary>
    /// 타이머 루틴
    /// 1. 현재 턴의 플레이어는 카드를 추가
    /// 2. 5초 동안 카드를 뒤집을 수 있는 상호작용 가능
    /// 3. 5초 후 아직 상호작용 false 후, 카드 강제로 뒤집기
    /// 4. 턴 1 더함
    /// 5. 타이머 루틴 재시작
    /// </summary>
    /// <returns></returns>
    private IEnumerator TimerStart() {
        Player player = Players[turn].GetComponent<Player>();
        player.AddPlayerCard();
        
        yield return new WaitForSeconds(0.15f);

        PlayedCards.Push(player.CurCard);

        for (int i = 0; i < 5; i++) {
            State.text = 5 - i + "";

            yield return new WaitForSeconds(1f);
        }

        Card card = player.CurCard.GetComponent<Card>();
        card.interactable = false;
        card.FlipForceForward();

        if (player.PlayerCards.Count == 0 && player.WaitingCard == null) {
            Players.Remove(player.gameObject);
            if (Players.Count == turn) turn = 0;
        } else turn++;
        if (turn == Players.Count) turn = 0;
        print(turn);
        TimerRoutine = StartCoroutine("TimerStart");
    }

    /// <summary>
    /// 벨 눌렀을 때의 코루틴
    /// </summary>
    private Coroutine Bell;

    public void OnClickBell() {
        Bell = StartCoroutine(RingBell());
    }

    /// <summary>
    /// 어느 플레이어가 종을 쳤을 때의 메소드
    /// 1. TimerRoutine 를 멈춘다.
    /// 2. 현재 턴 플레이어의 카드를 강제로 뒤집음
    /// 3-1. 카드 개수가 맞았다면 종을 친 플레이어에게 카드 주어짐
    /// 3-2. 종을 친 플레이어는 다른 플레이어에게 1장씩 줌
    /// 4. 이때 카드 개수가 0개인 플레이어는 플레이어 수에서 없앰
    /// 5. 다시 진행
    /// </summary>
    private IEnumerator RingBell() {
        // TimerRoutine 을 멈춤
        StopCoroutine(TimerRoutine);
        State.text = player + 1 + " 플레이어가 종을 침";

        // 현재 턴 플레이어의 카드를 강제로 뒤집음
        Card card = Players[turn].GetComponent<Player>().CurCard.GetComponent<Card>();
        card.interactable = false;
        card.FlipForceForward();

        yield return new WaitForSeconds(0.15f);

        // 각 플레이어들의 현재 카드 정보를 가져와 더하면서 저장
        Dictionary<CardKind, int> dict = new Dictionary<CardKind, int>();
        for(int i = 0; i < Players.Count; i++) {
            Player player = Players[i].GetComponent<Player>();
            if (player.CurCard == null) continue;
            CardInfo info = player.CurCard.GetComponent<Card>().info;
            if (dict.ContainsKey(info.kind)) {  // 현재 과일 개수는 인덱스에 맞추기 위해 0부터 시작하니 1 더함
                dict[info.kind] += info.num + 1;
            } else {
                dict.Add(info.kind, info.num + 1);
            }
        }

        bool isCorrect = false;

        foreach(int x in dict.Values) {
            if (x == 5) isCorrect = true;
        }

        if (isCorrect) {
            Player player = Players[this.player].GetComponent<Player>();
            
            // 총 개수가 5개인 종류가 있다면
            while (PlayedCards.Count > 0) {
                Card temp = PlayedCards.Pop().GetComponent<Card>();
                temp.MoveToPlayer(this.player, null);
                player.Draw = temp.info;
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(0.3f);
            TimerRoutine = StartCoroutine("TimerStart");

            print("맞음");
        } else {
            Players.ForEach(x => {
                Player player = x.GetComponent<Player>();
                if (player.order != this.player) {
                    print(this.player);
                    player.Draw = Players[this.player].GetComponent<Player>().Draw;
                    player.AddTempCard(this.player);
                }
            });

            Player curPlay = Players[this.player].GetComponent<Player>();
            if (curPlay.PlayerCards.Count == 0 && curPlay.WaitingCard == null) {
                Players.Remove(curPlay.gameObject);
                if (Players.Count - 1 == turn) turn = 0;
            } else turn++;
            if (turn == Players.Count) turn = 0;

            print(turn);

            yield return new WaitForSeconds(0.5f);
            TimerRoutine = StartCoroutine("TimerStart");

            print("틀림");
        }
        
    }

    #endregion

    #region Temp For Test

    /// <summary>
    /// 임시 메소드 - 테스트를 위한 현재 씬 재로드
    /// </summary>
    public void SceneReload() {
        SceneManager.LoadScene("03_Online_HG", LoadSceneMode.Single);
    }

    /// <summary>
    /// 임시 메소드 - 테스트를 위한 플레이어 변경
    /// </summary>
    public void ChangePlayer(int temp) {
        player = temp;
    }

    #endregion

    #region Common Methods

    private static System.Random rng = new System.Random();

    private void Shuffle<T>(IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public float IntRound(float Value, int Digit) {
        float Temp = Mathf.Pow(10, Digit);
        return Mathf.Round(Value * Temp) / Temp;
    }

    #endregion

    #region Info

    /*
    [ContextMenu("남은 플레이어 카드 자리")]
    void printPR() {
        for(int i = 0; i < FixedPR.Count; i++) {
            int[] PR = FixedPR[i];
            print("좌표 X : " + PR[0] + "\t좌표 Y : " + PR[1] + "/t각도 Z : " + PR[2] + "/t플레이어" + PR[3]);
        }
    }
    */

    #endregion
}
