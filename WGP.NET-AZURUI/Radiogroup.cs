using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace WGP.AzurUI
{
    public class Radiogroup : Widget
    {
        #region Protected Fields

        protected VertexArray _gradient;
        protected ObservableCollection<object> _items;
        protected VertexArray _lines;
        protected List<Text> _texts;

        #endregion Protected Fields

        #region Private Fields

        private bool requireUpdate;

        #endregion Private Fields

        #region Public Constructors

        public Radiogroup() : base()
        {
            requireUpdate = false;
            _gradient = new VertexArray(PrimitiveType.Quads);
            _items = new ObservableCollection<object>();
            _items.CollectionChanged += (sender, e) => UpdateTexts();
            _lines = new VertexArray(PrimitiveType.Lines);
            _texts = new List<Text>();
        }

        #endregion Public Constructors

        #region Public Properties

        public Collection<object> Items => _items;
        public override FloatRect LocalBounds => throw new NotImplementedException();

        public int SelectedIndex { get; set; }

        public object SelectedItem
        {
            get => Items[SelectedIndex];
            set
            {
                var index = Items.IndexOf(value);
                if (index != -1)
                    SelectedIndex = index;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void DrawOn(RenderTarget target)
        {
            Transform tr = Transform.Identity;
            tr.Translate(Position);
            target.Draw(_gradient, new RenderStates(tr));
            target.Draw(_lines, new RenderStates(tr));
            foreach (var text in _texts)
                target.Draw(text, new RenderStates(tr));
        }

        public override void Update(RenderWindow app)
        {
            if (requireUpdate)
                UpdateTexts();
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateTexts()
        {
            requireUpdate = false;
            _texts.Clear();
            foreach (var item in _items)
            {
                var tmp = new Text(item.ToString(), Engine.BaseFont, Engine.CharacterSize) { FillColor = Engine.BaseFontColor };
                _texts.Add(tmp);
            }
        }

        #endregion Private Methods
    }
}