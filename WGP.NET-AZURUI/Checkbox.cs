using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGP;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace WGP.AZURUI
{
    public class Checkbox : Widget
    {
        #region Protected Fields

        protected VertexArray _gradient;
        protected VertexArray _lines;
        protected VertexArray _outerLight;
        protected Text _text;

        #endregion Protected Fields

        #region Private Fields

        private State _currentState;
        private bool oldMouseState;

        #endregion Private Fields

        #region Public Constructors

        public Checkbox() : base()
        {
            _gradient = new VertexArray(PrimitiveType.TriangleFan);
            _outerLight = new VertexArray(PrimitiveType.TriangleStrip);
            _lines = new VertexArray(PrimitiveType.Lines);
            _text = new Text("", Engine.BaseFont, Engine.CharacterSize);
            _text.FillColor = Engine.BaseFontColor;
            Hue = Engine.DefaultHue;
            Pressing = false;
            Hovered = false;
            oldMouseState = false;
        }

        #endregion Public Constructors

        #region Public Enums

        public enum State
        {
            UNCHECKED,
            INDETERMINATE,
            CHECKED
        }

        #endregion Public Enums

        #region Public Properties

        public bool Checked
        {
            get => CurrentState != State.UNCHECKED;
            set => CurrentState = value ? State.CHECKED : State.UNCHECKED;
        }

        public State CurrentState
        {
            get => _currentState;
            set
            {
                bool throwChange = value != _currentState;
                _currentState = value;
                if (throwChange && StateChanged != null)
                    StateChanged();
            }
        }

        public bool Hovered { get; protected set; }
        public float Hue { get; set; }
        public override FloatRect LocalBounds => new FloatRect(0, 0, 20 + _text.GetGlobalBounds().Width, 18);
        public bool Pressing { get; protected set; }
        public Action StateChanged { get; set; }
        public string Text { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void DrawOn(RenderTarget target)
        {
            Transform tr = Transform.Identity;
            tr.Translate(Position);
            target.Draw(_gradient, new RenderStates(tr));
            target.Draw(_outerLight, new RenderStates(tr));
            target.Draw(_lines, new RenderStates(tr));
            target.Draw(_text, new RenderStates(tr));
        }

        public override void Update(RenderWindow app)
        {
            bool oldHover = Hovered;
            bool oldPress = Pressing;
            Hovered = GlobalBounds.Contains(app.MapPixelToCoords(Mouse.GetPosition(app)));
            if (oldMouseState != Mouse.IsButtonPressed(Mouse.Button.Left))
            {
                oldMouseState = Mouse.IsButtonPressed(Mouse.Button.Left);
                if (Pressing && !oldMouseState)
                    Checked = !Checked;
                if (oldMouseState && Hovered)
                    Pressing = true;
                else
                    Pressing = false;
            }
            if (!Hovered)
                Pressing = false;
            if (oldHover != Hovered || (!oldPress && Pressing))
                _chronometer.Restart();

            _text.DisplayedString = Text;
            _text.Origin = (Vector2f)new Vector2i(0, (int)_text.GetLocalBounds().Top);
            _text.Position = new Vector2f(20, (20 - Engine.CharacterSize) / 2);
            _lines.Clear();
            _gradient.Clear();
            _outerLight.Clear();
            var outlineColor = new HSVColor(Hue, .36f, .6f);
            if (Hovered)
                outlineColor.V = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), .6f, .8f);
            if (Pressing)
                outlineColor.V = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(1.5f)), .8f, 1f);
            for (int i = 0; i < 30; i++)
            {
                _lines.Append(new Vertex((Angle.Loop * i / 30).GenerateVector(9) + new Vector2f(9, 9), outlineColor));
                _lines.Append(new Vertex((Angle.Loop * (i + 1) / 30).GenerateVector(9) + new Vector2f(9, 9), outlineColor));
            }
            {
                float S;
                float bonusV;
                byte bonusA;
                if (Pressing)
                {
                    bonusA = (byte)(.4f * 255);
                    S = .75f;
                    bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(1.5f)), .25f, .5f);
                }
                else if (Hovered)
                {
                    bonusA = (byte)Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), 0f, .2f * 255);
                    S = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), .36f, .65f);
                    bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), 0f, .25f);
                }
                else
                {
                    bonusV = 0;
                    bonusA = 0;
                    S = .36f;
                }
                _gradient.Append(new Vertex(new Vector2f(9, 9), new HSVColor(Hue, S, .5f + bonusV, bonusA)));
                for (int i = 0; i <= 30; i++)
                    _gradient.Append(new Vertex(new Vector2f(9, 9) + (Angle.Loop * i / 30).GenerateVector(9), new HSVColor(Hue, S, .5f + bonusV, (byte)(128 + bonusA))));
            }
            {
                byte A = 0;
                if (Pressing)
                    A = (byte)Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(1.5f)), 0f, 255f);
                for (int i = 0; i <= 30; i++)
                {
                    _outerLight.Append(new Vertex(new Vector2f(9, 9) + (Angle.Loop * i / 30).GenerateVector(9), new HSVColor(Hue, .75f, 1, A)));
                    _outerLight.Append(new Vertex(new Vector2f(9, 9) + (Angle.Loop * i / 30).GenerateVector(15), new HSVColor(Hue, .75f, 1, 0)));
                }
            }
            if (CurrentState == State.CHECKED)
            {
                Vector2f center = new Vector2f(8, 8);
                _lines.Append(new Vertex(center + new Vector2f(-4, 1), Color.White));
                _lines.Append(new Vertex(center + new Vector2f(-1, 4), Color.White));

                _lines.Append(new Vertex(center + new Vector2f(-1, 4), Color.White));
                _lines.Append(new Vertex(center + new Vector2f(5, -2), Color.White));

                //outline
                _lines.Append(new Vertex(center + new Vector2f(-4, 0), new HSVColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(-1, 3), new HSVColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(-1, 3), new HSVColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(5, -3), new HSVColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(5, -3), new HSVColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(6, -2), new HSVColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(6, -2), new HSVColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(-1, 5), new HSVColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(-1, 5), new HSVColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(-5, 1), new HSVColor(Hue, .35f, 1)));
            }
            else if (CurrentState == State.INDETERMINATE)
            {
                Vector2f center = new Vector2f(8, 8);
                _lines.Append(new Vertex(center + new Vector2f(-4, 1), Color.White));
                _lines.Append(new Vertex(center + new Vector2f(7, 1), Color.White));

                //outline
                _lines.Append(new Vertex(center + new Vector2f(-4, 0), new HSVColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(6, 0), new HSVColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(6, 0), new HSVColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(7, 1), new HSVColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(6, 2), new HSVColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(-4, 2), new HSVColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(-5, 1), new HSVColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(-4, 0), new HSVColor(Hue, .35f, 1)));
            }
        }

        #endregion Public Methods
    }
}