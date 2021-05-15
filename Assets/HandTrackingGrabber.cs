using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;
using System;

public class HandTrackingGrabber : OVRGrabber
{
    private HandsManager hand;
    public float pinchTreshold = 0.7f;

    protected override void Start()
    {
        base.Start();
        hand = GetComponent<HandsManager>();
    }

    public override void Update()
    {
        base.Update();
        CheckIndexPinch();
    }

    void CheckIndexPinch()
    {
        float pinchStrength = GetComponent<OVRHand>().GetFingerPinchStrength(OVRHand.HandFinger.Index);
        bool isPinching = pinchStrength > pinchTreshold;

        if (!m_grabbedObj && isPinching && m_grabCandidates.Count > 0)
            GrabBegin();
        else if (m_grabbedObj && !isPinching)
            GrabEnd();
    }
}
