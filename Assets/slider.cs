using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class slider : MonoBehaviour
{
    public void UpdateSlider(string type)
    {
        var value = GetComponent<Slider>().value;
        
        GetComponentInChildren<TMPro.TextMeshProUGUI>().text = value.ToString() + " m";

        var main =FindObjectOfType<Main>();
        switch (type)
        {
            case "med":
                main.thresholdMedical = value;
                break;
            case "edu":
                main.thresholdEdu = value;
                break;
        }
       
    }
}
