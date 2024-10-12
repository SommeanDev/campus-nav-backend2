using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using TMPro;
using UnityEngine;

public class TeacherUIHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text teacherName;
    [SerializeField] private TMP_Dropdown statusDropdown;
    [SerializeField] private TMP_Text currentRoomText;
    [SerializeField] private TMP_Dropdown currentRoomDropdown;
    
    [SerializeField] private MessageHandler _messageHandler;
    
    private DatabaseReference _reference;
    private string _currentTeacherStatus;
    private Dictionary<string, object> _updates = new Dictionary<string, object>();
        
    void Start()
    {
        _reference = FirebaseDatabase.DefaultInstance.RootReference;
        teacherName.SetText($"Logged in as: {PlayerPrefs.GetString("Name")}");
        _currentTeacherStatus = _reference.Child("teacher").Child(PlayerPrefs.GetString("UserId")).Child("Status").GetValueAsync().ToString();
        currentRoomDropdown.gameObject.SetActive(false);
        currentRoomText.gameObject.SetActive(false);
    }

    public void EnableRoomSelection(int statusId)
    {
        _currentTeacherStatus = statusDropdown.options[statusId].text;
        if (_currentTeacherStatus == "Available")
        {
            currentRoomText.gameObject.SetActive(true);
            currentRoomDropdown.gameObject.SetActive(true);
            _updates["Status"] = _currentTeacherStatus;
            _updates["CurrentRoomNo"] = currentRoomDropdown.options[currentRoomDropdown.value].text;
        }
        else
        {
            currentRoomText.gameObject.SetActive(false);
            currentRoomDropdown.gameObject.SetActive(false);
        }
    }
    
    public void UpdateTeacherStatus()
    {
        _reference.UpdateChildrenAsync(_updates).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
            }
            else
            {
                Debug.Log("Teacher data updated successfully");
                _messageHandler.SendNotification("Teacher Status Update",
                    $"Teacher status updated to: {_currentTeacherStatus}");
            }
        });
        
        Debug.Log(_currentTeacherStatus);
        
        
    }
}
