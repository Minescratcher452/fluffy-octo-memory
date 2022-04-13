using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    public LineRenderer lr;
    public PlayerController pc;
    public Camera mainCamera;
    private Transform gunBarrel;
    // TODO: find a good value
    private int segments = 26; // how many segments on the circle

    // Start is called before the first frame update
    void Start()
    {
        // Get the transform of the gun barrel
        gunBarrel = pc.CurrentWeapon.GunBarrel.transform;

        // Set the line renderer's number of vertices
        lr.positionCount = segments + 1;
    }

    // Update is called once per frame
    void Update()
    {
        // Draw the "crosshair" circle
        // Please replace me with real UI code! I exist solely because *someone* couldn't be bothered to implement this properly while he did math.
        //
        // Get the mouse position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0); // strip out the Z coord, since it doesn't matter for rendering but it will affect Euclidean distance if present

        // Get the straight-line distance between the gun barrel and the mouse cursor
        Vector3 gunPos = gunBarrel.position;
        float dist = Mathf.Abs(Vector3.Distance(mousePos, gunPos));

        // Geometry is hell
        //
        // Since we define the x-axis parallel to the line which passes through the gun barrel and the cursor position, we don't need to bother to account for differences in world y-value,
        // and since we define the gun barrel to be at the origin, the mouse cursor is at (dist, 0) in the weird reference frame.
        // This makes the math *significantly* simpler to implement.
        // 
        //

        // Determine the angle of the tangent line from the horizontal
        float theta = (pc.CurrentWeapon.FireConeAngle() + pc.MovementAccuracyFactor) * Mathf.Deg2Rad; // weapon inaccuracy angle in degrees 
        float tanTheta = Mathf.Tan(theta);

        // Standard-form coefficients of the tangent line
        float A = 1;
        float B = -1 / tanTheta;
        float C = 0;

        // Calculate the radius.
        float radius = Mathf.Abs((A * dist) + C) / Mathf.Sqrt((A * A) + (B * B));

        // Draw the circle with the appropriate radius about the mouse cursor
        DrawCircle(mousePos, radius, segments, 0);
    }

    // Draw a circle using a LineRenderer.
    // Derived from code by MarkPixel and Quatum1000 here: https://forum.unity.com/threads/linerenderer-to-create-an-ellipse.74028/
    void DrawCircle(Vector3 center, float radius, int segments, float angle)
    {
        float x = 0;
        float y = 0;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = center.x + Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            y = center.y + Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            lr.SetPosition(i, new Vector3(x, y, 0));

            angle += (360f / segments);
        }
    }
}
