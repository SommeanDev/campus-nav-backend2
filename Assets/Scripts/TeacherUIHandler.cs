using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Firebase.Database;
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
    private Dictionary<string, object> _updates = new Dictionary<string, object>();
        
    void Start()
    {
        _reference = FirebaseDatabase.DefaultInstance.RootReference;
        teacherName.SetText($"Logged in as: {PlayerPrefs.GetString("Name")}");
        _currentTeacherStatus = _reference.Child("teacher").Child(PlayerPrefs.GetString("UserId")).Child("Status").GetValueAsync().ToString();
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
    }

    public void EnableRoomSelection(int statusId)
    {
        _currentTeacherStatus = statusDropdown.options[statusId].text;
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
                    $"Teacher status updated to: {_currentTeacherStatus} Room no: {currentRoomDropdown.options[currentRoomDropdown.value].text}");
            }
        });
        
        Debug.Log(_currentTeacherStatus);
        
        
    }
}
