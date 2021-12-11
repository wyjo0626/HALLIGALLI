using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Player : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// 플레이 정보
    /// 플레이어의 카드 뒷면, 카페트, 이펙트, 캐릭터에 관한 정보를 담고 있음
    /// + 각 플레이어가 조종할(뒤집을) 현재 카드와 다음에 조종할 카드 정보
    /// </summary>


    /// <summary>
    /// PlayerCards : 플레이어 카드 정보 큐
    /// CurCard     : 현재 플레이어가 조종할 카드 오브젝트
    /// WaitingCard : 플레이어의 다음 차례 카드 오브젝트
    /// </summary>
    public Queue<CardInfo> PlayerCards;
    public GameObject CurCard;
    public GameObject WaitingCard;
    public GameObject Dummy;

    /// <summary>
    /// 현 플레이어 카드 개수
    /// </summary>
    public Text State;

    /// <summary>
    /// 플레이어 차례(턴)
    /// </summary>
    public int order;

    void Awake()
    {
        PlayerCards = new Queue<CardInfo>();
    }

    public CardInfo Draw {
        get {
            CardInfo info = PlayerCards.Dequeue();
            ChangeState();
            if (PlayerCards.Count == 0) Destroy(Dummy);
            return info;
        }
        set {
            PlayerCards.Enqueue(value);
            ChangeState();
        }
    }

    /// <summary>
    /// 카드 생성
    /// WaitingCard 없다면(가장 최초) 카드 생성
    /// WaitingCard 가 있다면 CurCard 교체 후 카드 내기
    /// </summary>
    public void AddPlayerCard() {
        if (WaitingCard != null) CurCard = WaitingCard;
        if(PlayerCards.Count != 0) {
            WaitingCard = Instantiate(GameManager.Instance.Card, Vector3.zero, Quaternion.identity);
            CardProperty.Instance.InitCard(WaitingCard, Draw, order);
            WaitingCard.transform.localScale = Vector3.one;
        } else {
            WaitingCard = null;
        }
        if(CurCard != null) CurCard.GetComponent<Card>().MoveForward();
    }

    /// <summary>
    /// 임시 카드 생성
    /// 카드 오브젝트를 생성하여 종을 쳐 5개를 틀린 플레이어에게 카드를 주기 위한 메소드
    /// </summary>
    public void AddTempCard(int other) {
        GameObject temp = Instantiate(GameManager.Instance.Card, Vector3.zero, Quaternion.identity);
        CardProperty.Instance.InitCard(temp, other);
        temp.transform.localScale = Vector3.one;
        temp.GetComponent<Card>().MoveToPlayer(order, null);
    }

    public void ChangeState() {
        State.text = "" + (PlayerCards.Count + (WaitingCard == null ? 0 : 1));
    }

    [ContextMenu("Info")]
    void PrintInfo() {
        print("카드 장 수 : " + PlayerCards.Count);
        print("WaitingCard : " + WaitingCard);
        print("CurCard : " + CurCard);
    }
}
