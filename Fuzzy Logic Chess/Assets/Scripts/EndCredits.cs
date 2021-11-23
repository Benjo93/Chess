using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndCredits : MonoBehaviour
{
    public GameObject Scroller;

    public float time = 4.5f;

    // Update is called once per frame
    void Update()
    {
        // IF 'Scroller' y-axis = 296, scene switch to menu
        //Vector3 pos = GameObject.Find("Scroller").transform.position;
        // transform.position.y == 296;
        /*
        float pos = Scroller.transform.position.y;
        Debug.Log(pos);
        Debug.Log("this work?");
        if (pos == 296)
            SceneManager.LoadScene("Start Menu 2");
        */
        time -= Time.deltaTime;
        if(time < 0)
            SceneManager.LoadScene("Start Menu 2");
    }
}
