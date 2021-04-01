using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePlayer : MonoBehaviour
{
    public GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {
        var obj = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        obj.name = "Player";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
