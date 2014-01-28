using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class CutsceneEditorInput : TimelineInput
{
    #region Constants

    #endregion

    #region Variables
    CutsceneEditor m_CutsceneEditor;
    #endregion

    #region Properties
    Cutscene SelectedCutscene
    {
        get { return m_CutsceneEditor.GetSelectedCutscene(); }
    }
    #endregion

    #region Functions
    public CutsceneEditorInput(CutsceneEditor editor)
        : base(editor)
    {
        m_CutsceneEditor = editor;
    }

    protected override void DragTimeSlider(Vector2 mousePos, float amount)
    {
        base.DragTimeSlider(mousePos, amount);
        HandleTimeJump();
    }

    protected override void HandleTimeJump()
    {
        if (Application.isPlaying)
        {
            editor.m_MockPlaying = false;
            SelectedCutscene.FastForward(editor.CurrentTime, Cutscene.MaxFastForwardSpeed, OnFinishedFastForwarding);
            m_CutsceneEditor.IsFastForwarding = true;
        }
    }

    /// <summary>
    /// Returns a list of Cutscene events that the rubberband selection area intersects
    /// </summary>
    /// <returns></returns>
    override protected List<CutsceneTrackItem> GetRubberBandSelections()
    {
        List<CutsceneTrackItem> selected = new List<CutsceneTrackItem>();
        foreach (CutsceneEvent se in SelectedCutscene.CutsceneEvents)
        {
            if (!se.Hidden && Utils.IsRectOverlapping(m_RubberBandSelectionArea, se.GuiPosition))
            {
                selected.Add(se);
            }
        }
        return selected;
    }

    void OnFinishedFastForwarding(Cutscene cutscene)
    {
        m_CutsceneEditor.IsFastForwarding = false;
    }

    protected override void HandleSequencerDragDrop(string[] dragAndDropObjectPaths)
    {
        Array.ForEach<string>(dragAndDropObjectPaths, s => Debug.Log(s));
        string xmlFile = Array.Find<string>(dragAndDropObjectPaths, s => Path.GetExtension(s) == ".xml");
        string bmlFile = Array.Find<string>(dragAndDropObjectPaths, s => Path.GetExtension(s) == ".bml");
        if (!string.IsNullOrEmpty(xmlFile) && !string.IsNullOrEmpty(bmlFile))
        {
            m_CutsceneEditor.RequestFileOpenBMLXMLPair(xmlFile);
        }
        else if (!string.IsNullOrEmpty(xmlFile))
        {
            m_CutsceneEditor.RequestFileOpenXML(xmlFile);
        }
        else if (!string.IsNullOrEmpty(bmlFile))
        {
            m_CutsceneEditor.RequestFileOpenBML(bmlFile);
        }
    }
    #endregion
}
