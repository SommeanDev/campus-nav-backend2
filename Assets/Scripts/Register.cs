using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Register : MonoBehaviour
{
    [SerializeField] TMP_InputField username;
    [SerializeField] TMP_Dropdown role;
    [SerializeField] TMP_InputField email;
    [SerializeField] TMP_InputField password;
    [SerializeField] TMP_InputField semester;
    [SerializeField] TMP_InputField phoneNumber;
    [SerializeField] TMP_Dropdown branch;

    private string _userID;
    private DatabaseReference _reference;

    public void SetUserID(string userID)
    {
        _userID = userID;
    }

    public void SetDatabaseReference(DatabaseReference reference)
    {
        _reference = reference;
    }
    
    public void LoadRegisterPage()
    {
        SceneManager.LoadScene("Register");
    }
    
    public void RegisterUser()
    {
        if (string.IsNullOrEmpty(_userID) || _reference == null)
        {
            Debug.LogError("UserID or DatabaseReference not set!");
            return;
        }

        if (username == null || role == null || email == null)
        {
            Debug.LogError("One or more UI elements are not assigned!");
            return;
        }
        
        Debug.Log("Username: " + username.text);
        Debug.Log("Role: " + role.options[role.value].text);
        
        if (role.options[role.value].text == "Teacher")
        {
            Teacher newTeacher = new Teacher(username.text, email.text, password.text, phoneNumber.text);
            newTeacher.setStatus("Unavailable");
            string json = JsonUtility.ToJson(newTeacher);

            Debug.Log("Json Data: " + json);

            _reference.Child("teacher").Child(_userID).SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("SetRawJsonValueAsync faulted" + task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("SetRawJsonValueAsync completed");
                }
            });
        }
        else if (role.options[role.value].text == "Student")
        {
            Student newStudent = new Student(username.text, email.text, password.text, branch.options[branch.value].text, semester.text);
            string json = JsonUtility.ToJson(newStudent);

            Debug.Log("Json Data: " + json);

            _reference.Child("student").Child(_userID).SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("SetRawJsonValueAsync faulted" + task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("SetRawJsonValueAsync completed");
                }
            });
        }

        SceneManager.LoadScene("Login");
    }
}
