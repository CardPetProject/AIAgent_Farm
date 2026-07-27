using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class LocalGameSaveRepository
{
    private const string SaveDirectoryName = "Saves";
    private const string SaveFileName = "save_slot_0.json";

    public static string SaveDirectoryPath => Path.Combine(Application.persistentDataPath, SaveDirectoryName);
    public static string SaveFilePath => Path.Combine(SaveDirectoryPath, SaveFileName);

    public static bool HasSaveFile()
    {
        return File.Exists(SaveFilePath);
    }

    public static bool TrySave(GameStateSnapshot snapshot, out string savedPath, out string error)
    {
        savedPath = SaveFilePath;
        error = null;

        if (snapshot == null)
        {
            error = "snapshot is null.";
            return false;
        }

        try
        {
            Directory.CreateDirectory(SaveDirectoryPath);

            string json = JsonConvert.SerializeObject(snapshot, Formatting.Indented);
            string tempPath = $"{SaveFilePath}.tmp";

            File.WriteAllText(tempPath, json);

            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }

            File.Move(tempPath, SaveFilePath);
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    public static bool TryLoad(out GameStateSnapshot snapshot, out string loadedPath, out string error)
    {
        snapshot = null;
        loadedPath = SaveFilePath;
        error = null;

        if (!File.Exists(SaveFilePath))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            snapshot = JsonConvert.DeserializeObject<GameStateSnapshot>(json);

            if (snapshot == null)
            {
                error = "save file did not contain a valid snapshot.";
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }
}
