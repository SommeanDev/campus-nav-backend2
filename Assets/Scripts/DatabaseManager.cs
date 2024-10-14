using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine.SceneManagement;

public class DatabaseManager : MonoBehaviour
{
    private string _userID;
    private DatabaseReference _reference;
    private FirebaseApp _app;
    private FirebaseAuth _auth;
    [SerializeField] private MessageHandler _messageHandler;
    [SerializeField] private Register _register;
    [SerializeField] private Login _login;
    
    // Start is called before the first frame update
    void Start()
    {
        InitializeFirebase();
        //_messageHandler.SendNotification("App Started", "test app has started");
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // create and hold ref to firebase app
                _app = FirebaseApp.DefaultInstance;
                Debug.Log("Firebase is ready");
                
                // initialize authentication
                _auth = FirebaseAuth.DefaultInstance;
                SignInAnonymously();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void SignInAnonymously()
    {
        _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                FirebaseUser newUser = _auth.CurrentUser;
                if (newUser != null)
                {
                    _userID = newUser.UserId;
                    
                    InitializeDatabase();
                    ListenForDatabaseChanges();
                }
                else
                {
                    Debug.LogError($"Could not sign in: {task.Exception}");
                }
            }
        });
        
    }
    
    private void InitializeDatabase()
    {
        _reference = FirebaseDatabase.DefaultInstance.RootReference;

        if (_register == null)
        {
            Debug.LogError("Register component is not assigned in the inspector");
            return;
        }
        
        if (_login == null)
        {
            Debug.LogError("Login component is not assigned in the inspector");
            return;
        }

        _register.SetUserID(_userID);
        _register.SetDatabaseReference(_reference);
        _login.SetDatabaseReference(_reference);
    }
    
    public void CreateUser()
    {
        if (_register == null)
        {
            Debug.LogError("Register component is not assigned");
            return;
        }
        _register.RegisterUser();
    }

    public void ReadUser()
    {
        if (_login == null)
        {
            Debug.LogError("Login component is not assigned");
            return;
        }
        _login.AttemptLogin();
    }
    
    private void ListenForDatabaseChanges()
    {
        _reference.Child("teacher").Child(_userID).ValueChanged += HandleTeacherValueChanged;
        _reference.Child("student").Child(_userID).ValueChanged += HandleStudentValueChanged;
    }
    
    private void HandleTeacherValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError("Database error: " + e.DatabaseError.Message);
            return;
        }

        if (e.Snapshot != null && e.Snapshot.Exists)
        {
            // Data has been changed, trigger a notification
            Debug.Log("Data changed: " + e.Snapshot.GetRawJsonValue());
        }
    }
    
    private void HandleStudentValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError("Database error: " + e.DatabaseError.Message);
            return;
        }

        if (e.Snapshot != null && e.Snapshot.Exists)
        {
            // Data has been changed, trigger a notification
            Debug.Log("Data changed: " + e.Snapshot.GetRawJsonValue());
        }
    }

}
