using System.Collections;
using System.IO;
using UnityEngine;

public class CachedDownloader : MonoBehaviour {
    
    public static CachedDownloader instance = null; 
    
    void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);
    }

    static long GetInt64HashCode(string strText)
    {
        var s1 = strText.Substring(0, strText.Length / 2);
        var s2 = strText.Substring(strText.Length / 2);
        
        var x= ((long)s1.GetHashCode()) << 0x20 | s2.GetHashCode();
        
        return x;
    }

    static bool CheckFileOutOfDate(string filePath)
    {
        System.DateTime written = File.GetLastWriteTimeUtc(filePath);
        System.DateTime now = System.DateTime.UtcNow;
        double ageHours = now.Subtract(written).TotalHours;
        return ageHours>300;
    }

    static IEnumerator Download(WWW www, string filePath, bool web)
    {
        yield return www;
        
        if (www.error == null)
        {
            if (web)
            {
                Debug.Log("Saving downloaded content " + www.url + " to " + filePath);

                File.WriteAllBytes(filePath, www.bytes);

                Debug.Log("Saved " + www.url + " to " + filePath);
            }
            else
            {
                Debug.Log("Loaded cache " + www.url);
            }
        }
        else
        {
            if (!web)
            {
                File.Delete(filePath);
            }
            Debug.Log("WWW ERROR " + www.error);
        }
    }

    public static WWW GetCachedWWW(string url)
    {
        string filePath;
#if UNITY_EDITOR
        filePath = Application.dataPath + "/temp";
        System.IO.Directory.CreateDirectory(filePath);
#else
        filePath = Application.persistentDataPath;
#endif
        filePath += "/" + GetInt64HashCode(url);

        bool web = false;
        WWW www;
        bool useCached = false;
        useCached = System.IO.File.Exists(filePath) && !CheckFileOutOfDate(filePath);
        if (useCached)
        {
            string pathforwww = "file://" + filePath;
            Debug.Log("TRYING FROM CACHE " + url + "  file " + pathforwww);
            www = new WWW(pathforwww);
        }
        else
        {
            web = true;
            www = new WWW(url);
        }
        CachedDownloader.instance.StartCoroutine(Download(www, filePath, web));
        return www;
    }
}
