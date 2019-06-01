using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;

namespace WGP.AzurUI
{
    /// <summary>
    /// A plain text widget.
    /// </summary>
    public class Label : Widget
    {
        #region Private Fields

        private Text _display;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">Displayed text.</param>
        public Label(string text = "") : base()
        {
            _display = new Text(text, Engine.BaseFont, Engine.CharacterSize);
            _display.FillColor = Engine.BaseFontColor;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Enabled is useless here, don't use it.
        /// </summary>
        [Browsable(false), Bindable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool Enabled { get => base.Enabled; set => base.Enabled = value; }

        /// <summary>
        /// Hue is useless here, don't use it.
        /// </summary>
        [Browsable(false), Bindable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override float Hue { get => base.Hue; set => base.Hue = value; }

        public override FloatRect LocalBounds
        {
            get
            {
                var nbLines = Text.Count((c) => c == '\n');
                return new FloatRect(0, 0, _display.GetGlobalBounds().Width, nbLines * Engine.BaseFont.GetLineSpacing(Engine.CharacterSize) + Engine.CharacterSize);
            }
        }

        /// <summary>
        /// Displayed Text.
        /// </summary>
        public virtual string Text { get => _display.DisplayedString; set => _display.DisplayedString = value; }

        #endregion Public Properties

        #region Public Methods

        public static implicit operator Label(string str) => new Label(str);

        public sealed override void DrawOn(RenderTarget target) => DrawOn(target, new Vector2f());

        public override string ToString() => _display.DisplayedString;

        public sealed override void Update(RenderWindow app) => Update(app, new Vector2f());

        #endregion Public Methods

        #region Internal Methods

        internal virtual void DrawOn(RenderTarget target, Vector2f offset)
        {
            Transform tr = Transform.Identity;
            tr.Translate(Position + offset);
            target.Draw(_display, new RenderStates(tr));
        }

        internal virtual void Update(RenderWindow app, Vector2f offset)
        {
        }

        #endregion Internal Methods
    }
}