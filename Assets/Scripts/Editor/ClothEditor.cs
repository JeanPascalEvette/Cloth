using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Cloth))]
public class ClothEditor : Editor {

	// Use this for initialization
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Cloth myTarget = (Cloth)target;
        if (GUILayout.Button("Tear cloth in 2"))
        {
            myTarget.ripCloth();
        }
        if (GUILayout.Button("Tear cloth slightly"))
        {
            myTarget.ripCloth(10);
        }

    }
}
