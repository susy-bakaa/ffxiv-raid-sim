using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace no00ob.WaveSurvivalGame.UserInterface
{
    public class SliderInputLinker : MonoBehaviour
    {
        [SerializeField] private Slider targetSlider;
        [HideInInspector] public Slider Slider { get { return targetSlider; } }
        [SerializeField] private TMP_InputField targetInputField;
        [HideInInspector] public TMP_InputField InputField { get { return targetInputField; } }
        [SerializeField] private string inputFieldFormat = "{0}";

        public void Sync(int bias)
        {
            string str = targetInputField.text;
            //Debug.Log("1. " + str);

            char[] arr = str.ToCharArray();

            arr = Array.FindAll(arr, (c => (char.IsNumber(c))));
            str = new string(arr);

            //Debug.Log("2. "+str);

            if (int.TryParse(str, out int result))
            {
                if (result > targetSlider.maxValue)
                {
                    result = Mathf.RoundToInt(targetSlider.maxValue);
                }
                else if (result < targetSlider.minValue)
                {
                    result = Mathf.RoundToInt(targetSlider.minValue);
                }

                if (bias > 0)
                {
                    targetSlider.value = result;
                    str = result.ToString();
                }
                else
                {
                    int final = Mathf.RoundToInt(targetSlider.value);
                    str = final.ToString();
                    targetSlider.value = final;
                }
            }

            //Debug.Log("3. " + string.Format(inputFieldFormat, str));

            //Debug.LogError("BREAK");

            targetInputField.text = string.Format(inputFieldFormat, str);
        }
    }
}