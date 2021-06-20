using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

//This class is a link between the QRCode you click and the QRCode on the ModelToAlign. Turns out it was useless when I started networking. Got a headache from trying to make it work
public class QrCodeManager : NetworkBehaviour
{
    [SerializeField] private List<SerializableDictionary> qrCodes; //Unity has something against showing dictionaries in the editor, so I use this workaround
    
    [SyncVar] public NetworkIdentity modelToAlign;

    private readonly Dictionary<Transform, Transform> _actualQrCodeDic = new Dictionary<Transform, Transform>(); // Every QRCode has a corresponding QRCode, so the easy solution is to store it in a dictionary

    private void Start()
    {
        for (var i = 0; i < qrCodes.Count; i++)
        {
            _actualQrCodeDic.Add(qrCodes[i].key, qrCodes[i].value);
        }
    }
    
    [Command(requiresAuthority = false)]
    public void AlignModelToQrCode(NetworkIdentity qrCodeToAlignTo)
    {
        Transform qrCodeToAlignToTransform = qrCodeToAlignTo.transform;
        var qrCodeToAlignWith = _actualQrCodeDic[qrCodeToAlignToTransform];

        //Ill do this in two phases - The rotation phase and the position phase.

        //Rotation phase
        //Rotate the model so the QRCodeToAlignWith has the same rotation as QRCodeToAlignTo  
        Transform modelToAlignTransform = modelToAlign.transform;
        var rotationVar = qrCodeToAlignToTransform.rotation * Quaternion.Inverse(qrCodeToAlignWith.rotation); //The rotation we want to achieve times the inverse of our current rotation
        modelToAlignTransform.rotation = rotationVar * modelToAlignTransform.rotation; //simply rotate the parent to our desired rotation

        //Position phase
        //A vector contains a direction and a magnitude. So if I have a vector between the model and its qrcode, I know what the distance between the model and the actual qrcode needs to be
        var alignmentVector = modelToAlignTransform.position - qrCodeToAlignWith.position;
        modelToAlignTransform.position = qrCodeToAlignToTransform.position + alignmentVector; //since I already dealt with all the rotation, all that's left is to set the position
    }
}

[Serializable]
public struct SerializableDictionary
{
    public Transform key;
    public Transform value;
}