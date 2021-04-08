using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System;
using System.Runtime.InteropServices;
using System.IO;
[Serializable]
public class Manager_network : MonoBehaviour
{
    byte[] rB = new byte[10000]; //버퍼의 크기
    Thread ServerCheck_thread; //서버에서 보내는 패킷을 체크하기 위한 스레드
    Queue<string> netBuffer = new Queue<string>(); //버퍼를 저장하기 위한 큐
    string strIP = "127.0.0.1";

    int port = 8000;
    Socket sock;
    IPAddress ip;
    IPEndPoint endPoint;

    public GameObject prefab;
    //public bool flag = true;

    // public class WorldObject
    // {
    //     public int id;
    //     public float x;
    //     public float y;
    //     private object remote;
    // }
    [Serializable]
    public class Players
    {
        public int id;
        public float x;
        public float y;
        public double currentTime;
        //private object remote;
    };
    public Players[] players;
    public Players[] players_forCheck;
    public class ClientPlayer : Players{
        public int direction;
    };

    public double latency;

    public ClientPlayer clientPlayer;

    struct Message{
        public string message;
    };

    object buffer_lock = new object(); //queue충돌 방지용 lock
    void Start()
    {
        serverOn();
        StartCoroutine(buffer_update());
    }
    void Update()
    {
        if(clientPlayer != null){
            GameObject playerPrefab = GameObject.Find("1");
            if(clientPlayer.direction == 1){
                //playerPrefab.transform.position += new Vector3(-1, 0, 0) * 5.0f * Time.deltaTime;
                playerPrefab.transform.position += new Vector3(-1, 0, 0) * 5.0f * Time.deltaTime;
            }
            if(clientPlayer.direction == 2){
                //playerPrefab.transform.position += new Vector3(-1, 0, 0) * 5.0f * Time.deltaTime;
                playerPrefab.transform.position += new Vector3(1, 0, 0) * 5.0f * Time.deltaTime;
            }
            if(clientPlayer.direction == 3){
                //playerPrefab.transform.position += new Vector3(-1, 0, 0) * 5.0f * Time.deltaTime;
                playerPrefab.transform.position += new Vector3(0, 1, 0) * 5.0f * Time.deltaTime;
            }
            if(clientPlayer.direction == 4){
                //playerPrefab.transform.position += new Vector3(-1, 0, 0) * 5.0f * Time.deltaTime;
                playerPrefab.transform.position += new Vector3(0, -1, 0) * 5.0f * Time.deltaTime;
            }
        }
    }

    IEnumerator buffer_update()
    {
        while(true)
        {
            yield return null; //코루틴에서 반복문 쓸수잇게해줌
            BufferSystem();
        }
    }

    public void serverOn()
    {
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        ip = IPAddress.Parse(strIP);
        endPoint = new IPEndPoint(ip, port);
        sock.Connect(endPoint);
        ServerCheck_thread = new Thread(ServerCheck);
        ServerCheck_thread.Start(); //서버 체크 시스템 시작, 스레드를 사용하지 않으면 서버체크 Receive에서 유니티가 멈춤
        SendConnect();
    }

    void ServerCheck()
    {
        while(true)
        {
            //Debug.Log("작동중");
            try{
                sock.Receive(rB, 0, rB.Length, SocketFlags.None);//서버에서 온 패킷을 버퍼에 담기
                string t = Encoding.Default.GetString(rB); //큐에 버퍼를 넣을 준비
                t = t.Replace("\0", string.Empty); //버퍼 마지막에 공백이 있는지 검색하고 공백을 삭제
                lock(buffer_lock){ //큐 충돌방지
                   netBuffer.Enqueue(t); //큐에 버퍼 저장
                }
                System.Array.Clear(rB, 0, rB.Length); //버퍼를 사용후 초기화
            }catch{
                Debug.Log("Error!!");
            }
        }
    }

    void BufferSystem()
    {
        while(netBuffer.Count != 0){ //큐의 크기가 0이 아니면 작동, 만약 while을 안하면 프레임마다 버퍼를 처리하는데
        //많은 패킷을 처리할 땐 처리되는 양보다 쌓이는 양이 많아져 작동이 제대로 이루어지지않음
            string b = null;
            lock(buffer_lock)
            {
                b = netBuffer.Dequeue();
            }
            // Debug.Log("server -> " + b); //버퍼를 사용
            int index = b.IndexOf("[");
            if(index == -1){
                StringToObj(b);
            }else{
                ArrToObj(b);
            }
        }
    }

    int euckrCodepage = 51949;

    public void ServerSend(object obj)
    {
        // string json = JsonUtility.ToJson(obj);
        // //Debug.Log(json);
        // byte[] playerByte = Encoding.UTF8.GetBytes(json);
        byte[] playerByte = ObjToByte(obj);
        sock.Send(playerByte, 0, playerByte.Length, SocketFlags.None);
    }

    public void SendConnect()
    {
        Message sendMessage;
        sendMessage.message = "Connect";
        byte[] message = ObjToByte(sendMessage);
        sock.Send(message, 0, message.Length, SocketFlags.None);
    }

    private byte[] ObjToByte(object obj){
        string json = JsonUtility.ToJson(obj);
        byte[] returnValue = Encoding.UTF8.GetBytes(json);
        return returnValue;
    }

    private void StringToObj(string recv){
        clientPlayer = JsonUtility.FromJson<ClientPlayer>(recv);
        //Debug.Log(clientPlayer.currentTime);
        latency = DateTime.Now.TimeOfDay.TotalMilliseconds - clientPlayer.currentTime;
        // Debug.Log(latency + "ms");
        moveCharacter(clientPlayer.x, clientPlayer.y, clientPlayer.direction);
        
    }

    private void moveCharacter(float x, float y, int direction){
        GameObject playerPrefab = GameObject.Find("1");
        if(direction == 0){
            playerPrefab.transform.position = playerPrefab.transform.position;
        }
        if(direction == 1){
            playerPrefab.transform.position = new Vector3(clientPlayer.x, clientPlayer.y, 0) + new Vector3(-1, 0, 0) * 5.0f * Convert.ToSingle(latency) / 1000.0f;
        }
        if(direction == 2){
            playerPrefab.transform.position = new Vector3(clientPlayer.x, clientPlayer.y, 0) + new Vector3(1, 0, 0) * 5.0f * Convert.ToSingle(latency) / 1000.0f;
        }
        if(direction == 3){
            playerPrefab.transform.position = new Vector3(clientPlayer.x, clientPlayer.y, 0) + new Vector3(0, 1, 0) * 5.0f * Convert.ToSingle(latency) / 1000.0f;
        }
        if(direction == 4){
            playerPrefab.transform.position = new Vector3(clientPlayer.x, clientPlayer.y, 0) + new Vector3(0, -1, 0) * 5.0f * Convert.ToSingle(latency) / 1000.0f;
        }
    }

    private void ArrToObj(string recv){
        // Debug.Log(GameObject.Find("Player").GetComponent<Movement>().player.x);
        
        string jsonString= fixJson(recv);
        players_forCheck = JsonHelper.FromJson<Players>(jsonString);
        if(players.Length != players_forCheck.Length){
            if(players.Length == 0){
                for(int i = 0; i < players_forCheck.Length; i++){
                    CreatePlayers(players_forCheck[i].id, new Vector3(players_forCheck[i].x, players_forCheck[i].y, 0));
                }
            }else{
                for(int i = 0; i < players_forCheck.Length; i++){
                    if(players[i].id != players_forCheck[i].id){
                        CreatePlayers(players_forCheck[i].id, new Vector3(players_forCheck[i].x, players_forCheck[i].y, 0));
                    }
                }
            }
        }
        players = players_forCheck;
        //Debug.Log(players[0].currentTime);
        UpdateCharacterPos();
    }

    public void UpdateCharacterPos(){
        GameObject playerPrefab = GameObject.Find("Player");
        //추후 토큰값으로 유저 판별기능 넣어야 구별가능
        Vector3 playerVector = new Vector3(players[0].x, players[0].y, 0);
        //Debug.Log(playerVector);
        // playerPrefab.transform.position = playerVector;
    }

    string fixJson(string value)
    {
        value = "{\"Items\":" + value + "}";
        return value;
    }

    private void OnApplicationQuit()
    {
        ServerCheck_thread.Abort();
        sock.Close();
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }

    public void CreatePlayers(int id, Vector3 position){
        var obj = Instantiate(prefab, position, Quaternion.identity);
        obj.name = id.ToString();
        //obj.Id = id;
    }
}