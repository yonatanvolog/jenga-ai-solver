using System;
using UnityEngine;
using System.IO;

public class Screenshot : MonoBehaviour
{
    public Camera screenshotCamera; // Assign the camera from which you want to capture the screenshot in the Inspector
    public int resolutionWidth = 128;  // Set the initial width of the screenshot in the Inspector
    public int resolutionHeight = 72;  // Set the initial height of the screenshot in the Inspector
    private string folderPath = "./screenshots/"; // The path of your project folder

    private void Start()
    {
        TakeScreenshot();
    }

    public void SetFinalWidth(int width)
    {
        // Calculate resolutionWidth and resolutionHeight to maintain a 16:9 aspect ratio
        resolutionWidth = Mathf.RoundToInt(width * (1 / screenshotCamera.rect.width));
        resolutionHeight = Mathf.RoundToInt((resolutionWidth * 9) / 16);

        Debug.Log($"Screenshot resolution set to {resolutionWidth}x{resolutionHeight}");
    }

    public void TakeScreenshot()
    {
        if (screenshotCamera == null)
        {
            Debug.LogError("Screenshot Camera is not assigned!");
            return;
        }

        // Clear the screenshots folder before taking a new screenshot
        ClearScreenshotsFolder();
        
        if (!Directory.Exists(folderPath)) // If this path does not exist yet
        {
            Directory.CreateDirectory(folderPath); // It will get created
        }

        var screenshotName = 
            "Screenshot_" + 
            System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + // Puts the current time right into the screenshot name
            ".png"; // Put your favorite data format here

        // Render the camera's view to a RenderTexture
        RenderTexture renderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        screenshotCamera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        screenshotCamera.Render();

        // Create a Texture2D with the size of the RenderTexture
        Texture2D texture = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        texture.Apply();

        // Crop the image based on the camera's viewport rect 'w' value
        int croppedWidth = Mathf.RoundToInt(texture.width * screenshotCamera.rect.width);
        Texture2D croppedTexture = new Texture2D(croppedWidth, texture.height);
        Color[] pixels = texture.GetPixels(0, 0, croppedWidth, texture.height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        // Reset the camera's target texture
        screenshotCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Save the cropped Texture2D as a PNG
        string screenshotPath = Path.Combine(folderPath, screenshotName);
        File.WriteAllBytes(screenshotPath, croppedTexture.EncodeToPNG());

        Debug.Log("Screenshot saved: " + screenshotPath); // You get instant feedback in the console

        // Optionally, destroy the textures to free memory
        Destroy(texture);
        Destroy(croppedTexture);
    }

    public void ClearScreenshotsFolder()
    {
        if (Directory.Exists(folderPath))
        {
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            Debug.Log("All previous screenshots deleted.");
        }
    }
}
