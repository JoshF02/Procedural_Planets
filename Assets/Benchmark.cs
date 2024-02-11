using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Benchmark : MonoBehaviour
{
    int frames = 0;
    float timer = 0f;
    float minDt = 1000f;
    float maxDt = 0f;
    bool hasStarted = false;
    bool startMoving = false;

    public float duration;
    public float waitDuration;
    public float speed;
    public float distanceFromPlanet;

    void Awake()
    {
        Reset();
    }

    void Update()
    {
        if (hasStarted) {
            frames++;

            float temp = Time.deltaTime;
            timer += temp;
            if (temp < minDt) minDt = temp;
            if (temp > maxDt) maxDt = temp;

            if (timer >= duration) {
                Debug.Log("Frames: " + frames + "    Timer: " + timer + "    Min FPS: " + (1f / maxDt).ToString("0.0") + "    Max FPS: " 
                + (1f / minDt).ToString("0.0") + "    Average FPS: " + (frames/timer).ToString("0.0"));

                Reset();
            }
        }
        else {
            if (Input.GetKey(KeyCode.Y)) {  // benchmark with camera movement
                hasStarted = true;
                Debug.Log("Starting movement benchmark...");
                StartCoroutine(CameraMovement());
            }

            if (Input.GetKey(KeyCode.U)) {  // stationary benchmark
                hasStarted = true;
                Debug.Log("Starting stationary benchmark...");
            }
        }
    }

    void Reset()
    {
        frames = 0;
        timer = 0f;
        minDt = 1000f;
        maxDt = 0f;
        hasStarted = false;
        startMoving = false;
        transform.position = new Vector3(0f, 0f, -distanceFromPlanet);
    }

    IEnumerator CameraMovement()
    {
        yield return new WaitForSeconds(waitDuration);
        startMoving = true;
    }

    void FixedUpdate()
    {
        if(startMoving)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}
