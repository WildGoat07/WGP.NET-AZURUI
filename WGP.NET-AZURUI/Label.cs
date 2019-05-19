using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

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
        public Label(string text = "")
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
        public new float Hue { get => base.Hue; set => base.Hue = value; }

        public override FloatRect LocalBounds => new FloatRect(0, 0, _display.GetGlobalBounds().Width, Engine.CharacterSize);

        /// <summary>
        /// Displayed Text.
        /// </summary>
        public virtual string Text { get => _display.DisplayedString; set => _display.DisplayedString = value; }

        #endregion Public Properties

        #region Public Methods

        public static implicit operator Label(string str) => new Label(str);

        public override void DrawOn(RenderTarget target)
        {
            Transform tr = Transform.Identity;
            tr.Translate(Position);
            target.Draw(_display, new RenderStates(tr));
        }

        public override void Update(RenderWindow app)
        {
        }

        #endregion Public Methods
    }
}