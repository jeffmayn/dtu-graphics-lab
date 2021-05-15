using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerDatas;
    public UnityEvent onRecognized;

 

}

public class GestureDetector : MonoBehaviour
{

    public delegate void ShoutOutGesture();
    public static event ShoutOutGesture OnShoutOutFistGesture;

    public delegate void ShoutOutGesture2();
    public static event ShoutOutGesture2 OnShoutOutOpenHandGesture;

    public delegate void ShoutOutGesture3();
    public static event ShoutOutGesture3 OnShoutOutHandsActive;

    public delegate void ShoutOutGesture4();
    public static event ShoutOutGesture4 OnShoutOutHandsNotActive;

    public GameObject righthand;
    public OVRSkeleton skeleton;
    public List<Gesture> gestures;
    private List<OVRBone> fingerBones;
    public Text gestureText;
    public Text modeText;

    public GameObject _customHandLeftVariant;
    public GameObject _customHandRightVariant;
    public GameObject _ovrHandPrefabLeft;
    public GameObject _ovrHandPrefabRight;

    public bool debugMode = true;
    public float threshold = 0.1f;
    private Gesture previousGesture;

    public GameObject worktable;

    private bool timerOn;


    // Start is called before the first frame update
    void Start()
    {

        previousGesture = new Gesture();
        gestureText.text = "Detected: none";
        modeText.text = "Mode: controllers";
        timerOn = false;
        

    }

    // Update is called once per frame
    void Update()
    {
        if (righthand.activeSelf)
        {
            
             if (OnShoutOutHandsActive != null)
            {
                OnShoutOutHandsActive();
            }

            modeText.text = "Mode: hands";
            worktable.SetActive(false);
            
            
            fingerBones = new List<OVRBone>(skeleton.Bones);

            if (debugMode && Input.GetKeyDown(KeyCode.Space))
            {
                Save();
            }

            Gesture currenGesture = Recognize();
            bool hasRecognized = !currenGesture.Equals(new Gesture());
            if (hasRecognized && !currenGesture.Equals(previousGesture))
            {
                Debug.Log("New gesture found: " + currenGesture.name);
                gestureText.text = "Detected: " + currenGesture.name;

                if(currenGesture.name == "Thumps Up" && !timerOn)
                {
                    TimeController.instance.BeginTimer();
                    timerOn = true;

                } else if (currenGesture.name == "Thumps Up" && timerOn)
                {
                    TimeController.instance.EndTimer();
                    timerOn = false;
                }

                //   GrabControl.Yell += GetGesture;

                if (OnShoutOutFistGesture != null && currenGesture.name == "Fist")
                {
                    OnShoutOutFistGesture();
                }

                if (OnShoutOutOpenHandGesture != null && currenGesture.name == "Open Hand")
                {
                    OnShoutOutOpenHandGesture();
                }



                //  Text txtmy = GameObject.Find("Canvas/mode_text");
                previousGesture = currenGesture;
                currenGesture.onRecognized?.Invoke();
            }
        }else
        {
            if (OnShoutOutHandsNotActive != null)
            {
                OnShoutOutHandsNotActive();
            }
            modeText.text = "Mode: controllers";
            worktable.SetActive(true);
        }


    }

    void Save()
    {
        Gesture g = new Gesture();
        g.name = "New Gesture";
        List<Vector3> data = new List<Vector3>();

        foreach (var bone in fingerBones)
        {
            data.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }
        g.fingerDatas = data;
        gestures.Add(g);
    }

    Gesture Recognize()
    {
        Gesture currentGesture = new Gesture();
        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for (int i = 0; i < fingerBones.Count; i++)
            {
                Vector3 currentData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                float distance = Vector3.Distance(currentData, gesture.fingerDatas[i]);
                if (distance > threshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;
            }
            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }
    
        return currentGesture;
    }
}
