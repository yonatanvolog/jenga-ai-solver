using UnityEngine;
using System.IO;

public class Screenshot : MonoBehaviour
{
    [SerializeField] private Camera screenshotCamera;
    [SerializeField] private int resolutionWidth = 128;
    [SerializeField] private int resolutionHeight = 72;
    private string folderPath = "./screenshots/";

    private void Start()
    {
        TakeScreenshot();
    }

    public void SetFinalWidth(int width)
    {
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

        ClearScreenshotsFolder();

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string screenshotName = "Screenshot_" + System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".png";

        RenderTexture renderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        screenshotCamera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        screenshotCamera.Render();

        Texture2D texture = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        texture.Apply();

        int croppedWidth = Mathf.RoundToInt(texture.width * screenshotCamera.rect.width);
        Texture2D croppedTexture = new Texture2D(croppedWidth, texture.height);
        Color[] pixels = texture.GetPixels(0, 0, croppedWidth, texture.height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        screenshotCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        string screenshotPath = Path.Combine(folderPath, screenshotName);
        File.WriteAllBytes(screenshotPath, croppedTexture.EncodeToPNG());

        Debug.Log("Screenshot saved: " + screenshotPath);

        Destroy(texture);
        Destroy(croppedTexture);
    }

    private void ClearScreenshotsFolder()
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
