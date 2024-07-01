using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Renderer))]
public class DrawOnObject : MonoBehaviour
{
    public Camera cam;

    public int textureWidth = 2048;
    public int textureHeight = 2048;

    private static Color drawColor = Color.white;

    private int brushSize = 10;

    private Vector2? lastInputPosition = null;
    
    private Renderer rend;
    private void Start()
    {
        rend = GetComponent<Renderer>();

        LoadTextureFromFile();
        ClearTexture();
    }

    private void Update()
    {
        Vector2? inputPosition = null;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            inputPosition = Mouse.current.position.ReadValue();
        }

        if (inputPosition.HasValue)
        {
            Draw(inputPosition.Value);
        }
        else
        {
            lastInputPosition = null;
        }
    }




    private void Draw(Vector2 touchPosition)
    {
        Ray ray = cam.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (rend == null || meshCollider == null)
            {
                lastInputPosition = null;
                return;
            }

            Texture2D tex = rend.material.mainTexture as Texture2D;
            if (tex == null)
            {
                lastInputPosition = null;
                return;
            }

            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= tex.width;
            pixelUV.y *= tex.height;

            if (lastInputPosition.HasValue)
            {
                Vector2 lastUV = lastInputPosition.Value;
                Vector2 currentUV = pixelUV;
                BresenhamLine(tex, (int)lastUV.x, (int)lastUV.y, (int)currentUV.x, (int)currentUV.y, brushSize, drawColor);
            }

            



            DrawCircle(tex, (int)pixelUV.x, (int)pixelUV.y, brushSize, drawColor);
            tex.Apply();

            lastInputPosition = pixelUV;
        }
        else
        {
            lastInputPosition = null;
        }
    }

    private void BresenhamLine(Texture2D tex, int x0, int y0, int x1, int y1, int radius, Color col)
    {
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        for (; ; )
        {
            DrawCircle(tex, x0, y0, radius, col);
            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }


    private void DrawCircle(Texture2D tex, int centerX, int centerY, int radius, Color col)
    {
        int startX = Mathf.Max(centerX - radius, 0);
        int startY = Mathf.Max(centerY - radius, 0);
        int endX = Mathf.Min(centerX + radius, tex.width);
        int endY = Mathf.Min(centerY + radius, tex.height);

        int width = endX - startX;
        int height = endY - startY;

        Color[] colors = tex.GetPixels(startX, startY, width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if ((x + startX - centerX) * (x + startX - centerX) + (y + startY - centerY) * (y + startY - centerY) <= radius * radius)
                {
                    colors[y * width + x] = col;
                }
            }
        }

        tex.SetPixels(startX, startY, width, height, colors);
    }



    public void SaveTextureToFile()
    {
        if (rend != null && rend.material != null && rend.material.mainTexture != null)
        {
            Texture2D tex = rend.material.mainTexture as Texture2D;
            if (tex != null)
            {
                byte[] bytes = tex.EncodeToPNG();
                if (bytes != null && bytes.Length > 0)
                {
                    string path = Path.Combine(Application.persistentDataPath, "SavedTexture1.png");
                    File.WriteAllBytes(path, bytes);
                }
            }
        }
    }

    public void LoadTextureFromFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "SavedTexture1.png");
        if (File.Exists(path))
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(fileData))
            {
                Renderer rend = GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.mainTexture = tex;
                }

            }

        }

    }

    public void ClearTexture()
    {
        if (rend != null && rend.material != null && rend.material.mainTexture != null)
        {
            Texture2D tex = rend.material.mainTexture as Texture2D;
            if (tex != null)
            {
                Color[] colors = new Color[tex.width * tex.height];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.white;
                }
                tex.SetPixels(colors);
                tex.Apply();
            }
        }
    }

    public static void SetDrawColor(Color color)
    {
        drawColor = color;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }
}