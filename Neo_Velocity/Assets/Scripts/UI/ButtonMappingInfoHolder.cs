using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMappingInfoHolder : MonoBehaviour
{
    // Also use this for checkboxes / sliders / inputfields, just call a different function in Buttonmapping with it
    [SerializeField] public Button button;
    [SerializeField] public TMP_Text text_field;
    [SerializeField] public string text;
    [SerializeField] public Toggle checkBox;
    [SerializeField] public Slider slider;
    [SerializeField] public TMP_Text inputTextField;
    [SerializeField] public bool IsSlider; // True if attached to a slider, false if for a text field
}
