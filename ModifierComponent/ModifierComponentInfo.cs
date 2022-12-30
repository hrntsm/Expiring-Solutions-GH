using System;
using System.Drawing;

using Grasshopper;
using Grasshopper.Kernel;

namespace ModifierComponent
{
    public class ModifierComponentInfo : GH_AssemblyInfo
    {
        public override string Name => "ModifierComponent Info";
        public override Bitmap Icon => null;
        public override string Description => "";
        public override Guid Id => new Guid("C1316009-5C0E-4880-8855-FE7CB059CB24");
        public override string AuthorName => "";
        public override string AuthorContact => "";
    }
}
