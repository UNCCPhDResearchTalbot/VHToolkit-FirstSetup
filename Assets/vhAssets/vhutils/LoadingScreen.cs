using UnityEngine;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    #region Variables
    public bool m_ShowAtStart = true;
    float m_TotalDisplayTime = 0;
    static LoadingScreen _LoadingScreen;
    #endregion

    #region Properties
    public float TotalDisplayTime
    {
        get { return m_TotalDisplayTime; }
    }
    #endregion

    public static LoadingScreen Get()
    {
        if (_LoadingScreen == null)
        {
            _LoadingScreen = Object.FindObjectOfType(typeof(LoadingScreen)) as LoadingScreen;
        }

        return _LoadingScreen;
    }

    public void Start()
    {
        if (guiTexture == null)
        {
            Debug.LogError("LoadingScreen needs a guiTexture");
        }

        if (!m_ShowAtStart)
        {
            ShowLoadingScreen(false);
        }
    }

    public void SetTexture(string tex)
    {
        if (string.IsNullOrEmpty(tex))
        {
            return;
        }

        Texture texture = (Texture)Resources.Load(tex, typeof(Texture));
        if (texture == null)
        {
            Debug.LogError("No such texture: '" + tex + "' exists");
        }
        else
        {
            SetTexture(texture);
        }
    }

    public void SetTexture(Texture tex)
    {
        guiTexture.texture = tex;
        //guiTexture.pixelInset = new Rect(-tex.width / 2, -tex.height / 2, tex.width, tex.height);
        guiTexture.pixelInset = new Rect(-Screen.width / 2, -Screen.height / 2, Screen.width, Screen.height);
    }

    /// <summary>
    /// displays the loading screen until the current level is finished loading
    /// </summary>
    public void ShowLevelLoadingScreen()
    {
        ShowLoadingScreen(true);
        StartCoroutine(ShowLevelLoadingScreenCoroutine());
    }

    IEnumerator ShowLevelLoadingScreenCoroutine()
    {
        while (Application.isLoadingLevel)
        {
            yield return new WaitForEndOfFrame();
        }
        ShowLoadingScreen(false);
    }

    /// <summary>
    /// displays the loading screen for a number of seconds
    /// </summary>
    /// <param name="seconds">the time, in seconds, that you want the loading screen to display</param>
    public void ShowLoadingScreen(float seconds)
    {
        ShowLoadingScreen(true);
        StartCoroutine(ShowLoadingScreenCoroutine(0, seconds, 0));
    }

    public void ShowLoadingScreen(float fadeInTime, float secondsAtFullOpacity, float fadeOutTime)
    {
        ShowLoadingScreen(true);
        StartCoroutine(ShowLoadingScreenCoroutine(fadeInTime, secondsAtFullOpacity, fadeOutTime));
    }

    public IEnumerator ShowLoadingScreenCoroutine(float fadeInTime, float secondsAtFullOpacity, float fadeOutTime)
    {
        m_TotalDisplayTime = fadeInTime + secondsAtFullOpacity + fadeOutTime;
        float timer = fadeInTime;
        Color newColor = guiTexture.color;

        if (timer > 0)
        {
            newColor.a = 0;
            guiTexture.color = newColor;
        }

        // fade in
        while (timer > 0)
        {
            yield return new WaitForEndOfFrame();
            timer -= Time.deltaTime;
            newColor.a = 1.0f - timer / fadeInTime;
            guiTexture.color = newColor;
        }

        // hold full opacity
        yield return new WaitForSeconds(secondsAtFullOpacity);

        // fade out
        timer = fadeOutTime;
        while (timer > 0)
        {
            yield return new WaitForEndOfFrame();
            timer -= Time.deltaTime;
            newColor.a = timer / fadeOutTime;
            guiTexture.color = newColor;
        }

        ShowLoadingScreen(false);
        guiTexture.color = Color.white;
    }

    public void ShowLoadingScreen(bool show)
    {
#if UNITY_2_6 || UNITY_2_6_1 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 ||UNITY_3_3 ||UNITY_3_4 || UNITY_3_5
        gameObject.active = show;
#else
        gameObject.SetActive(show);
#endif
        
    }
}
