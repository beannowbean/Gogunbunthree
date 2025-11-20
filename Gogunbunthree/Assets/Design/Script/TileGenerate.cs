using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGenerate : MonoBehaviour
{
    public GameObject[] tiles;
    int tileNum;
    GameObject tile1;
    GameObject tile2;
    public float tileSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Tile")
        {
            Debug.Log("T");
            createNewTile(other);
        }
    }

    private void createNewTile(Collider oldTile)
    {
        Debug.Log("T");
        MeshCollider meshCollider = oldTile.gameObject.GetComponent<MeshCollider>();
        Bounds bounds = meshCollider.bounds;
        float oldTilePos = bounds.max.z;
        int curNum = tileNum;
        tileNum = Random.Range(0, tiles.Length);
        while(tileNum == curNum)
        {
            Random.Range(0, tiles.Length);
        }
        GameObject newTile = tiles[tileNum];
        oldTile.gameObject.SetActive(false);
        newTile.SetActive(true);
        MeshCollider newMeshCollider = newTile.GetComponent<MeshCollider>();
        float halfLength = newMeshCollider.bounds.extents.z;
        newTile.transform.position = new Vector3(oldTile.transform.position.x, oldTile.transform.position.y, 
            oldTilePos + halfLength);
    }
}
