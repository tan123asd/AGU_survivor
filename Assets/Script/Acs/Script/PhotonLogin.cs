using UnityEngine;
using Photon.Pun;
using TMPro;

public class PhotonLogin : MonoBehaviourPunCallbacks
{
    public TMP_InputField inputUsername;
    public string nickName;

    [Header("UI Panels")]
    public GameObject step2Login;
    public GameObject step3Room;

    void Start()
    {
        this.nickName = "";
        this.inputUsername.text = this.nickName;

        if (step2Login) step2Login.SetActive(true);
        if (step3Room) step3Room.SetActive(false);
    }

    public virtual void OnChangeName()
    {
        this.nickName = this.inputUsername.text;
    }

    public virtual void Login()
    {
        string name = this.nickName;
        Debug.Log(transform.name + ": Login " + name);

        //PhotonNetwork.SendRate = 20;
        //PhotonNetwork.SerializationRate = 5;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LocalPlayer.NickName = name;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(transform.name + ": OnConnectedToMaster");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
        if (step2Login) step2Login.SetActive(false);
        if (step3Room) step3Room.SetActive(true);
    }
}
