using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// A tiny custom editor for ExampleScript component
[CustomEditor(typeof(ScreenArea))]
public class ExampleEditor : Editor
{
    float size = 1f;
    Vector2 bounds = new Vector2(3, 3);

    protected virtual void OnSceneGUI()
    {
        if (Event.current.type == EventType.Layout)
        {
            Transform transform = ((ScreenArea)target).transform;
            Handles.color = Handles.xAxisColor;
            Handles.DotHandleCap(
                0,
                transform.position + new Vector3(bounds.x, 0, 0),
                transform.rotation * Quaternion.LookRotation(Vector3.right),
                size,
                EventType.Layout
            );
        }


        if (Event.current.type == EventType.Repaint)
        {
            Transform transform = ((ScreenArea)target).transform;
            Handles.color = Handles.xAxisColor;
            Handles.DotHandleCap(
                0,
                transform.position + new Vector3(bounds.x, 0, 0),
                transform.rotation * Quaternion.LookRotation(Vector3.right),
                size,
                EventType.Repaint
            );
            Handles.color = Handles.yAxisColor;
            Handles.DotHandleCap(
                0,
                transform.position + new Vector3(0, bounds.y, 0),
                transform.rotation * Quaternion.LookRotation(Vector3.up),
                size,
                EventType.Repaint
            );
        }
    }
}
