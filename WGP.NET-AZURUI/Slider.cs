using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;

namespace WGP.AzurUI
{
    public class Slider : Widget
    {
        #region Protected Fields

        /// <summary>
        /// Vertice of the downside part of the pad.
        /// </summary>
        protected VertexArray DownPadVertice;

        /// <summary>
        /// Vertice used by the light of the pad 1.
        /// </summary>
        protected VertexArray Light1;

        /// <summary>
        /// Vertice used by the light of the pad 2.
        /// </summary>
        protected VertexArray Light2;

        /// <summary>
        /// Vertice used by the lines.
        /// </summary>
        protected VertexArray Lines;

        /// <summary>
        /// Shape of the pad 1.
        /// </summary>
        protected CircleShape Pad1;

        /// <summary>
        /// Shape of the pad 2.
        /// </summary>
        protected CircleShape Pad2;

        /// <summary>
        /// Texture of the pad.
        /// </summary>
        protected RenderTexture PadRenderer;

        /// <summary>
        /// List of the displayed texts.
        /// </summary>
        protected List<Text> Texts;

        /// <summary>
        /// Vertice of the upside part of the pad.
        /// </summary>
        protected VertexArray UpPadVertice;

        #endregion Protected Fields

        #region Private Fields

        private int _divisions;
        private float _maximum;
        private float _minimum;
        private int _subDivision;
        private TickType _tickConfig;
        private float _value1;
        private float _value2;
        private bool oldMouseState;
        private bool requireUpdate;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Slider() : base()
        {
            ValueChanged = null;
            Light1 = new VertexArray(PrimitiveType.TriangleFan);
            Light2 = new VertexArray(PrimitiveType.TriangleFan);
            Lines = new VertexArray(PrimitiveType.Lines);
            UpPadVertice = new VertexArray(PrimitiveType.TriangleFan);
            DownPadVertice = new VertexArray(PrimitiveType.Quads);
            Pad2 = new CircleShape(6.5f);
            Pad1 = new CircleShape(6.5f);
            Pad1.Origin = new Vector2f(7, 7);
            Pad2.Origin = new Vector2f(7, 7);
            PadRenderer = new RenderTexture(13, 13);
            Pad1.Texture = PadRenderer.Texture;
            Pad2.Texture = PadRenderer.Texture;
            Texts = new List<Text>();
            DoublePad = false;
            Minimum = 0;
            Maximum = 100;
            Range = Tuple.Create(0f, 0f);
            TickConfig = TickType.DEFAULT;
            Step = 0;
        }

        #endregion Public Constructors

        #region Public Enums

        /// <summary>
        /// How the side ticks of the slider will be displayed.
        /// </summary>
        [Flags]
        public enum TickType
        {
            /// <summary>
            /// Nothing is diplayed.
            /// </summary>
            NONE = 0,

            /// <summary>
            /// The top is displayed if horizontally drawn, left is displayed if vertically drawn.
            /// </summary>
            TOP_LEFT = 1,

            /// <summary>
            /// The bot is displayed if horizontally drawn, right is displayed if vertically drawn.
            /// </summary>
            BOT_RIGHT = 2,

            /// <summary>
            /// The numbers will be displayed every division.
            /// </summary>
            SHOW_TEXT = 4,

            /// <summary>
            /// Both sides of the slider will be ticked.
            /// </summary>
            BOTH_WITHOUT_TEXT = TOP_LEFT | BOT_RIGHT,

            /// <summary>
            /// Both sides of the slider will be ticked and numbered.
            /// </summary>
            ALL = SHOW_TEXT | BOTH_WITHOUT_TEXT,

            /// <summary>
            /// Only top/left will be ticked and numbered by default.
            /// </summary>
            DEFAULT = TOP_LEFT | SHOW_TEXT
        }

        #endregion Public Enums

        #region Public Properties

        /// <summary>
        /// Number of divisions between the min and max values.
        /// </summary>
        public int Divisions
        {
            get => _divisions;
            set
            {
                if (value != _divisions)
                {
                    _divisions = value;
                    requireUpdate = true;
                }
            }
        }

        /// <summary>
        /// True if the slider has two pads for range picking.
        /// </summary>
        public bool DoublePad { get; set; }

        /// <summary>
        /// True if the mouse hovers the widget.
        /// </summary>
        public bool Hovered { get; protected set; }

        /// <summary>
        /// The AABB of the widget without its position.
        /// </summary>
        public override FloatRect LocalBounds
        {
            get
            {
                if (Orientation == Engine.Orientation.VERTICAL)
                {
                    var pts = new List<Vector2f>();
                    foreach (var item in Texts)
                    {
                        pts.Add(item.GetGlobalBounds().TopLeft());
                        pts.Add(item.GetGlobalBounds().BotRight());
                    }
                    pts.Add(new Vector2f(-7, 0));
                    pts.Add(new Vector2f(6, 0));
                    pts.Add(new Vector2f(0, -Size));
                    if ((TickConfig & TickType.BOT_RIGHT) == TickType.BOT_RIGHT)
                        pts.Add(new Vector2f(15, 0));
                    if ((TickConfig & TickType.TOP_LEFT) == TickType.TOP_LEFT)
                        pts.Add(new Vector2f(-16, 0));
                    return Utilities.CreateRect(pts);
                }
                else
                {
                    var pts = new List<Vector2f>();
                    foreach (var item in Texts)
                    {
                        pts.Add(item.GetGlobalBounds().TopLeft());
                        pts.Add(item.GetGlobalBounds().BotRight());
                    }
                    pts.Add(new Vector2f(0, -7));
                    pts.Add(new Vector2f(0, 6));
                    pts.Add(new Vector2f(Size, 0));
                    if ((TickConfig & TickType.BOT_RIGHT) == TickType.BOT_RIGHT)
                        pts.Add(new Vector2f(0, 15));
                    if ((TickConfig & TickType.TOP_LEFT) == TickType.TOP_LEFT)
                        pts.Add(new Vector2f(0, -16));
                    return Utilities.CreateRect(pts);
                }
            }
        }

        /// <summary>
        /// Maximum value of the slider.
        /// </summary>
        public float Maximum
        {
            get => _maximum;
            set
            {
                if (value != _maximum)
                {
                    _maximum = value;
                    _value1.Capped(Minimum, Maximum);
                    _value2.Capped(Minimum, Maximum);
                    requireUpdate = true;
                }
            }
        }

        /// <summary>
        /// Minimum value of the slider.
        /// </summary>
        public float Minimum
        {
            get => _minimum;
            set
            {
                if (value != _minimum)
                {
                    _minimum = value;
                    _value1.Capped(Minimum, Maximum);
                    _value2.Capped(Minimum, Maximum);
                    requireUpdate = true;
                }
            }
        }

        /// <summary>
        /// The number formatting when calling Single.ToString(format)
        /// </summary>
        public string NumberFormat { get; set; }

        /// <summary>
        /// The orientation of the slider.
        /// </summary>
        public Engine.Orientation Orientation { get; set; }

        /// <summary>
        /// index of the pad being clicked (1 for the pad 1, 2 for the pad 2). 0 if nothing is being clicked.
        /// </summary>
        public int Pressing { get; protected set; }

        /// <summary>
        /// Range value of the double pad slider.
        /// </summary>
        public Tuple<float, float> Range
        {
            get => Tuple.Create(Utilities.Min(_value1, _value2), Utilities.Max(_value1, _value2));
            set
            {
                _value1 = value.Item1.Capped(Minimum, Maximum);
                _value2 = value.Item2.Capped(Minimum, Maximum);
            }
        }

        /// <summary>
        /// Length of the slider in pixels.
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// Step of the changed value when the user changes the value.
        /// </summary>
        public float Step { get; set; }

        /// <summary>
        /// Number of subdivisions between each division.
        /// </summary>
        public int SubDivisions
        {
            get => _subDivision;
            set
            {
                if (value != _subDivision)
                {
                    _subDivision = value;
                    requireUpdate = true;
                }
            }
        }

        /// <summary>
        /// Configuration of the way the ticks are displayed.
        /// </summary>
        public TickType TickConfig
        {
            get => _tickConfig;
            set
            {
                if (value != _tickConfig)
                {
                    _tickConfig = value;
                    requireUpdate = true;
                }
            }
        }

        /// <summary>
        /// The value of the single pad slider.
        /// </summary>
        public float Value { get => _value1; set => _value1 = value.Capped(Minimum, Maximum); }

        /// <summary>
        /// Triggered when the value/range is changed.
        /// </summary>
        public Action ValueChanged { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Draws the widget on the target.
        /// </summary>
        /// The widget should be moved according to its Position when inherited.
        /// <param name="target">Target to draw the widget on.</param>
        public override void DrawOn(RenderTarget target)
        {
            PadRenderer.Clear(new HSVColor(Hue, .2f, 1));
            PadRenderer.Draw(UpPadVertice);
            PadRenderer.Draw(DownPadVertice);
            PadRenderer.Display();
            Transform tr = Transform.Identity;
            tr.Translate(Position);
            target.Draw(Lines, new RenderStates(tr));
            target.Draw(Light1, new RenderStates(tr));
            target.Draw(Light2, new RenderStates(tr));
            target.Draw(Pad1, new RenderStates(tr));
            if (DoublePad)
                target.Draw(Pad2, new RenderStates(tr));
            foreach (var item in Texts)
                target.Draw(item, new RenderStates(tr));
        }

        /// <summary>
        /// Updates the widget (graphics and events).
        /// </summary>
        /// <param name="app">Windows on which the widget is DIRECTLY drawn on.</param>
        public override void Update(RenderWindow app)
        {
            if (requireUpdate)
                UpdateTexts();
            bool oldHover = Hovered;
            int oldPress = Pressing;
            Hovered = GlobalBounds.Contains(app.MapPixelToCoords(Mouse.GetPosition(app)));
            float decal;
            if (Orientation == Engine.Orientation.HORIZONTAL)
                decal = app.MapPixelToCoords(Mouse.GetPosition(app)).X - Position.X;
            else
                decal = app.MapPixelToCoords(Mouse.GetPosition(app)).Y - Position.Y;
            if (oldMouseState != Mouse.IsButtonPressed(Mouse.Button.Left))
            {
                oldMouseState = Mouse.IsButtonPressed(Mouse.Button.Left);
                if (oldMouseState && Hovered)
                {
                    if (DoublePad)
                    {
                        if (Orientation == Engine.Orientation.HORIZONTAL)
                            Pressing = (decal - Pad1.Position.X).Abs() <= (decal - Pad2.Position.X).Abs() ? 1 : 2;
                        else
                            Pressing = (decal - Pad1.Position.Y).Abs() <= (decal - Pad2.Position.Y).Abs() ? 1 : 2;
                    }
                    else
                        Pressing = 1;
                }
                else
                {
                    Pressing = 0;
                    if (oldPress != 0)
                        ValueChanged?.Invoke();
                }
            }
            if (oldHover != Hovered || (oldPress == 0 && Pressing != 0))
                _chronometer.Restart();
            int iSize = (int)Size;
            Lines.Clear();
            DownPadVertice.Clear();
            UpPadVertice.Clear();
            Light1.Clear();
            Light2.Clear();
            if (Pressing == 1)
            {
                if (Orientation == Engine.Orientation.HORIZONTAL)
                {
                    var perc = Utilities.Percent(app.MapPixelToCoords(Mouse.GetPosition(app)).X - Position.X, 0, Size);
                    _value1 = Utilities.Interpolation(perc, Minimum, Maximum);
                    if (Step > 0)
                    {
                        var fac = _value1 / Step;
                        fac = (float)Math.Round(fac);
                        _value1 = fac * Step;
                    }
                }
                else if (Orientation == Engine.Orientation.VERTICAL)
                {
                    var perc = Utilities.Percent(app.MapPixelToCoords(Mouse.GetPosition(app)).Y - Position.Y, 0, -Size);
                    _value1 = Utilities.Interpolation(perc, Minimum, Maximum);
                    if (Step > 0)
                    {
                        var fac = _value1 / Step;
                        fac = (float)Math.Round(fac);
                        _value1 = fac * Step;
                    }
                }
            }
            else if (Pressing == 2)
            {
                if (Orientation == Engine.Orientation.HORIZONTAL)
                {
                    var perc = Utilities.Percent(app.MapPixelToCoords(Mouse.GetPosition(app)).X - Position.X, 0, Size);
                    _value2 = Utilities.Interpolation(perc, Minimum, Maximum);
                    if (Step > 0)
                    {
                        var fac = _value2 / Step;
                        fac = (float)Math.Round(fac);
                        _value2 = fac * Step;
                    }
                }
                else if (Orientation == Engine.Orientation.VERTICAL)
                {
                    var perc = Utilities.Percent(app.MapPixelToCoords(Mouse.GetPosition(app)).Y - Position.Y, 0, -Size);
                    _value2 = Utilities.Interpolation(perc, Minimum, Maximum);
                    if (Step > 0)
                    {
                        var fac = _value2 / Step;
                        fac = (float)Math.Round(fac);
                        _value2 = fac * Step;
                    }
                }
            }
            if (Orientation == Engine.Orientation.HORIZONTAL)
            {
                Lines.Append(new Vertex(new Vector2f(), Color.Black));
                Lines.Append(new Vertex(new Vector2f(iSize, 0), Color.Black));

                Lines.Append(new Vertex(new Vector2f(0, -1), new HSVColor(Hue, .25f, .25f)));
                Lines.Append(new Vertex(new Vector2f(iSize, -1), new HSVColor(Hue, .25f, .25f)));

                Lines.Append(new Vertex(new Vector2f(0, 1), new HSVColor(Hue, .25f, .5f)));
                Lines.Append(new Vertex(new Vector2f(iSize, 1), new HSVColor(Hue, .25f, .5f)));

                int pos1, pos2;
                pos1 = (int)(Utilities.Percent(_value1, Minimum, Maximum) * iSize);
                if (DoublePad)
                    pos2 = (int)(Utilities.Percent(_value2, Minimum, Maximum) * iSize);
                else
                    pos2 = 0;

                Lines.Append(new Vertex(new Vector2f(pos1, -2), new HSVColor(Hue, .75f, 1, 128)));
                Lines.Append(new Vertex(new Vector2f(pos2, -2), new HSVColor(Hue, .75f, 1, 128)));

                Lines.Append(new Vertex(new Vector2f(pos1, 2), new HSVColor(Hue, .75f, 1, 128)));
                Lines.Append(new Vertex(new Vector2f(pos2, 2), new HSVColor(Hue, .75f, 1, 128)));

                if (Pressing == 0)
                {
                    Lines.Append(new Vertex(new Vector2f(pos1, 0), new HSVColor(Hue, .75f, .75f)));
                    Lines.Append(new Vertex(new Vector2f(pos2, 0), new HSVColor(Hue, .75f, .75f)));

                    Lines.Append(new Vertex(new Vector2f(pos1, -1), new HSVColor(Hue, .35f, .85f)));
                    Lines.Append(new Vertex(new Vector2f(pos2, -1), new HSVColor(Hue, .35f, .85f)));

                    Lines.Append(new Vertex(new Vector2f(pos1, 1), new HSVColor(Hue, .35f, .85f)));
                    Lines.Append(new Vertex(new Vector2f(pos2, 1), new HSVColor(Hue, .35f, .85f)));
                }
                else
                {
                    Lines.Append(new Vertex(new Vector2f(pos1, 0), new HSVColor(Hue, .2f, 1)));
                    Lines.Append(new Vertex(new Vector2f(pos2, 0), new HSVColor(Hue, .2f, 1)));

                    Lines.Append(new Vertex(new Vector2f(pos1, -1), new HSVColor(Hue, .6f, 1)));
                    Lines.Append(new Vertex(new Vector2f(pos2, -1), new HSVColor(Hue, .6f, 1)));

                    Lines.Append(new Vertex(new Vector2f(pos1, 1), new HSVColor(Hue, .6f, 1)));
                    Lines.Append(new Vertex(new Vector2f(pos2, 1), new HSVColor(Hue, .6f, 1)));
                }
                if (Hovered && Pressing == 0)
                {
                    Light1.Append(new Vertex(new Vector2f(pos1 - .5f, -.5f), new HSVColor(Hue, .6f, 1)));
                    for (int i = 0; i <= 20; i++)
                        Light1.Append(new Vertex(new Vector2f(pos1 - .5f, -.5f) + (Angle.Loop * i / 20).GenerateVector(12), new HSVColor(Hue, .6f, 1, 0)));
                    if (DoublePad)
                    {
                        Light2.Append(new Vertex(new Vector2f(pos2 - .5f, -.5f), new HSVColor(Hue, .6f, 1)));
                        for (int i = 0; i <= 20; i++)
                            Light2.Append(new Vertex(new Vector2f(pos2 - .5f, -.5f) + (Angle.Loop * i / 20).GenerateVector(12), new HSVColor(Hue, .6f, 1, 0)));
                    }
                }
                if (Pressing == 1)
                {
                    Light1.Append(new Vertex(new Vector2f(pos1 - .5f, -.5f), new HSVColor(Hue, .6f, 1)));
                    for (int i = 0; i <= 20; i++)
                        Light1.Append(new Vertex(new Vector2f(pos1 - .5f, -.5f) + (Angle.Loop * i / 20).GenerateVector(12), new HSVColor(Hue, .6f, 1, 0)));
                }
                else if (Pressing == 2)
                {
                    Light2.Append(new Vertex(new Vector2f(pos2 - .5f, -.5f), new HSVColor(Hue, .6f, 1)));
                    for (int i = 0; i <= 20; i++)
                        Light2.Append(new Vertex(new Vector2f(pos2 - .5f, -.5f) + (Angle.Loop * i / 20).GenerateVector(12), new HSVColor(Hue, .6f, 1, 0)));
                }
                Pad1.Position = new Vector2f(pos1, 0);
                Pad2.Position = new Vector2f(pos2, 0);

                UpPadVertice.Append(new Vertex(new Vector2f(6.5f, 6.5f), new HSVColor(Hue, .6f, .65f)));
                for (int i = 0; i <= 10; i++)
                    UpPadVertice.Append(new Vertex(new Vector2f(6.5f, 6.5f) + (Angle.Loop * i / 20 + Angle.Loop / 2).GenerateVector(7), new HSVColor(Hue, .2f, .95f)));
                DownPadVertice.Append(new Vertex(new Vector2f(0, 6.5f), new HSVColor(Hue, 1, .65f)));
                DownPadVertice.Append(new Vertex(new Vector2f(13, 6.5f), new HSVColor(Hue, 1, .65f)));
                DownPadVertice.Append(new Vertex(new Vector2f(13, 13), new HSVColor(Hue - 20, .75f, .95f)));
                DownPadVertice.Append(new Vertex(new Vector2f(0, 13), new HSVColor(Hue - 20, .75f, .95f)));

                if ((TickConfig & TickType.BOT_RIGHT) == TickType.BOT_RIGHT)
                {
                    Lines.Append(new Vertex(new Vector2f(0, 7), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(0, 15), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(iSize, 7), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(iSize, 15), Engine.BaseFontColor));
                    for (int j = 1; j <= SubDivisions; j++)
                    {
                        int subPos = (int)Utilities.Interpolation(Utilities.Percent(j, 0f, SubDivisions + 1), 0f, iSize / (Divisions + 1));
                        Lines.Append(new Vertex(new Vector2f(subPos, 10), Engine.BaseFontColor));
                        Lines.Append(new Vertex(new Vector2f(subPos, 15), Engine.BaseFontColor));
                    }
                    for (int i = 1; i <= Divisions; i++)
                    {
                        int pos = (int)Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), 0f, iSize);
                        Lines.Append(new Vertex(new Vector2f(pos, 7), Engine.BaseFontColor));
                        Lines.Append(new Vertex(new Vector2f(pos, 15), Engine.BaseFontColor));
                        for (int j = 1; j <= SubDivisions; j++)
                        {
                            int subPos = (int)Utilities.Interpolation(Utilities.Percent(j, 0f, SubDivisions + 1), 0f, iSize / (Divisions + 1)) + pos;
                            Lines.Append(new Vertex(new Vector2f(subPos, 10), Engine.BaseFontColor));
                            Lines.Append(new Vertex(new Vector2f(subPos, 15), Engine.BaseFontColor));
                        }
                    }
                }
                if ((TickConfig & TickType.TOP_LEFT) == TickType.TOP_LEFT)
                {
                    Lines.Append(new Vertex(new Vector2f(0, -8), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(0, -16), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(iSize, -8), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(iSize, -16), Engine.BaseFontColor));
                    for (int j = 1; j <= SubDivisions; j++)
                    {
                        int subPos = (int)Utilities.Interpolation(Utilities.Percent(j, 0f, SubDivisions + 1), 0f, iSize / (Divisions + 1));
                        Lines.Append(new Vertex(new Vector2f(subPos, -11), Engine.BaseFontColor));
                        Lines.Append(new Vertex(new Vector2f(subPos, -16), Engine.BaseFontColor));
                    }
                    for (int i = 1; i <= Divisions; i++)
                    {
                        int pos = (int)Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), 0f, iSize);
                        Lines.Append(new Vertex(new Vector2f(pos, -8), Engine.BaseFontColor));
                        Lines.Append(new Vertex(new Vector2f(pos, -16), Engine.BaseFontColor));
                        for (int j = 1; j <= SubDivisions; j++)
                        {
                            int subPos = (int)Utilities.Interpolation(Utilities.Percent(j, 0f, SubDivisions + 1), 0f, iSize / (Divisions + 1)) + pos;
                            Lines.Append(new Vertex(new Vector2f(subPos, -11), Engine.BaseFontColor));
                            Lines.Append(new Vertex(new Vector2f(subPos, -16), Engine.BaseFontColor));
                        }
                    }
                }
            }
            else if (Orientation == Engine.Orientation.VERTICAL)
            {
                Lines.Append(new Vertex(new Vector2f(), Color.Black));
                Lines.Append(new Vertex(new Vector2f(0, -iSize), Color.Black));

                Lines.Append(new Vertex(new Vector2f(-1, 0), new HSVColor(Hue, .25f, .25f)));
                Lines.Append(new Vertex(new Vector2f(-1, -iSize), new HSVColor(Hue, .25f, .25f)));

                Lines.Append(new Vertex(new Vector2f(1, 0), new HSVColor(Hue, .25f, .5f)));
                Lines.Append(new Vertex(new Vector2f(1, -iSize), new HSVColor(Hue, .25f, .5f)));

                int pos1, pos2;
                pos1 = -(int)(Utilities.Percent(_value1, Minimum, Maximum) * iSize);
                if (DoublePad)
                    pos2 = -(int)(Utilities.Percent(_value2, Minimum, Maximum) * iSize);
                else
                    pos2 = 0;

                Lines.Append(new Vertex(new Vector2f(-2, pos1), new HSVColor(Hue, .75f, 1, 128)));
                Lines.Append(new Vertex(new Vector2f(-2, pos2), new HSVColor(Hue, .75f, 1, 128)));

                Lines.Append(new Vertex(new Vector2f(2, pos1), new HSVColor(Hue, .75f, 1, 128)));
                Lines.Append(new Vertex(new Vector2f(2, pos2), new HSVColor(Hue, .75f, 1, 128)));

                if (Pressing == 0)
                {
                    Lines.Append(new Vertex(new Vector2f(0, pos1), new HSVColor(Hue, .75f, .75f)));
                    Lines.Append(new Vertex(new Vector2f(0, pos2), new HSVColor(Hue, .75f, .75f)));

                    Lines.Append(new Vertex(new Vector2f(-1, pos1), new HSVColor(Hue, .35f, .85f)));
                    Lines.Append(new Vertex(new Vector2f(-1, pos2), new HSVColor(Hue, .35f, .85f)));

                    Lines.Append(new Vertex(new Vector2f(1, pos1), new HSVColor(Hue, .35f, .85f)));
                    Lines.Append(new Vertex(new Vector2f(1, pos2), new HSVColor(Hue, .35f, .85f)));
                }
                else
                {
                    Lines.Append(new Vertex(new Vector2f(0, pos1), new HSVColor(Hue, .2f, 1)));
                    Lines.Append(new Vertex(new Vector2f(0, pos2), new HSVColor(Hue, .2f, 1)));

                    Lines.Append(new Vertex(new Vector2f(-1, pos1), new HSVColor(Hue, .6f, 1)));
                    Lines.Append(new Vertex(new Vector2f(-1, pos2), new HSVColor(Hue, .6f, 1)));

                    Lines.Append(new Vertex(new Vector2f(1, pos1), new HSVColor(Hue, .6f, 1)));
                    Lines.Append(new Vertex(new Vector2f(1, pos2), new HSVColor(Hue, .6f, 1)));
                }
                if (Hovered && Pressing == 0)
                {
                    Light1.Append(new Vertex(new Vector2f(-.5f, pos1 - .5f), new HSVColor(Hue, .6f, 1)));
                    for (int i = 0; i <= 20; i++)
                        Light1.Append(new Vertex(new Vector2f(-.5f, pos1 - .5f) + (Angle.Loop * i / 20).GenerateVector(12), new HSVColor(Hue, .6f, 1, 0)));
                    if (DoublePad)
                    {
                        Light2.Append(new Vertex(new Vector2f(-.5f, pos2 - .5f), new HSVColor(Hue, .6f, 1)));
                        for (int i = 0; i <= 20; i++)
                            Light2.Append(new Vertex(new Vector2f(-.5f, pos2 - .5f) + (Angle.Loop * i / 20).GenerateVector(12), new HSVColor(Hue, .6f, 1, 0)));
                    }
                }
                if (Pressing == 1)
                {
                    Light1.Append(new Vertex(new Vector2f(-.5f, pos1 - .5f), new HSVColor(Hue, .6f, 1)));
                    for (int i = 0; i <= 20; i++)
                        Light1.Append(new Vertex(new Vector2f(-.5f, pos1 - .5f) + (Angle.Loop * i / 20).GenerateVector(12), new HSVColor(Hue, .6f, 1, 0)));
                }
                else if (Pressing == 2)
                {
                    Light2.Append(new Vertex(new Vector2f(-.5f, pos2 - .5f), new HSVColor(Hue, .6f, 1)));
                    for (int i = 0; i <= 20; i++)
                        Light2.Append(new Vertex(new Vector2f(-.5f, pos2 - .5f) + (Angle.Loop * i / 20).GenerateVector(12), new HSVColor(Hue, .6f, 1, 0)));
                }
                Pad1.Position = new Vector2f(0, pos1);
                Pad2.Position = new Vector2f(0, pos2);

                UpPadVertice.Append(new Vertex(new Vector2f(6.5f, 6.5f), new HSVColor(Hue, .6f, .65f)));
                for (int i = 0; i <= 10; i++)
                    UpPadVertice.Append(new Vertex(new Vector2f(6.5f, 6.5f) + (Angle.Loop * i / 20 + Angle.Loop / 2).GenerateVector(7), new HSVColor(Hue, .2f, .95f)));
                DownPadVertice.Append(new Vertex(new Vector2f(0, 6.5f), new HSVColor(Hue, 1, .65f)));
                DownPadVertice.Append(new Vertex(new Vector2f(13, 6.5f), new HSVColor(Hue, 1, .65f)));
                DownPadVertice.Append(new Vertex(new Vector2f(13, 13), new HSVColor(Hue - 20, .75f, .95f)));
                DownPadVertice.Append(new Vertex(new Vector2f(0, 13), new HSVColor(Hue - 20, .75f, .95f)));

                if ((TickConfig & TickType.BOT_RIGHT) == TickType.BOT_RIGHT)
                {
                    Lines.Append(new Vertex(new Vector2f(7, 0), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(15, 0), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(7, -iSize), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(15, -iSize), Engine.BaseFontColor));
                    for (int j = 1; j <= SubDivisions; j++)
                    {
                        int subPos = (int)Utilities.Interpolation(Utilities.Percent(j, 0f, SubDivisions + 1), 0f, -iSize / (Divisions + 1));
                        Lines.Append(new Vertex(new Vector2f(10, subPos), Engine.BaseFontColor));
                        Lines.Append(new Vertex(new Vector2f(15, subPos), Engine.BaseFontColor));
                    }
                    for (int i = 1; i <= Divisions; i++)
                    {
                        int pos = (int)Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), 0f, -iSize);
                        Lines.Append(new Vertex(new Vector2f(7, pos), Engine.BaseFontColor));
                        Lines.Append(new Vertex(new Vector2f(15, pos), Engine.BaseFontColor));
                        for (int j = 1; j <= SubDivisions; j++)
                        {
                            int subPos = (int)Utilities.Interpolation(Utilities.Percent(j, 0f, SubDivisions + 1), 0f, -iSize / (Divisions + 1)) + pos;
                            Lines.Append(new Vertex(new Vector2f(10, subPos), Engine.BaseFontColor));
                            Lines.Append(new Vertex(new Vector2f(15, subPos), Engine.BaseFontColor));
                        }
                    }
                }
                if ((TickConfig & TickType.TOP_LEFT) == TickType.TOP_LEFT)
                {
                    Lines.Append(new Vertex(new Vector2f(-8, 0), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(-16, 0), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(-8, -iSize), Engine.BaseFontColor));
                    Lines.Append(new Vertex(new Vector2f(-16, -iSize), Engine.BaseFontColor));
                    for (int j = 1; j <= SubDivisions; j++)
                    {
                        int subPos = (int)Utilities.Interpolation(Utilities.Percent(j, 0f, SubDivisions + 1), 0f, -iSize / (Divisions + 1));
                        Lines.Append(new Vertex(new Vector2f(-11, subPos), Engine.BaseFontColor));
                        Lines.Append(new Vertex(new Vector2f(-16, subPos), Engine.BaseFontColor));
                    }
                    for (int i = 1; i <= Divisions; i++)
                    {
                        int pos = (int)Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), 0f, -iSize);
                        Lines.Append(new Vertex(new Vector2f(-8, pos), Engine.BaseFontColor));
                        Lines.Append(new Vertex(new Vector2f(-16, pos), Engine.BaseFontColor));
                        for (int j = 1; j <= SubDivisions; j++)
                        {
                            int subPos = (int)Utilities.Interpolation(Utilities.Percent(j, 0f, SubDivisions + 1), 0f, -iSize / (Divisions + 1)) + pos;
                            Lines.Append(new Vertex(new Vector2f(-11, subPos), Engine.BaseFontColor));
                            Lines.Append(new Vertex(new Vector2f(-16, subPos), Engine.BaseFontColor));
                        }
                    }
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateTexts()
        {
            requireUpdate = false;
            Texts.Clear();
            if ((TickConfig & TickType.SHOW_TEXT) == TickType.SHOW_TEXT)
            {
                int iSize = (int)Size;
                if ((TickConfig & TickType.BOT_RIGHT) == TickType.BOT_RIGHT)
                {
                    if (Orientation == Engine.Orientation.VERTICAL)
                    {
                        for (int i = 1; i <= Divisions; i++)
                        {
                            int pos = (int)Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), 0f, -iSize);
                            var tmp = new Text(Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), Minimum, Maximum).ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f(0, (int)(tmp.GetLocalBounds().Top + tmp.GetGlobalBounds().Height / 2));
                            tmp.Position = new Vector2f(17, pos);
                            Texts.Add(tmp);
                        }
                        {
                            var tmp = new Text(Minimum.ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f(0, (int)(tmp.GetLocalBounds().Top + tmp.GetGlobalBounds().Height));
                            tmp.Position = new Vector2f(17, 0);
                            Texts.Add(tmp);
                            tmp = new Text(Maximum.ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f(0, (int)(tmp.GetLocalBounds().Top));
                            tmp.Position = new Vector2f(17, -iSize);
                            Texts.Add(tmp);
                        }
                    }
                    else if (Orientation == Engine.Orientation.HORIZONTAL)
                    {
                        for (int i = 1; i <= Divisions; i++)
                        {
                            int pos = (int)Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), 0f, iSize);
                            var tmp = new Text(Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), Minimum, Maximum).ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f((int)(tmp.GetGlobalBounds().Width / 2), 0);
                            tmp.Position = new Vector2f(pos, 17);
                            Texts.Add(tmp);
                        }
                        {
                            var tmp = new Text(Minimum.ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f(0, 0);
                            tmp.Position = new Vector2f(0, 17);
                            Texts.Add(tmp);
                            tmp = new Text(Maximum.ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f((int)tmp.GetGlobalBounds().Width, 0);
                            tmp.Position = new Vector2f(iSize, 17);
                            Texts.Add(tmp);
                        }
                    }
                }
                if ((TickConfig & TickType.TOP_LEFT) == TickType.TOP_LEFT)
                {
                    if (Orientation == Engine.Orientation.VERTICAL)
                    {
                        for (int i = 1; i <= Divisions; i++)
                        {
                            int pos = (int)Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), 0f, -iSize);
                            var tmp = new Text(Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), Minimum, Maximum).ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f(tmp.GetGlobalBounds().Width, (int)(tmp.GetLocalBounds().Top + tmp.GetGlobalBounds().Height / 2));
                            tmp.Position = new Vector2f(-19, pos);
                            Texts.Add(tmp);
                        }
                        {
                            var tmp = new Text(Minimum.ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f(tmp.GetGlobalBounds().Width, (int)(tmp.GetLocalBounds().Top + tmp.GetGlobalBounds().Height));
                            tmp.Position = new Vector2f(-19, 0);
                            Texts.Add(tmp);
                            tmp = new Text(Maximum.ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f(tmp.GetGlobalBounds().Width, (int)(tmp.GetLocalBounds().Top));
                            tmp.Position = new Vector2f(-19, -iSize);
                            Texts.Add(tmp);
                        }
                    }
                    else if (Orientation == Engine.Orientation.HORIZONTAL)
                    {
                        for (int i = 1; i <= Divisions; i++)
                        {
                            int pos = (int)Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), 0f, iSize);
                            var tmp = new Text(Utilities.Interpolation(Utilities.Percent(i, 0f, Divisions + 1), Minimum, Maximum).ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f((int)(tmp.GetGlobalBounds().Width / 2), Engine.CharacterSize);
                            tmp.Position = new Vector2f(pos, -19);
                            Texts.Add(tmp);
                        }
                        {
                            var tmp = new Text(Minimum.ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f(0, Engine.CharacterSize);
                            tmp.Position = new Vector2f(0, -19);
                            Texts.Add(tmp);
                            tmp = new Text(Maximum.ToString(NumberFormat), Engine.BaseFont, Engine.CharacterSize);
                            tmp.FillColor = Engine.BaseFontColor;
                            tmp.Origin = new Vector2f((int)(tmp.GetGlobalBounds().Width), Engine.CharacterSize);
                            tmp.Position = new Vector2f(iSize, -19);
                            Texts.Add(tmp);
                        }
                    }
                }
            }
        }

        #endregion Private Methods
    }
}