using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMove : MonoBehaviour
{
    public TileGenerate tileGenerate;
    // Start is called before the first frame update
    void Start()
    {
        tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>();
    }

    // Update is called once per frame
    void Update()
    {

        if(gameObject.activeSelf == true)
        {
            transform.position += new Vector3(0, 0, -tileGenerate.tileSpeed * Time.deltaTime);
        }
        
    }
}
