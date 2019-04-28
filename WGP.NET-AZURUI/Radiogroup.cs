using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace WGP.AzurUI
{
    /// <summary>
    /// List of checkboxes with only one choice.
    /// </summary>
    public class Radiogroup : Widget
    {
        #region Protected Fields

        /// <summary>
        /// Vertice used for the gradient.
        /// </summary>
        protected VertexArray _gradient;

        /// <summary>
        /// Internal list of the items.
        /// </summary>
        protected ObservableCollection<object> _items;

        /// <summary>
        /// Vertice used for the lines.
        /// </summary>
        protected VertexArray _lines;

        /// <summary>
        /// List of the texts to display.
        /// </summary>
        protected List<Text> _texts;

        /// <summary>
        /// Chronometer for the pad moving animation.
        /// </summary>
        protected Chronometer PadChrono;

        #endregion Protected Fields

        #region Private Fields

        private int _selectedIndex;
        private float currPadPos;
        private bool oldMouseState;
        private float oldPadPos;
        private bool requireUpdate;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Radiogroup() : base()
        {
            PadChrono = new Chronometer(Engine.Chronometer);
            _selectedIndex = -1;
            PressingOn = -1;
            HoveredOn = -1;
            requireUpdate = false;
            _gradient = new VertexArray(PrimitiveType.Quads);
            _items = new ObservableCollection<object>();
            _items.CollectionChanged += (sender, e) => UpdateTexts();
            _lines = new VertexArray(PrimitiveType.Lines);
            _texts = new List<Text>();
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Index of the item hovered by the mouse. -1 if no item is hovered.
        /// </summary>
        public int HoveredOn { get; protected set; }

        /// <summary>
        /// Items of the radiogroup.
        /// </summary>
        public Collection<object> Items => _items;
        public override FloatRect LocalBounds => throw new NotImplementedException();
        public int PressingOn { get; protected set; }

        /// <summary>
        /// The index of the selected item.
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    oldPadPos = currPadPos;
                    PadChrono.Restart();
                    if (_selectedIndex == -1)
                        oldPadPos = value * Engine.CharacterSize;
                    _selectedIndex = value;
                    SelectionChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// The currently selected item.
        /// </summary>
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

        /// <summary>
        /// Triggered when the selection has changed.
        /// </summary>
        public Action SelectionChanged { get; set; }

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
            foreach (var text in _texts)
                target.Draw(text, new RenderStates(tr));
        }

        /// <summary>
        /// Updates the widget (graphics and events).
        /// </summary>
        /// <param name="app">Windows on which the widget is DIRECTLY drawn on.</param>
        public override void Update(RenderWindow app)
        {
            if (requireUpdate)
                UpdateTexts();
            _gradient.Clear();
            _lines.Clear();
            int oldHover = HoveredOn;
            int oldPress = PressingOn;
            HoveredOn = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                var msRelativePos = app.MapPixelToCoords(Mouse.GetPosition(app)) - Position;
                var box = new FloatRect(0, i * Engine.CharacterSize, 10 + _texts[i].GetLocalBounds().Width, Engine.CharacterSize);
                if (box.Contains(msRelativePos))
                    HoveredOn = i;
            }
            for (int i = 0; i < Items.Count; i++)
            {
                if (oldMouseState != Mouse.IsButtonPressed(Mouse.Button.Left))
                {
                    if (PressingOn == i && oldMouseState)
                    {
                        SelectedIndex = i;
                        PressingOn = -1;
                    }
                    if (HoveredOn == i && !oldMouseState)
                        PressingOn = i;
                }
            }
            if (oldHover != HoveredOn)
                PressingOn = -1;
            if (((HoveredOn == -1 && oldHover != -1) || (HoveredOn != -1 && oldHover == -1)) || (oldPress != PressingOn && PressingOn != -1))
                _chronometer.Restart();
            oldMouseState = Mouse.IsButtonPressed(Mouse.Button.Left);
            if (!Enabled)
            {
                HoveredOn = -1;
                PressingOn = -1;
            }
            if (Items.Count > 0)
            {
                currPadPos = (int)Utilities.Interpolation(Utilities.Percent(PadChrono.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), oldPadPos, (SelectedIndex * Engine.CharacterSize));
                _lines.Append(new Vertex(new Vector2f(0, 0), new HSVColor(Hue, .3f, .5f)));
                _lines.Append(new Vertex(new Vector2f(6, 0), new HSVColor(Hue, .3f, .5f)));

                _lines.Append(new Vertex(new Vector2f(7, 0), new HSVColor(Hue, .3f, .5f)));
                _lines.Append(new Vertex(new Vector2f(7, Engine.CharacterSize * Items.Count), new HSVColor(Hue, .3f, .5f)));

                _lines.Append(new Vertex(new Vector2f(6, Engine.CharacterSize * Items.Count + 1), new HSVColor(Hue, .3f, .5f)));
                _lines.Append(new Vertex(new Vector2f(0, Engine.CharacterSize * Items.Count + 1), new HSVColor(Hue, .3f, .5f)));

                _lines.Append(new Vertex(new Vector2f(0, Engine.CharacterSize * Items.Count), new HSVColor(Hue, .3f, .5f)));
                _lines.Append(new Vertex(new Vector2f(0, 0), new HSVColor(Hue, .3f, .5f)));

                if (HoveredOn != -1)
                {
                    byte A;
                    float S;
                    if (PressingOn != -1)
                    {
                        A = (byte)Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(1)), 64f, 200);
                        S = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(1)), .5f, .85f);
                    }
                    else
                    {
                        A = (byte)Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), 0f, 64);
                        S = Utilities.Interpolation(Utilities.Percent(_chronometer.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), 0, .5f);
                    }
                    _gradient.Append(new Vertex(new Vector2f(0, 0), new HSVColor(Hue, S, .75f, A)));
                    _gradient.Append(new Vertex(new Vector2f(6, 0), new HSVColor(Hue, S, .75f, A)));
                    _gradient.Append(new Vertex(new Vector2f(6, Engine.CharacterSize * Items.Count), new HSVColor(Hue, S, .75f, A)));
                    _gradient.Append(new Vertex(new Vector2f(0, Engine.CharacterSize * Items.Count), new HSVColor(Hue, S, .75f, A)));
                }

                if (SelectedIndex != -1)
                {
                    _gradient.Append(new Vertex(new Vector2f(0, 6 + currPadPos), new HSVColor(Hue, .8f, .8f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 6 + currPadPos), new HSVColor(Hue, .8f, .8f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 1 + currPadPos), new HSVColor(Hue, .42f, .8f)));
                    _gradient.Append(new Vertex(new Vector2f(0, 1 + currPadPos), new HSVColor(Hue, .42f, .8f)));

                    _lines.Append(new Vertex(new Vector2f(1, 1 + currPadPos), new HSVColor(Hue, .32f, .8f)));
                    _lines.Append(new Vertex(new Vector2f(5, 1 + currPadPos), new HSVColor(Hue, .32f, .8f)));

                    _gradient.Append(new Vertex(new Vector2f(0, 6 + currPadPos), new HSVColor(Hue, 1f, .65f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 6 + currPadPos), new HSVColor(Hue, 1f, .65f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 11 + currPadPos), new HSVColor(Hue - 20, 1, 1)));
                    _gradient.Append(new Vertex(new Vector2f(0, 11 + currPadPos), new HSVColor(Hue - 20, 1, 1)));

                    _lines.Append(new Vertex(new Vector2f(1, 12 + currPadPos), new HSVColor(Hue - 20, 1, 1)));
                    _lines.Append(new Vertex(new Vector2f(5, 12 + currPadPos), new HSVColor(Hue - 20, 1, 1)));

                    //light
                    float min = 1, max = Items.Count * Engine.CharacterSize;
                    float highBorder = Utilities.Max(1 + currPadPos - Engine.CharacterSize, 0);
                    float lowBorder = Utilities.Min(11 + currPadPos + Engine.CharacterSize, Items.Count * Engine.CharacterSize);
                    byte highAlpha = (byte)Utilities.Interpolation(Utilities.Percent(currPadPos, 0, Engine.CharacterSize), 255f, 0);
                    byte lowAlpha = (byte)Utilities.Interpolation(Utilities.Percent((Items.Count - 1) * Engine.CharacterSize - currPadPos, 0, Engine.CharacterSize), 255f, 0);
                    _gradient.Append(new Vertex(new Vector2f(0, 1 + currPadPos), new HSVColor(Hue, .75f, .75f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 1 + currPadPos), new HSVColor(Hue, .75f, .75f)));
                    _gradient.Append(new Vertex(new Vector2f(6, highBorder), new HSVColor(Hue, .75f, .75f, highAlpha)));
                    _gradient.Append(new Vertex(new Vector2f(0, highBorder), new HSVColor(Hue, .75f, .75f, highAlpha)));

                    _gradient.Append(new Vertex(new Vector2f(0, 11 + currPadPos), new HSVColor(Hue, .75f, .75f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 11 + currPadPos), new HSVColor(Hue, .75f, .75f)));
                    _gradient.Append(new Vertex(new Vector2f(6, lowBorder), new HSVColor(Hue, .75f, .75f, lowAlpha)));
                    _gradient.Append(new Vertex(new Vector2f(0, lowBorder), new HSVColor(Hue, .75f, .75f, lowAlpha)));
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateTexts()
        {
            requireUpdate = false;
            _texts.Clear();
            int decal = 0;
            foreach (var item in _items)
            {
                var tmp = new Text(item.ToString(), Engine.BaseFont, Engine.CharacterSize)
                {
                    FillColor = Engine.BaseFontColor,
                    Position = new Vector2f(10, decal * Engine.CharacterSize)
                };
                _texts.Add(tmp);
                decal++;
            }
        }

        #endregion Private Methods
    }
}