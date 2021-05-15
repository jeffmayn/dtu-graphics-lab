using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleModes : MonoBehaviour
{
    private Color initialColor;
    private bool isInteracting;
    private bool canUse;
    private MeshRenderer _meshRenderer;

    private bool isActive;
    public bool toggleHandsOn;

    public GameObject _customHandLeftVariant;
    public GameObject _customHandRightVariant;
    public GameObject _ovrHandPrefabLeft;
    public GameObject _ovrHandPrefabRight;
    public GameObject _ovrCameraRig;

    void Start()
    {

        isActive = false;
        toggleHandsOn = false;

    }

    void Update() {}

    IEnumerator FlashButton()
    {
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator FlashButton2()
    {
        yield return new WaitForSeconds(0.3f);
    }

    public void switchModes()
    {
        if (!toggleHandsOn)
        {
          
            _customHandLeftVariant.SetActive(true);
            _customHandRightVariant.SetActive(true); ;
            _ovrHandPrefabLeft.SetActive(false); 
            _ovrHandPrefabRight.SetActive(false);
        }

    }

    private void OnTriggerEnter(Collider other)
    {

        if (toggleHandsOn)
        {
          //  modeText.text = "Mode: hands";
            Debug.Log("Swithing to hands-mode");
         
       
            _customHandLeftVariant.SetActive(false);
            _customHandRightVariant.SetActive(false); ;
            _ovrHandPrefabLeft.SetActive(true); ;
            _ovrHandPrefabRight.SetActive(true);
            toggleHandsOn = false;
           
        } else
        {
          //  modeText.text = "Mode: controllers";
            Debug.Log("Swithing to controllers-mode");
            
            _customHandLeftVariant.SetActive(true);
            _customHandRightVariant.SetActive(true); ;
            _ovrHandPrefabLeft.SetActive(false); ;
            _ovrHandPrefabRight.SetActive(false);
            toggleHandsOn = true;
        }
    }

    private void OnTriggerExit(Collider other) {}
}
