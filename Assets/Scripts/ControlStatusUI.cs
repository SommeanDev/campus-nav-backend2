using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControlStatusUI : MonoBehaviour
{
    [SerializeField] TMP_Dropdown role;
    [SerializeField] TMP_Dropdown branch;
    [SerializeField] TMP_Text branchText;
    [SerializeField] TMP_Text semesterText;
    [SerializeField] TMP_Text phoneText;
    [SerializeField] TMP_InputField phoneInputField;
    [SerializeField] TMP_InputField semesterInputField;
    
    // Start is called before the first frame update
    void Start()
    {
        phoneInputField.gameObject.SetActive(false);
        phoneText.enabled = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (role.options[role.value].text == "Teacher")
        {
            branch.gameObject.SetActive(false);
            branchText.enabled = false;
            
            semesterInputField.gameObject.SetActive(false);
            semesterText.enabled = false;
            
            phoneInputField.gameObject.SetActive(true);
            phoneText.enabled = true;
        }
        else
        {
            branch.gameObject.SetActive(true);
            branchText.enabled = true;
            
            semesterInputField.gameObject.SetActive(true);
            semesterText.enabled = true;
            
            phoneInputField.gameObject.SetActive(false);
            phoneText.enabled = false;
        }
    }
}
