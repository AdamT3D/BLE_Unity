using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageReader : MonoBehaviour
{
    public ArduinoHM10Test BLEInput;
    public Text currentPage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (BLEInput.inputText.Contains("Page1:1"))
        {
            currentPage.text = "Page 1";
        }else if(BLEInput.inputText.Contains("Page2:1"))
        {
            currentPage.text = "Page 2";
        }
        else
        {
            currentPage.text = "value: " + BLEInput.inputText;
        }
    }
}
