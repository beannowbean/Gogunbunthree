using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public Player player;
    public string streetLightTag;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(streetLightTag))
        {
            Debug.Log("Contact!");
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;

            // 갈고리가 닿았을 때 위치가 가로등에 종속되도
            transform.SetParent(collision.transform);
            player.isHooked = true;
        }
    }
}
