using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCollider : MonoBehaviour
{
    public bool isCollided = false;
    public GameObject collided = null;
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
        collided = other.gameObject;
        isCollided = true;
    }
    private void OnTriggerExit(Collider other)
    {
        collided = null;
        isCollided = false;
    }

}
