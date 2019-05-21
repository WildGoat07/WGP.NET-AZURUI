using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;

namespace WGP.AzurUI
{
    /// <summary>
    /// An advanced label with formatting.
    /// </summary>
    public class Richtext : Label
    {
        #region Private Fields

        private VertexArray _lines;

        private bool _requireUpdate;

        private List<Tuple<Sprite, int>> _sprites;

        private string _text;

        private List<Text> _texts;

        private List<Tuple<List<Texture>, int>> _textures;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">Markdown formatted text.</param>
        public Richtext(string text = "") : base(text)
        {
            Text = text;
            _texts = new List<Text>();
            _lines = new VertexArray(PrimitiveType.Lines);
            _sprites = new List<Tuple<Sprite, int>>();
            _textures = new List<Tuple<List<Texture>, int>>();
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// When enabled, a widget handle events, otherwise it becomes grey and doesn't react.
        /// </summary>
        public new bool Enabled { get => base.Enabled; set => base.Enabled = value; }

        /// <summary>
        /// Hue of the links.
        /// </summary>
        public new float Hue { get => base.Hue; set => base.Hue = value; }

        /// <summary>
        /// Maximum width of the widget, it will try to wrap its content. 0 for no maximum width.
        /// </summary>
        public float MaxWidth { get; set; }

        /// <summary>
        /// Formatted text.
        /// </summary>
        /// The formatting is based on Markdown.
        public override string Text { get => _text; set { _text = value; _requireUpdate = true; } }

        #endregion Public Properties

        #region Public Methods

        public override string ToString() => _text;

        #endregion Public Methods

        #region Internal Methods

        internal override void DrawOn(RenderTarget target, Vector2f offset)
        {
            Transform tr = Transform.Identity;
            tr.Translate(Position + offset);
            foreach (var item in _texts)
                target.Draw(item, new RenderStates(tr));
            target.Draw(_lines, new RenderStates(tr));
            foreach (var item in _sprites)
                target.Draw(item.Item1, new RenderStates(tr));
        }

        internal override void Update(RenderWindow app, Vector2f offset)
        {
            base.Update(app, offset);
            if (_requireUpdate)
            {
                _requireUpdate = false;
                _chronometer.Restart();
            }
        }

        #endregion Internal Methods
    }
}