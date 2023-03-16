using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] GameObject followed;

    void LateUpdate()
    {
        transform.position = new Vector3(followed.transform.position.x, followed.transform.position.y, transform.position.z);
    }
}
