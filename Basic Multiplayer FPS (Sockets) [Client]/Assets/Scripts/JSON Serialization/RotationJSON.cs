using System;
using UnityEngine;

[Serializable]
public class RotationJSON
{
    //for just sending if only rotation data si changed
    public float[] rotation;
    [SerializeField]public string clientId;
    [SerializeField]public string name;
    
    public RotationJSON(Quaternion _rotation, string _id)
    {
        clientId = _id;
        rotation = new float[]
        {
            _rotation.eulerAngles.x,
            _rotation.eulerAngles.y,
            _rotation.eulerAngles.z
        };
    }
}