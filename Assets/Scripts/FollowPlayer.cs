using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public Transform player;
    public float lerp = .5f;

    public float velocity = 1;
    public float size = 2;
    public float impulse = 1;
    public float impulseDecay = .8f;

    Vector3 movePosition;

    // Start is called before the first frame update
    void Start()
    {
        movePosition = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 position = Vector3.Lerp(movePosition, player.position, lerp * Time.deltaTime);
        position.z = movePosition.z;

        movePosition = position;

        position.x += Mathf.PerlinNoise(Time.time * velocity, 0) * size * impulse;
        position.y += Mathf.PerlinNoise(0, Time.time * velocity) * size * impulse;
        transform.position = position;


        impulse = Mathf.Max(0, impulse - Time.deltaTime * impulseDecay );
    }

    public void Impulse()
    {
        impulse = 1;
    }

}
