using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Metrics : MonoBehaviour
{
    [Header("Controls")]
    [Range(0, 1)]
    public float ScreenSlider = 0;

    [Range(0, 1)]
    public float GuestSlider = 0;

    public string ScreenText = "";
    public string GuestText = "";

    [Header("UI")]
    public Text Text;

    public Slider Slider;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        //UpdateGuestUI
        List<Guest> guests = GuestManager.Instance.GuestList();
        foreach (Guest guest in guests)
        {
            if (guest.GetSliderValue() != GuestSlider)
                guest.SetSlider(GuestSlider);
            if (guest.GetText() != GuestText)
                guest.SetText(GuestText);
        }

        //Update Screen UI
        if (Slider == null) { Debug.Log("null Slider"); }
        if (Slider.value != ScreenSlider)
            Slider.value = ScreenSlider;
        if (Text.text != ScreenText)
            Text.text = ScreenText;
    }
}