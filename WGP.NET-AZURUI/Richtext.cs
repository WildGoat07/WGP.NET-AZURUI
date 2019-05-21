using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
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
        #region Public Fields

        /// <summary>
        /// Actions triggered when clicking an [action="ID"] where ID is the key.
        /// </summary>
        public ReadOnlyDictionary<string, Action> Actions;

        #endregion Public Fields

        #region Internal Fields

        internal static int ACTION = 256;
        internal static int BOLD = 1;
        internal static int HEADLINE1 = 16;
        internal static int HEADLINE2 = 32;
        internal static int HEADLINE3 = 64;
        internal static int ITALIC = 2;
        internal static int LIST = 512;
        internal static int LIST_ELEMENT = 1024;
        internal static int NONE = 0;
        internal static int STRIKETHROUGH = 8;
        internal static int UNDERLINED = 4;
        internal static int URI = 128;

        #endregion Internal Fields

        #region Private Fields

        private Dictionary<string, Action> _actions;
        private List<Component> _components;
        private Vector2f _globalOffset;
        private List<Tuple<FloatRect, Trigger>> _hitboxes;
        private VertexArray _lines;
        private float _maxWidth;
        private bool _requireUpdate;
        private List<Tuple<Sprite, int>> _sprites;

        private string _text;

        private List<Text> _texts;

        private List<Tuple<List<Texture>, Time>> _textures;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">Markdown formatted text.</param>
        public Richtext(string text = "") : base(text)
        {
            _texts = new List<Text>();
            _lines = new VertexArray(PrimitiveType.Lines);
            _sprites = new List<Tuple<Sprite, int>>();
            _textures = new List<Tuple<List<Texture>, Time>>();
            _hitboxes = new List<Tuple<FloatRect, Trigger>>();
            _components = new List<Component>();
            Text = text;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// When enabled, a widget handle events, otherwise it becomes grey and doesn't react.
        /// </summary>
        public new bool Enabled { get => base.Enabled; set { base.Enabled = value; _requireUpdate = true; } }

        /// <summary>
        /// Hue of the links.
        /// </summary>
        public override float Hue
        {
            get => base.Hue;
            set
            {
                base.Hue = value;
                _requireUpdate = true;
            }
        }

        /// <summary>
        /// Maximum width of the widget, it will try to wrap its content. 0 for no maximum width.
        /// </summary>
        public float MaxWidth { get => _maxWidth; set { _maxWidth = value; _requireUpdate = true; } }

        /// <summary>
        /// Formatted text.
        /// </summary>
        /// The formatting is the following : [b]...[/] for bold, [i]...[/] for italic, [u]...[/] for
        /// underlined, [strike]...[/] for strikethrough, [h1]...[/] for biggest titles, [h3]...[/]
        /// for smallest titles, [uri="some URI (can be relative)"]...[/] for a clickable link to an
        /// uri, [action="ID"]...[/] for a clickable link to an action, [img="URI to img (can be
        /// relative)"/] for an image (support animated gifs), [ul][li]...[/][/] for unorganised
        /// lists, [ol][li]...[/][/], [line/] for an horizontal line. Use '\[' to escape the '[' character.
        public override string Text
        {
            set
            {
                var index = new int();
                _textures.Clear();
                Parse(value, ref index, new Part() { Modifier = NONE, ListIndex = -1 });
                _requireUpdate = true;
            }
            get => _text;
        }

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

        internal Uri GetUri(string str)
        {
            Uri uri;
            try
            {
                uri = new Uri(str);
            }
            catch (Exception)
            {
                try
                {
                    uri = new Uri(Path.Combine(Environment.CurrentDirectory, str));
                }
                catch (Exception)
                {
                    uri = null;
                }
            }
            return uri;
        }

        internal override void Update(RenderWindow app, Vector2f offset)
        {
            base.Update(app, offset);
            if (_requireUpdate)
            {
                _requireUpdate = false;
                _texts.Clear();
                _hitboxes.Clear();
                _lines.Clear();
                _sprites.Clear();
            }
            var texList = new List<Texture>();
            foreach (var item in _textures)
            {
                int selectedIndex = (int)(_chronometer.ElapsedTime.AsSeconds() / item.Item2.AsSeconds());
                texList.Add(item.Item1[selectedIndex % item.Item1.Count]);
            }
            foreach (var item in _sprites)
            {
                item.Item1.Texture = texList[item.Item2];
            }
        }

        #endregion Internal Methods

        #region Private Methods

        private void GenerateSubComponentText(string str, Part part)
        {
            if (str.Length == 0)
                return;
            var result = new Component();
            result.text = str;
            result.IsDefaultColor = true;
            result.IsImage = false;
            result.IsLine = false;
            result.BeginList = false;
            result.ListIndex = part.ListIndex;
            result.CharSize = Engine.CharacterSize;
            if ((part.Modifier & BOLD) != 0)
                result.Style |= SFML.Graphics.Text.Styles.Bold;
            if ((part.Modifier & ITALIC) != 0)
                result.Style |= SFML.Graphics.Text.Styles.Italic;
            if ((part.Modifier & UNDERLINED) != 0)
                result.Style |= SFML.Graphics.Text.Styles.Underlined;
            if ((part.Modifier & STRIKETHROUGH) != 0)
                result.Style |= SFML.Graphics.Text.Styles.StrikeThrough;
            if ((part.Modifier & HEADLINE1) != 0)
            {
                result.CharSize = Engine.CharacterSize * 4;
                result.Style |= SFML.Graphics.Text.Styles.Bold;
            }
            if ((part.Modifier & HEADLINE2) != 0)
                result.CharSize = Engine.CharacterSize * 2;
            if ((part.Modifier & HEADLINE3) != 0)
                result.CharSize = (uint)(Engine.CharacterSize * 1.5f);
            if ((part.Modifier & ACTION) != 0)
            {
                result.IsDefaultColor = false; /*NewColor(Hue, .8f, 1, Engine.BaseFontColor.A);
                _hitboxes.Add(new Tuple<FloatRect, Trigger>(new FloatRect(offset - new Vector2f(0, result.CharacterSize),
                    new Vector2f(result.GetGlobalBounds().Width, result.CharacterSize)), new Trigger() { Type = Trigger.Mode.ACTION, Action = part.Action }));*/
            }
            if ((part.Modifier & URI) != 0)
            {
                result.IsDefaultColor = false; /*NewColor(Hue, .8f, 1, Engine.BaseFontColor.A);
                _hitboxes.Add(new Tuple<FloatRect, Trigger>(new FloatRect(offset - new Vector2f(0, result.CharacterSize),
                    new Vector2f(result.GetGlobalBounds().Width, result.CharacterSize)), new Trigger() { Type = Trigger.Mode.URI, Uri = part.linkURI }));*/
            }
            if ((part.Modifier & LIST) != 0 && (part.Modifier & LIST_ELEMENT) == 0)
            {
                result.text = "";
                result.BeginList = true;
            }
            if ((part.Modifier & LIST_ELEMENT) != 0)
            {
                result.text = "•  " + result.text;
            }
            _components.Add(result);
        }

        private void Parse(string str, ref int index, Part currentPart)
        {
            string currentText = "";
            while (index < str.Length)
            {
                char currChar = str[index];
                if (currChar == '\\' && index + 1 <= str.Length)
                {
                    currentText += str[index + 1];
                    index += 2;
                    continue;
                }
                else if (currChar != '[')
                    currentText += currChar;
                else
                {
                    try
                    {
                        if (str.Substring(index, 3) == "[/]")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            index += 2;
                            return;
                        }
                        else if (str.Substring(index, 3) == "[b]")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= BOLD;
                            index += 3;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 3) == "[u]")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= UNDERLINED;
                            index += 3;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 3) == "[i]")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= ITALIC;
                            index += 3;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 4) == "[h1]")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= HEADLINE1;
                            pushed.Modifier ^= HEADLINE2;
                            pushed.Modifier ^= HEADLINE3;
                            index += 4;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 4) == "[h2]")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= HEADLINE2;
                            index += 4;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 4) == "[h3]")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= HEADLINE3;
                            index += 4;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 8) == "[strike]")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= STRIKETHROUGH;
                            index += 8;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 6) == "[uri=\"")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= URI;
                            index += 6;
                            string uri = "";
                            while (str[index] != '"')
                                uri += str[index++];
                            pushed.linkURI = GetUri(uri);
                            index += 2;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 9) == "[action=\"")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= ACTION;
                            index += 9;
                            string action = "";
                            while (str[index] != '"')
                                action += str[index++];
                            pushed.Action = action;
                            index += 2;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 4) == "[ol]")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.ListIndex = 1;
                            pushed.Modifier |= LIST;
                            index += 4;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 4) == "[ul]")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.ListIndex = 0;
                            pushed.Modifier |= LIST;
                            index += 4;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 7) == "[line/]")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            _components.Add(new Component() { IsLine = true });
                            index += 7;
                        }
                        else if (str.Substring(index, 6) == "[img=\"")
                        {
                            GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            index += 6;
                            string uriString = "";
                            while (str[index] != '"')
                                uriString += str[index++];
                            index += 3;
                            var uri = GetUri(uriString);
                            Vector2u size;
                            var compo = new Component();
                            compo.IsImage = true;
                            compo.Image = _textures.Count;
                            _components.Add(compo);
                            if (uri == null)
                            {
                                var tex = new Texture((Image)Properties.Resources.imgMissing);
                                size = tex.Size;
                                _textures.Add(new Tuple<List<Texture>, Time>(new List<Texture>() { tex }, Time.Zero));
                            }
                            else
                            {
                                System.Drawing.Image img;
                                try
                                {
                                    var data = new MemoryStream(Engine.Client.DownloadData(uri));
                                    img = System.Drawing.Image.FromStream(data);
                                    if (System.Drawing.ImageAnimator.CanAnimate(img))
                                    {
                                        //******************************************
                                        PropertyItem item = img.GetPropertyItem(0x5100); // https://stackoverflow.com/a/3785231
                                        var delay = Time.FromMilliseconds((item.Value[0] + item.Value[1] * 256) * 10);
                                        //******************************************

                                        var dim = new FrameDimension(img.FrameDimensionsList[0]);
                                        var frameCount = img.GetFrameCount(new FrameDimension(img.FrameDimensionsList[0]));
                                        var texList = new List<Texture>();
                                        for (int i = 0; i < frameCount; i++)
                                        {
                                            img.SelectActiveFrame(dim, i);
                                            texList.Add(new Texture((Image)new System.Drawing.Bitmap(img)));
                                        }
                                        size = texList[0].Size;
                                        _textures.Add(new Tuple<List<Texture>, Time>(texList, delay));
                                    }
                                    else
                                    {
                                        var tex = new Texture((Image)new System.Drawing.Bitmap(img));
                                        size = tex.Size;
                                        _textures.Add(new Tuple<List<Texture>, Time>(new List<Texture>() { tex }, Time.Zero));
                                    }
                                }
                                catch (Exception)
                                {
                                    var tex = new Texture((Image)Properties.Resources.imgMissing);
                                    size = tex.Size;
                                    _textures.Add(new Tuple<List<Texture>, Time>(new List<Texture>() { tex }, Time.Zero));
                                }
                            }
                        }
                        else
                            throw new InvalidOperationException("Invalid or corrupted format.");
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new InvalidOperationException("Invalid or corrupted format.");
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        throw new InvalidOperationException("Invalid or corrupted format.");
                    }
                    catch (InvalidOperationException e)
                    {
                        throw e;
                    }
                }
                index++;
            }
            GenerateSubComponentText(currentText, currentPart);
        }

        #endregion Private Methods

        #region Internal Structs

        internal struct Part
        {
            #region Public Fields

            public string Action;
            public Uri ImgUri;
            public Uri linkURI;
            public int ListIndex;
            public int Modifier;

            #endregion Public Fields
        }

        #endregion Internal Structs

        #region Internal Classes

        internal class Trigger
        {
            #region Public Fields

            public string Action;

            public Mode Type;

            public Uri Uri;

            #endregion Public Fields

            #region Internal Enums

            internal enum Mode
            {
                ACTION,
                URI
            }

            #endregion Internal Enums
        }

        #endregion Internal Classes

        #region Private Classes

        private class Component
        {
            #region Public Fields

            public bool BeginList;
            public uint CharSize;
            public int Image;
            public bool IsDefaultColor;
            public bool IsImage;
            public bool IsLine;
            public int ListIndex;
            public Text.Styles Style;
            public string text;

            #endregion Public Fields
        }

        #endregion Private Classes
    }
}