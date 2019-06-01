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
    /// A simple clickable button.
    /// </summary>
    public class Button : Widget
    {
        #region Public Fields

        public bool AutoSize;

        #endregion Public Fields

        #region Protected Fields

        /// <summary>
        /// The vertice used for the gradient.
        /// </summary>
        protected VertexArray _gradient;

        /// <summary>
        /// The vertice used for the lines.
        /// </summary>
        protected VertexArray _lines;

        #endregion Protected Fields

        #region Private Fields

        private bool oldMouseState;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Button() : base()
        {
            AutoSize = false;
            Text = new Label();
            _lines = new VertexArray(PrimitiveType.Lines);
            _gradient = new VertexArray(PrimitiveType.Quads);
            Pressing = false;
            Hovered = false;
            oldMouseState = false;
            Clicked = null;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Triggered when clicking the button.
        /// </summary>
        public Action Clicked { get; set; }

        /// <summary>
        /// Size of the button. The origin is in the middle.
        /// </summary>
        public Vector2f HalfSize { get; set; }

        /// <summary>
        /// True if the mouse hovers the button.
        /// </summary>
        public bool Hovered { get; protected set; }

        /// <summary>
        /// The AABB of the widget without its position.
        /// </summary>
        public override FloatRect LocalBounds => new FloatRect(-HalfSize, HalfSize * 2);

        /// <summary>
        /// True when the mouse is clicking the button.
        /// </summary>
        public bool Pressing { get; protected set; }

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
                if (Pressing && !oldMouseState && Clicked != null)
                    Clicked();
                if (oldMouseState && Hovered)
                    Pressing = true;
                else
                    Pressing = false;
            }
            if (oldHover != Hovered || (!oldPress && Pressing))
                _chronometer.Restart();
            float s = .3f;
            float bonusV = 0;
            if (!Enabled)
            {
                Hovered = false;
                Pressing = false;
            }
            if (Hovered)
            {
                s = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromMilliseconds(500)), .3f, .5f);
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromMilliseconds(500)), 0f, .2f);
            }
            else
                Pressing = false;
            if (Pressing)
            {
                s = .6f;
                bonusV = .4f;
            }
            if (Text != null)
            {
                Text.Position = -(Vector2f)(Vector2i)(Text.GlobalBounds.Size() / 2);
                Text.Update(app, Position);
            }
            if (AutoSize)
            {
                HalfSize = (Vector2f)(Vector2i)(Text.GlobalBounds.Size() / 2) + new Vector2f(5, 5);
            }
            _gradient.Clear();
            if (Pressing)
            {
                _gradient.Append(new Vertex(new Vector2f(-HalfSize.X + 1, -HalfSize.Y + 1), NewColor(Hue, s / 2, .47f,
                    (byte)Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromMilliseconds(1000)), 60f, 255))));
                _gradient.Append(new Vertex(new Vector2f(HalfSize.X - 2, -HalfSize.Y + 1), NewColor(Hue, s / 2, .47f,
                    (byte)Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromMilliseconds(1000)), 60f, 255))));
            }
            else if (Hovered)
            {
                _gradient.Append(new Vertex(new Vector2f(-HalfSize.X + 1, -HalfSize.Y + 1), NewColor(Hue, s / 2, .47f,
                    (byte)Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromMilliseconds(500)), 0f, 60))));
                _gradient.Append(new Vertex(new Vector2f(HalfSize.X - 2, -HalfSize.Y + 1), NewColor(Hue, s / 2, .47f,
                    (byte)Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromMilliseconds(500)), 0f, 60))));
            }
            else
            {
                _gradient.Append(new Vertex(new Vector2f(-HalfSize.X + 1, -HalfSize.Y + 2), NewColor(Hue, s / 2, .47f, 0)));
                _gradient.Append(new Vertex(new Vector2f(HalfSize.X - 2, -HalfSize.Y + 2), NewColor(Hue, s / 2, .47f, 0)));
            }
            _gradient.Append(new Vertex(new Vector2f(HalfSize.X - 2, HalfSize.Y - 2), NewColor(Hue, s, .47f + bonusV)));
            _gradient.Append(new Vertex(new Vector2f(-HalfSize.X + 1, HalfSize.Y - 2), NewColor(Hue, s, .47f + bonusV)));

            _lines.Clear();
            var actualBonus = bonusV * 1.2f;
            var bonus0 = 0;
            Time threshold = Time.Zero;
            Time increase = Time.FromSeconds(1f) / 9f;

            #region light

            if (Pressing)
            {
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, -HalfSize.Y + 1), NewColor(Hue, s, .47f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, -HalfSize.Y + 1), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, -HalfSize.Y + 1), NewColor(Hue, s, .47f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 1, -HalfSize.Y + 3), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 1, -HalfSize.Y + 3), NewColor(Hue, s, .47f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 1, HalfSize.Y - 3), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 1, HalfSize.Y - 3), NewColor(Hue, s, .47f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, HalfSize.Y - 1), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, HalfSize.Y - 1), NewColor(Hue, s, .47f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, HalfSize.Y - 1), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, HalfSize.Y - 1), NewColor(Hue, s, .47f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 1, HalfSize.Y - 3), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 1, HalfSize.Y - 3), NewColor(Hue, s, .47f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 1, -HalfSize.Y + 3), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 1, -HalfSize.Y + 3), NewColor(Hue, s, .47f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, -HalfSize.Y + 1), NewColor(Hue, s, .47f + bonusV)));
            }
            else
            {
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, -HalfSize.Y + 1), NewColor(Hue, s, .47f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, -HalfSize.Y + 1), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, -HalfSize.Y + 1), NewColor(Hue, s, .47f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 1, -HalfSize.Y + 3), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 1, -HalfSize.Y + 3), NewColor(Hue, s, .47f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 1, HalfSize.Y - 3), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 1, HalfSize.Y - 3), NewColor(Hue, s, .47f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, HalfSize.Y - 1), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, HalfSize.Y - 1), NewColor(Hue, s, .47f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, HalfSize.Y - 1), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, HalfSize.Y - 1), NewColor(Hue, s, .47f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 1, HalfSize.Y - 3), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 1, HalfSize.Y - 3), NewColor(Hue, s, .47f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 1, -HalfSize.Y + 3), NewColor(Hue, s, .47f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 1, -HalfSize.Y + 3), NewColor(Hue, s, .47f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, -HalfSize.Y + 1), NewColor(Hue, s, .47f + bonusV)));
            }

            #endregion light

            #region dark

            threshold = Time.Zero;
            if (Pressing)
            {
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, -HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, -HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, -HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(HalfSize.X, -HalfSize.Y + 3), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X, -HalfSize.Y + 3), NewColor(Hue, s, .27f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(HalfSize.X, HalfSize.Y - 3), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X, HalfSize.Y - 3), NewColor(Hue, s, .27f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X, HalfSize.Y - 3), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X, HalfSize.Y - 3), NewColor(Hue, s, .27f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X, -HalfSize.Y + 3), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X, -HalfSize.Y + 3), NewColor(Hue, s, .27f + bonusV)));
                threshold += increase;
                bonusV = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, threshold, threshold + increase), bonus0, actualBonus);
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, -HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));
            }
            else
            {
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, -HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, -HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, -HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(HalfSize.X, -HalfSize.Y + 3), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X, -HalfSize.Y + 3), NewColor(Hue, s, .27f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(HalfSize.X, HalfSize.Y - 3), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X, HalfSize.Y - 3), NewColor(Hue, s, .27f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(HalfSize.X - 3, HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X, HalfSize.Y - 3), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X, HalfSize.Y - 3), NewColor(Hue, s, .27f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X, -HalfSize.Y + 3), NewColor(Hue, s, .27f + bonusV)));

                _lines.Append(new Vertex(new Vector2f(-HalfSize.X, -HalfSize.Y + 3), NewColor(Hue, s, .27f + bonusV)));
                _lines.Append(new Vertex(new Vector2f(-HalfSize.X + 3, -HalfSize.Y), NewColor(Hue, s, .27f + bonusV)));
            }

            #endregion dark
        }

        #endregion Public Methods
    }
}