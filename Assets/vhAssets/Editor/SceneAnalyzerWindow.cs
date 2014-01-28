using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

public class SceneAnalyzerWindow : EditorWindow {
    #region Constants
    const string SavedWindowPosXKey = "SceneAnalyzerWindowX";
    const string SavedWindowPosYKey = "SceneAnalyzerWindowY";
    const string SavedWindowWKey = "SceneAnalyzerWindowW";
    const string SavedWindowHKey = "SceneAnalyzerWindowH";
    const float SpaceBetweenObjects = 14;
    const float ColumnWidth = 120;
    const float RecommendationColumnWidth = 240;
    const int TabDefault = 1;
    const int TabIndent = 3;
    const int TriCountWarning = 4500;
    
    /// <summary>
    /// Data collected from renderable objects in the scene
    /// </summary>
    struct PerformanceData
    {
        public int numDrawCalls;
        public int numVerts;
        public int numTris;
        public int numTextures; 
        public int numBones;
        public int warningLevel;
        public float vertbbRatio;
        //public float texturebbRatio;       
        
        static public PerformanceData operator +(PerformanceData left, PerformanceData right)
        {
            PerformanceData retVal = new PerformanceData();
            retVal.numDrawCalls = left.numDrawCalls + right.numDrawCalls;
            retVal.numVerts = left.numVerts + right.numVerts;
            retVal.numTris = left.numTris + right.numTris;
            retVal.numBones = left.numBones + right.numBones; 
            retVal.numTextures = left.numTextures + right.numTextures;
            retVal.warningLevel = left.warningLevel + right.warningLevel;
            return retVal;
        }
    }
    
    /// <summary>
    /// Collects data from a renderable object and draws it on the window
    /// </summary>
    class HierarchyObject
    {
        #region Variables
        public bool showChildren = false;
        public Renderer component;
        public HierarchyObject parent;
        public List<HierarchyObject> children = new List<HierarchyObject>();     
        MeshFilter meshFilter;
        PerformanceData perfData;
        PerformanceData totalHierarchyData;
        string textureSizes = string.Empty;
        string recommendations = string.Empty;
        string gameobjectHierarchy = string.Empty;
        #endregion
        
        #region Properties
        public int numDrawCalls { get { return perfData.numDrawCalls; } }
        public int numTris { get { return perfData.numTris; } }
        public int numVerts { get { return perfData.numVerts; } }
        public int numBones { get { return perfData.numBones; } } 
        public int numTextures { get { return perfData.numTextures; } }
        public int warningLevel { get { return perfData.warningLevel; } }
        public bool hasChildren { get { return children.Count > 0; } } 
        public float vertbbRatio { get { return perfData.vertbbRatio; } } 
        public PerformanceData PerfData { get { return perfData; } }
        public PerformanceData TotalHierarchyData { get { return totalHierarchyData; } }
        public string name { get { return component != null ? component.name : ""; } }
        public string hierarchyName { get { return gameobjectHierarchy; } } 
        #endregion       
        
        #region Functions
        public HierarchyObject(Renderer c)
        {
            perfData = new PerformanceData();
            component = c;
            perfData.numDrawCalls = component.sharedMaterials.Length;
           
            // find textures associated with this meshes materials
            Dictionary<float, int> textureCountList = new Dictionary<float, int>();
            List<string> shaderNames = new List<string>();
            bool useTextureAtlas = false;
            bool hasNullTextures = false;
            
            for (int i = 0; i < perfData.numDrawCalls; i++)
            {
                if (component.sharedMaterials[i] == null)
                    continue;   
                
                // WARNING: this code block was provided by unity as a generic
                // way of finding variable types inside a shader. It is not part
                // of their API and could change at any time
                Shader shader = component.sharedMaterials[i].shader;
                var assembly = typeof(Editor).Assembly;
                var shaderUtil = assembly.GetType("UnityEditor.ShaderUtil");
                var getPropertyCount = shaderUtil.GetMethod("GetPropertyCount");
                var getPropertyType = shaderUtil.GetMethod("GetPropertyType");
                var getPropertyName = shaderUtil.GetMethod("GetPropertyName");
                int properties = (int)getPropertyCount.Invoke(null, new object[] { shader });
                for (int j = 0; j < properties; ++j)
                {
                    // shaderPropertyType is one of the following:
                    //    Color - 0
                    //    Vector - 1
                    //    Float - 2
                    //    Range - 3
                    //    TexEnv - 4
                    object shaderPropertyType = getPropertyType.Invoke(null, new object[] { shader, j }); 
                    string shaderPropertyName = (string)getPropertyName.Invoke(null, new object[] { shader, j });
                    if ((int)shaderPropertyType == 4) // Texture
                    {
                        if (shaderNames.Contains(shader.name))
                        {
                            useTextureAtlas = true;
                            perfData.warningLevel += 1;
                        }
                        else
                        {
                            shaderNames.Add(shader.name);   
                        }
                        
                        //Debug.Log(string.Format("[{0}]: {1} : {2}", j, shaderPropertyType, shaderPropertyName));
                        Texture shaderTexture = component.sharedMaterials[i].GetTexture(shaderPropertyName);
                        if (shaderTexture != null)
                        {
                            ++perfData.numTextures;
                            float largestDimension = Mathf.Max(shaderTexture.width, shaderTexture.height);
                            float key = largestDimension / 1024.0f;
                            if (!textureCountList.ContainsKey(key))
                                textureCountList.Add(key, 1);
                            else
                                textureCountList[key]++;
                            
                            
                            //perfData.combinedTextureWH.x = shaderTexture.width;
                            //perfData.combinedTextureWH.y = shaderTexture.height;
                            //Debug.Log(component.sharedMaterials[i].GetTexture(shaderPropertyName).name);
                        }
                        else
                        {
                            hasNullTextures = true;
                            perfData.warningLevel += 1;
                        }
                    }
                }
            }   
            
            // build a string based off the texture list
            StringBuilder builder = new StringBuilder(); 
            foreach (KeyValuePair<float, int> kvp in textureCountList)
            {
                string decimalPlaces = kvp.Key < 1 ? "f2" : "f0";
                builder.Append(string.Format("({0}) {1}k, ", kvp.Value, kvp.Key.ToString(decimalPlaces))); 
            }
            if (textureCountList.Count > 0)
            {
                builder.Remove(builder.Length - 2, 2);
            }
            textureSizes = string.Format("{0} {1}", numTextures, builder.ToString());
            
            
            // get mesh data (verts, tris, bones, etc) from the mesh
            Mesh sharedMesh = null;
            if (component is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer smr = component as SkinnedMeshRenderer;
                sharedMesh = smr.sharedMesh;
                perfData.numBones = smr.bones.Length;
            }
            else
            {
                meshFilter = component.GetComponent<MeshFilter>();
                if (meshFilter != null)
                    sharedMesh = meshFilter.sharedMesh; 
            }
            
            if (sharedMesh != null)
            {
                perfData.numTris = sharedMesh.triangles.Length;
                perfData.numVerts = sharedMesh.vertexCount;
                Vector3 bb = sharedMesh.bounds.extents;
                //bb += new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon); // takes care of 0's 
                float volume = (bb.z * bb.x * bb.y);// find volume first
                //Debug.Log(bb + "  " +volume);
                if (volume != 0)
                    perfData.vertbbRatio = Mathf.Round((float)perfData.numVerts / volume);  
            }
            
            // handle recommandations
            if (useTextureAtlas)
            {
                recommendations += string.Format("[Multiple materials using same shader, use texture atlas]");
            }
            if (hasNullTextures)
            {
                recommendations += string.Format("[Shader has empty texture slots, use different shader]");
            }
            if (numTris >= TriCountWarning)
            {
                perfData.warningLevel += numTris / TriCountWarning;
                recommendations += string.Format("[Triangle count high, aim for 1500-4000 or split up the mesh]");
            }  
            
            // fill out hierachy name
            Transform parent = component.transform.parent;
            while (parent != null)
            {
                gameobjectHierarchy = gameobjectHierarchy.Insert(0, string.Format("{0}\\", parent.name));   
                parent = parent.parent;
            }
            gameobjectHierarchy += component.name;
            /*if (gameobjectHierarchy.Length > 0)
            {
                gameobjectHierarchy = gameobjectHierarchy.Remove(gameobjectHierarchy.Length - 1, 1);   
            }*/
        }
        
        public virtual void Draw()
        {
            EditorGUILayout.BeginHorizontal("Label");
            string numDrawCallsStr = numDrawCalls.ToString();
            string numVertsStr = numVerts.ToString();
            string numTrisStr = numTris.ToString();
            string numBonesStr = numBones.ToString();
            string vertbbRatioStr = vertbbRatio.ToString("f0") + ":1";
            if (hasChildren)
            {
                showChildren = EditorGUILayout.Foldout(showChildren, name);
                numDrawCallsStr += string.Format(" ({0})", totalHierarchyData.numDrawCalls);
                numVertsStr += string.Format(" ({0})", totalHierarchyData.numVerts);
                numTrisStr += string.Format(" ({0})", totalHierarchyData.numTris);
                numBonesStr += string.Format(" ({0})", totalHierarchyData.numBones);
            }
            else
            {
                EditorGUILayout.SelectableLabel(name, GUILayout.Height(SpaceBetweenObjects)); 
            }
            
            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = TabDefault;
            EditorGUILayout.SelectableLabel(numDrawCallsStr, GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth));
            EditorGUILayout.SelectableLabel(numVertsStr, GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth));
            EditorGUILayout.SelectableLabel(numTrisStr, GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth));
            EditorGUILayout.SelectableLabel(numBonesStr, GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth));
            EditorGUILayout.SelectableLabel(textureSizes, GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth));
            EditorGUILayout.SelectableLabel(vertbbRatioStr, GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth));
            EditorGUILayout.SelectableLabel(recommendations, GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(RecommendationColumnWidth));
            //EditorGUILayout.SelectableLabel(gameobjectHierarchy, GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(RecommendationColumnWidth));
            if (GUILayout.Button(gameobjectHierarchy, GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(RecommendationColumnWidth)))
            {
                Selection.activeGameObject = component.gameObject;
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.FrameSelected();
                }
            }
            EditorGUI.indentLevel = prevIndent;
            EditorGUILayout.EndHorizontal();
        }
        
        PerformanceData GetHierarchyData()
        {
            PerformanceData retVal = new PerformanceData();
            
            // depth first traversal
            Stack<HierarchyObject> stack = new Stack<HierarchyObject>();
            stack.Push(this);
            while (stack.Count != 0)
            {
                HierarchyObject ho = stack.Pop();
                retVal += ho.PerfData;
                for (int i = 0; i < ho.children.Count; i++)
                {
                    stack.Push(ho.children[i]);
                }
            }
            
            return retVal;
        }
        
        public void CalculateHierarchyTotal()
        {
            totalHierarchyData = GetHierarchyData();
        }
        #endregion
    }
    
    /// <summary>
    /// Represents a button that can be used to sort data on the gui
    /// </summary>
    public class SortingColumn
    {
        public enum ColumnType
        {
            Draw_Calls,
            Verts,
            Tris,
            Bones,
            Textures,
            Vert_BB_Ratio,
            Recommendation,
            GameObject_Hierarchy,
        }
        
        public ColumnType columnType;
        public string columnText;
        public bool ascendingOrder;
        public bool isSelected; 
        float columnWidth;
        
        public SortingColumn(ColumnType _columnType, string _columnText, bool _ascendingOrder, bool _isSelected, float _columnWidth)
        {
            columnType = _columnType;
            columnText = _columnText;
            ascendingOrder = _ascendingOrder;
            isSelected = _isSelected; 
            columnWidth = _columnWidth;
        }
        
        public bool Draw()
        {
            string buttonText = columnText;
            Color prevColor = GUI.color;
            if (isSelected)
            {
                GUI.color = Color.yellow; 
                buttonText += string.Format(" {0}", ascendingOrder ? "(Asc)" : "(Desc)");
            }
            bool retVal = GUILayout.Button(buttonText, /*EditorStyles.toolbarButton,*/ GUILayout.Width(columnWidth));
            GUI.color = prevColor;
            return retVal;
        }
    }
    #endregion
    
    #region Variables
    bool showRenderObjects = true;
    List<HierarchyObject> m_RenderObjects = new List<HierarchyObject>();
    List<HierarchyObject> m_SearchObjects = new List<HierarchyObject>();
    Vector2 m_ScrollPosition = Vector2.zero;
    string searchString = string.Empty;
    float m_MaxScrollHeight;
    Rect m_ViewArea = new Rect(10, 0, 1, 1);
    Rect m_RenderObjectsScrollerPosition = new Rect(10, 50, 1, 1);
    
    List<SortingColumn> m_Columns = new List<SortingColumn>();
    SortingColumn m_SelectedColumn;
    PerformanceData m_totaledData;
    
    
    //bool hack = false;
    #endregion
    
    #region Functions
    [MenuItem("VH/Scene Reporting/Scene Analyzer")]
    static void Init()
    {
        SceneAnalyzerWindow window = (SceneAnalyzerWindow)EditorWindow.GetWindow(typeof(SceneAnalyzerWindow));
        //window.autoRepaintOnSceneChange = true;
        window.position = new Rect(PlayerPrefs.GetFloat(SavedWindowPosXKey, 0),
            PlayerPrefs.GetFloat(SavedWindowPosYKey, 0), PlayerPrefs.GetFloat(SavedWindowWKey, 1557),
            PlayerPrefs.GetFloat(SavedWindowHKey, 349));
        
        window.AddSortingColumns();
        window.Refresh();
        window.Show();
        
        EditorApplication.hierarchyWindowChanged += window.HierarchyChanged;
    }
 
    // Update is called once per frame
    void OnGUI() 
    {
        EditorGUILayout.BeginHorizontal();
       
        DrawSearchBox();
        
        DrawUtilities();
        
        EditorGUILayout.EndHorizontal();
            
        // draw columns
        DrawColumns();
        
        // object totals
        DrawTotals();    

        //m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
        m_ViewArea.width = position.width - 40;
        m_ViewArea.height = m_MaxScrollHeight;
        m_RenderObjectsScrollerPosition.width = position.width - 20;
        m_RenderObjectsScrollerPosition.height = position.height - 60;
        
        m_ScrollPosition = GUI.BeginScrollView(m_RenderObjectsScrollerPosition, m_ScrollPosition, m_ViewArea);
        if (m_ScrollPosition.y < 0)
            m_ScrollPosition.y = 0;
        
        if (showRenderObjects)
        {
            DrawObjectHierarchy(m_SearchObjects, TabIndent, false);
        }    

        GUI.EndScrollView();
          
        //EditorGUILayout.EndScrollView();
    }
    
    /// <summary>
    /// Draws the search box.
    /// </summary>
    void DrawSearchBox()
    {
        string prev = searchString;
        searchString = EditorGUILayout.TextField(searchString, EditorStyles.toolbarTextField, GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth)); 
        if (prev != searchString)
        {
            m_SearchObjects.Clear();
            for (int i = 0; i < m_RenderObjects.Count; i++)
            {
                SearchNamesRecursive(m_RenderObjects[i], m_SearchObjects);
            }
            
            if (m_SelectedColumn != null)
            {
                SortBy(m_SearchObjects, m_SelectedColumn.columnType, m_SelectedColumn.ascendingOrder);
            }
        }
    }
    
    /// <summary>
    /// Used with the search box to only render hierarchy objects that match the search string
    /// </summary>
    void SearchNamesRecursive(HierarchyObject ho, List<HierarchyObject> searchObjects)
    {
        if (Regex.IsMatch(ho.name, searchString, RegexOptions.IgnoreCase))
        {
            searchObjects.Add(ho);
        }
        else
        {
            for (int i = 0; i < ho.children.Count; i++)
            {
                SearchNamesRecursive(ho.children[i], searchObjects);
            }
        }
    }
    
    /// <summary>
    /// Draws the columns.
    /// </summary>
    void DrawColumns()
    {
        EditorGUILayout.BeginHorizontal();
        showRenderObjects = EditorGUILayout.Foldout(showRenderObjects, "Render Objects");     
        for (int i = 0; i < m_Columns.Count; i++)
        {
            if (m_Columns[i].Draw())
                SortingColumnClicked(m_Columns[i]);
        } 
        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// Draws the utilities (refresh button)
    /// </summary>
    void DrawUtilities()
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
        if (GUILayout.Button("Refresh"))
        {
            Refresh();
        }
        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// Draws the totals.
    /// </summary>
    void DrawTotals()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.indentLevel = TabDefault;
        EditorGUILayout.SelectableLabel("Totals");
        EditorGUILayout.SelectableLabel(m_totaledData.numDrawCalls.ToString(), GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth));
        EditorGUILayout.SelectableLabel(m_totaledData.numVerts.ToString(), GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth));
        EditorGUILayout.SelectableLabel(m_totaledData.numTris.ToString(), GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth));
        EditorGUILayout.SelectableLabel(m_totaledData.numBones.ToString(), GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth));
        EditorGUILayout.SelectableLabel(m_totaledData.numTextures.ToString(), GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth));
        EditorGUILayout.SelectableLabel("", GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(ColumnWidth)); //vert:bb ratio
        EditorGUILayout.SelectableLabel("", GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(RecommendationColumnWidth)); //recommendations
        EditorGUILayout.SelectableLabel("", GUILayout.Height(SpaceBetweenObjects), GUILayout.Width(RecommendationColumnWidth)); //gameobject hierarchy
        EditorGUILayout.EndHorizontal();
    }
    
    
    /// <summary>
    /// Uses a depth first traversal to render object hierarchies
    /// </summary>
    void DrawObjectHierarchy(List<HierarchyObject> objects, int indentLevel, bool drawAll)
    {
        // this a hack to make sure the render objects aren't offset in the y direction
        GUILayout.Space(-65);
        
        Stack<HierarchyObject> stack = new Stack<HierarchyObject>();
 
        for (int i = 0; i < objects.Count; i++)
        {
            float y = i * SpaceBetweenObjects;
            if (y > m_ScrollPosition.y + position.height
                || y < m_ScrollPosition.y
                )
            {
                if (y < m_ScrollPosition.y)
                {
                    GUILayout.Space(SpaceBetweenObjects);
                }
                
                continue;
            }

            EditorGUI.indentLevel = indentLevel;
            
            stack.Push(objects[i]);
            while (stack.Count != 0)
            {
                HierarchyObject ho = stack.Pop();
                ho.Draw();
                
                if (ho.showChildren)
                {
                    EditorGUI.indentLevel += TabIndent;
                    for (int j = 0; j < ho.children.Count; j++)
                    {
                        stack.Push(ho.children[j]);
                    }
                }
            }
            stack.Clear();
        }
    }
    
    /// <summary>
    /// Clears all data on the gui and recollects it based on what is in the scene
    /// </summary>
    void Refresh()
    {
        m_MaxScrollHeight = 0;
        m_ScrollPosition = Vector2.zero;
        m_SearchObjects.Clear();
        AddSortingColumns();
        FindObjects<Renderer>(m_RenderObjects);
        m_SearchObjects.AddRange(m_RenderObjects);
        m_totaledData = GetPerformanceDataTotals();
    }
    
    void FindObjects<T>(List<HierarchyObject> list) where T : Component
    {
        list.Clear();
        // find the objects
        T[] components = (T[])Editor.FindObjectsOfType(typeof(T));
        Stack<GameObject> stack = new Stack<GameObject>();
        for (int i = 0; i < components.Length; i++)
        {
            // add them if they don't have a parent or if their parent doesn't have the component that you were searching for
            if (components[i].transform.parent == null || components[i].transform.parent.GetComponent<T>() == null)
            {
                //BuildObjectList(list, null, components[i] as Renderer); // TODO: fix this
                stack.Push(components[i].gameObject);
                HierarchyObject parent = null;
                while (stack.Count > 0)
                {
                    GameObject go = stack.Pop().gameObject;
                    Renderer r = go.GetComponent<Renderer>();
                    if (r != null)
                    {
                        HierarchyObject child = new HierarchyObject(r);
                        m_MaxScrollHeight += SpaceBetweenObjects;
                        
                        if (parent != null)
                        {
                            child.parent = parent;
                            parent.children.Add(child);
                        }
                        else
                        {
                            // it's a top level node (no parent)
                            list.Add(child);
                        }  
                        
                        parent = child;
                    }
                    
                    
                    Transform trans = go.transform;
                    for (int j = 0; j < trans.childCount; j++)
                    {
                        stack.Push(trans.GetChild(j).gameObject);
                    }
                }
                stack.Clear();
            }
        }
        
        for (int i = 0; i < list.Count; i++)
        {
            list[i].CalculateHierarchyTotal();
        }

        // alphabetize
        list.Sort((x, y) => string.Compare(x.name, y.name));
    }
    
    void BuildObjectList(List<HierarchyObject> list, HierarchyObject parent, Renderer comp)
    {
        HierarchyObject child = new HierarchyObject(comp);

        if (parent != null)
        {
            parent.children.Add(child);
        }
        else
        {
            // it's a top level node (no parent)
            list.Add(child);
        }
        
        Transform trans = comp.gameObject.transform;
        for (int i = 0; i < trans.childCount; i++)
        {
            Renderer mr = trans.GetChild(i).GetComponent<Renderer>();
            if (mr != null)
            {
                BuildObjectList(list, child, mr); 
            }
        }
    }
    
    void OnDestroy()
    {
        SaveLocation();
    }

    void SaveLocation()
    {
        PlayerPrefs.SetFloat(SavedWindowPosXKey, position.x);
        PlayerPrefs.SetFloat(SavedWindowPosYKey, position.y);
        PlayerPrefs.SetFloat(SavedWindowWKey, position.width);
        PlayerPrefs.SetFloat(SavedWindowHKey, position.height);
    } 
    
    /// <summary>
    /// Returns the tallied up performance stats for all render objects listed
    /// </summary>
    PerformanceData GetPerformanceDataTotals()
    {
        PerformanceData retVal = new PerformanceData();
        for (int i = 0; i < m_RenderObjects.Count; i++)
        {
            retVal += m_RenderObjects[i].TotalHierarchyData;//m_RenderObjects[i].GetHierarchyData();
        }
        return retVal;
    }
    
    /// <summary>
    /// Sorts the data by the column that was clicked
    /// </summary>
    void SortingColumnClicked(SortingColumn column)
    {
        // unselect old column and select new one
        if (m_SelectedColumn != null)
            m_SelectedColumn.isSelected = false;
        m_SelectedColumn = column;
        m_SelectedColumn.isSelected = true;
        
        column.ascendingOrder = !column.ascendingOrder;
        SortBy(m_SearchObjects, column.columnType, column.ascendingOrder);
    }
    
    void SortBy(List<HierarchyObject> listToSort, SortingColumn.ColumnType column, bool ascending)
    {
        switch (column)
        {
        case SortingColumn.ColumnType.Draw_Calls:
            if (ascending)
                listToSort.Sort((x, y) => x.TotalHierarchyData.numDrawCalls == y.TotalHierarchyData.numDrawCalls ? string.Compare(x.name, y.name) : x.TotalHierarchyData.numDrawCalls - y.TotalHierarchyData.numDrawCalls);
            else
                listToSort.Sort((x, y) => x.TotalHierarchyData.numDrawCalls == y.TotalHierarchyData.numDrawCalls ? string.Compare(x.name, y.name) : y.TotalHierarchyData.numDrawCalls - x.TotalHierarchyData.numDrawCalls);
            break;
            
        case SortingColumn.ColumnType.Verts:
            if (ascending)
                listToSort.Sort((x, y) => x.TotalHierarchyData.numVerts == y.TotalHierarchyData.numVerts ? string.Compare(x.name, y.name) : x.TotalHierarchyData.numVerts - y.TotalHierarchyData.numVerts);
            else
                listToSort.Sort((x, y) => x.TotalHierarchyData.numVerts == y.TotalHierarchyData.numVerts ? string.Compare(x.name, y.name) : y.TotalHierarchyData.numVerts - x.TotalHierarchyData.numVerts);
            break;
            
        case SortingColumn.ColumnType.Tris:
            if (ascending)
                listToSort.Sort((x, y) => x.TotalHierarchyData.numTris == y.TotalHierarchyData.numTris ? string.Compare(x.name, y.name) : x.TotalHierarchyData.numTris - y.TotalHierarchyData.numTris);
            else
                listToSort.Sort((x, y) => x.TotalHierarchyData.numTris == y.TotalHierarchyData.numTris ? string.Compare(x.name, y.name) : y.TotalHierarchyData.numTris - x.TotalHierarchyData.numTris);
            break;
            
        case SortingColumn.ColumnType.Bones:
            if (ascending)
                listToSort.Sort((x, y) => x.TotalHierarchyData.numBones == y.TotalHierarchyData.numBones ? string.Compare(x.name, y.name) : x.TotalHierarchyData.numBones - y.TotalHierarchyData.numBones);
            else
                listToSort.Sort((x, y) => x.TotalHierarchyData.numBones == y.TotalHierarchyData.numBones ? string.Compare(x.name, y.name) : y.TotalHierarchyData.numBones - x.TotalHierarchyData.numBones);
            break;
            
        case SortingColumn.ColumnType.Textures:
            if (ascending)
                listToSort.Sort((x, y) => x.TotalHierarchyData.numTextures == y.TotalHierarchyData.numTextures ? string.Compare(x.name, y.name) : x.TotalHierarchyData.numTextures - y.TotalHierarchyData.numTextures);
            else
                listToSort.Sort((x, y) => x.TotalHierarchyData.numTextures == y.TotalHierarchyData.numTextures ? string.Compare(x.name, y.name) : y.TotalHierarchyData.numTextures - x.TotalHierarchyData.numTextures);
            break;
            
        case SortingColumn.ColumnType.Vert_BB_Ratio:
            if (ascending)
                listToSort.Sort((x, y) => (int)x.TotalHierarchyData.vertbbRatio == (int)y.TotalHierarchyData.vertbbRatio ? string.Compare(x.name, y.name) : (int)(x.TotalHierarchyData.vertbbRatio - y.TotalHierarchyData.vertbbRatio));
            else
                listToSort.Sort((x, y) => (int)x.TotalHierarchyData.vertbbRatio == (int)y.TotalHierarchyData.vertbbRatio ? string.Compare(x.name, y.name) : (int)(y.TotalHierarchyData.vertbbRatio - x.TotalHierarchyData.vertbbRatio));
            break;
            
        case SortingColumn.ColumnType.Recommendation:
            if (ascending)
                listToSort.Sort((x, y) => x.TotalHierarchyData.warningLevel == y.TotalHierarchyData.warningLevel ? string.Compare(x.name, y.name) : x.TotalHierarchyData.warningLevel - y.TotalHierarchyData.warningLevel);
            else
                listToSort.Sort((x, y) => x.TotalHierarchyData.warningLevel == y.TotalHierarchyData.warningLevel ? string.Compare(x.name, y.name) : y.TotalHierarchyData.warningLevel - x.TotalHierarchyData.warningLevel);
            break;
        case SortingColumn.ColumnType.GameObject_Hierarchy:
            if (ascending)
                listToSort.Sort((x, y) => string.Compare(x.hierarchyName, y.hierarchyName));
            else
                listToSort.Sort((x, y) => string.Compare(y.hierarchyName, x.hierarchyName));
            break;
        }
    }
    
    public void AddSortingColumns()
    {
        m_Columns.Clear();
        AddSortingColumn(SortingColumn.ColumnType.Draw_Calls, "Draw Calls", ColumnWidth);
        AddSortingColumn(SortingColumn.ColumnType.Verts, "Verts", ColumnWidth);
        AddSortingColumn(SortingColumn.ColumnType.Tris, "Tris", ColumnWidth);
        AddSortingColumn(SortingColumn.ColumnType.Bones, "Bones", ColumnWidth);
        AddSortingColumn(SortingColumn.ColumnType.Textures, "Textures", ColumnWidth);
        AddSortingColumn(SortingColumn.ColumnType.Vert_BB_Ratio, "Vert:BB", ColumnWidth);
        AddSortingColumn(SortingColumn.ColumnType.Recommendation, "Recommendation", RecommendationColumnWidth);
        AddSortingColumn(SortingColumn.ColumnType.GameObject_Hierarchy, "GameObject Hierarchy", RecommendationColumnWidth);
    }
    
    void AddSortingColumn(SortingColumn.ColumnType columnType, string columnText, float columnWidth)
    {
        m_Columns.Add(new SortingColumn(columnType, columnText, true, false, columnWidth));
    }
    
    public void HierarchyChanged()
    {
        Refresh();   
    }
    
    #endregion
}
