/// <summary>
/// AG brightness effect. This gives the flexibility to control the camera render texture brightness.
/// 
/// Requires the shader named "AG/Brightness Effect".
/// </summary>
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("AG/Image Effects/Brightness")]
public class AGBrightnessEffect : MonoBehaviour {
    
    public Shader brightnessShader;
    public float brightness = 0f;
    private const string AgBrightnessShaderName = "AG/Effect/Brightness";
    private Material m_AgBrightnessMaterial;
    
    void Start(){
        //Check for required shader
        if (brightnessShader == null){
            //Warn and disable this script if the shader is not found to prevent camera from shutting off
            Debug.LogWarning("Required shader (" + AgBrightnessShaderName + ") not assigned. Disabling this component.");
            this.gameObject.GetComponent<AGBrightnessEffect>().enabled = false;
            return;
        }
        
        m_AgBrightnessMaterial = new Material(brightnessShader);
    }
    
    void OnRenderImage (RenderTexture source, RenderTexture destination) {
        //Pass brightness to the shader for brightness influence
        m_AgBrightnessMaterial.SetFloat ("Brightness", brightness);
        Graphics.Blit (source, destination, m_AgBrightnessMaterial);
    }
    
}