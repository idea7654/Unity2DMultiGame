using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Movement : MonoBehaviour
{
    private float moveSpeed;
    private Vector3 moveDirection;

    public enum DirectionEnum{
        stop,
        left,
        right,
        up,
        down
    };

    public class Direction{
        public float x = 0.0f;
        public float y = 0.0f;
    };
    public Direction playerDirection = new Direction();
    public Direction direction_check = new Direction();

    public struct Player
    {
        public string message;
        public float speed;
        public DirectionEnum direction;
        public int id;
        public float x;
        public float y;
        public double currentTime;
    };
    public Player player;

    public DirectionEnum playerDirectionEnum;
    public float x;
    public float y;

    //Manager_network manager_network = GameObject.Find("Player").GetComponent<Manager_network>();
    //Debug.Log(manager_network.)
    private void Awake(){
        moveSpeed = 5.0f;   
    }

    private void Update(){
        //float x = Input.GetAxisRaw("Horizontal");
        //float y = Input.GetAxisRaw("Vertical");
        if(Input.GetKey(KeyCode.LeftArrow)){
            x = -1.0f;
            y = 0.0f;
        }
        if(Input.GetKey(KeyCode.RightArrow)){
            x = 1.0f;
            y = 0.0f;
        }
        if(Input.GetKey(KeyCode.UpArrow)){
            x = 0.0f;
            y = 1.0f;
        }
        if(Input.GetKey(KeyCode.DownArrow)){
            x = 0.0f;
            y = -1.0f;
        }
        if(!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)){
            x = 0.0f;
            y = 0.0f;
        }

        moveDirection = new Vector3(x, y, 0);

        playerDirection.x = x;
        playerDirection.y = y;

        if(playerDirection.x != direction_check.x || playerDirection.y != direction_check.y){
            if(x == 0 && y == 0){
                playerDirectionEnum = DirectionEnum.stop;
            }else if(x == -1 && y == 0){
                playerDirectionEnum = DirectionEnum.left;
            }else if(x == 1 && y == 0){
                playerDirectionEnum = DirectionEnum.right;
            }else if(x == 0 && y == 1){
                playerDirectionEnum = DirectionEnum.up;
            }else{
                playerDirectionEnum = DirectionEnum.down;
            }
            player.message = "PlayerMove";
            player.direction = playerDirectionEnum;
            player.id = 1; //추후에 로그인 등으로 로직처리해야함
            player.speed = moveSpeed;
            player.x = transform.position.x;
            player.y = transform.position.y;
            player.currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            GameObject.Find("Main Camera").GetComponent<Manager_network>().ServerSend(player);
        }

        direction_check.x = x;
        direction_check.y = y;

        //transform.position += moveDirection * moveSpeed * Time.deltaTime;
        //player.message = "PlayerMove";
        //player.id = 1; //추후에 로그인 등으로 로직처리해야됨
        //player.x = transform.position.x;
        //player.y = transform.position.y;
        //player.speed = moveSpeed;
        //player.direction = moveDirection;
        //GameObject.Find("Main Camera").GetComponent<Manager_network>().ServerSend(player);
    }
}
