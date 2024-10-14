using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class TeacherUIHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text teacherName;
    [SerializeField] private TMP_Dropdown statusDropdown;
    [SerializeField] private TMP_Text floorText;
    [SerializeField] private TMP_Dropdown floorDropdown;
    [SerializeField] private TMP_Text currentRoomText;
    [SerializeField] private TMP_Dropdown currentRoomDropdown;
    
    [SerializeField] private MessageHandler _messageHandler;
    
    private DatabaseReference _reference;
    private string _currentTeacherStatus;
    public string GetCurrentTeacherStatus() => _currentTeacherStatus;
    public string SetCurrentTeacherStatus(int statusId) => _currentTeacherStatus = statusDropdown.options[statusId].text;
    private bool _isListenerRegistered = false;
    
    private Dictionary<string, object> _updates = new Dictionary<string, object>();
    private List<string> _studentIds = new List<string>();
    private List<string> _studentNames = new List<string>();
    
    void Start()
    {
        _reference = FirebaseDatabase.DefaultInstance.RootReference;
        teacherName.SetText($"Logged in as: {PlayerPrefs.GetString("Name")}");
        SetCurrentTeacherStatus(statusDropdown.value);
        _updates["Status"] = _currentTeacherStatus;
        currentRoomDropdown.gameObject.SetActive(false);
        currentRoomText.gameObject.SetActive(false);
        floorText.gameObject.SetActive(false);
        floorDropdown.gameObject.SetActive(false);
    }

    public void Update()
    {
        int currentFloor = Convert.ToInt32(floorDropdown.options[floorDropdown.value].text);
        List<TMP_Dropdown.OptionData> currentRooms = new List<TMP_Dropdown.OptionData>();

        for(int i = 1; i < 13; i++)
        {
            //StringBuilder stringBuilder = new StringBuilder();
            int roomNo = currentFloor * 100 + i;
            string roomNoStr = roomNo.ToString();
            TMP_Dropdown.OptionData newOptionData = new TMP_Dropdown.OptionData();
            newOptionData.text = roomNoStr;
            currentRooms.Add(newOptionData);
        }
        
        currentRoomDropdown.options = currentRooms;
    }

    private void FixedUpdate()
    {
        _updates["Status"] = _currentTeacherStatus;
        _updates["CurrentRoomNo"] = currentRoomDropdown.options[currentRoomDropdown.value].text;
        //Debug.Log(_updates["CurrentRoomNo"]);

        
        if (_currentTeacherStatus == "Unavailable" && !_isListenerRegistered)
        {
            _reference.Child("student").ValueChanged += HandleStudentValueChange;
            _isListenerRegistered = true;
        }
    }

    private void HandleStudentValueChange(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError(e.DatabaseError.Message);
            return;
        }
        
        UpdateStudentList();
    }

    public void EnableRoomSelection(int statusId)
    {
        SetCurrentTeacherStatus(statusId);
        if (_currentTeacherStatus == "Available")
        {
            currentRoomText.gameObject.SetActive(true);
            currentRoomDropdown.gameObject.SetActive(true);
            floorDropdown.gameObject.SetActive(true);
            floorText.gameObject.SetActive(true);
        }
        else
        {
            floorDropdown.gameObject.SetActive(false);
            floorText.gameObject.SetActive(false);
            currentRoomText.gameObject.SetActive(false);
            currentRoomDropdown.gameObject.SetActive(false);
        }
    }
    
    public void UpdateTeacherStatus()
    {
        _reference.Child("teacher").Child(PlayerPrefs.GetString("UserId")).UpdateChildrenAsync(_updates).ContinueWith(task =>
        {
            Debug.Log("Updating teacher status");
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
            }
            else
            {
                Debug.Log(_currentTeacherStatus);
                Debug.Log("Teacher data updated successfully");
                _messageHandler.SendNotification("Teacher Status Update",
                    $"Teacher status updated to: {_currentTeacherStatus} Room no: {currentRoomDropdown.options[currentRoomDropdown.value].text}");
            }
        });
    }

    public void UpdateStudentList()
    {
        string targetTeacherId = PlayerPrefs.GetString("UserId");
        Dictionary<string, object> studentListUpdates = new Dictionary<string, object>();
        
        _reference.Child("student").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Fetching students");
                DataSnapshot snapshot = task.Result;
                foreach (var childSnapshot in snapshot.Children)
                {
                    string studentId = childSnapshot.Key;
                    var selectedTeachers = childSnapshot.Child("selectedTeachers").Children;
                    
                    foreach (var teacherIdSnapshot in selectedTeachers)
                    {
                        if (teacherIdSnapshot.Value.ToString() == targetTeacherId)
                        {
                            _studentIds.Add(teacherIdSnapshot.Key);
                            string studentName = childSnapshot.Child("Name").Value.ToString();
                            _studentNames.Add(studentName);
                            string studentEmail = childSnapshot.Child("Email").Value.ToString();
                
                            Debug.Log($"Student ID: {studentId}");
                            Debug.Log($"Student Name: {studentName}");
                            Debug.Log($"Student Email: {studentEmail}");
                        }
                    }
                }
                
                foreach (var kvp in _studentIds)
                {
                    Debug.Log($"Value: {kvp}");
                }
                
                studentListUpdates["StudentIds"] = _studentIds;
                
                _messageHandler.SendNotification("Student waiting notification", $"Student {_studentNames[0]} wants to meet.");
                
                /*DatabaseReference teacherReference = FirebaseDatabase.DefaultInstance.GetReference("teacher").Child(PlayerPrefs.GetString("UserId"));
                Debug.Log("Calling UpdateChildrenAsync for teacherReference");
                teacherReference.UpdateChildrenAsync(studentListUpdates).ContinueWith(updateTask =>
                {
                    Debug.Log("Updating student waitlist");
                    foreach (var kvp in studentListUpdates)
                    {
                        Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
                    }
                    Debug.Log($"Task status: {task.Status}");
                    if (updateTask.IsFaulted)
                    {
                        Debug.LogError(updateTask.Exception);
                    }
                    else if(updateTask.IsCompleted)
                    {
                        Debug.Log(_currentTeacherStatus);
                        Debug.Log("Teacher data updated successfully");
                        _messageHandler.SendNotification("Student waiting notification", $"Student {_studentNames[0]} wants to meet.");
                    }
                });*/
            }
        });
    }
}
