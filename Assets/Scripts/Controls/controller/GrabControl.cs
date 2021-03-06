using System;
using System.Collections;
using System.Collections.Generic;
using Controls;
using UnityEngine;
using UnityEngine.Events;

public class GrabControl : MonoBehaviour {
	public enum State
	{
		EMPTY,
		TOUCHING,
		HOLDING
	};

    public enum Gesture
    {
        EMPTY,
        FIST,
        OPENHAND
    };

  

    public OVRInput.Controller Controller = OVRInput.Controller.RTouch;

    public Gesture GestureState = Gesture.EMPTY;
    public State HandState = State.EMPTY;
    private State PrevHandState = State.EMPTY;
    private Gesture PrevGestureState = Gesture.EMPTY;

    public GrabControl OtherHand;
	
	private List<IInteractableObject> _interactableObjects;
	private IInteractableObject currentInteraction;
	
	private ControlsManager controls;

	public SphereCollider controllerSphereCollider;
    public VertexHandleController collidedVertexHandle;
    public FaceHandleController collidedFaceHandle;


    private float lastIndexFingerState = 0;

    public Color defaultColor;
    public Color hoverColor;

    private MeshRenderer _meshRenderer;

    private Vector3 prevPos;
    private Quaternion prevRot;

    private GameObject collidedObject;

    bool handsActive;

  //  public UnityEvent onRecognized;

    void OnEnable()
    {
        GestureDetector.OnShoutOutFistGesture += FistRecognized;
        GestureDetector.OnShoutOutOpenHandGesture += OpenHandRecognized;
        GestureDetector.OnShoutOutHandsActive += HandsActive;
        GestureDetector.OnShoutOutHandsNotActive += HandsNotActive;
    }

    void OnDisable()    
    {
        GestureDetector.OnShoutOutFistGesture -= FistRecognized;
        GestureDetector.OnShoutOutOpenHandGesture -= OpenHandRecognized;
        GestureDetector.OnShoutOutHandsActive -= HandsActive;
        GestureDetector.OnShoutOutHandsNotActive -= HandsNotActive;
    }

    void FistRecognized() => GestureState = Gesture.FIST;
    void OpenHandRecognized() => GestureState = Gesture.OPENHAND;
    void HandsActive() => handsActive = true;
    void HandsNotActive() => handsActive = false;



    void Awake()
    {
        _interactableObjects = new List<IInteractableObject>();
        controls = FindObjectOfType<ControlsManager>();
        controllerSphereCollider = transform.childCount > 0 ? transform.GetChild(0).GetComponent<SphereCollider>() : GetComponent<SphereCollider>();
 
        _meshRenderer = GetComponent<MeshRenderer>();
        defaultColor = _meshRenderer.material.color;
    }

    public IInteractableObject GetCurrentInteraction()
    {
        return currentInteraction;
    }

    private void UpdateHoverColor()
    {
        _meshRenderer.material.color = _interactableObjects.Count == 0 ? defaultColor : hoverColor;
    }

    void OnTriggerEnter(Collider collider)
    {
      
        collidedObject = collider.gameObject;
        var iobj = collider.GetComponent<IInteractableObject>();
        var colliderGameObject = collider.gameObject;

        if(colliderGameObject.GetComponent<VertexHandleController>() != null)
        {
            collidedVertexHandle = colliderGameObject.GetComponent<VertexHandleController>();
        }
        else if (colliderGameObject.GetComponent<FaceHandleController>() != null)
        {
            collidedFaceHandle = colliderGameObject.GetComponent<FaceHandleController>();
        }


        if (HandState == State.EMPTY || HandState == State.TOUCHING)
        {
            if (iobj != null)
            {
                

                if(colliderGameObject.GetComponent<ExtrudableController>() != null)
                {
                    _interactableObjects.Insert(0, iobj);
                }
                else
                {
                    if (_interactableObjects.Count > 0)
                    {
                        _interactableObjects[_interactableObjects.Count - 1].EndHighlight();
                    }
                    iobj.StartHighlight();
                    _interactableObjects.Add(iobj);
                }

                HandState = State.TOUCHING;
                UpdateHoverColor();
            }
        } 
        
    }

    void OnTriggerExit(Collider collider)
    {
        collidedObject = null;
        var iobj = collider.GetComponent<IInteractableObject>();
        if (iobj != null)
        {
            if (HandState != State.HOLDING)
            {
                if (_interactableObjects.Count > 0)
                {
                    
                    iobj.EndHighlight();
                    _interactableObjects.Remove(iobj);
                
                }
                if (_interactableObjects.Count > 0)
                {
                    _interactableObjects[_interactableObjects.Count-1].StartHighlight();
                }
                else
                {
                    HandState = State.EMPTY;
               //     GestureState = Gesture.EMPTY;
                }
            }
            else // remove while holding
            {
                _interactableObjects.Remove(iobj);
            }
            UpdateHoverColor();
        }

        collidedVertexHandle = null;
        collidedFaceHandle = null;

    }

    //This Trigger stay is only really here because a trigger enter is not detected when expanding a hitbox, which we do with the extrudable controller.
    //So when you extrude, you can be in the model with your hand and not move it, this check prevents that.
    void OnTriggerStay(Collider collider)
    {
        collidedObject = collider.gameObject;

        if(_interactableObjects.Count == 0)
        {
            OnTriggerEnter(collider);
        }
 
    }


    void HandsUpdate()
    {
        while (_interactableObjects.Count > 0 && _interactableObjects[_interactableObjects.Count - 1] == null)
        {
            _interactableObjects.RemoveAt(_interactableObjects.Count - 1);
            UpdateHoverColor();
        }
        switch (HandState)
        {
            case State.TOUCHING:
                if (_interactableObjects.Count > 0 && GestureState == Gesture.FIST)
                // 
                {
                    _interactableObjects[_interactableObjects.Count - 1].StartInteraction(controllerSphereCollider, this);
                    currentInteraction = _interactableObjects[_interactableObjects.Count - 1];

                    HandState = State.HOLDING;
               //     GestureState = Gesture.FIST;


                }

                break;
            case State.HOLDING:
          //      Debug.Log("HOLDING, _interactableObjects.Count: " + _interactableObjects.Count);

                if (GestureState == Gesture.OPENHAND)
                {
                    currentInteraction.EndInteraction(this);
                    if (currentInteraction != null && !_interactableObjects.Contains(currentInteraction))
                    {
                        currentInteraction.EndHighlight();
                    }

                    // clean up deleted objects
                    _interactableObjects.RemoveAll(obj => (obj == null));

                    // GestureState = _interactableObjects.Count == 0 ? Gesture.EMPTY : Gesture.FIST;
                    HandState = _interactableObjects.Count == 0 ? State.EMPTY : State.TOUCHING;
                    controls.DestroyInvalidObjects();
                    controls.UpdateControls();
                    UpdateHoverColor();
                    currentInteraction = null;
                }


                break;
        }



        //Movement of model
        /*if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, Controller) > 0.5f 
            && IsInsideModel()
            && currentInteraction == null)
        {
            controls.Extrudable.transform.root.Translate(transform.position - prevPos, Space.World);
            Quaternion deltaRot = transform.rotation * Quaternion.Inverse(prevRot);
            controls.Extrudable.transform.root.position = RotatePointAroundPivot(controls.Extrudable.transform.root.position, transform.position, deltaRot);
            controls.Extrudable.transform.root.Rotate(deltaRot.eulerAngles,Space.World);
            //controls.UpdateControls();
        }
        
        
        prevPos = transform.position;
        prevRot = transform.rotation;

        */

        //Debug state changes
        if (PrevHandState != HandState)
        {
            //Debug.Log(Controller.ToString() +": " + HandState.ToString());
        }
        PrevHandState = HandState;
      //  PrevGestureState = GestureState;

    }

    void ControllersUpdate()
    {
        while (_interactableObjects.Count > 0 && _interactableObjects[_interactableObjects.Count - 1] == null)
        {
            _interactableObjects.RemoveAt(_interactableObjects.Count - 1);
            UpdateHoverColor();
        }
        switch (HandState)
        {
            case State.TOUCHING:
                if (_interactableObjects.Count > 0 && (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, Controller) >= 0.5f))
                {
                    _interactableObjects[_interactableObjects.Count - 1].StartInteraction(controllerSphereCollider, this);
                    currentInteraction = _interactableObjects[_interactableObjects.Count - 1];

                    HandState = State.HOLDING;

                }

                break;
            case State.HOLDING:

                if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, Controller) < 0.5f)
                {
                    currentInteraction.EndInteraction(this);
                    if (currentInteraction != null && !_interactableObjects.Contains(currentInteraction))
                    {
                        currentInteraction.EndHighlight();
                    }

                    // clean up deleted objects
                    _interactableObjects.RemoveAll(obj => (obj == null));

                    // GestureState = _interactableObjects.Count == 0 ? Gesture.EMPTY : Gesture.FIST;
                    HandState = _interactableObjects.Count == 0 ? State.EMPTY : State.TOUCHING;
                    controls.DestroyInvalidObjects();
                    controls.UpdateControls();
                    UpdateHoverColor();
                    currentInteraction = null;
                }


                break;
        }



        //Movement of model
        /*if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, Controller) > 0.5f 
            && IsInsideModel()
            && currentInteraction == null)
        {
            controls.Extrudable.transform.root.Translate(transform.position - prevPos, Space.World);
            Quaternion deltaRot = transform.rotation * Quaternion.Inverse(prevRot);
            controls.Extrudable.transform.root.position = RotatePointAroundPivot(controls.Extrudable.transform.root.position, transform.position, deltaRot);
            controls.Extrudable.transform.root.Rotate(deltaRot.eulerAngles,Space.World);
            //controls.UpdateControls();
        }
        
        
        prevPos = transform.position;
        prevRot = transform.rotation;

        */

        //Debug state changes
        if (PrevHandState != HandState)
        {
            //Debug.Log(Controller.ToString() +": " + HandState.ToString());
        }
        PrevHandState = HandState;
     //   PrevGestureState = GestureState;
    }







    void Update()
    {

        //   Debug.Log("HANDACTIVE: " + handsActive);
       // Debug.Log("_interactableObjects: " + _interactableObjects.Count.ToString());

        if (handsActive)
        {
            HandsUpdate();

        } else
        {
            ControllersUpdate();

        }

       

    }

    private bool IsInsideModel()
    {
        if(collidedObject != null && collidedObject.GetComponent<ExtrudableMesh>() != null)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        return rotation * (point - pivot) + pivot;
    }
}
