using System;
using System.IO;
using UnityEngine;

public class ScreenCaptureButton : MonoBehaviour
{
    [SerializeField] private string filePrefix = "farm_screenshot";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            CaptureScreenToDesktop();
        }
    }

    public void CaptureScreenToDesktop()
    {
        string saveDirectory = GetDesktopPath();
        string fileName = $"{filePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string filePath = Path.Combine(saveDirectory, fileName);

        ScreenCapture.CaptureScreenshot(filePath);
        Debug.Log($"[ScreenCaptureButton] Screenshot saved: {filePath}", this);
    }

    private string GetDesktopPath()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        if (!string.IsNullOrEmpty(desktopPath) && Directory.Exists(desktopPath))
        {
            return desktopPath;
        }

        Debug.LogWarning(
            $"[ScreenCaptureButton] Desktop path is unavailable. Using persistentDataPath: {Application.persistentDataPath}",
            this);
        return Application.persistentDataPath;
    }
}
