using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cameramovement : MonoBehaviour
{
    
    [SerializeField] GameObject player;
    [SerializeField] Transform camera;
    [SerializeField] float camSpeed;
    [SerializeField] float camDistance;
    [SerializeField] float camUpMax;
 
    // Update is called once per frame
    void FixedUpdate()
    {
        CamFollow();
    }

    private void CamFollow()
    {
        Vector3 playerPos = player.transform.position + player.transform.up * 2;
        Vector3 newpos = Vector3.Slerp(camera.position, camera.position + (playerPos - camera.position) - (playerPos - camera.position).normalized * camDistance, camSpeed);
        Vector3 upoffset = Vector3.ProjectOnPlane(playerPos - newpos, Vector3.ProjectOnPlane(-(playerPos - newpos), player.transform.up));

        newpos = newpos - player.transform.up * upoffset.magnitude + player.transform.up * camUpMax;
        camera.position = newpos;
        camera.rotation = Quaternion.LookRotation(playerPos - camera.position, player.transform.up);
    }
}
