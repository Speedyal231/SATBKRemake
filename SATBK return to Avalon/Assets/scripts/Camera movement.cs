using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cameramovement : MonoBehaviour
{
    
    [SerializeField] GameObject player;
    [SerializeField] Transform camera;
    [SerializeField] float camSpeed;
 
    // Update is called once per frame
    void Update()
    {
        Vector3 pb = -player.transform.forward.normalized;
        camera.position = player.transform.position + pb * 5 + player.transform.up.normalized * 3 ;
        Vector3 lookpos = Vector3.Slerp(player.transform.forward, camera.forward, camSpeed) ;
        camera.rotation = Quaternion.LookRotation(lookpos, player.transform.up);
    }
}
