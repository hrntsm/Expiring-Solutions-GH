using System.Drawing;

using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

using Rhino.Geometry;

namespace TestComponent
{
    internal class AttributesNumberMultiplier : GH_Attributes<ObjectNumberMultiplier>
    {
        public AttributesNumberMultiplier(ObjectNumberMultiplier owner)
          : base(owner)
        {

        }

        public override bool AllowMessageBalloon
        {
            get { return false; }
        }
        public override bool HasInputGrip
        {
            get { return false; }
        }
        public override bool HasOutputGrip
        {
            get { return false; }
        }

        private const int InnerRadius = 30;
        private const int OuterRadius = 40;

        public override bool IsPickRegion(PointF point)
        {
            return Grasshopper.GUI.GH_GraphicsUtil.IsPointInEllipse(Bounds, point);
        }
        protected override void Layout()
        {
            Pivot = GH_Convert.ToPoint(Pivot);
            Bounds = new RectangleF(Pivot.X - OuterRadius, Pivot.Y - OuterRadius, 2 * OuterRadius, 2 * OuterRadius);
        }

        protected RectangleF InnerBounds
        {
            get
            {
                RectangleF inner = Bounds;
                int inflation = OuterRadius - InnerRadius;
                inner.Inflate(-inflation, -inflation);
                return inner;
            }
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            switch (channel)
            {
                case GH_CanvasChannel.Wires:
                    graphics.FillEllipse(Brushes.HotPink, Bounds);
                    foreach (IModifiable mod in Owner.TargetObjects())
                    {
                        if (mod == null)
                            continue;

                        if (!(mod is IGH_DocumentObject obj))
                            continue;

                        DrawTargetArrow(graphics, obj.Attributes.Bounds);
                    }
                    break;

                case GH_CanvasChannel.Objects:
                    var capsule = GH_Capsule.CreateCapsule(InnerBounds, GH_Palette.Normal, InnerRadius, 0);
                    capsule.Render(graphics, Selected, Owner.Locked, true);
                    capsule.Dispose();

                    string text = string.Format("{0:0.00}", Owner.Factor);
                    Grasshopper.GUI.GH_GraphicsUtil.RenderCenteredText(graphics, text, GH_FontServer.Large, Color.Black, Pivot);
                    break;
            }
        }

        private void DrawTargetArrow(Graphics graphics, RectangleF target)
        {
            PointF cp = Grasshopper.GUI.GH_GraphicsUtil.BoxClosestPoint(Pivot, target);
            double distance = Grasshopper.GUI.GH_GraphicsUtil.Distance(Pivot, cp);
            if (distance < OuterRadius)
                return;

            var circle = new Circle(new Point3d(Pivot.X, Pivot.Y, 0.0), OuterRadius - 2);
            var tp = GH_Convert.ToPointF(circle.ClosestPoint(new Point3d(cp.X, cp.Y, 0.0)));

            var arrowPen = new Pen(Color.HotPink, OuterRadius - InnerRadius)
            {
                EndCap = System.Drawing.Drawing2D.LineCap.RoundAnchor
            };
            graphics.DrawLine(arrowPen, tp, cp);
            arrowPen.Dispose();
        }

        private bool _drawing;
        private RectangleF _drawingBox;

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, Grasshopper.GUI.GH_CanvasMouseEvent e)
        {
            _drawing = false;
            _drawingBox = InnerBounds;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // If on outer disc, but not in inner disc.. then start a wire drawing process.
                bool onOuterDisc = Grasshopper.GUI.GH_GraphicsUtil.IsPointInEllipse(Bounds, e.CanvasLocation);
                bool onInnerDisc = Grasshopper.GUI.GH_GraphicsUtil.IsPointInEllipse(InnerBounds, e.CanvasLocation);
                if (onOuterDisc && !onInnerDisc)
                {
                    // Begin arrow drawing behaviour.
                    _drawing = true;
                    sender.CanvasPostPaintObjects += CanvasPostPaintObjects;
                    return GH_ObjectResponse.Capture;
                }
            }

            // Otherwise revert to default behaviour.
            return base.RespondToMouseDown(sender, e);
        }
        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, Grasshopper.GUI.GH_CanvasMouseEvent e)
        {
            if (_drawing)
            {
                _drawingBox = new RectangleF(e.CanvasLocation, new SizeF(0, 0));

                GH_Document doc = sender.Document;
                if (doc != null)
                {
                    IGH_Attributes att = doc.FindAttribute(e.CanvasLocation, true);
                    if (att != null)
                    {
                        if (att.DocObject is IModifiable)
                            _drawingBox = att.Bounds;
                    }
                }
                sender.Invalidate();
                return GH_ObjectResponse.Handled;
            }

            return base.RespondToMouseMove(sender, e);
        }
        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, Grasshopper.GUI.GH_CanvasMouseEvent e)
        {
            if (_drawing)
            {
                _drawing = false;
                sender.CanvasPostPaintObjects -= CanvasPostPaintObjects;

                GH_Document doc = sender.Document;
                if (doc != null)
                {
                    IGH_Attributes att = doc.FindAttribute(e.CanvasLocation, true);
                    if (att != null)
                        if (att.DocObject is IModifiable)
                        {
                            Owner.RecordUndoEvent("Add Modifier");
                            Owner.AddTarget(att.DocObject.InstanceGuid);
                            if (att.DocObject is IGH_ActiveObject obj)
                                obj.ExpireSolution(true);
                        }
                }

                sender.Invalidate();
                return GH_ObjectResponse.Release;
            }

            return base.RespondToMouseUp(sender, e);
        }
        void CanvasPostPaintObjects(GH_Canvas sender)
        {
            if (!_drawing) return;
            DrawTargetArrow(sender.Graphics, _drawingBox);
        }
    }
}
