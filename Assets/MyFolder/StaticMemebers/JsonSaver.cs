using System;
using System.IO;
using UnityEngine;

[Serializable]
public class buttonData
{
    public string[] dirtyPath;
    public string[] cleanPath;
    public Vector2 size;
}

[Serializable]
public class Settings
{
    public buttonData buttonData;
    public string[] backgroundPath;
    public string[] bkgPath;
    public float playTime;
    public Vector2 spawnInterval;
    public int maxSpawnableNum;
    public Vector2 burningInterval;
    public int burningSpawnableNum;
    public Vector2 scaleMinToMax;
    public float blockTop;
    public float blockBottom;
    public Vector2 backgroundPos;
    public string[] itemPath;
    public string[] portNames;
    public Vector2 speed;
    public string[] hoverMovie;
}

public class JsonSaver : MonoBehaviour
{
    public static JsonSaver instance;
    public Settings settings;

    public Sprite[] dirtySprites;
    public string[] cleanPath;

    public string[] backgroundPath;
    public string[] bkgPath;
    public string[] itemPath;
    public string[] hoverMovie;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        settings = LoadJsonData<Settings>("settings.json");
        GetImage(settings.buttonData.dirtyPath, out dirtySprites);

        SetPath(settings.buttonData.cleanPath, out cleanPath);
        SetPath(settings.backgroundPath, out backgroundPath);
        SetPath(settings.bkgPath, out bkgPath);
        SetPath(settings.itemPath, out itemPath);
        SetPath(settings.hoverMovie, out hoverMovie);
    }

    private T LoadJsonData<T>(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        filePath = filePath.Replace("\\", "/");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Debug.Log("Loaded JSON: " + json); // JSON 문자열 출력
            return JsonUtility.FromJson<T>(json);
        }

        Debug.LogWarning("File does not exist!");
        return default;
    }


    private void GetImage(string[] paths, out Sprite[] sprites)
    {
        // Sprite 배열 초기화
        sprites = new Sprite[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, paths[i]);

            if (File.Exists(fullPath))
            {
                // 파일에서 텍스처 읽기
                byte[] imageData = File.ReadAllBytes(fullPath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);

                // 텍스처를 Sprite로 변환
                sprites[i] = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
            else
            {
                Debug.LogError($"Image file not found at path: {fullPath}");
                sprites[i] = null;
            }
        }
    }

    private void SetPath(string[] path, out string[] fixedPath)
    {
        if (path == null)
        {
            fixedPath = null; // 길이가 0일 때는 null 반환
            return;
        }

        int length = path.Length;
        fixedPath = new string[length];

        //  Debug.Log("path Length : " + length);
        for (int i = 0; i < length; i++)
        {
            fixedPath[i] = Path.Combine(Application.streamingAssetsPath, path[i]);
            fixedPath[i] = fixedPath[i].Replace("\\", "/");
            //  Debug.Log("fixedPath : " + fixedPath[i]);
        }
    }
    
    
/*private void SetMoviePath(VideoData[] path, out string[] fixedPath)
{
    if (path == null)
    {
        fixedPath = null; // 길이가 0일 때는 null 반환
        return;
    }

    int length = path.Length;
    fixedPath = new string[length];

    //  Debug.Log("path Length : " + length);
    for (int i = 0; i < length; i++)
    {
        fixedPath[i] = Path.Combine(Application.streamingAssetsPath, path[i].path);
        fixedPath[i] = fixedPath[i].Replace("\\", "/");
        //  Debug.Log("fixedPath : " + fixedPath[i]);
    }
}*/
}

