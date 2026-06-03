using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class DebrisPlacementTool : EditorWindow
{
    private List<DebrisClass> debrisList = new();
    private bool isPrefabListOpened = true;
    
    private GUIStyle titleStyle;
    private EditorListDrawerStyle listStyle;

    private Vector3 _maxRotation = Vector3.up * 180;
    private float _minScale = 1f;
    private float _maxScale = 1.5f;
    private float _placementRadius = 5;
    private int _minAmount = 1,  _maxAmount = 3;
    
    [MenuItem("Tools/Debris Placement")]
    public static void ShowWindow()
    {
        DebrisPlacementTool window = GetWindow<DebrisPlacementTool>();
        
        window.titleContent = new GUIContent("Debris Placement");
    }

    private void CreateGUI()
    {
        LoadPreferences();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += DuringSceneGUI;
        
        titleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 18
        };
        titleStyle.normal.textColor = Color.white;

        listStyle = new EditorListDrawerStyle
        {
            p_titleStyle = titleStyle,
            p_label = (i) => i.ToString()
        };
    }


    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }
    private void DuringSceneGUI(SceneView obj)
    {
        Event e = Event.current;
        
        //if 'ctrl', 'shift', 'alt' or 'cmd' is pressed, the action is ignored
        bool actionBlocking = e.control || e.shift || e.alt || e.command;
        if(actionBlocking) return;

        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        switch (e.type)
        {
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlId);
                break;
            
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    TryPlaceDebris(e);
                    //used to keep control until freed
                    //GUIUtility.hotControl = controlId;
                    e.Use();
                }
                break;
            case EventType.MouseUp:
                //freeing control
                if (GUIUtility.hotControl == controlId)
                {
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
        }
    }

    private void OnGUI()
    {
        EditorUtilities.DrawList(
            debrisList,
            "Prefabs",
            listStyle,
            ref isPrefabListOpened, 
            DB =>
            {
                if (DB == null) DB = new();
                
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("Prefab");
                DB.p_debrisPrefab = EditorGUILayout.ObjectField(DB.p_debrisPrefab, typeof(GameObject), false) as GameObject;
                GUILayout.EndVertical();
                
                GUILayout.BeginVertical(GUILayout.Width(100));
                GUILayout.Label("Weight");
                DB.p_ObjectWeight = EditorGUILayout.FloatField(DB.p_ObjectWeight);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                return DB;
            });
        
        _maxRotation = EditorGUILayout.Vector3Field("Max Rotation", _maxRotation);
        _maxRotation.x = _maxRotation.x > 180 ? 180 :  _maxRotation.x < 0 ? 0 : _maxRotation.x;
        _maxRotation.y = _maxRotation.y > 180 ? 180 :  _maxRotation.y < 0 ? 0 : _maxRotation.y;
        _maxRotation.z = _maxRotation.z > 180 ? 180 :  _maxRotation.z < 0 ? 0 : _maxRotation.z;
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale Range");
        _minScale = EditorGUILayout.FloatField("Min", _minScale);
        _maxScale = EditorGUILayout.FloatField("Max", _maxScale);
        GUILayout.EndHorizontal();
        
        GUILayout.Space(15);
        _placementRadius = EditorGUILayout.FloatField("Placement Radius", _placementRadius);
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Amount Range");
        _minAmount = EditorGUILayout.IntField("Min", _minAmount);
        _maxAmount = EditorGUILayout.IntField("Max", _maxAmount);
        GUILayout.EndHorizontal();
        
        GUILayout.Space(30);
        GUILayout.BeginHorizontal();
        Color bgColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.3f, 0.3f, 0.8f);
        if (GUILayout.Button("Save Preferences"))
        {
            SavePreferences();
        }
        GUI.backgroundColor = new Color(0.5f, 0.5f, 0.8f);
        if (GUILayout.Button("Load Preferences"))
        {
            LoadPreferences();
        }
        GUI.backgroundColor = bgColor;
        GUILayout.EndHorizontal();
        
    }

    void TryPlaceDebris(Event e)
    {
        if(debrisList.Count == 0) return;
        
        int undoGroup = Undo.GetCurrentGroup();
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Debris Placement");
        
        
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            int amountToPlace = Random.Range(_minAmount, _maxAmount);
            for (int i = 0; i < amountToPlace; i++)
            {
                float scale = Random.Range(_minScale, _maxScale);

                GameObject randomPrefab = GetRandomPrefab();
                Debug.Log(randomPrefab.name);
                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(randomPrefab);
                Undo.RegisterCreatedObjectUndo(obj, $"Debris {i}");
                
                GetRandomPositionRotation(hit, out Vector3 pos, out Quaternion rot);
                
                obj.transform.position = pos;
                obj.transform.rotation = rot;
                obj.transform.localScale = Vector3.one * scale;
            }
            Undo.CollapseUndoOperations(undoGroup);
        }
    }

    void GetRandomPositionRotation(RaycastHit hit, out Vector3 pos, out Quaternion rot)
    {
        Vector3 normal = hit.normal;
        
        Vector3 forward = Vector3.Cross(normal, Vector3.up);
        if(forward.sqrMagnitude < 0.001f)
            forward = Vector3.Cross(normal, Vector3.forward);
        
        forward.Normalize();
        Vector3 biTangent = Vector3.Cross(normal, forward);

        Vector2 randomInCircle = Random.insideUnitCircle * _placementRadius;
        
        pos = hit.point +
               forward * randomInCircle.x +
               biTangent * randomInCircle.y;
        
        rot = Quaternion.LookRotation(forward, normal) *
                   Quaternion.Euler(
                       Random.Range(-_maxRotation.x, _maxRotation.x),
                       Random.Range(-_maxRotation.y, _maxRotation.y),
                       Random.Range(-_maxRotation.z, _maxRotation.z)
                       );
    }

    GameObject GetRandomPrefab()
    {
        float total = 0f;

        foreach (DebrisClass d in debrisList)
            total += d.p_ObjectWeight;
        
        float random = Random.Range(0f, total);
        float sum = 0f;

        foreach (DebrisClass debris in debrisList)
        {
            sum += debris.p_ObjectWeight;
            
            if (random <= sum) return debris.p_debrisPrefab;
        }
        
        
        return debrisList[^1].p_debrisPrefab;
    }

    void LoadPreferences()
    {
        DebrisPlacementPreferences DPP = new DebrisPlacementPreferences().GetFromJSon();
        
        isPrefabListOpened = DPP.p_isPrefabListOpened;
        _maxRotation = DPP.p_maxRotation;
        _minScale = DPP.p_minScale;
        _maxScale = DPP.p_maxScale;
        _placementRadius = DPP.p_placementRadius;
        _minAmount = DPP.p_minAmount;
        _maxAmount = DPP.p_maxAmount;
        
        debrisList.Clear();
        foreach (SavableDebrisClass debris in DPP.p_debrisList)
        {
            debrisList.Add(new DebrisClass
            {
                p_debrisPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(debris.p_prefabPathGuid)),
                p_ObjectWeight = debris.p_objectWeight
            });
        }
    }
    void SavePreferences()
    {
        new DebrisPlacementPreferences(
            debrisList,
            isPrefabListOpened, 
            _maxRotation, 
            _minScale, 
            _maxScale, 
            _placementRadius, 
            _minAmount, 
            _maxAmount).SaveToJson();
    }
}

public class DebrisClass
{
    public GameObject p_debrisPrefab;
    public float p_ObjectWeight = 1;
}
