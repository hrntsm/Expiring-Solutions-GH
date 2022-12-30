using System;

using Grasshopper.Kernel;

namespace TestComponent
{
    public class ComponentAddNumbers : GH_Component, IModifiable
    {
        public ComponentAddNumbers()
          : base("Add numbers", "AddNum", "Add two numbers", "Test", "Test")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("First number", "A", "First number in addition", GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("Second number", "B", "Second number in addition", GH_ParamAccess.item, 5.0);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Result", "C", "Added result", GH_ParamAccess.item);
        }

        public event EventHandler<ModifiableEventArgs> ModifyValue;
        protected override void SolveInstance(IGH_DataAccess da)
        {
            double a = 0.0;
            double b = 0.0;
            if (!da.GetData(0, ref a)) return;
            if (!da.GetData(1, ref b)) return;

            double result = a + b;

            if (ModifyValue != null)
            {
                var e = new ModifiableEventArgs(result);
                ModifyValue(this, e);
                result = e.Value;
            }

            da.SetData(0, result);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{6E7FB9B7-38CE-41F1-BE1D-FA8C1728C984}"); }
        }
    }
}
