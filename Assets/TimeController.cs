using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
    public static TimeController instance;
    public Text counter;
    private TimeSpan timePlaying;
    private bool timerGoing;
    private float elapsedTime;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        counter.text = "Time: 00:00:00";
        timerGoing = false;
    }

    public void BeginTimer()
    {
        timerGoing = true;
        elapsedTime = 0f;

        StartCoroutine(UpdateTimer());
    }

    public void EndTimer()
    {
        timerGoing = false;
    }

    private IEnumerator UpdateTimer()
    {
        while (timerGoing)
        {
            elapsedTime += Time.deltaTime;
            timePlaying = TimeSpan.FromSeconds(elapsedTime);
            string timePlanugStr = "Time: " + timePlaying.ToString("mm':'ss'.'ff");
            counter.text = timePlanugStr;

            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
