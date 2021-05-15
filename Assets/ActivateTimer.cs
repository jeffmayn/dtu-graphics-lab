using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateTimer : MonoBehaviour
{

    private bool isOn;
    private Color initialColor;
    private UndoManager _undoManager;
    private MeshRenderer _meshRenderer;

    private bool isInteracting;
    private bool canUse;

    private bool isActive;
    private bool fromTop;
    public GameObject _customHandRightVariant;

    private void OnTriggerEnter(Collider other)
    {

        if (isOn)
        {
            TimeController.instance.EndTimer();
            isOn = false;
        }
        else
        {
            TimeController.instance.BeginTimer();
            isOn = true;
        }
    }

   
    void Start()
    {


        isOn = false;
        _meshRenderer = GetComponent<MeshRenderer>();
        initialColor = _meshRenderer.material.color;
        _undoManager = FindObjectOfType<UndoManager>();
        isInteracting = false;
        isActive = false;
        fromTop = false;
        canUse = GetComponent<MeshRenderer>().isVisible;

    }

    IEnumerator FlashButton()
    {
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator FlashButton2()
    {
        yield return new WaitForSeconds(0.3f);
    }


}
