using UnityEngine;
using System.Collections;

public class RendererEvents : GenericEvents
{
    #region Functions
    public override string GetEventType() { return GenericEventNames.Renderer; }
    #endregion

    #region Events
    public class RendererEvent_Base : ICutsceneEventInterface { }

    public class RendererEvent_SetMaterial : RendererEvent_Base
    {
        #region Functions
        public void SetMaterial(Renderer renderer, Material mat)
        {
            renderer.material = mat;
        }

        public void SetMaterial(Renderer renderer, Material mat, int matIndex)
        {
            Material[] materials = renderer.materials; // returns a clone
            materials[matIndex] = mat;
            renderer.materials = materials; // have to deep copy
        }

        public override object SaveRewindData(CutsceneEvent ce)
        { 
            object retVal = null;
            Renderer renderer = Cast<Renderer>(ce, 0);
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    retVal = renderer.material;
                    break;

                case 1:
                    retVal = renderer.materials[Param(ce, 1).intData];
                    break;
            }
            return retVal;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Renderer renderer = Cast<Renderer>(ce, 0);
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    SetMaterial(renderer, Cast<Material>(ce, 1));
                    break;

                case 1:
                    SetMaterial(renderer, Cast<Material>(ce, 1), Param(ce, 2).intData);
                    break;
            }
        }
        #endregion
    }

    public class RendererEvent_SetMaterialColor : RendererEvent_Base
    {
        #region Functions
        public void SetMaterialColor(Renderer renderer, Color color)
        {
            renderer.material.color = color;
        }

        public void SetMaterialColor(Renderer renderer, Color color, string propertyName)
        {
            renderer.material.SetColor(propertyName, color);
        }

        public void SetMaterialColor(Renderer renderer, Color color, int matIndex)
        {
            Material[] materials = renderer.materials; // returns a clone
            materials[matIndex].color = color;
            renderer.materials = materials; // have to deep copy
        }

        public void SetMaterialColor(Renderer renderer, Color color, int matIndex, string propertyName)
        {
            Material[] materials = renderer.materials; // returns a clone
            materials[matIndex].SetColor(propertyName, color);
            renderer.materials = materials; // have to deep copy
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            object retVal = null;
            Renderer renderer = Cast<Renderer>(ce, 0);
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    retVal = renderer.material.color;
                    break;

                case 1:
                    retVal = renderer.material.GetColor(Param(ce, 2).stringData);
                    break;

                case 2:
                    retVal = renderer.materials[Param(ce, 2).intData].GetColor("_Color");
                    break;

                case 3:
                    retVal = renderer.materials[Param(ce, 2).intData].GetColor(Param(ce, 3).stringData);
                    break;
            }
            return retVal;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Renderer renderer = Cast<Renderer>(ce, 0);
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    SetMaterialColor(renderer, (Color)rData);
                    break;

                case 1:
                    SetMaterialColor(renderer, (Color)rData, Param(ce, 2).stringData);
                    break;

                case 2:
                    SetMaterialColor(renderer, (Color)rData, Param(ce, 2).intData);
                    break;

                case 3:
                    SetMaterialColor(renderer, (Color)rData, Param(ce, 2).intData, Param(ce, 3).stringData);
                    break;
            }
        }
        #endregion
    }

    public class RendererEvent_SetMaterialShader : RendererEvent_Base
    {
        #region Functions
        public void SetMaterialShader(Renderer renderer, Shader shader)
        {
            renderer.material.shader = shader;
        }

        public void SetMaterialShader(Renderer renderer, Shader shader, int matIndex)
        {
            Material[] materials = renderer.materials; // returns a clone
            materials[matIndex].shader = shader;
            renderer.materials = materials; // have to deep copy
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            object retVal = null;
            Renderer renderer = Cast<Renderer>(ce, 0);
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    retVal = renderer.material.shader;
                    break;

                case 1:
                    retVal = renderer.materials[Param(ce, 2).intData].shader;
                    break;
            }
            return retVal;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Renderer renderer = Cast<Renderer>(ce, 0);
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    SetMaterialShader(renderer, (Shader)rData);
                    break;

                case 1:
                    SetMaterialShader(renderer, (Shader)rData, Param(ce, 2).intData);
                    break;
            }
        }
        #endregion
    }

    public class RendererEvent_SetMaterialUVs : RendererEvent_Base
    {
        #region Functions
        public void SetMaterialUVs(Renderer renderer, Vector2 offset, Vector2 scale)
        {
            renderer.material.mainTextureOffset = offset;
            renderer.material.mainTextureScale = scale;
        }

        public void SetMaterialUVs(Renderer renderer, Vector2 offset, Vector2 scale, string propertyName)
        {
            renderer.material.SetTextureOffset(propertyName, offset);
            renderer.material.SetTextureScale(propertyName, scale);
        }

        public void SetMaterialUVs(Renderer renderer, Vector2 offset, Vector2 scale, int matIndex)
        {
            Material[] materials = renderer.materials; // returns a clone
            materials[matIndex].mainTextureOffset = offset;
            materials[matIndex].mainTextureScale = scale;
            renderer.materials = materials; // have to deep copy
        }

        public void SetMaterialUVs(Renderer renderer, Vector2 offset, Vector2 scale, int matIndex, string propertyName)
        {
            Material[] materials = renderer.materials; // returns a clone
            materials[matIndex].SetTextureOffset(propertyName, offset);
            materials[matIndex].SetTextureScale(propertyName, scale);
            renderer.materials = materials; // have to deep copy
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            Vector2 offset = Vector2.zero, scale = Vector2.zero;
            Vector4 retVal = Vector4.zero;
            Renderer renderer = Cast<Renderer>(ce, 0);
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    offset = renderer.material.mainTextureOffset;
                    scale = renderer.material.mainTextureScale;  
                    break;

                case 1:
                    offset = renderer.material.GetTextureOffset(Param(ce, 3).stringData);
                    scale = renderer.material.GetTextureScale(Param(ce, 3).stringData);
                    break;

                case 2:
                    offset = renderer.materials[Param(ce, 3).intData].mainTextureOffset;
                    scale = renderer.materials[Param(ce, 3).intData].mainTextureScale;
                    break;

                case 3:
                    offset = renderer.materials[Param(ce, 3).intData].GetTextureOffset(Param(ce, 4).stringData);
                    scale = renderer.materials[Param(ce, 3).intData].GetTextureScale(Param(ce, 4).stringData);
                    break;
            }

            retVal.Set(offset.x, offset.y, scale.x, scale.y);
            return retVal;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Renderer renderer = Cast<Renderer>(ce, 0);
            Vector4 offsetAndScale = (Vector4)rData;
            Vector2 offset = new Vector2(offsetAndScale.x, offsetAndScale.y);
            Vector2 scale = new Vector2(offsetAndScale.z, offsetAndScale.w); 
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    SetMaterialUVs(renderer, offset, scale);
                    break;

                case 1:
                    SetMaterialUVs(renderer, offset, scale, Param(ce, 3).stringData);
                    break;

                case 2:
                    SetMaterialUVs(renderer, offset, scale, Param(ce, 3).intData);
                    break;

                case 3:
                    SetMaterialUVs(renderer, offset, scale, Param(ce, 3).intData, Param(ce, 4).stringData);
                    break;
            }
        }
        #endregion
    }

    public class RendererEvent_SetMaterialTexture : RendererEvent_Base
    {
        #region Functions
        public void SetMaterialTexture(Renderer renderer, Texture tex)
        {
            renderer.material.mainTexture = tex;
        }

        public void SetMaterialTexture(Renderer renderer, Texture tex, string propertyName)
        {
            renderer.material.SetTexture(propertyName, tex);
        }

        public void SetMaterialTexture(Renderer renderer, Texture tex, int matIndex)
        {
            Material[] materials = renderer.materials; // returns a clone
            materials[matIndex].mainTexture = tex;
            renderer.materials = materials; // have to deep copy
        }

        public void SetMaterialTexture(Renderer renderer, Texture tex, int matIndex, string propertyName)
        {
            Material[] materials = renderer.materials; // returns a clone
            materials[matIndex].SetTexture(propertyName, tex);
            renderer.materials = materials; // have to deep copy
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            Renderer renderer = Cast<Renderer>(ce, 0);
            object retVal = null;
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    retVal = renderer.material.mainTexture;
                    break;

                case 1:
                    retVal = renderer.material.GetTexture(Param(ce, 2).stringData);
                    break;

                case 2:
                    retVal = renderer.materials[Param(ce, 2).intData].mainTexture;
                    break;

                case 3:
                    retVal = renderer.materials[Param(ce, 2).intData].GetTexture(Param(ce, 3).stringData);
                    break;
            }
            return retVal;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Renderer renderer = Cast<Renderer>(ce, 0);
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    SetMaterialTexture(renderer, (Texture)rData);
                    break;

                case 1:
                    SetMaterialTexture(renderer, (Texture)rData, Param(ce, 2).stringData);
                    break;

                case 2:
                    SetMaterialTexture(renderer, (Texture)rData, Param(ce, 2).intData);
                    break;

                case 3:
                    SetMaterialTexture(renderer, (Texture)rData, Param(ce, 2).intData, Param(ce, 2).stringData);
                    break;
            }
        }
        #endregion
    }

    public class RendererEvent_SetMaterialFloat : RendererEvent_Base
    {
        #region Functions
        public void SetMaterialFloat(Renderer renderer, float f, string propertyName)
        {
            renderer.material.SetFloat(propertyName, f);
        }

        public void SetMaterialFloat(Renderer renderer, float f, string propertyName,  int matIndex)
        {
            Material[] materials = renderer.materials; // returns a clone
            materials[matIndex].SetFloat(propertyName, f);
            renderer.materials = materials; // have to deep copy
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            Renderer renderer = Cast<Renderer>(ce, 0);
            object retVal = null;
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    retVal = renderer.material.GetFloat(Param(ce, 2).stringData);
                    break;

                case 1:
                    retVal = renderer.materials[Param(ce, 3).intData].GetFloat(Param(ce, 2).stringData);
                    break;
            }
            return retVal;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Renderer renderer = Cast<Renderer>(ce, 0);
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    SetMaterialFloat(renderer, (float)rData, Param(ce, 2).stringData);
                    break;

                case 1:
                    SetMaterialFloat(renderer, (float)rData, Param(ce, 2).stringData, Param(ce, 3).intData);
                    break;
            }
        }
        #endregion
    }

    public class RendererEvent_EnableRenderer : RendererEvent_Base
    {
        #region Functions
        public void EnableRenderer(Renderer renderer, bool enabled)
        {
            renderer.enabled = enabled;
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<Renderer>(ce, 0).enabled;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Cast<Renderer>(ce, 0).enabled = (bool)rData;
        }
        #endregion
    }
    #endregion
}
