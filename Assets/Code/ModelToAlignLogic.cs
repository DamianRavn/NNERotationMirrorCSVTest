using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class ModelToAlignLogic : NetworkBehaviour
{
    public Transform[] qrCodes;
    public NetworkIdentity thisNetworkIdentity;
    public Dictionary<uint, Transform> _correspondingQrCodes = new Dictionary<uint, Transform>();

    public Dictionary<uint, Transform> CorrespondingQrCodes
    {
        get
        {
            if (_correspondingQrCodes.Count == 0)
            {
                return PopulateDictionary();
            }
            else
            {
                return _correspondingQrCodes;
            }
        }
    }

    private Dictionary<uint, Transform> PopulateDictionary()
    {
        Transform QRCodesParent = GameObject.FindWithTag("QRCodesParent").transform;

        if (QRCodesParent.childCount != qrCodes.Length)
        {
            Debug.LogError("There needs to be an equal amount of QRCodes, otherwise nothing makes sense");
        }

        for (int i = 0; i < qrCodes.Length; i++)
        {
            _correspondingQrCodes.Add(QRCodesParent.GetChild(i).GetComponent<NetworkIdentity>().netId, qrCodes[i]);
        }
        
        return _correspondingQrCodes;
    }
}
