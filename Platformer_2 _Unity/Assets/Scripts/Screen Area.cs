using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScreenArea : MonoBehaviour
{
    public float value = 7.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        DrawBox(new Vector2(0, 0), new Vector2(2, 1));
    }

    void DrawBox(Vector2 cornerPosition, Vector2 size)
    {
        Gizmos.DrawRay(cornerPosition, size * Vector2.up);

        Gizmos.DrawRay(cornerPosition, size * Vector2.right);

        Gizmos.DrawRay(cornerPosition + size, size * Vector2.down);

        Gizmos.DrawRay(cornerPosition + size, size * Vector2.left);
    }
}

// A tiny custom editor for ExampleScript component
[CustomEditor(typeof(ScreenArea))]
public class ExampleEditor : Editor
{
    float size = 1f;

    protected virtual void OnSceneGUI()
    {
        if (Event.current.type == EventType.Repaint)
        {
            Transform transform = ((ScreenArea)target).transform;
            Handles.color = Handles.xAxisColor;
            Handles.DotHandleCap(
                0,
                transform.position + new Vector3(3f, 0f, 0f),
                transform.rotation * Quaternion.LookRotation(Vector3.right),
                size,
                EventType.Repaint
            );
            Handles.color = Handles.yAxisColor;
            Handles.DotHandleCap(
                0,
                transform.position + new Vector3(0f, 3f, 0f),
                transform.rotation * Quaternion.LookRotation(Vector3.up),
                size,
                EventType.Repaint
            );
            Handles.color = Handles.zAxisColor;
            Handles.DotHandleCap(
                0,
                transform.position + new Vector3(0f, 0f, 3f),
                transform.rotation * Quaternion.LookRotation(Vector3.forward),
                size,
                EventType.Repaint
            );
        }
    }
}
