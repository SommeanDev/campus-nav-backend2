using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private DatabaseReference databaseReference;

    public void SetDatabaseReference(DatabaseReference databaseReference)
    {
        this.databaseReference = databaseReference;
    }
    
    public void LoadLoginPage()
    {
        SceneManager.LoadScene("Login");
    }
    
    public void AttemptLogin()
    {
        string email = emailInput.text;
        StartCoroutine(CheckUserTypeAndLogin(email));
    }

    private IEnumerator CheckUserTypeAndLogin(string email)
    {
        bool userFound = false;

        // Check in teacher node
        var teacherTask = databaseReference.Child("teacher").OrderByChild("Email").EqualTo(email).GetValueAsync();
        yield return new WaitUntil(() => teacherTask.IsCompleted);

        if (teacherTask.Exception != null)
        {
            Debug.LogError($"Failed to query teacher database: {teacherTask.Exception}");
        }
        else if (teacherTask.Result.Exists)
        {
            userFound = true;
            Debug.Log("Teacher found!");
            // Implement teacher login logic here
            LoginAsTeacher(teacherTask.Result);
        }

        // If not found in teacher, check in student node
        if (!userFound)
        {
            var studentTask = databaseReference.Child("student").OrderByChild("Email").EqualTo(email).GetValueAsync();
            yield return new WaitUntil(() => studentTask.IsCompleted);

            if (studentTask.Exception != null)
            {
                Debug.LogError($"Failed to query student database: {studentTask.Exception}");
            }
            else if (studentTask.Result.Exists)
            {
                userFound = true;
                Debug.Log("Student found!");
                // Implement student login logic here
                LoginAsStudent(studentTask.Result);
            }
        }

        if (!userFound)
        {
            Debug.Log("User not found!");
            // Handle case where user is not found
            DisplayLoginError("User not found. Please check your email or register.");
        }
    }

    private void LoginAsTeacher(DataSnapshot teacherData)
    {
        SceneManager.LoadScene("TeacherUI");
        // Implement teacher-specific login logic
        foreach (var child in teacherData.Children)
        {
            string teacherId = child.Key;
            string name = child.Child("Name").Value.ToString();
            string status = child.Child("Status").Value.ToString();
            
            Debug.Log($"Logged in as Teacher. ID: {teacherId}, Name: {name}, Status: {status}");
            // TODO: Set up teacher-specific UI or game state
            PlayerPrefs.SetString("UserId", teacherId);
            PlayerPrefs.SetString("Name", name);
        }
    }

    private void LoginAsStudent(DataSnapshot studentData)
    {
        SceneManager.LoadScene("StudentUI");
        // Implement student-specific login logic
        foreach (var child in studentData.Children)
        {
            string studentId = child.Key;
            string name = child.Child("Name").Value.ToString();
            
            Debug.Log($"Logged in as Student. ID: {studentId}, Name: {name}");
            // TODO: Set up student-specific UI or game state
            PlayerPrefs.SetString("UserId", studentId);
            PlayerPrefs.SetString("Name", name);
        }
    }

    private void DisplayLoginError(string message)
    {
        // TODO: Display error message to the user, e.g., using a UI Text element
        Debug.LogWarning(message);
    }
}
