using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UIElements;

public class TeacherListUI : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private RectTransform _teacherListContainer;
    [SerializeField] private StudentUIHandler _studentUIHandler;
    
    private ListView _teacherListView;
    private List<Teacher> _teachers = new List<Teacher>();
    private List<string> _teacherIDs = new List<string>();
    private List<string> _subscribedTeacherIDs = new List<string>();
    

    [SerializeField] private int widthPercent = 50;
    [SerializeField] private int heightPercent = 50;
    private void Awake()
    {
        UnityMainThreadDispatcher.Instance();
    }

    private void OnEnable()
    {
        var root = _uiDocument.rootVisualElement;
        
        _teacherListView = root.Q<ListView>("TeachersList");

        if (_teacherListView == null)
        {
            Debug.LogError("TeachersList Not found in UI Document");
        }
        
        // Set up List View
        _teacherListView.makeItem = () =>
        {
            var container = new VisualElement();
            container.AddToClassList("teacher-item");
            container.style.flexDirection = FlexDirection.Row;
            
            var label = new Label();
            label.name = "TeacherLabel";
            label.AddToClassList("teacher-label");
            
            var toggle = new Toggle();
            toggle.name = "teacherToggle";
            toggle.AddToClassList("teacher-toggle");
            
            container.Add(label);
            container.Add(toggle);
            
            return container;
        };
        _teacherListView.bindItem = (element, i) =>
        {
            var label = element.Q<Label>("TeacherLabel");
            var toggle = element.Q<Toggle>("teacherToggle");
            
            label.text = _teachers[i].ToString();

            toggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    if (!_subscribedTeacherIDs.Contains(_teacherIDs[i]))
                    {
                        _subscribedTeacherIDs.Add(_teacherIDs[i]);
                    }
                }
                else
                {
                    if (_subscribedTeacherIDs.Contains(_teacherIDs[i]))
                    {
                        _subscribedTeacherIDs.Remove(_teacherIDs[i]);
                    }
                }
                Debug.Log($"Item {_teachers[i].ToString()} selected: {evt.newValue}");
            });
            Debug.Log($"Binding item {i}: {_teachers[i].ToString()}");
        };

        _teacherListView.style.height = new StyleLength(new Length(heightPercent, LengthUnit.Percent));
        _teacherListView.style.flexGrow = 1;
        _teacherListView.style.width = new StyleLength(new Length(widthPercent, LengthUnit.Percent));
        
        Debug.Log("UI Setup Completed. Fetching teachers data from Firebase.");
        FetchTeachersFromDatabase();
    }

    public void OnSaveButtonClicked()
    {
        Debug.Log("Clicked Save Button");
        _studentUIHandler.SaveSelectedTeachers(_subscribedTeacherIDs);
    }

    private void FetchTeachersFromDatabase()
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("teacher").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch teachers: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                _teachers.Clear();

                foreach (var childSnapshot in snapshot.Children)
                {
                    string teacherID = childSnapshot.Key;
                    string name = childSnapshot.Child("Name").Value?.ToString() ?? "Unknown";
                    string email = childSnapshot.Child("Email").Value?.ToString() ?? "no email";
                    string password = childSnapshot.Child("Password").Value?.ToString() ?? "no password";
                    string phone = childSnapshot.Child("Phone").Value?.ToString() ?? "no phone";
                    
                    _teachers.Add(new Teacher(name, email, password, phone));
                    _teacherIDs.Add(teacherID);
                }

                Debug.Log($"Fetched {_teachers.Count} teachers");
                
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    _teacherListView.itemsSource = _teachers;
                    _teacherListView.RefreshItems();
                    Debug.Log("ListView Updated with new data");
                });
            }
        });
    }
}
