using Network;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListView : MonoBehaviour
{
    private MyGameManager gameManager;
    private List<GameObject> roomItems;
    private int updatetime = 1;
    private DateTime time;


    public GameObject roomItem;
    public GameObject roomList;

    private void Start()
    {
        roomItems = new List<GameObject>();
        gameManager = GameObject.Find("GameController").GetComponent<MyGameManager>();
        gameManager.OnReceive += ReceiveMessage;
        time = DateTime.Now;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        if ((DateTime.Now - time).Seconds > updatetime)
        {
            time = DateTime.Now;
            gameManager.client.Send(new Packet()
                .Add(Property.Type, PacketType.Request)
                .Add(Property.TargetModule, "RoomsModule")
                .Add(Property.Method, "getList"));
        }
    }

    private void ReceiveMessage(Packet packet)
    {
        if (packet.Get<string>(Property.Method) != "getList") return;

        foreach (var item in roomItems) { Destroy(item); }

        foreach (var text in packet.Get<string[]>(Property.Data))
        {
            var newItem = Instantiate(roomItem, roomList.transform, false);
            Text data = newItem.GetComponentInChildren<Text>();
            data.text = text;

            newItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                gameManager.client.Send(new Packet()
                    .Add(Property.Type, PacketType.Request)
                    .Add(Property.TargetModule, "RoomsModule")
                    .Add(Property.Method, "join")
                    .Add(Property.Data, int.Parse(data.text.Split("|")[0])));
                Debug.Log("Enter room: " + data.text);
            });
            roomItems.Add(newItem);
        }
    }
}