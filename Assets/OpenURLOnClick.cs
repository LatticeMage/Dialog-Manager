using UnityEngine;

public class OpenURLOnClick : MonoBehaviour
{
    public string url;

    private void Update()
    {
        // Check for a left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object is this ball
                if (hit.transform == transform)
                {
                    OpenURL();
                }
            }
        }
    }

    void OpenURL()
    {
        Application.OpenURL(url);
    }
}
