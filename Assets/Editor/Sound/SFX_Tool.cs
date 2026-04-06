using System;
using System.Collections.Generic;
using MyPrint;
using PlasticGui.WorkspaceWindow.Items;
using ScriptableObjectsDefinitions;
using TMPro;
using UnityEditor;
using UnityEngine;

public class SFX_Tool : EditorWindow
{
    private SoundsDataSO _data;
    private Vector2 _scrollPos;
    private GUIStyle _itemStyle;
    
    public Texture2D _sliderThumbTex;
    
    [MenuItem("Tools/SFX Manager")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SFX_Tool));
    }

    private void OnEnable()
    {
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/Editor/Sound/Textures/icon.png"
        );

        titleContent = new GUIContent(" SFX Tool", icon);
        
        _sliderThumbTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Sound/Textures/ScrollVolume.png");
    }

    void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), GetColor(PaletteColor.Black));
        
        Title();
        Variable();
        NewData();
        Data();
    }


    private void Title()
    {
        GUIStyle _centeredStyle = new GUIStyle(GUI.skin.label);
        _centeredStyle.alignment = TextAnchor.MiddleCenter;
        _centeredStyle.fontStyle = FontStyle.Bold;
        _centeredStyle.fontSize = 48;
        
        GUILayout.Label("SFX Manager", _centeredStyle);
        GUILayout.Space(16);
    }

    private void Variable()
    {
        GUILayout.BeginHorizontal();
        
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.normal.background = MakeTex(2, 2, GetColor(PaletteColor.Green));
        
        if (GUILayout.Button("Select Sound Data", style, GUILayout.Height(30)))
        {
            EditorGUIUtility.ShowObjectPicker<SoundsDataSO>(_data, false, "", 0);
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.alignment = TextAnchor.MiddleCenter;
        boxStyle.fontSize = 16;
        boxStyle.normal.textColor = Color.black;
        boxStyle.normal.background = MakeTex(2, 2, GetColor(PaletteColor.Yellow));

        GUILayout.Box(
            _data != null ? _data.name : "No Data Selected",
            boxStyle,
            GUILayout.Height(40),
            GUILayout.ExpandWidth(true)
        );

        GUILayout.Space(16);

        if (Event.current.commandName == "ObjectSelectorUpdated")
        {
            _data = EditorGUIUtility.GetObjectPickerObject() as SoundsDataSO;
            Repaint();
        }
    }

    private void Data()
    {
        if (_data == null) return;

        _itemStyle = new GUIStyle(GUI.skin.box);
        _itemStyle.alignment = TextAnchor.MiddleCenter;
        _itemStyle.fontStyle = FontStyle.Bold;
        _itemStyle.fontSize = 14;
        _itemStyle.normal.background = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.1f));

        _scrollPos = EditorGUILayout.BeginScrollView(
            _scrollPos,
            GUILayout.Height(700),
            GUILayout.ExpandWidth(true)
        );

        GUILayout.BeginHorizontal();

        int i = 0;
        foreach (SoundData s in _data.sounds)
        {
            GUILayout.BeginVertical(_itemStyle, GUILayout.Width(180), GUILayout.Height(600));
            

            GUIStyle style=new GUIStyle(GUI.skin.box){alignment=TextAnchor.MiddleCenter};
            
            //Id
            GUILayout.Label(i.ToString(), style, GUILayout.ExpandWidth(true), GUILayout.Height(20), GUILayout.ExpandWidth(true));
            
            GUILayout.Space(8);

            EditorGUI.BeginChangeCheck();

            style.fontSize = 32;
            
            //Nom
            s.soundName = EditorGUILayout.TextField(s.soundName, style, GUILayout.ExpandWidth(true), GUILayout.Height(64), GUILayout.ExpandWidth(true));

            GUILayout.Space(16);
            
            //Clip
            s.audioClip = (AudioClip)EditorGUILayout.ObjectField(s.audioClip, typeof(AudioClip), false);
            
            GUILayout.Space(16);
            
            GUILayout.Label($"Volume : {s.volume:F2}",new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter});
            
            GUILayout.Space(8);

            Rect sliderRect = GUILayoutUtility.GetRect(60, 400);

            s.volume = CustomVerticalSlider.Draw(
                sliderRect,
                s.volume,
                0f,
                1f,
                _sliderThumbTex
            );
            
            GUILayout.Space(16);
            
            if (GUILayout.Button("Preview"))
            {
                if (s.audioClip == null)
                {
                    Debug.LogWarning("No AudioClip assigned.");
                    return;
                }

                GameObject tempGO = new GameObject("EditorAudioPreview");
                AudioSource source = tempGO.AddComponent<AudioSource>();

                source.clip = s.audioClip;
                source.volume = s.volume;
                source.playOnAwake = false;

                source.Play();

                Destroy(tempGO, 2f);
            }
            
            GUIStyle deleteStyle = new GUIStyle(GUI.skin.button);
            deleteStyle.normal.textColor = Color.white;
            deleteStyle.normal.background = MakeTex(2, 2, Color.darkRed);

            if (GUILayout.Button("Delete", deleteStyle))
            {
                Undo.RecordObject(_data, "Delete Sound");

                _data.sounds.RemoveAt(i);

                EditorUtility.SetDirty(_data);
                Repaint();

                GUILayout.EndVertical();
                break;
            }
            
            i++;

            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }
    
    private void NewData()
    {
        GUILayout.Space(10);

        if (_data == null)
        {
            GUILayout.Label("No data selected");
            return;
        }

        if (_data.sounds == null)
            _data.sounds = new List<SoundData>();

        GUIStyle button = new GUIStyle(GUI.skin.button);
        button.normal.textColor = Color.white;
        button.normal.background = MakeTex(2, 2, GetColor(PaletteColor.DarkGreen));
        
        if (GUILayout.Button("Add Sound", button, GUILayout.Height(30)))
        {
            Undo.RecordObject(_data, "Add Sound");

            _data.sounds.Add(new SoundData()
            {
                soundName = "New Sound",
                volume = 0.5f
            });

            EditorUtility.SetDirty(_data);
            Repaint();
        }
        
        GUILayout.Space(10);
    }
    
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    public Color GetColor(PaletteColor p)
    {
        Color color;

        switch (p)
        {
            case PaletteColor.Yellow:        ColorUtility.TryParseHtmlString("#FFDE42", out color); return color;
            case PaletteColor.Green:        ColorUtility.TryParseHtmlString("#4C5C2D", out color); return color;
            case PaletteColor.Black:        ColorUtility.TryParseHtmlString("#1B0C0C", out color); return color;
            case PaletteColor.DarkGreen:    ColorUtility.TryParseHtmlString("#313E17", out color); return color;
            default: return Color.white;
        }
    }

    public enum PaletteColor
    {
        Yellow, Green, DarkGreen, Black
    }
    
    private void DrawOutline(Rect rect, float thickness, Color color)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }
}
