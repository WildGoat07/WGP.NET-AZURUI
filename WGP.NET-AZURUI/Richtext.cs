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

        private Chronometer _gifChrono;

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
        public Richtext(string text = "")
        {
            Text = text;
            _texts = new List<Text>();
            _lines = new VertexArray(PrimitiveType.Lines);
            _sprites = new List<Tuple<Sprite, int>>();
            _textures = new List<Tuple<List<Texture>, int>>();
            _gifChrono = new Chronometer();
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

        public override FloatRect LocalBounds => base.LocalBounds;

        /// <summary>
        /// Maximum width of the widget, it will try to wrap its content.
        /// </summary>
        public float MaxWidth { get; set; }

        /// <summary>
        /// Formatted text.
        /// </summary>
        /// The formatting is based on Markdown, currently supported : headlines, emphasis, ordered
        /// and unordered lists, links, image links, horizontal lines
        public override string Text { get => _text; set { _text = value; _requireUpdate = true; } }

        #endregion Public Properties

        #region Public Methods

        public override void DrawOn(RenderTarget target)
        {
            Transform tr = Transform.Identity;
            tr.Translate(Position);
            foreach (var item in _texts)
                target.Draw(item, new RenderStates(tr));
            target.Draw(_lines, new RenderStates(tr));
            foreach (var item in _sprites)
                target.Draw(item.Item1, new RenderStates(tr));
        }

        public override string ToString() => Text;

        public override void Update(RenderWindow app)
        {
            base.Update(app);
            if (_requireUpdate)
            {
                _requireUpdate = false;
                bool end = false;
                Vector2f offset = new Vector2f();
                int index = 0;
                while (!end)
                {
                    bool endLine = false;
                    while (!endLine)
                    {
                        char curr = _text[index];
                        if (curr == '#')
                        {
                            int headlineLevel = 1;
                            bool parsing = true;
                            while (parsing)
                            {
                                index++;
                                curr = _text[index];
                                if (curr == ' ')
                                    parsing = false;
                                else if (curr == '#' && headlineLevel < 6)
                                    headlineLevel++;
                                else
                                {
                                    parsing = false;
                                    headlineLevel = 0;
                                }
                            }
                        }
                    }
                }
                _gifChrono.Restart();
            }
        }

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

        #endregion Internal Methods

        #region Private Classes

        private class Modifier
        {
            #region Public Fields

            public Type ModifierType;

            public dynamic Options;

            #endregion Public Fields

            #region Public Enums

            public enum Type
            {
                HEADLINE,
                BOLD,
                ITALIC,
                STRIKETHROUGH,
                OREDERED_LIST,
                UNORDERED_LIST,
                LINK,
                REF,
                IMAGE,
                HORIZONTAL_LINE,
                PARAGRAPH
            }

            #endregion Public Enums
        }

        #endregion Private Classes
    }
}