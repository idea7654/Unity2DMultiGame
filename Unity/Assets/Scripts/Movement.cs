using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Movement : MonoBehaviour
{
    private float moveSpeed;
    private Vector3 moveDirection;

    public struct Player
    {
        public string message;
        public float speed;
        public Vector3 direction;
        public float time;
    };
    public Player player;

    //Manager_network manager_network = GameObject.Find("Player").GetComponent<Manager_network>();
    //Debug.Log(manager_network.)
    private void Awake(){
        moveSpeed = 5.0f;   
    }

    private void Update(){
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(x, y, 0);
        // transform.position = transform.position + new Vector3(1, 0, 0) * 1;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        player.message = "PlayerMove";
        //player.x = transform.position.x;
        //player.y = transform.position.y;
        player.speed = moveSpeed;
        player.direction = moveDirection;
        GameObject.Find("Main Camera").GetComponent<Manager_network>().ServerSend(player);
    }
}
