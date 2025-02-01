using System;
using System.IO;
using UnityEngine;


namespace MrLewisPaite.JSONSave
{
    public class JSONStorage
    {
        // saving
        public static void SaveDataToFile<T>(T data, string path)
        {
            try
            {
                string jsonString = JsonUtility.ToJson(data);
                File.WriteAllText(path, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured while saving data: {ex.Message}");
            }
        }

        // loading
        public static T LoadFromFile<T>(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string jsonString = File.ReadAllText(path);
                    return JsonUtility.FromJson<T>(jsonString);
                }
                else
                {
                    throw new FileNotFoundException("File wasn't found");
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"An error occured while loading data: {ex.Message}");
                return default;
            }
        }
    }
}