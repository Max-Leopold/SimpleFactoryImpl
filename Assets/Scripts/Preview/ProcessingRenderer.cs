using System;
using System.Collections.Generic;
using SimpleFactoryServerLib.Objects;
using SimpleFactoryServerLib.Objects.Materials;
using SimpleFactoryServerLib.Objects.ProcessingObjects;

public class ProcessingRenderer : StorageRenderer
{
    protected override Material GetMaterialToRender()
    {
        if (_worldObject is ProcessingMachine)
        {
            ProcessingMachine castedWObject = (ProcessingMachine) _worldObject;
            if (castedWObject?.recipe?.output != null)
            {
                return castedWObject.recipe.output;
            }
        }

        return null;
    }
}