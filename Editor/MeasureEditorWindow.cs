using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class MeasureEditorWindow : EditorWindow {

	List<MeasureEntry> measuresList = new List<MeasureEntry>();
    List<GameObject> dummiesList = new List<GameObject>();

	Rect rulerButtonRect = new Rect(10, 10, 25, 25);

    float dotsSpace = 3.0f;
    float maxDistance = 20f;
    bool snapToColliders = true;
    bool removeDummies = false;

    Vector2 scrollPos;
    Vector2 cursorScreenPos;
    Vector3 cursorWorldPos;

    GameObject dummy=null;

    // Workaround for selection timing
    EditorWindowTimer selectEventTimer; 
    float clickTimerDuration = .25f;
    bool startingSelectionMode = false;

    bool selectObjectsMode = false;
	MeasureEntry currentMeasure = null;
    
	GUIStyle separatorStyle = new GUIStyle();

	public bool SelectObjectsMode {
		get {
			return selectObjectsMode;
		}
		set {
			if (value) {

				currentMeasure = null;
				Selection.activeObject = null;
				Selection.selectionChanged += OnSelectionChanged;

                startingSelectionMode = true;

			} else {
				
				Selection.selectionChanged -= OnSelectionChanged;
				currentMeasure = null;
                if (dummy != null)
                {
                    dummiesList.Remove(dummy);
                    DestroyImmediate(dummy);
                    measuresList.RemoveAt(measuresList.Count-1);
                }
			}

			selectObjectsMode = value;
		}
	}

    [MenuItem ("Window/Measures")]
	public static void Init(){
		MeasureEditorWindow window = (MeasureEditorWindow)EditorWindow.GetWindow(typeof (MeasureEditorWindow));
		window.titleContent = new GUIContent("Measures");

		window.Show ();
	}

	void Awake(){
		separatorStyle.fixedHeight = 1;

        selectEventTimer = new EditorWindowTimer(clickTimerDuration);
	}

	void Update(){
		Repaint ();
		SceneView.RepaintAll ();

        if (selectEventTimer.Update() == EditorWindowTimer.TimerStatus.Over) // If no selection is detected
        {
            if (startingSelectionMode)
            {
                startingSelectionMode = false;
            }
            else
            {
                if (selectObjectsMode)
                {
                    if (currentMeasure == null)
                    {
                        StartMeasure(CreateDummy(cursorWorldPos));
                    }
                    else
                    {
                        EndMeasure(dummy);
                    }

                }

            }

            selectEventTimer.Reset();
        }

        if (dummy != null)
        {
            dummy.transform.position = cursorWorldPos;
        }
    }

	void OnDestroy(){
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
	}

	void OnFocus(){
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
		SceneView.onSceneGUIDelegate += OnSceneGUI;
	}

	void OnGUI(){
        cursorScreenPos = Event.current.mousePosition;

        GUILayout.BeginHorizontal ();

		GUILayout.BeginVertical ();
		GUILayout.Label ("Measures", EditorStyles.boldLabel);
        GUILayout.Label(new GUIContent("Count: " + measuresList.Count, "Actual count of measures"));
        GUILayout.Label(new GUIContent("Dummies: " + dummiesList.Count, "Count of dummies remaining"));
        maxDistance = EditorGUILayout.FloatField(new GUIContent("Max. measure length", "Measure length if not snapping"), maxDistance);
        snapToColliders = EditorGUILayout.Toggle(new GUIContent("Snap", "Toggle measure snapping to objects with a collider"), snapToColliders);
        removeDummies = EditorGUILayout.Toggle(new GUIContent("Clean dummies", "Remove unused measure dummies when a measure is erased"), removeDummies);
        GUILayout.EndVertical ();

		GUILayout.BeginVertical ();
		if (GUILayout.Button ("Clear all")) {
                RemoveAllMeasure();
		}

		GUILayout.EndVertical ();

		GUILayout.EndHorizontal ();
		EditorGUILayout.LabelField("", separatorStyle);

		scrollPos = EditorGUILayout.BeginScrollView (scrollPos, false, false);

		for (int i=0; i<measuresList.Count; i++) {
            DrawMeasureEntryInspector(i);
		}

		GUILayout.EndScrollView ();
	}

    private void RemoveAllMeasure()
    {
        for (int i = 0; i < measuresList.Count; i++)
        {
            if (removeDummies)
            {
                CleanMeasureDummies(measuresList[i].A, i);
                measuresList[i].A = null;

                CleanMeasureDummies(measuresList[i].B, i);
                measuresList[i].B = null;

            }
        }

        measuresList.Clear();

        if (removeDummies)
            dummiesList.Clear();

    }

    void DrawMeasureEntryInspector(int i)
    {
        GUILayout.BeginHorizontal();
        measuresList[i].Shown = EditorGUILayout.Foldout(measuresList[i].Shown, "Measure " + (i + 1));
        if (GUILayout.Button("X", GUILayout.Height(14), GUILayout.Width(20)))
        {
            RemoveMeasure(i);
        }
        GUILayout.EndHorizontal();

        if (measuresList[i].Shown)
        {
            EditorGUI.indentLevel++;
            GUILayout.BeginVertical();
            measuresList[i].UpdateDistance();

            GUILayout.BeginHorizontal();
            measuresList[i].A = (GameObject)EditorGUILayout.ObjectField(measuresList[i].A, typeof(GameObject), true);
            measuresList[i].B = (GameObject)EditorGUILayout.ObjectField(measuresList[i].B, typeof(GameObject), true);
            GUILayout.EndHorizontal();

            measuresList[i].Color = EditorGUILayout.ColorField(measuresList[i].Color);
            EditorGUILayout.Space();

            if (measuresList[i].A != null && measuresList[i].B != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Distance:");
                EditorGUILayout.SelectableLabel(measuresList[i].FDistance.ToString(), EditorStyles.numberField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("X:");
                EditorGUILayout.SelectableLabel(measuresList[i].Vec3Distance.x.ToString(), EditorStyles.numberField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                GUILayout.Label("Y:");
                EditorGUILayout.SelectableLabel(measuresList[i].Vec3Distance.y.ToString(), EditorStyles.numberField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                GUILayout.Label("Z:");
                EditorGUILayout.SelectableLabel(measuresList[i].Vec3Distance.z.ToString(), EditorStyles.numberField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            EditorGUI.indentLevel--;
            EditorGUILayout.Separator();
        }
        EditorGUILayout.LabelField("", separatorStyle);
        EditorGUILayout.Space();
    }

    void RemoveMeasure(int i)
    {
        if (removeDummies)
        {
            CleanMeasureDummies(measuresList[i].A, i);
            CleanMeasureDummies(measuresList[i].B, i);
        }

        measuresList.RemoveAt(i);
    }

    void CleanMeasureDummies(GameObject dummy, int index)
    {
        if (dummiesList.Contains(dummy))
        {
            if (!IsDummyUsedElsewhere(dummy, index))
            {
                dummiesList.Remove(dummy);
                DestroyImmediate(dummy);
            }
        }
    }

    bool IsDummyUsedElsewhere(GameObject dummy, int excludedIndex)
    {
        for(int i=0; i<measuresList.Count; i++)
        {
            if(i != excludedIndex)
            {
                if (measuresList[i].A == dummy || measuresList[i].B == dummy)
                    return true;
            }
        }

        return false;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        UpdateCursorPosition();

        for (int i = 0; i < measuresList.Count; i++) {
			if (measuresList [i].A != null && measuresList [i].B != null) {
				
				GUIStyle style = new GUIStyle ();
				style.normal.textColor = measuresList [i].Color;
				// TODO : Add label texture
				//style.normal.background = Texture2D.blackTexture;
				Handles.color = measuresList [i].Color;

				measuresList [i].UpdateDistance ();
				Vector3 center = Vector3.Lerp (measuresList [i].A.transform.position, measuresList [i].B.transform.position, .5f);

				Handles.DrawDottedLine (measuresList [i].A.transform.position, measuresList [i].B.transform.position, dotsSpace);
				Handles.Label (center,measuresList [i].FDistance.ToString(), style);
			}
		}

		Handles.BeginGUI ();
		if (GUI.Button (rulerButtonRect, Resources.Load<Texture>(SelectObjectsMode ? "ruler_white":"ruler_black"), EditorStyles.miniButton)) {
			SelectObjectsMode = !SelectObjectsMode;
		}
		Handles.EndGUI ();
	}

    void UpdateCursorPosition()
    {
        cursorScreenPos = Event.current.mousePosition;

        RaycastHit hit;
        Ray worldRay = HandleUtility.GUIPointToWorldRay(cursorScreenPos);

        if (Physics.Raycast(worldRay, out hit))
        {
            cursorWorldPos = snapToColliders ? hit.collider.gameObject.transform.position : hit.point;
        }
        else
        {
            cursorWorldPos = worldRay.origin + worldRay.direction * maxDistance;
        }

        // Click handling
        if (Event.current.type == EventType.MouseDown)
        {
            if (selectEventTimer.CurrentStatus == EditorWindowTimer.TimerStatus.NotStarted)
            {
                selectEventTimer.Start();
            }
        }
    }

    void OnSelectionChanged()
    {
        selectEventTimer.Reset();

        if (Selection.activeTransform != null && selectObjectsMode)
        {
            if (currentMeasure == null)
            {
                StartMeasure(Selection.activeTransform.gameObject);
            }
            else
            {
                EndMeasure(Selection.activeTransform.gameObject, true);
            }

            Selection.activeTransform = null;
        }
    }

    void StartMeasure(GameObject origin)
    {
        dummy = CreateDummy(cursorWorldPos);

        currentMeasure = new MeasureEntry()
        {
            A = origin,
            B = dummy
        };
        measuresList.Add(currentMeasure);
    }

    void EndMeasure(GameObject target, bool destroyDummy=false)
    {
        currentMeasure.B = target;
        currentMeasure = null;

        if (destroyDummy)
        {
            dummiesList.Remove(dummy);
            DestroyImmediate(dummy);
        } else
            dummy = null;
    }

    GameObject CreateDummy(Vector3 position)
    {
        GameObject newDummy = new GameObject("Measure Dummy");
        newDummy.transform.position = position;
        dummiesList.Add(newDummy);

        return newDummy;
    }
}
