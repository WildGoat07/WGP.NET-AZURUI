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
        /// Vertice used for the lines.
        /// </summary>
        protected VertexArray _lines;

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
            _gradient = new VertexArray(PrimitiveType.Quads);
            _items = new ObservableCollection<Label>();
            _items.CollectionChanged += (sender, ev) =>
            {
                if (ev.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && ev.NewItems.Contains(null))
                    throw new ArgumentNullException("A radio label can't be set to null");
                if (ev.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace && ev.NewItems.Contains(null))
                    throw new ArgumentNullException("A radio label can't be set to null");
            };
            _lines = new VertexArray(PrimitiveType.Lines);
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
        public Collection<Label> Items => _items;

        /// <summary>
        /// The AABB of the widget without its position.
        /// </summary>
        public override FloatRect LocalBounds
        {
            get
            {
                var result = new FloatRect();
                foreach (var item in Items)
                {
                    result.Width = Utilities.Max(result.Width, item.GlobalBounds.Width);
                    result.Height += item.GlobalBounds.Height;
                }
                result.Width += 10;
                return result;
            }
        }

        /// <summary>
        /// Index of the item on which the mouse is clicking. -1 if no item is being clicked.
        /// </summary>
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
        /// Triggered when the selection has changed.
        /// </summary>
        public Action SelectionChanged { get; set; }

        #endregion Public Properties

        #region Protected Properties

        /// <summary>
        /// Items of the radiogroup.
        /// </summary>
        protected ObservableCollection<Label> _items { get; set; }

        #endregion Protected Properties

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
            foreach (var text in Items)
                text?.DrawOn(target, Position);
        }

        /// <summary>
        /// Updates the widget (graphics and events).
        /// </summary>
        /// <param name="app">Windows on which the widget is DIRECTLY drawn on.</param>
        public override void Update(RenderWindow app)
        {
            foreach (var item in _items)
                item.Update(app, Position);
            _gradient.Clear();
            _lines.Clear();
            int oldHover = HoveredOn;
            int oldPress = PressingOn;
            HoveredOn = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                var msPos = app.MapPixelToCoords(Mouse.GetPosition(app)) - Position;
                var box = Items[i].GlobalBounds;
                if (box.Contains(msPos))
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
                currPadPos = (int)Utilities.Interpolation(Utilities.Percent(PadChrono.ElapsedTime, Time.Zero, Time.FromSeconds(.5f)), oldPadPos, Items[SelectedIndex].Position.Y);
                _lines.Append(new Vertex(new Vector2f(0, 0), NewColor(Hue, .3f, .5f)));
                _lines.Append(new Vertex(new Vector2f(6, 0), NewColor(Hue, .3f, .5f)));

                _lines.Append(new Vertex(new Vector2f(7, 0), NewColor(Hue, .3f, .5f)));
                _lines.Append(new Vertex(new Vector2f(7, LocalBounds.Height), NewColor(Hue, .3f, .5f)));

                _lines.Append(new Vertex(new Vector2f(6, LocalBounds.Height + 1), NewColor(Hue, .3f, .5f)));
                _lines.Append(new Vertex(new Vector2f(0, LocalBounds.Height + 1), NewColor(Hue, .3f, .5f)));

                _lines.Append(new Vertex(new Vector2f(0, LocalBounds.Height), NewColor(Hue, .3f, .5f)));
                _lines.Append(new Vertex(new Vector2f(0, 0), NewColor(Hue, .3f, .5f)));

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
                    _gradient.Append(new Vertex(new Vector2f(0, 0), NewColor(Hue, S, .75f, A)));
                    _gradient.Append(new Vertex(new Vector2f(6, 0), NewColor(Hue, S, .75f, A)));
                    _gradient.Append(new Vertex(new Vector2f(6, LocalBounds.Height), NewColor(Hue, S, .75f, A)));
                    _gradient.Append(new Vertex(new Vector2f(0, LocalBounds.Height), NewColor(Hue, S, .75f, A)));
                    S = Utilities.Min(S, .5f);
                    _gradient.Append(new Vertex(new Vector2f(10, Items[HoveredOn].Position.Y), NewColor(Hue, S + .25f, .75f, (byte)Utilities.Min(255, A * 4))));
                    _gradient.Append(new Vertex(new Vector2f(10 + Items[HoveredOn].GlobalBounds.Width, Items[HoveredOn].Position.Y), NewColor(Hue, S + .25f, .75f, 0)));
                    _gradient.Append(new Vertex(new Vector2f(10 + Items[HoveredOn].GlobalBounds.Width, Items[HoveredOn].GlobalBounds.Bot()), NewColor(Hue, S + .25f, .75f, 0)));
                    _gradient.Append(new Vertex(new Vector2f(10, Items[HoveredOn].GlobalBounds.Bot()), NewColor(Hue, S + .25f, .75f, (byte)Utilities.Min(255, A * 4))));
                }

                if (SelectedIndex != -1)
                {
                    _gradient.Append(new Vertex(new Vector2f(0, 6 + currPadPos), NewColor(Hue, .8f, .8f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 6 + currPadPos), NewColor(Hue, .8f, .8f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 1 + currPadPos), NewColor(Hue, .42f, .8f)));
                    _gradient.Append(new Vertex(new Vector2f(0, 1 + currPadPos), NewColor(Hue, .42f, .8f)));

                    _lines.Append(new Vertex(new Vector2f(1, 1 + currPadPos), NewColor(Hue, .32f, .8f)));
                    _lines.Append(new Vertex(new Vector2f(5, 1 + currPadPos), NewColor(Hue, .32f, .8f)));

                    _gradient.Append(new Vertex(new Vector2f(0, 6 + currPadPos), NewColor(Hue, 1f, .65f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 6 + currPadPos), NewColor(Hue, 1f, .65f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 11 + currPadPos), NewColor(Hue - 20, 1, 1)));
                    _gradient.Append(new Vertex(new Vector2f(0, 11 + currPadPos), NewColor(Hue - 20, 1, 1)));

                    _lines.Append(new Vertex(new Vector2f(1, 12 + currPadPos), NewColor(Hue - 20, 1, 1)));
                    _lines.Append(new Vertex(new Vector2f(5, 12 + currPadPos), NewColor(Hue - 20, 1, 1)));

                    //light
                    float highBorder = Utilities.Max(1 + currPadPos - Engine.CharacterSize, 0);
                    float lowBorder = Utilities.Min(11 + currPadPos + Engine.CharacterSize, Items.Count * Engine.CharacterSize);
                    byte highAlpha = (byte)Utilities.Interpolation(Utilities.Percent(currPadPos, 0, Engine.CharacterSize), 255f, 0);
                    byte lowAlpha = (byte)Utilities.Interpolation(Utilities.Percent((Items.Count - 1) * Engine.CharacterSize - currPadPos, 0, Engine.CharacterSize), 255f, 0);
                    _gradient.Append(new Vertex(new Vector2f(0, 1 + currPadPos), NewColor(Hue, .75f, .75f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 1 + currPadPos), NewColor(Hue, .75f, .75f)));
                    _gradient.Append(new Vertex(new Vector2f(6, highBorder), NewColor(Hue, .75f, .75f, highAlpha)));
                    _gradient.Append(new Vertex(new Vector2f(0, highBorder), NewColor(Hue, .75f, .75f, highAlpha)));

                    _gradient.Append(new Vertex(new Vector2f(0, 11 + currPadPos), NewColor(Hue, .75f, .75f)));
                    _gradient.Append(new Vertex(new Vector2f(6, 11 + currPadPos), NewColor(Hue, .75f, .75f)));
                    _gradient.Append(new Vertex(new Vector2f(6, lowBorder), NewColor(Hue, .75f, .75f, lowAlpha)));
                    _gradient.Append(new Vertex(new Vector2f(0, lowBorder), NewColor(Hue, .75f, .75f, lowAlpha)));

                    int decal = 0;
                    foreach (var item in Items)
                    {
                        item.Update(app);
                        item.Position = new Vector2f(10, decal);
                        decal += (int)item.GlobalBounds.Height;
                    }
                }
            }
        }

        #endregion Public Methods
    }
}