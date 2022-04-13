
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(Camera))]
public class IconCreator : MonoBehaviour
{
//    public static event Action<string, Texture2D> IconCreated; 

    public Camera Camera => GetComponent<Camera>();

    public Vector2 Size
    {
        get => new Vector2(Camera.orthographicSize * 2 * Camera.aspect, Camera.orthographicSize * 2);
        set
        {
            Camera.orthographicSize = value.y / 2;
            Camera.aspect = value.x / value.y;
        }
    }

    public LayerMask LayerMask
    {
        get => Camera.cullingMask;
        set => Camera.cullingMask = value;
    }

    public string FileName { get; set; }

    public static string CreateIcon(GameObject go, Vector2 size, LayerMask layerMask, string fileName)
    {
        var iconCreator = Instantiate(
            Resources.Load<IconCreator>(nameof(IconCreator))
            , go.transform.position - Vector3.forward * 3f
            , Quaternion.identity);
        iconCreator.transform.LookAt(go.transform);
        iconCreator.Size = size;
        iconCreator.LayerMask = layerMask;
        iconCreator.FileName = fileName;
        var shot = iconCreator.TakeScreenShot();
        Destroy(iconCreator.gameObject);
        return shot;
    }


    private string TakeScreenShot()
    {
        try
        {
            var height = Camera.pixelHeight;
            var width = (int) (Camera.aspect * height);
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
           
            Camera.targetTexture = rt;
            var screenShot = new Texture2D(width, height, TextureFormat.ARGB32, false);
            Camera.Render();
        
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            Camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            rt.Release();
            Destroy(rt);
           
//            screenShot.Resize(width , height , TextureFormat.ARGB32, true);
//            screenShot.Apply();
      
            var bytes = screenShot.EncodeToPNG();
            File.WriteAllBytes(FileName, bytes);
            Destroy(screenShot);
          


            return FileName;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return "";
        }
    }


    public static IEnumerator LoadIconFromFileAsync(string path, Action<Texture2D> completed = null)
    {
        using (var request = UnityWebRequestTexture.GetTexture($"{path}"))
        {
            yield return request.SendWebRequest();
            if (!string.IsNullOrEmpty(request.error))
            {
                completed?.Invoke(null);
                yield break;
            }

            var tex = DownloadHandlerTexture.GetContent(request);

            if (tex == null)
            {
                completed?.Invoke(null);
                yield break;
            }

            completed?.Invoke(tex);
        }
    }
}

public class IconCreateTask : CustomYieldInstruction
{
    public override bool keepWaiting => !Completed;
    public bool Completed { get; private set; }

    public string Error { get; private set; }
    public Texture2D Texture { get; private set; }
    public string Path { get; private set; }

    public void Complete(Texture2D texture, string path)
    {
        Texture = texture;
        Path = path;
        Completed = true;
    }

    public void Complete(string error)
    {
        Error = error;
        Completed = true;
    }
}