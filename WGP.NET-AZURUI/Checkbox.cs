using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGP;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace WGP.AzurUI
{
    /// <summary>
    /// Checkbox widget.
    /// </summary>
    public class Checkbox : Widget
    {
        #region Protected Fields

        /// <summary>
        /// The vertice used for the gradient.
        /// </summary>
        protected VertexArray _gradient;

        /// <summary>
        /// The vertice used for the lines.
        /// </summary>
        protected VertexArray _lines;

        /// <summary>
        /// The vertice used for the light emitted.
        /// </summary>
        protected VertexArray _outerLight;

        #endregion Protected Fields

        #region Private Fields

        private State _currentState;

        private bool oldMouseState;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Checkbox() : base()
        {
            _gradient = new VertexArray(PrimitiveType.TriangleFan);
            _outerLight = new VertexArray(PrimitiveType.TriangleStrip);
            _lines = new VertexArray(PrimitiveType.Lines);
            Text = new Label();
            Pressing = false;
            Hovered = false;
            oldMouseState = false;
        }

        #endregion Public Constructors

        #region Public Enums

        /// <summary>
        /// State of a checkbox.
        /// </summary>
        public enum State
        {
            /// <summary>
            /// The box is unchecked.
            /// </summary>
            UNCHECKED,

            /// <summary>
            /// The box is neither checked/unchecked.
            /// </summary>
            INDETERMINATE,

            /// <summary>
            /// The box is checked.
            /// </summary>
            CHECKED
        }

        #endregion Public Enums

        #region Public Properties

        /// <summary>
        /// True if the box is checked or indeterminate.
        /// </summary>
        public bool Checked
        {
            get => CurrentState != State.UNCHECKED;
            set => CurrentState = value ? State.CHECKED : State.UNCHECKED;
        }

        /// <summary>
        /// Current check state of the checkbox.
        /// </summary>
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

        /// <summary>
        /// True if the mouse hovers the checkbox.
        /// </summary>
        public bool Hovered { get; protected set; }

        /// <summary>
        /// The AABB of the widget without its position.
        /// </summary>
        public override FloatRect LocalBounds
        {
            get
            {
                return Text == null ? Utilities.CreateRect(new Vector2f(), new Vector2f(18, 18)) : Utilities.CreateRect(new Vector2f(), new Vector2f(18, 18), Text.GlobalBounds.TopLeft(), Text.GlobalBounds.BotRight());
            }
        }

        /// <summary>
        /// True if the mouse is clicking the checkbox.
        /// </summary>
        public bool Pressing { get; protected set; }

        /// <summary>
        /// Triggered when the checkbox changed its state.
        /// </summary>
        public Action StateChanged { get; set; }

        /// <summary>
        /// The text displayed.
        /// </summary>
        public Label Text { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Draws the widget on the target.
        /// </summary>
        /// The widget should be moved according to its Position when inherited.
        /// <param name="target">Target to draw the widget on.</param>
        public override void DrawOn(RenderTarget target)
        {
            Transform tr = Transform.Identity;
            tr.Translate(Position);
            target.Draw(_gradient, new RenderStates(tr));
            target.Draw(_outerLight, new RenderStates(tr));
            target.Draw(_lines, new RenderStates(tr));
            Text?.DrawOn(target, Position);
        }

        /// <summary>
        /// Updates the widget (graphics and events).
        /// </summary>
        /// <param name="app">Windows on which the widget is DIRECTLY drawn on.</param>
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
            if (!Enabled)
            {
                Hovered = false;
                Pressing = false;
            }

            if (Text != null)
                Text.Position = new Vector2f(20, 9 - Text.GlobalBounds.Height / 2);
            _lines.Clear();
            _gradient.Clear();
            _outerLight.Clear();
            var outlineColor = NewColor(Hue, .36f, .6f);
            if (Hovered)
                outlineColor.V = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), .6f, .8f);
            if (Pressing)
                outlineColor.V = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(1f)), .8f, 1f);
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
                    bonusA = (byte)Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), .3f * 255, .4f * 255);
                    S = .75f;
                    bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(1f)), .35f, .5f);
                }
                else if (Hovered)
                {
                    bonusA = (byte)Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), .1f * 255, .2f * 255);
                    S = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), .5f, .75f);
                    bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), .2f, .35f);
                }
                else
                {
                    bonusV = .2f;
                    bonusA = (byte)(.1f * 255);
                    S = .5f;
                }
                _gradient.Append(new Vertex(new Vector2f(9, 9), NewColor(Hue, S, .5f + bonusV, bonusA)));
                for (int i = 0; i <= 30; i++)
                    _gradient.Append(new Vertex(new Vector2f(9, 9) + (Angle.Loop * i / 30).GenerateVector(9), NewColor(Hue, S, .5f + bonusV, (byte)(128 + bonusA))));
            }
            {
                byte A = 0;
                if (Pressing)
                    A = (byte)Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(1f)), 0f, 255f);
                for (int i = 0; i <= 30; i++)
                {
                    _outerLight.Append(new Vertex(new Vector2f(9, 9) + (Angle.Loop * i / 30).GenerateVector(9), NewColor(Hue, .75f, 1, A)));
                    _outerLight.Append(new Vertex(new Vector2f(9, 9) + (Angle.Loop * i / 30).GenerateVector(15), NewColor(Hue, .75f, 1, 0)));
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
                _lines.Append(new Vertex(center + new Vector2f(-4, 0), NewColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(-1, 3), NewColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(-1, 3), NewColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(5, -3), NewColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(5, -3), NewColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(6, -2), NewColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(6, -2), NewColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(-1, 5), NewColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(-1, 5), NewColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(-5, 1), NewColor(Hue, .35f, 1)));
            }
            else if (CurrentState == State.INDETERMINATE)
            {
                Vector2f center = new Vector2f(8, 8);
                _lines.Append(new Vertex(center + new Vector2f(-4, 1), Color.White));
                _lines.Append(new Vertex(center + new Vector2f(7, 1), Color.White));

                //outline
                _lines.Append(new Vertex(center + new Vector2f(-4, 0), NewColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(6, 0), NewColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(6, 0), NewColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(7, 1), NewColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(6, 2), NewColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(-4, 2), NewColor(Hue, .35f, 1)));

                _lines.Append(new Vertex(center + new Vector2f(-5, 1), NewColor(Hue, .35f, 1)));
                _lines.Append(new Vertex(center + new Vector2f(-4, 0), NewColor(Hue, .35f, 1)));
            }
        }

        #endregion Public Methods
    }
}