using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] Transform playerTransform;
    Vector3 cameraOffset;

    void Start()
    {
        cameraOffset = transform.position - playerTransform.position;
    }

    void LateUpdate()
    {
        Vector3 newPos = new Vector3(playerTransform.position.x + cameraOffset.x, playerTransform.position.y + cameraOffset.y, playerTransform.position.z + cameraOffset.z);

        transform.position = newPos;
    }
}
