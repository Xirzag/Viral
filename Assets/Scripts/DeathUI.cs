using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathUI : MonoBehaviour
{

    FollowPlayer camera;
    public TMPro.TextMeshProUGUI text;
    int deaths = 0;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<FollowPlayer>();
    }

    public void Die()
    {
        camera.Impulse();
        deaths++;
        text.text = "Muertes: " + deaths;
    }
    

}
