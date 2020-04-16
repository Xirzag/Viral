using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour
{

    public Vector2 speed;
    public float turnLerp = .8f;
    public GameObject laser;
    public float laserSpeed = 10f;
    public float laserLifespan = 2;
    public Transform shotPosition;

    private Rigidbody2D rb;
    private Camera camera;

    public float shootInterval = .2f;
    private float lastShoot = -Mathf.Infinity;

    public ParticleSystem forward;
    public ParticleSystem backL;
    public ParticleSystem backR;

    public AudioSource moveAudio;
    public AudioSource laserAudio;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        camera = Camera.main;
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {

            if(lastShoot + shootInterval < Time.time)
            {
                GameObject newLaser = Instantiate(laser, shotPosition.position, transform.rotation);
                newLaser.GetComponent<Rigidbody2D>().velocity = transform.up * laserSpeed;
                lastShoot = Time.time;
                laserAudio.Play();
                Destroy(newLaser, laserLifespan);
            }

        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        DoMoveFx(v, h);
        PlayMoveSounds(v, h);

        Plane floor = new Plane(Vector3.forward, Vector3.zero);

        Vector3 mousePosition = Input.mousePosition;
        Ray mouseRay = camera.ScreenPointToRay(mousePosition);
        float distance = 0;
        floor.Raycast(mouseRay, out distance);

        Vector3 mouseWorldPos = mouseRay.GetPoint(distance);

        Vector3 diff = (mouseWorldPos - transform.position).normalized;

        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        Quaternion desiredRotation = Quaternion.Euler(0, 0, angle - 90);

        transform.rotation = Quaternion.Lerp(desiredRotation, transform.rotation, turnLerp);

        rb.AddForce(transform.up * v * speed.y + transform.right * h * speed.x);

    }

    private void DoMoveFx(float v, float h)
    {
        if (v > 0 && !forward.isPlaying)
        {
            forward.Play();
        }
        if (v == 0 && forward.isPlaying) forward.Stop();
        if (v < 0)
        {
            if (forward.isPlaying) forward.Stop();
            if (!backL.isPlaying) backL.Play();
            if (!backR.isPlaying) backR.Play();
        }
        if (h < 0 && !backR.isPlaying) backR.Play();
        if (h > 0 && !backL.isPlaying) backL.Play();
        if (v >= 0)
        {
            if (h <= 0 && backL.isPlaying) backL.Stop();
            if (h >= 0 && backR.isPlaying) backR.Stop();
        }
    }

    private void PlayMoveSounds(float v, float h)
    {
        if (v == 0 && h == 0 && moveAudio.isPlaying) moveAudio.Stop();
        if ((v != 0 || h != 0) && !moveAudio.isPlaying) moveAudio.Play();
    }
}
