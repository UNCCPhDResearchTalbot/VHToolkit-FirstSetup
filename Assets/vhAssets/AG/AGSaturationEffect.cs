/// <summary>
/// AG saturation effect. This gives the flexibility to control the camera render texture saturation.
/// 
/// Requires the shader named "AG/Saturation Effect".
/// </summary>
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("AG/Image Effects/Saturation")]
public class AGSaturationEffect : MonoBehaviour {
    
    public Shader saturationShader;
    public float saturation = 0f;
    private const string AgSaturationShaderName = "AG/Effect/Saturation";
    private Material m_AgSaturationMaterial;
    
    void Start(){
        //Check for required shader
        if (saturationShader == null){
            //Warn and disable this script if the shader is not found to prevent camera from shutting off
            Debug.LogWarning("Required shader (" + AgSaturationShaderName + ") not assigned. Disabling this component.");
            this.gameObject.GetComponent<AGSaturationEffect>().enabled = false;
            return;
        }
        
        m_AgSaturationMaterial = new Material(saturationShader);
    }
    
    void OnRenderImage (RenderTexture source, RenderTexture destination) {
        //Pass saturation to the shader for saturation influence
        m_AgSaturationMaterial.SetFloat ("Saturation", saturation);
        Graphics.Blit (source, destination, m_AgSaturationMaterial);
    }
    
}