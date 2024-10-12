using UnityEngine;
using Unity.Notifications.Android;
using System;

public class MessageHandler : MonoBehaviour
{
    private const string ChannelId = "test_channel";
    private static bool _isChannelCreated = false; // Static flag to ensure channel is created only once

    void Start()
    {
        CreateNotificationChannel();
        RequestNotificationPermission();
    }

    void CreateNotificationChannel()
    {
        // Only create the channel if it hasn't been created already
        if (!_isChannelCreated)
        {
            var channel = new AndroidNotificationChannel()
            {
                Id = ChannelId,
                Name = "Test Notifications",
                Importance = Importance.High,
                Description = "Generic notifications for testing firebase",
            };

            AndroidNotificationCenter.RegisterNotificationChannel(channel);
            Debug.Log($"Notification channel created. ID: {ChannelId}");

            _isChannelCreated = true; // Set flag to true after creating the channel
        }
        else
        {
            Debug.Log($"Notification channel already exists. ID: {ChannelId}");
        }
    }

    void RequestNotificationPermission()
    {
        Debug.Log($"Notification permission status: {AndroidNotificationCenter.UserPermissionToPost}");
    }

    public void SendNotification(string title, string message)
    {
        Debug.Log($"Attempting to send notification - Title: {title}, Body: {message}");

        try
        {
            var notification = new AndroidNotification()
            {
                Title = title,
                Text = message,
                SmallIcon = "icon_small",
                LargeIcon = "icon_large",
                FireTime = DateTime.Now.AddSeconds(5)
            };

            // Send the notification
            var id = AndroidNotificationCenter.SendNotification(notification, ChannelId);

            if (id >= 0)
            {
                Debug.Log($"Notification sent successfully with ID: {id}");

                // Check the status of the notification
                var status = AndroidNotificationCenter.CheckScheduledNotificationStatus(id);
                Debug.Log($"Notification status: {status}");
            }
            else
            {
                Debug.LogError($"Failed to send notification. ID: {id}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception when sending notification: {e.Message}");
        }
    }
}
