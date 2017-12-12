using UnityEngine;

// Extensions add functions to existing classes
// for example: SetColor(gameObject, Color.white);
// becomes: gameObject.SetColor(Color.white);

public static class Extensions
{
    // This adds a function called SetColor to the GameObject class
    // SetColor sets the colour of the material of the object
    public static void SetColor(this GameObject gameObj, Color color)
    {
        Renderer renderer = gameObj.GetComponent<Renderer>(); // Get the renderer component of the gameObject, if one exists
        if (renderer != null)
        {
            renderer.material.color = color; // Set the colour of the material attached to the renderer to whatever colour is passed in
        }
    }

    // The adds a function called GetColor to the GameObject class
    // GetColor returns the colour of the material of the object, if it has one (otherwise returns white)
    public static Color GetColor(this GameObject gameObj)
    {
        Renderer renderer = gameObj.GetComponent<Renderer>(); // Get the renderer component of the gameObject, if one exists
        if (renderer != null)
        {
            return renderer.material.color; // Return the colour of the material attached to the renderer
        }
        else return Color.white; // Otherwise return white
    }

    // The adds a function called MoveTo to the GameObject class
    // MoveTo moves the object to the location of another object
    public static void MoveTo(this GameObject gameObject, GameObject target)
    {
        // Set current position to target position
        gameObject.transform.position = target.transform.position;
    }
}