using System;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class CharControllerMotor : NetworkBehaviour
{
    public float speed = 10.0f;
    public float sensitivity = 30.0f;
    public Camera cam;
    private CharacterController _character;
    private readonly float _gravity = -9.8f;
    private float _moveFb, _moveLr;
    private float _rotX, _rotY;
    
    public float yAxisAngleLock = 90f;
    private Transform _playerTransform;
 
    private Quaternion _playerTargetRot;
    private Quaternion _cameraTargetRot;

    [SerializeField] private CSVHandler csvHandler; //used to fetch the csv file
    [SerializeField] private UIHandler _uiHandler; //Handles the ui
    
    
    private bool interacting => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E);

    //[SyncVar] public uint modelToAlignNetId; Could not for the life of me get SyncVar to work, no idea why
    private ModelToAlignLogic _modelToAlignLogic;

    private void Start()
    {
        if (isLocalPlayer)
        {
            _playerTransform = transform;
            _playerTargetRot = _playerTransform.rotation;
            _cameraTargetRot = cam.transform.rotation;

            cam.enabled = true; //I had an issue where different players would all see from random cameras. this fixed the issue
            
            _character = GetComponent<CharacterController>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _modelToAlignLogic = FindObjectOfType<ModelToAlignLogic>();
            
            if (isServer)
            {
                NetworkServer.Spawn(_modelToAlignLogic.gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        Rigidbody rb = new Rigidbody();
        rb.AddForce(0,0,500*Time.deltaTime);
        rb.AddForce(500 * Time.deltaTime, 0, 0);
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            
            HandleMovement();
            HandleCameraRotation();
            
            RaycastLogic();
        }
    }

    private void RaycastLogic()
    {
        RaycastHit hit;
        var rayLength = 1.5f;
        var rayFromCamera = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        // Does the ray intersect any objects
        if (Physics.Raycast(rayFromCamera, out hit, rayLength))
        {
            Transform hitTransform = hit.transform;
            NetworkIdentity actualQrCode = null;
            if(hitTransform.parent != null) hitTransform.parent.TryGetComponent(out actualQrCode);
            if (actualQrCode != null && _modelToAlignLogic.CorrespondingQrCodes.ContainsKey(actualQrCode.netId))
            {
                RaycastingQRCode(actualQrCode);
            }
            else if (hitTransform.tag == "DataObject")
            {
                RaycastingDataObject(hitTransform);
            }
            else
            {
                _uiHandler.HandleUIState(UIState.none);
            }
        }
        else
        {
            _uiHandler.HandleUIState(UIState.none);
        }
    }

    private void RaycastingQRCode(NetworkIdentity actualQrCode)
    {
        if(_uiHandler._currentUIState != UIState.interactionDoneState) _uiHandler.HandleUIState(UIState.interactionState);
        if (interacting)
        {
            _uiHandler.HandleUIState(UIState.interactionDoneState);
            HandleModelMovement(actualQrCode.netId,  _modelToAlignLogic.thisNetworkIdentity.netId);
        }
    }

    private void RaycastingDataObject(Transform hitTransform)
    {
        if(_uiHandler._currentUIState != UIState.csvState) _uiHandler.HandleUIState(UIState.interactionState);
        if (interacting)
        {
            _uiHandler.HandleUIState(UIState.csvState);
            _uiHandler.UpdateCSVText("Fetching data...");
            csvHandler.HandleCSVFile(hitTransform.name, (string text)=> _uiHandler.UpdateCSVText(text));
        }
    }
    
    //Command means that the function will be run on the server
    [Command]
    private void HandleModelMovement(uint qrCodeToAlignToNetID, uint ModelToAlignNetID)
    {
        //Getting all the variables I need
        ModelToAlignLogic modelToAlignLogic = NetworkIdentity.spawned[ModelToAlignNetID].GetComponent<ModelToAlignLogic>();
        var _correspondingQrCodes = modelToAlignLogic.CorrespondingQrCodes;
        Transform qrCodeToAlignToTransform = NetworkIdentity.spawned[qrCodeToAlignToNetID].transform;
        var qrCodeToAlignWithTransform = _correspondingQrCodes[qrCodeToAlignToNetID];

        //Ill do this in two phases - The rotation phase and the position phase.

        //Rotation phase
        //Rotate the model so the QRCodeToAlignWith has the same rotation as QRCodeToAlignTo  
        Transform modelToAlignTransform = modelToAlignLogic.transform;
        var rotationDif = qrCodeToAlignToTransform.rotation * Quaternion.Inverse(qrCodeToAlignWithTransform.rotation); //The rotation we want to achieve times the inverse of our current rotation = Difference
        modelToAlignTransform.rotation = rotationDif * modelToAlignTransform.rotation; //simply rotate the parent to our desired rotation

        //Position phase
        //A vector contains a direction and a magnitude. So if I have a vector between the model and its qrcode, I know what the distance between the model and the actual qrcode needs to be
        var alignmentVector = modelToAlignTransform.position - qrCodeToAlignWithTransform.position;
        modelToAlignTransform.position = qrCodeToAlignToTransform.position + alignmentVector; //since I already dealt with all the rotation, all that's left is to set the position
    }
    
    private void HandleMovement()
    {
        _moveFb = Input.GetAxis("Horizontal") * speed;
        _moveLr = Input.GetAxis("Vertical") * speed;
        
        var movement = new Vector3(_moveFb, _gravity, _moveLr);

        movement = transform.rotation * movement;
        _character.Move(movement * Time.deltaTime);
    }

    private void HandleCameraRotation()
    {
        _rotX = Input.GetAxis("Mouse X") * sensitivity;
        _rotY = Input.GetAxis("Mouse Y") * sensitivity;
        
        _playerTargetRot *= Quaternion.Euler(0f, _rotX, 0f);
 
        _cameraTargetRot *= Quaternion.Euler(-_rotY, 0f, 0f);
 
        _cameraTargetRot = LockCameraMovement(_cameraTargetRot);
 
        _playerTransform.localRotation = _playerTargetRot;
        cam.transform.localRotation = _cameraTargetRot;
    }
 
    private Quaternion LockCameraMovement(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;
        var angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
 
        angleX = Mathf.Clamp(angleX, -yAxisAngleLock, yAxisAngleLock);
 
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        
        return q;
    }
}