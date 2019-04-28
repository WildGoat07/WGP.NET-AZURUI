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
    /// Base class for the widgets.
    /// </summary>
    public abstract class Widget
    {
        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Widget()
        {
            _chronometer = new Chronometer(Engine.Chronometer);
            Position = new Vector2f();
            Hue = Engine.DefaultHue;
            Enabled = true;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// When enabled, a widget handle events, otherwise it becomes grey and doesn't react.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// AABB of the widget after applying its position.
        /// </summary>
        public FloatRect GlobalBounds => new FloatRect(LocalBounds.TopLeft() + Position, LocalBounds.Size());

        /// <summary>
        /// Hue of the theme.
        /// </summary>
        public float Hue { get; set; }

        /// <summary>
        /// The AABB of the widget without its position.
        /// </summary>
        public abstract FloatRect LocalBounds { get; }

        /// <summary>
        /// The position of the widget.
        /// </summary>
        public Vector2f Position { get; set; }

        #endregion Public Properties

        #region Protected Properties

        /// <summary>
        /// Internal chronometer, for animations purposes.
        /// </summary>
        protected Chronometer _chronometer { get; private set; }

        #endregion Protected Properties

        #region Public Methods

        /// <summary>
        /// Draws the widget on the target.
        /// </summary>
        /// The widget should be moved according to its Position when inherited.
        /// <param name="target">Target to draw the widget on.</param>
        public abstract void DrawOn(RenderTarget target);

        /// <summary>
        /// Updates the widget (graphics and events).
        /// </summary>
        /// <param name="app">Windows on which the widget is DIRECTLY drawn on.</param>
        public abstract void Update(RenderWindow app);

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Use this method to create a new color instead of newHSVColor(). Handles the Enabled porperty.
        /// </summary>
        /// <param name="hue">Hue of the color.</param>
        /// <param name="saturation">Saturation of the color.</param>
        /// <param name="value">Value of the color.</param>
        /// <param name="alpha">Transparency of the color.</param>
        /// <returns>Created color.</returns>
        protected HSVColor NewColor(float hue, float saturation, float value, byte alpha = 255) => new HSVColor(hue, Enabled ? saturation : 0, Enabled ? value : value * .7f, alpha);

        #endregion Protected Methods
    }
}