using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using TMPro;
using UnityEngine;

public class StudentUIHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;

    [SerializeField] private MessageHandler _messageHandler;
    
    private DatabaseReference _reference;
    private List<string> _observedTeachersList;
    private Dictionary<string, string> _previousTeacherStatus;
    
    // Start is called before the first frame update
    void Awake()
    {
        _reference = FirebaseDatabase.DefaultInstance.RootReference;
        _previousTeacherStatus = new Dictionary<string, string>();
        titleText.SetText($"Logged in as: {PlayerPrefs.GetString("Name")}");    
    }

    public void SaveSelectedTeachers(List<string> selectedTeachers)
    {
        _observedTeachersList = selectedTeachers; 
        Debug.Log(_observedTeachersList.ToArray());
        RemovePreviousListeners();
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("student").Child(PlayerPrefs.GetString("UserId"));
        UpdateStudentSelectedTeachers(selectedTeachers, reference);
        SubscribeToTeacherStatusUpdates(selectedTeachers);
    }

    private void SubscribeToTeacherStatusUpdates(List<string> selectedTeachers)
    {
        foreach (var teacherId in selectedTeachers)
        {
            FetchTeacherName(teacherId).ContinueWith(fetchNameTask =>
            {
                if (fetchNameTask.IsFaulted)
                {
                    Debug.Log(fetchNameTask.Exception);
                }
                else if (fetchNameTask.IsCompleted)
                {
                    string teacherName = fetchNameTask.Result;
                    AddTeacherStatusListener(teacherId, teacherName);
                }
            });
        }
    }

    private Task<string> FetchTeacherName(string teacherId)
    {
        return _reference.Child("teacher").Child(teacherId).Child("Name").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                return task.Result.Value.ToString();
            }
            throw new Exception($"Failed to fetch teacher name from: {teacherId}");
        });
    }

    private void AddTeacherStatusListener(string teacherId, string teacherName)
    {
        _reference.Child("teacher").Child(teacherId).Child("Status").ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError("Database Error: " + args.DatabaseError.Message);
                return;
            }

            if (args.Snapshot != null && args.Snapshot.Exists)
            {
                HandleTeacherStatusChange(teacherId, teacherName, args.Snapshot.Value.ToString());
            }
        };
    }

    private void HandleTeacherStatusChange(string teacherId, string teacherName, string currentStatus)
    {
        Debug.Log("Running HandleTeacherStatusChange function");
        string previousStatus = _previousTeacherStatus.ContainsKey(teacherId) ? _previousTeacherStatus[teacherId] : null;

        if (currentStatus != previousStatus)
        {
            if (currentStatus == "Available")
            {
                FetchTeacherRoomNo(teacherId).ContinueWith(roomNoTask =>
                {
                    if (roomNoTask.IsCompleted)
                    {
                        string roomNo = roomNoTask.Result;
                        _messageHandler.SendNotification("Teacher Available", $"{teacherName} is now Available at room no. {roomNo}");
                        Debug.Log($"Notification: Teacher with ID {teacherName} is now available at room no. {roomNo}!");
                    }
                    else
                    {
                        Debug.LogError("Error retrieving teacher room no.: " + roomNoTask.Exception);
                    }
                });
            }
            _previousTeacherStatus[teacherId] = currentStatus;
        }
    }

    private void UpdateStudentSelectedTeachers(List<string> selectedTeachers, DatabaseReference reference)
    {
        Debug.Log("Updating student selected teachers");
        Dictionary<string, object> updates = new Dictionary<string, object>();
        // Add the selectedTeachers list to the updates dictionary
        updates["selectedTeachers"] = selectedTeachers.ToDictionary(item => selectedTeachers.IndexOf(item), item => item);
        
        reference.UpdateChildrenAsync(updates).ContinueWith(task => 
        {
            if (task.IsCompleted)
            {
                Debug.Log("Selected teachers successfully added to Firebase.");
            }
            else
            {
                Debug.LogError("Error updating Firebase data: " + task.Exception);
            }
        });
    }

    private Task<string> FetchTeacherRoomNo(string teacherId)
    {
        return _reference.Child("teacher").Child(teacherId).Child("CurrentRoomNo")
            .GetValueAsync().ContinueWith(
                task =>
                { 
                    if (task.IsCompleted && task.Result.Exists)
                    {
                        return task.Result.Value.ToString();   
                    }

                    throw new Exception("Error retrieving teacher room no: " + task.Exception);
                });
    }

    private void RemovePreviousListeners()
    {
        Debug.Log("Running RemovePreviousListeners function");
        foreach (var teacherId in _observedTeachersList)
        {
            // Remove old listeners to avoid duplicated events
            _reference.Child("teacher").Child(teacherId).Child("Status").ValueChanged -= null;
        }
    }

}
