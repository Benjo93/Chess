using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicScaleBoard : MonoBehaviour
{
    private float scaleToFitMultiplier;

    void Awake()
    {
        RepositionBoard();
    }
    public void RepositionBoard()
    {
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
    }
    public void RefitBoard()
    {
        float boardWidthProportion = 121 / 100f; // board's width scale
        float screenRatio = (float)Screen.width / (float)Screen.height; // screen ratio
        float spaceToFit = screenRatio * .55f; // available space's width scale
        scaleToFitMultiplier = spaceToFit / boardWidthProportion; // multiplier for board scale to fit on space 
        transform.localScale = new Vector3(transform.position.x * scaleToFitMultiplier, transform.position.y * scaleToFitMultiplier, 1);
    }
}
