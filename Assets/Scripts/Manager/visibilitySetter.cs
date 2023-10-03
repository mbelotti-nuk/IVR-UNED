using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class visibilitySetter : MonoBehaviour
{
    [SerializeField] public Slider slider;
    [SerializeField] public TextMeshProUGUI sliderText;
    [SerializeField] public visibilityController controller;
    // Start is called before the first frame update
    void Start()
    {
        slider.onValueChanged.AddListener((v) => { sliderText.text = v.ToString("0.00"); });
    }

    public void changeValue()
    {
        controller.changeVisibility(slider.value);
    }
}
