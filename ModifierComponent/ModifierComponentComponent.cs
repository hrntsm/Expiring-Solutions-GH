using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;

using Rhino.Geometry;

namespace ModifierComponent
{
    public class ModifierComponentComponent : GH_Component
    {
        public ModifierComponentComponent()
          : base("ModifierComponent Component", "Nickname",
            "Description of component",
            "Category", "Subcategory")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("9CD5F92C-D6D3-411D-A430-99A3C2FC7747");
    }
}
