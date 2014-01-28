using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneManager : MonoBehaviour
{
    #region Variables
    public bool m_AutoPlayCutscenes = true;
    List<Cutscene> m_Cutscenes = new List<Cutscene>();
    List<Cutscene> m_UnplayedCutscenes = new List<Cutscene>();
    #endregion

    #region Properties
    public int NumCutscenes
    {
        get { return m_Cutscenes.Count; }
    }
    #endregion

    #region Functions
    void Start()
    {
        FindAllCutscenes();

        if (m_AutoPlayCutscenes)
        {
            PlayCutscenes();
        }
    }

    void FindAllCutscenes()
    {
        m_Cutscenes.Clear();
        m_Cutscenes.AddRange((Cutscene[])GameObject.FindObjectsOfType(typeof(Cutscene)));

        // sort them by start time
        m_Cutscenes.Sort(delegate(Cutscene cutscene1, Cutscene cutscene2) { return cutscene1.StartTime <= cutscene2.StartTime ? -1 : 1; });
    }

    /// <summary>
    /// Find a cutscene by it's given name
    /// </summary>
    /// <param name="cutsceneName"></param>
    /// <returns></returns>
    public Cutscene GetCutsceneByName(string cutsceneName)
    {
        return m_Cutscenes.Find(delegate(Cutscene cutscene) { return cutscene.CutsceneName == cutsceneName; });
    }

    /// <summary>
    /// Plays all cutscenes at their appropriate starting time
    /// </summary>
    public void PlayCutscenes()
    {
        StartCoroutine(PlayCutscenesCoroutine());
    }

    IEnumerator PlayCutscenesCoroutine()
    {
        m_UnplayedCutscenes.Clear();
        m_UnplayedCutscenes.AddRange(m_Cutscenes);
        float timePassed = 0;

        while (m_UnplayedCutscenes.Count > 0)
        {
            for (int i = 0; i < m_UnplayedCutscenes.Count; i++)
            {  
                if (timePassed >= m_UnplayedCutscenes[i].StartTime)
                {
                    m_UnplayedCutscenes[i].Play();
                    m_UnplayedCutscenes.RemoveAt(i--);
                }
                else
                {
                    // these are sorted by start time in ascending order, so we can early out
                    break;
                }
            }
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    #endregion
}
