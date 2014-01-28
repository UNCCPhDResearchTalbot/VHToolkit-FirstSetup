using UnityEngine;
using UnityEditor;
using System.Collections;

class MaterialImportSettings : AssetPostprocessor
{
    public override int GetPostprocessOrder() { return -10; }

    void OnPreprocessModel()
    {
        ModelImporter modelImporter = (ModelImporter)assetImporter;

        // MATERIAL NAME
        //     ModelImporterMaterialName.BasedOnMaterialName
        //     ModelImporterMaterialName.BasedOnModelNameAndMaterialName
        //     ModelImporterMaterialName.BasedOnTextureName
        modelImporter.materialName = ModelImporterMaterialName.BasedOnMaterialName;

        // MATERIAL SEARCH
        //     ModelImporterMaterialSearch.Everywhere
        //     ModelImporterMaterialSearch.Local
        //     ModelImporterMaterialSearch.RecursiveUp
        modelImporter.materialSearch = ModelImporterMaterialSearch.Local;
    }
}
