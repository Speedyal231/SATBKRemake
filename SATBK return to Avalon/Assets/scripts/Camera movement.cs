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
    [SerializeField] float lookSpeed;
    [SerializeField] float lookDistance;
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        //enable player input script.
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //CamFollow();
        Vector2 look = GetLookVectorNormalized();
        CamLook(look);
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

    private void CamLook(Vector2 look)
    {
        Vector3 playerPos = player.transform.position + player.transform.up * 2;
        camera.rotation = Quaternion.LookRotation(playerPos - camera.position, player.transform.up);

        Vector3 newpos = camera.position + (playerPos - camera.position) - (playerPos - camera.position).normalized * camDistance;

        //camera.position = newpos;
        camera.RotateAround(playerPos, player.transform.up, look.x * lookDistance);
        camera.RotateAround(playerPos, player.transform.right, look.y * lookDistance);
        

    }

    private Vector2 GetLookVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Keyboard.CameraLook.ReadValue<Vector2>() / 100;
        //Debug.Log("pre calc vector" + inputVector);
        inputVector = inputVector.magnitude > 1 ? inputVector.normalized : inputVector;
        Debug.Log("post calc vector" + inputVector);
        return inputVector;
    }
}
