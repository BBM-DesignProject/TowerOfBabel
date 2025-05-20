using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
	public Transform cam;

    void Start()
    {
        // Eğer 'cam' Inspector'dan atanmamışsa, ana kamerayı bul ve ata.
        if (cam == null)
        {
            if (Camera.main != null)
            {
                cam = Camera.main.transform;
            }
            else
            {
                Debug.LogError("Billboard: Main Camera not found in the scene. Please ensure a camera is tagged as 'MainCamera'.", this);
            }
        }
    }

    void LateUpdate()
    {
        if (cam != null) // Kamera atanmışsa çalış
        {
            transform.LookAt(transform.position + cam.forward);
        }
    }
}
