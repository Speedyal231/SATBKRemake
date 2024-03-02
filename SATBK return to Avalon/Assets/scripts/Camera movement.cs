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
    Vector3 currCamPos;

    private void Awake()
    {
        //enable player input script.
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }

    private void Start()
    {
        Vector3 playerPos = player.transform.position + player.transform.up * 2;
        camera.rotation = Quaternion.LookRotation(playerPos - camera.position, player.transform.up);
        currCamPos = -player.transform.forward * camDistance;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //CamFollow();
        Vector2 look = GetLookVectorNormalized();
        CamLook2(look);
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
        camera.position = newpos;
        camera.RotateAround(playerPos, player.transform.up, look.x * lookDistance);
        camera.RotateAround(playerPos, camera.right, look.y * lookDistance);
    }

    private void CamLook2(Vector2 look)
    {
        Vector3 playerPos = player.transform.position + player.transform.up * 2;

        //may need to flip x and y
        float angleX = look.x * lookDistance;
        float catenaX = 2 * camDistance * Mathf.Sin(angleX / 2);
        Vector3 xChange = camera.forward.normalized * catenaX * Mathf.Cos(90 - (angleX / 2)) + camera.right.normalized * catenaX * Mathf.Sin(90 - (angleX / 2));

        float angleY = look.y * lookDistance;
        float catenaY = 2 * camDistance * Mathf.Sin(angleY / 2);
        Vector3 YChange = camera.forward.normalized * catenaY * Mathf.Cos(90 - (angleY / 2)) + camera.up.normalized * catenaY * Mathf.Sin(90 - (angleY / 2));

        Vector3 totalChange = YChange + xChange;

        currCamPos += totalChange;

        camera.position = player.transform.position + currCamPos;
        camera.rotation = Quaternion.LookRotation(playerPos - camera.position, player.transform.up);
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
