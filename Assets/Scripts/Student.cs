
using System.Collections.Generic;
using UnityEngine;

public class Student
{
    public string Name;
    public string Email;
    public string Password;
    public string Branch;
    public string Semester;
    
    public List<Teacher> SubscribedToTeachers;

    public Student(string name, string email, string password, string branch, string semester)
    {
        this.Name = name;
        this.Email = email;
        this.Password = password;
        this.Branch = branch;
        this.Semester = semester;
    }

    public void MakeSubscribedToList(List<Teacher> subscribedToTeachers)
    {
        this.SubscribedToTeachers = subscribedToTeachers;
        Debug.Log(subscribedToTeachers.ToArray().ToString());
    }
}
