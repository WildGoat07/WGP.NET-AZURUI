using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;

namespace WGP.AzurUI
{
    public class Progressbar : Widget
    {
        #region Protected Fields

        protected Time AnimDuration;
        protected VertexArray Gradient;
        protected VertexArray Light;
        protected VertexArray Lines;
        protected float oldPerc;

        #endregion Protected Fields

        #region Public Constructors

        public Progressbar() : base()
        {
            Gradient = new VertexArray(PrimitiveType.Quads);
            Light = new VertexArray(PrimitiveType.TriangleFan);
            Lines = new VertexArray(PrimitiveType.Lines);
            Orientation = Engine.Orientation.HORIZONTAL;
            Reverse = false;
            Size = 100;
            PercentFilled = .5f;
            _chronometer.ElapsedTime = Time.FromSeconds(10);
            AnimDuration = Time.FromSeconds(1);
        }

        #endregion Public Constructors

        #region Public Properties

        public override FloatRect LocalBounds => new FloatRect(0, 0, Size, 0);
        public Engine.Orientation Orientation { get; set; }
        public float PercentFilled { get; protected set; }
        public bool Reverse { get; set; }
        public float Size { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void ChangeFilling(float percent, bool EnableAnimation = true)
        {
            oldPerc = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, AnimDuration), oldPerc, PercentFilled);
            if (EnableAnimation)
                _chronometer.Restart();
            AnimDuration = Utilities.Interpolation((percent - PercentFilled).Abs(), Time.Zero, Time.FromSeconds(1f));
            PercentFilled = percent.Capped(0, 1);
        }

        public override void DrawOn(RenderTarget target)
        {
            Transform tr = Transform.Identity;
            tr.Translate(Position);
            target.Draw(Gradient, new RenderStates(tr));
            target.Draw(Lines, new RenderStates(tr));
            target.Draw(Light, new RenderStates(tr));
        }

        public override void Update(RenderWindow app)
        {
            Time currTime = _chronometer.ElapsedTime;
            int iSize = (int)Size;
            Lines.Clear();
            Gradient.Clear();
            Light.Clear();
            int drawSecondBar = _chronometer.ElapsedTime < AnimDuration ? 1 : 0;
            if (drawSecondBar == 1 && oldPerc > PercentFilled)
                drawSecondBar = 2;
            float currentFilling;
            if (drawSecondBar == 2)
                currentFilling = new PowFunction(1f / 6).Interpolation(Utilities.Percent(currTime, Time.Zero, AnimDuration), oldPerc, PercentFilled);
            else
                currentFilling = Utilities.Interpolation(Utilities.Percent(currTime, Time.Zero, AnimDuration), oldPerc, PercentFilled);
            float secondFilling = 0;
            if (drawSecondBar == 1)
                secondFilling = new PowFunction(1f / 6).Interpolation(Utilities.Percent(currTime, Time.Zero, AnimDuration), oldPerc, PercentFilled);
            else if (drawSecondBar == 2)
                secondFilling = Utilities.Interpolation(Utilities.Percent(currTime, Time.Zero, AnimDuration), oldPerc, PercentFilled);
            if (_chronometer.Paused)
            {
                secondFilling = PercentFilled;
                currentFilling = oldPerc;
            }
            float currentHue, secondHue;
            currentHue = Utilities.Interpolation(currentFilling, Hue, Hue - 20);
            secondHue = Utilities.Interpolation(secondFilling, Hue, Hue - 20);
            currentFilling *= Size;
            secondFilling *= Size;
            currentFilling = (int)currentFilling;
            secondFilling = (int)secondFilling;
            Lines.Append(new Vertex(new Vector2f(-1, 0), NewColor(Hue, .2f, .1f)));
            Lines.Append(new Vertex(new Vector2f(iSize, 0), NewColor(Hue, .2f, .1f)));

            Lines.Append(new Vertex(new Vector2f(0, 0), NewColor(Hue, .2f, .1f)));
            Lines.Append(new Vertex(new Vector2f(0, 14), NewColor(Hue, .2f, .1f)));

            Lines.Append(new Vertex(new Vector2f(0, 14), NewColor(Hue, .2f, .6f)));
            Lines.Append(new Vertex(new Vector2f(iSize, 14), NewColor(Hue, .2f, .6f)));

            Lines.Append(new Vertex(new Vector2f(iSize + 1, 0), NewColor(Hue, .2f, .6f)));
            Lines.Append(new Vertex(new Vector2f(iSize + 1, 14), NewColor(Hue, .2f, .6f)));

            if (drawSecondBar != 0)
            {
                Gradient.Append(new Vertex(new Vector2f(1, 1), NewColor(Hue - 20, .25f, 1)));
                Gradient.Append(new Vertex(new Vector2f(1, 7), NewColor(Hue - 20, .25f, .9f)));
                Gradient.Append(new Vertex(new Vector2f(secondFilling, 7), NewColor(secondHue - 20, .25f, 1)));
                Gradient.Append(new Vertex(new Vector2f(secondFilling, 1), NewColor(secondHue - 20, .25f, 1)));
                Gradient.Append(new Vertex(new Vector2f(1, 7), NewColor(Hue - 20, .35f, .9f)));
                Gradient.Append(new Vertex(new Vector2f(1, 12), NewColor(Hue - 20, .35f, .9f)));
                Gradient.Append(new Vertex(new Vector2f(secondFilling, 12), NewColor(secondHue - 20, .35f, .9f)));
                Gradient.Append(new Vertex(new Vector2f(secondFilling, 7), NewColor(secondHue - 20, .35f, .9f)));

                Lines.Append(new Vertex(new Vector2f(0, 1), NewColor(Hue - 20, .2f, 1)));
                Lines.Append(new Vertex(new Vector2f(secondFilling, 1), NewColor(Hue - 20, .2f, 1)));
                Lines.Append(new Vertex(new Vector2f(1, 1), NewColor(Hue - 20, .2f, 1)));
                Lines.Append(new Vertex(new Vector2f(1, 13), NewColor(Hue - 20, .2f, 1)));
                Lines.Append(new Vertex(new Vector2f(0, 13), NewColor(Hue - 20, .35f, .74f)));
                Lines.Append(new Vertex(new Vector2f(secondFilling, 13), NewColor(Hue - 20, .35f, .74f)));
                Lines.Append(new Vertex(new Vector2f(secondFilling, 0), NewColor(Hue - 20, .35f, .74f)));
                Lines.Append(new Vertex(new Vector2f(secondFilling, 13), NewColor(Hue - 20, .35f, .74f)));
            }

            Gradient.Append(new Vertex(new Vector2f(1, 1), NewColor(Hue, .65f, .85f)));
            Gradient.Append(new Vertex(new Vector2f(1, 7), NewColor(Hue, .65f, .85f)));
            Gradient.Append(new Vertex(new Vector2f(currentFilling, 7), NewColor(currentHue, .65f, .85f)));
            Gradient.Append(new Vertex(new Vector2f(currentFilling, 1), NewColor(currentHue, .65f, .85f)));
            Gradient.Append(new Vertex(new Vector2f(1, 7), NewColor(Hue, .85f, .75f)));
            Gradient.Append(new Vertex(new Vector2f(1, 12), NewColor(Hue, .85f, .75f)));
            Gradient.Append(new Vertex(new Vector2f(currentFilling, 12), NewColor(currentHue, .85f, .75f)));
            Gradient.Append(new Vertex(new Vector2f(currentFilling, 7), NewColor(currentHue, .85f, .75f)));

            Lines.Append(new Vertex(new Vector2f(0, 1), NewColor(Hue, .5f, 1)));
            Lines.Append(new Vertex(new Vector2f(currentFilling, 1), NewColor(Hue, .5f, 1)));
            Lines.Append(new Vertex(new Vector2f(1, 1), NewColor(Hue, .5f, 1)));
            Lines.Append(new Vertex(new Vector2f(1, 13), NewColor(Hue, .5f, 1)));
            Lines.Append(new Vertex(new Vector2f(0, 13), NewColor(Hue, .85f, .4f)));
            Lines.Append(new Vertex(new Vector2f(currentFilling, 13), NewColor(Hue, .85f, .4f)));
            if (drawSecondBar == 0)
            {
                Lines.Append(new Vertex(new Vector2f(currentFilling, 0), NewColor(Hue, .85f, .4f)));
                Lines.Append(new Vertex(new Vector2f(currentFilling, 13), NewColor(Hue, .85f, .4f)));
            }
            Light.Append(new Vertex(new Vector2f(currentFilling, 7), NewColor(Hue - 20, .5f, 1)));

            var tr = Transform.Identity;
            tr.Scale(4, 14);
            for (int i = 0; i <= 20; i++)
                Light.Append(new Vertex(new Vector2f(currentFilling, 7) + tr.TransformPoint((Angle.Loop * i / 20).GenerateVector()), NewColor(Hue, .5f, 1, 0)));
        }

        #endregion Public Methods
    }
}