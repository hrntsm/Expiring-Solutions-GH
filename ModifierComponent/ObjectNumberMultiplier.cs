using System;
using System.Collections.Generic;

using Grasshopper.Kernel;


namespace TestComponent
{
    public class ObjectNumberMultiplier : GH_ActiveObject, IGH_InstanceGuidDependent
    {
        public ObjectNumberMultiplier()
          : base("Multiplier", "Mul", "Multiplies outcomes", "Test", "Test")
        {
            var random = new Random();
            Factor = Math.Round(random.NextDouble() * 10, 1);
        }
        public override void CreateAttributes()
        {
            m_attributes = new AttributesNumberMultiplier(this);
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("{65268634-7FE9-405C-BA36-BD1806F410EA}"); }
        }

        private readonly List<Guid> _targetIds = new List<Guid>();
        private readonly List<IModifiable> _targetObjs = new List<IModifiable>();

        public void AddTarget(Guid target)
        {
            if (_targetIds.Contains(target))
                return;
            _targetIds.Add(target);
            _targetObjs.Clear();
        }
        public void RemoveTarget(Guid target)
        {
            _targetIds.Remove(target);
            _targetObjs.Clear();
        }

        public IEnumerable<IModifiable> TargetObjects()
        {
            if (_targetIds.Count != _targetObjs.Count)
            {
                GH_Document doc = OnPingDocument();
                if (doc == null)
                    return new IModifiable[] { };

                _targetObjs.Clear();
                foreach (Guid id in _targetIds)
                {
                    IGH_DocumentObject obj = doc.FindObject(id, true);
                    if (obj == null)
                    {
                        _targetObjs.Add(null);
                        continue;
                    }
                    _targetObjs.Add(obj as IModifiable);
                }
            }

            return _targetObjs;
        }

        public override void AddedToDocument(GH_Document document)
        {
            document.SolutionStart += DocumentSolutionStart;
        }
        public override void RemovedFromDocument(GH_Document document)
        {
            document.SolutionStart -= DocumentSolutionStart;
        }
        void DocumentSolutionStart(object sender, GH_SolutionEventArgs e)
        {
            foreach (IModifiable mod in TargetObjects())
            {
                mod.ModifyValue -= ModifyValue;
                mod.ModifyValue += ModifyValue;
            }
        }
        void ModifyValue(object sender, ModifiableEventArgs e)
        {
            // First make sure the target object is still in the same document.
            if (!(sender is IGH_DocumentObject obj)) return;
            if (obj.OnPingDocument().RuntimeID != OnPingDocument().RuntimeID)
                return;

            // If everything is hunky dory, multiply the value.
            if (!Locked)
                e.Value *= Factor;
        }

        public double Factor { get; set; }
        public override bool DependsOn(IGH_ActiveObject potentialSource)
        {
            return false;
        }
        public override bool IsDataProvider
        {
            get { return false; }
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetDouble("MultiplierFactor", Factor);
            writer.SetInt32("TargetCount", _targetIds.Count);
            for (int i = 0; i < _targetIds.Count; i++)
                writer.SetGuid("TargetID", i, _targetIds[i]);

            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            Factor = reader.GetDouble("MultiplierFactor");

            _targetIds.Clear();
            _targetObjs.Clear();
            int targetCount = reader.GetInt32("TargetCount");
            for (int i = 0; i < targetCount; i++)
            {
                Guid id = Guid.Empty;
                if (reader.TryGetGuid("TargetID", i, ref id))
                    AddTarget(id);
            }

            return base.Read(reader);
        }

        void IGH_InstanceGuidDependent.InstanceGuidsChanged(SortedDictionary<Guid, Guid> map)
        {
            _targetObjs.Clear();
            for (int i = 0; i < _targetIds.Count; i++)
            {
                Guid id = _targetIds[i];
                if (map.ContainsKey(id))
                    _targetIds[i] = map[id];
            }
        }
    }
}
