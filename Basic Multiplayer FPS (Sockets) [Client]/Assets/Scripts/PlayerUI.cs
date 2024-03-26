using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    private Camera _camera;
    public UnityEngine.UI.Text nameText;
    
    private void Start()
    {
        _camera = Camera.main;
    }

    public void SetName(string name)
    {
        nameText.text = name;
    }
    
    void LateUpdate()
    {
        transform.LookAt(_camera.transform);
    }
}
