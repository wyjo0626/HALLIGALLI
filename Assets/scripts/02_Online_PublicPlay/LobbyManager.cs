using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";   // 게임 버전

    // Test

    [SerializeField]
    public Text state;
    [SerializeField]
    public Text[] Others;   // 다른 플레이어에 관한 정보 텍스트
    [SerializeField]
    public Text Player;     // 자신의 텍스트

    /// <summary>
    /// 씬 시작과 동시에 네트워크 접속 시도
    /// </summary>
    void Awake() {
        // 접속에 필요한 게임 버전 설정
        PhotonNetwork.GameVersion = gameVersion;
        // 플레이어 닉네임 설정
        PhotonNetwork.NickName = "Test Player";
        Player.text = PhotonNetwork.NickName;
        // 설정한 정보로 마스터 서버 접속 시도
        PhotonNetwork.ConnectUsingSettings();

        // 접속 중임을 텍스트로 표시
        state.text = "서버 접속 중 ...";
    }

    /// <summary>
    /// 마스터 서버 접속 실패 시 자동 실행
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause) {
        // 마스터 서버 재접속 시도
        PhotonNetwork.ConnectUsingSettings();

        // 재접속 정보 표시
        state.text = "서버와 연결되지 않음\n접속 재시도 중 ...";
    }

    /// <summary>
    /// 마스터 서버 접속 성공 시 자동 실행
    /// </summary>
    public override void OnConnectedToMaster() {
        // 랜덤 룸 접속 시도
        Connect();

        // 름 접속 중임을 텍스트로 표시
        state.text = "룸 접속 중 ...";
    }



    /// <summary>
    /// 룸 접속 시도
    /// </summary>
    public void Connect() {
        // 마스터 서버 접속 중
        if (PhotonNetwork.IsConnected) {
            // 룸 접속 실행
            PhotonNetwork.JoinRandomRoom();

            // 접속 중 정보 표시
            state.text = "룸 접속 ...";
        } else {
            // 마스터 서버에 접속 중이 아니면 마스터 서버 재접속
            PhotonNetwork.ConnectUsingSettings();

            // 재접속 정보 표시
            state.text = "서버와 연결되지 않음\n접속 재시도 중 ...";
        }
    }

    /// <summary>
    /// 빈 방이 없어 랜덤 룸 참가에 실패한 경우
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRandomFailed(short returnCode, string message) {
        // 최대 4명 수용할 빈 방 생성
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });

        // 상태 표시
        state.text = "방 생성 중";
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        state.text = "방 생성 실패 " + message;
    }

    public override void OnCreatedRoom() {
        // 상탱 표시
        state.text = "방 생성 완료";
    }

    /// <summary>
    /// 룸에 참가 완료 시 자동 실행
    /// </summary>
    public override void OnJoinedRoom() {
        // 접속 완료 표시
        state.text = "방 참가 성공";

        for(int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++) {
            Others[i].text = PhotonNetwork.PlayerListOthers[i].NickName;
        }

    }

    /*
    /// <summary>
    /// 원격 플레이어가 룸에 들어왔을 때
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer) {
        Others[PhotonNetwork.PlayerListOthers.Length - 1].text = newPlayer.NickName;
    }
    */

    [ContextMenu("포톤 정보")]
    private void PrintInfo() {
        if (PhotonNetwork.InRoom) {
            print("현재 방 이름 : " + PhotonNetwork.CurrentRoom.Name);
            print("현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount);
            print("현재 방 최대인원수 : " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "방에 있는 플레이어 목록 : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
            print(playerStr);
        } else {
            print("접속한 인원 수 : " + PhotonNetwork.CountOfPlayers);
            print("방 개수 : " + PhotonNetwork.CountOfRooms);
            print("모든 방에 있는 인원 수 : " + PhotonNetwork.CountOfPlayersInRooms);
            print("로비에 있는지? : " + PhotonNetwork.InLobby);
            print("방에 있는지? : " + PhotonNetwork.InRoom);
            print("연결됐는지? : " + PhotonNetwork.IsConnected);
        }
    }
}
