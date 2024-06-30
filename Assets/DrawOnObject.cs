using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrawOnObject : MonoBehaviour
{
    public Camera cam;
    
    public int textureWidth = 2048;
    public int textureHeight = 2048;

    private static Color drawColor = Color.white;
    
    private int brushSize = 10;
    
    private Vector2? lastInputPosition = null;
    
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
        }else
        {
            lastInputPosition = null; 
        }
    }

    private void Start()
    {
        LoadTextureFromFile();
        ClearTexture();
    }

    

    private void Draw(Vector2 touchPosition)
    {
        Ray ray = cam.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Renderer rend = hit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (rend == null || rend.material == null || rend.material.mainTexture == null || meshCollider == null)
            {
                lastInputPosition = null;
                return;
            }

            Texture2D tex = rend.material.mainTexture as Texture2D;
            if (tex != null)
            {
                Vector2 pixelUV = hit.textureCoord;
                pixelUV.x *= tex.width;
                pixelUV.y *= tex.height;

                if (lastInputPosition.HasValue)
                {
                    Vector2 lastUV = lastInputPosition.Value;
                    float distance = Vector2.Distance(lastUV, pixelUV);
                    for (float i = 0; i < distance; i += 1f)
                    {
                        Vector2 interpolatedPoint = Vector2.Lerp(lastUV, pixelUV, i / distance);
                        DrawCircle(tex, (int)interpolatedPoint.x, (int)interpolatedPoint.y, brushSize, drawColor);
                    }
                }

               
                DrawCircle(tex, (int)pixelUV.x, (int)pixelUV.y, brushSize, drawColor);
                tex.Apply();

                lastInputPosition = pixelUV; 
            }else
            {
                lastInputPosition = null;
            }
        }
        else
        {
            lastInputPosition = null;
        }
    }



    private void DrawCircle(Texture2D tex, int centerX, int centerY, int radius, Color col)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int px = Mathf.Clamp(centerX + x, 0, tex.width - 1);
                    int py = Mathf.Clamp(centerY + y, 0, tex.height - 1);
                    tex.SetPixel(px, py, col);
                }
            }
        }
    }

    public void SaveTextureToFile()
    {
        Renderer rend = GetComponent<Renderer>();
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
        Renderer rend = GetComponent<Renderer>();
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

