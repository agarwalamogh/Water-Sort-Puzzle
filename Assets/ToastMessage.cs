using UnityEngine;

public class ToastMessage : Singleton<ToastMessage>
{
    

    string _toastString;
    string _input;
    AndroidJavaObject _currentActivity;
    AndroidJavaClass _unityPlayer;
    AndroidJavaObject _context;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _currentActivity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            _context = _currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        }
    }


    public static void ShowToastOnUiThread(string toastString)
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            return;
        }

        Instance._toastString = toastString;
        Instance._currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(Instance.ShowToast));
    }

    void ShowToast()
    {
        Debug.Log(this + ": Running on UI thread");

        var t = new AndroidJavaClass("android.widget.Toast");
        var javaString = new AndroidJavaObject("java.lang.String", _toastString);
        var toast = t.CallStatic<AndroidJavaObject>("makeText", _context, javaString, t.GetStatic<int>("LENGTH_SHORT"));
        toast.Call("show");
    }
}