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
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Is Trigger가 체크되어 있을 경우 사용
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(streetLightTag))
        {
            // 로직은 동일
            Debug.Log("Contact via Trigger!");
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;

            transform.SetParent(other.transform);
            player.isHooked = true;
        }
    }
}
