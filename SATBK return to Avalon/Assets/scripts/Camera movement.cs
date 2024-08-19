using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Cameramovement : MonoBehaviour
{
    
    [SerializeField] GameObject player;
    [SerializeField] Transform camera;
    [SerializeField, Range(0,1)] float camSpeed;
    [SerializeField] float camDistance;
    [SerializeField] float camUpMax;
    [SerializeField] float lookSpeed;
    [SerializeField] float lookDistance;
    [SerializeField] bool yinvert;
    [SerializeField] float lockedCamTime;
    private PlayerInputActions playerInputActions;
    Vector3 currCamPos;
    float currentLockedCamTime;

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
        Count();
        Vector2 look = GetLookVectorNormalized();
        camChangeInit(look);
        CamLook2(look);
        CamCorrection();
    }

    private void CamFollow()
    {
        Vector3 playerPos = player.transform.position + player.transform.up * 2;
        Vector3 newpos = Vector3.Slerp(camera.position, camera.position + (playerPos - camera.position) - (playerPos - camera.position).normalized * camDistance, camSpeed);
        Vector3 upoffset = Vector3.ProjectOnPlane(playerPos - newpos, Vector3.ProjectOnPlane(-(playerPos - newpos), player.transform.up));

        newpos = Vector3.Slerp(newpos, newpos - player.transform.up * upoffset.magnitude + player.transform.up * camUpMax, 0.1f);
        currCamPos = playerPos - camera.position;
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
        Vector3 xChange = camera.forward.normalized * (camDistance * (1 - Mathf.Cos(angleX))) + camera.right.normalized * (camDistance * Mathf.Sin(angleX));

        float angleY = look.y * lookDistance;
        float catenaY = 2 * camDistance * Mathf.Sin(angleY / 2);
        Vector3 yChange = camera.forward.normalized * (camDistance * (1 - Mathf.Cos(angleY))) + camera.up.normalized * (camDistance * Mathf.Sin(angleY));

        Vector3 totalChange = yChange + xChange;

        currCamPos += totalChange;

        camera.position = player.transform.position + currCamPos;
        //camera.position = Vector3.Slerp(camera.position, player.transform.position + currCamPos, camSpeed);
        camera.rotation = Quaternion.LookRotation(playerPos - camera.position, player.transform.up);
    }

    private void CamCorrection()
    {
        Vector3 playerPos = player.transform.position + player.transform.up * 2;
        if ((playerPos - camera.position).magnitude != camDistance)
        {
            camera.position += camera.forward.normalized * ((playerPos - camera.position).magnitude - camDistance);
        }
    }

    private Vector2 GetLookVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Keyboard.CameraLook.ReadValue<Vector2>() / 100;
        //Debug.Log("pre calc vector" + inputVector);
        inputVector = inputVector.magnitude > 1 ? inputVector.normalized : inputVector;
        Debug.Log("post calc vector" + inputVector);
        if (yinvert)
        {
            return new Vector3(inputVector.x,-inputVector.y);
        } 
        else
        {
            return inputVector;
        }
    }

    private void camChangeInit(Vector2 Look) 
    {
        if ( Look.magnitude > 0.05)
        {
            currentLockedCamTime = lockedCamTime;
        }
    }

    private void Count()
    {
        if (currentLockedCamTime > 0)
            currentLockedCamTime -= Time.fixedDeltaTime;
    }
}
