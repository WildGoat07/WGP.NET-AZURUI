using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using WGP;

namespace WGP.AzurUI
{
    /// <summary>
    /// An advanced label with formatting. It is NOT recommended for often changes, especially when
    /// adding an image (and even more if this image is on the web), as it will load it every time.
    /// </summary>
    public class Richtext : Label
    {
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

        private FloatRect AABB;

        private bool oldMouseState;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">Markdown formatted text.</param>
        public Richtext(string text = "") : base(text)
        {
            oldMouseState = false;
            _texts = new List<Text>();
            _lines = new VertexArray(PrimitiveType.Lines);
            _sprites = new List<Tuple<Sprite, int>>();
            _textures = new List<Tuple<List<Texture>, Time>>();
            _hitboxes = new List<Tuple<FloatRect, Trigger>>();
            _components = new List<Component>();
            _actions = new Dictionary<string, Action>();
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

        public override FloatRect LocalBounds => AABB;

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
        /// relative)"/] for an image (support animated gifs, but insanely slow to load it, thanks to
        /// sfml, so i have to copy the bitmaps 2 times), [line/] for an horizontal line. Use '\[' to
        /// escape the '[' character.
        public override string Text
        {
            set
            {
                var index = new int();
                _textures.Clear();
                _actions.Clear();
                Parse(value, ref index, new Part() { Modifier = NONE, ListIndex = -1 });
                _requireUpdate = true;
            }
            get => _text;
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Sets the action linked to its key called by clicking on the [action] component.
        /// </summary>
        /// <param name="key">Name of the action.</param>
        /// <param name="action">Action called.</param>
        public void SetAction(string key, Action action)
        {
            if (_actions.ContainsKey(key))
                _actions[key] = action;
            else
                throw new KeyNotFoundException("No action named \"" + key + "\" found.");
        }

        public override string ToString() => _text;

        #endregion Public Methods

        #region Internal Methods

        internal string[] AlternateSplit(string str, int tabSpace = 4)
        {
            str = str.Replace("\t", new string(' ', tabSpace));
            var result = new List<string>();
            var curr = "";
            foreach (var character in str)
            {
                curr += character;
                if (character == ' ')
                {
                    result.Add(curr);
                    curr = "";
                }
            }
            if (curr.Length > 0)
                result.Add(curr);
            return result.ToArray();
        }

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
                    //If the path is a relative file:///
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
                Vector2f currentOffset = new Vector2f();
                var currRects = new List<Tuple<FloatRect, int>>();
                var compoHeight = new List<Tuple<int, float>>();
                var hitboxesToManage = new List<Tuple<FloatRect, int, Trigger>>();
                foreach (var item in _components)
                {
                    var addedThisLoop = new List<Tuple<FloatRect, int>>();
                    if (item.Text.Length > 0)
                    {
                        var texts = AlternateSplit(item.Text);
                        foreach (var t in texts)
                        {
                            StringBuilder currentWord = new StringBuilder();
                            for (int i = 0; i < t.Length; i++)
                            {
                                if (t[i] == '\n')
                                {
                                    var text = new Text(currentWord.ToString(), Engine.BaseFont, item.CharSize);
                                    if (item.IsDefaultColor)
                                        text.FillColor = Engine.BaseFontColor;
                                    else
                                        text.FillColor = NewColor(Hue, .8f, 1, Engine.BaseFontColor.A);
                                    text.Style = item.Style;
                                    if (currentOffset.X + text.GetGlobalBounds().Width > MaxWidth && MaxWidth > 0 && currentOffset.X > 0 && currentWord.Length > 0)
                                    {
                                        currentOffset.X = 0;
                                        currentOffset.Y += 1;
                                    }
                                    text.Position = currentOffset;
                                    currRects.Add(new Tuple<FloatRect, int>(text.GetGlobalBounds(), (int)currentOffset.Y));
                                    addedThisLoop.Add(new Tuple<FloatRect, int>(text.GetGlobalBounds(), (int)currentOffset.Y));
                                    compoHeight.Add(new Tuple<int, float>((int)currentOffset.Y, item.CharSize));
                                    currentOffset.X = 0;
                                    currentOffset.Y += 1;
                                    currentWord.Clear();
                                    _texts.Add(text);
                                }
                                else
                                    currentWord.Append(t[i]);
                                if (i != '\n' && i == t.Length - 1)
                                {
                                    var text = new Text(currentWord.ToString(), Engine.BaseFont, item.CharSize);
                                    if (item.IsDefaultColor)
                                        text.FillColor = Engine.BaseFontColor;
                                    else
                                        text.FillColor = NewColor(Hue, .8f, 1, Engine.BaseFontColor.A);
                                    text.Style = item.Style;
                                    if (currentOffset.X + text.GetGlobalBounds().Width > MaxWidth && MaxWidth > 0 && currentOffset.X > 0)
                                    {
                                        currentOffset.X = 0;
                                        currentOffset.Y += 1;
                                    }
                                    text.Position = currentOffset;
                                    currRects.Add(new Tuple<FloatRect, int>(text.GetGlobalBounds(), (int)currentOffset.Y));
                                    addedThisLoop.Add(new Tuple<FloatRect, int>(text.GetGlobalBounds(), (int)currentOffset.Y));
                                    compoHeight.Add(new Tuple<int, float>((int)currentOffset.Y, item.CharSize));
                                    currentOffset.X += text.GetGlobalBounds().Width;
                                    _texts.Add(text);
                                }
                            }
                        }
                    }
                    if (item.IsImage)
                    {
                        if (currentOffset.X + _textures[item.Image].Item1[0].Size.X > MaxWidth && MaxWidth > 0 && currentOffset.X > 0)
                        {
                            currentOffset.X = 0;
                            currentOffset.Y += 1;
                        }
                        var sprite = new Sprite(_textures[item.Image].Item1[0]);
                        sprite.Position = currentOffset;
                        currRects.Add(new Tuple<FloatRect, int>(sprite.GetGlobalBounds(), (int)currentOffset.Y));
                        compoHeight.Add(new Tuple<int, float>((int)currentOffset.Y, sprite.Texture.Size.Y));
                        addedThisLoop.Add(new Tuple<FloatRect, int>(sprite.GetGlobalBounds(), (int)currentOffset.Y));
                        currentOffset.X += sprite.Texture.Size.X;
                        _sprites.Add(new Tuple<Sprite, int>(sprite, item.Image));
                    }
                    if (item.IsLine)
                    {
                        if (currentOffset.X > 0)
                        {
                            currentOffset.X = 0;
                            currentOffset.Y += 1;
                        }
                        _lines.Append(new Vertex(currentOffset));
                        _lines.Append(new Vertex(currentOffset));
                        compoHeight.Add(new Tuple<int, float>((int)currentOffset.Y, Engine.CharacterSize));
                        currentOffset.X += MaxWidth;
                    }
                    if (item.Action != null && item.Action.Length > 0)
                    {
                        foreach (var box in addedThisLoop)
                        {
                            var trigger = new Trigger();
                            trigger.Action = item.Action;
                            trigger.Type = Trigger.Mode.ACTION;
                            hitboxesToManage.Add(new Tuple<FloatRect, int, Trigger>(box.Item1, box.Item2, trigger));
                        }
                    }
                    if (item.Link != null && item.Link.OriginalString.Length > 0)
                    {
                        foreach (var box in addedThisLoop)
                        {
                            var trigger = new Trigger();
                            trigger.Uri = item.Link;
                            trigger.Type = Trigger.Mode.URI;
                            hitboxesToManage.Add(new Tuple<FloatRect, int, Trigger>(box.Item1, box.Item2, trigger));
                        }
                    }
                }
                var linesHeight = new List<float>();
                for (int i = 0; i <= currentOffset.Y; i++)
                    linesHeight.Add(0);
                foreach (var height in compoHeight)
                    linesHeight[height.Item1] = Utilities.Max(linesHeight[height.Item1], height.Item2);
                for (int i = 1; i < linesHeight.Count; i++)
                    linesHeight[i] += linesHeight[i - 1];
                linesHeight.Insert(0, 0);
                {
                    var pts = new List<Vector2f>();
                    foreach (var box in currRects)
                    {
                        pts.Add(new Vector2f(box.Item1.Right(), linesHeight[box.Item2 + 1]));
                        pts.Add(new Vector2f(box.Item1.Left, linesHeight[box.Item2]));
                    }
                    AABB = Utilities.CreateRect(pts);
                }
                foreach (var sprite in _sprites)
                    sprite.Item1.Position = new Vector2f(sprite.Item1.Position.X, linesHeight[(int)sprite.Item1.Position.Y]);
                foreach (var text in _texts)
                    text.Position = new Vector2f(text.Position.X, linesHeight[(int)text.Position.Y]);
                for (uint i = 0; i < _lines.VertexCount; i += 2)
                {
                    Vector2f pos = _lines[i].Position;
                    pos.Y = (linesHeight[(int)pos.Y] + linesHeight[(int)pos.Y + 1]) / 2;
                    _lines[i] = new Vertex(pos, Engine.BaseFontColor);
                    pos.X = AABB.Width;
                    _lines[i + 1] = new Vertex(pos, Engine.BaseFontColor);
                }
                foreach (var box in hitboxesToManage)
                {
                    var rect = box.Item1;
                    rect.Top = linesHeight[box.Item2];
                    _hitboxes.Add(new Tuple<FloatRect, Trigger>(rect, box.Item3));
                }
            }
            var texList = new List<Texture>();
            foreach (var item in _textures)
            {
                //for animated gifs
                int selectedIndex = (int)(Math.Round(_chronometer.ElapsedTime.AsSeconds() / item.Item2.AsSeconds()));
                texList.Add(item.Item1[selectedIndex % item.Item1.Count]);
            }
            foreach (var item in _sprites)
            {
                item.Item1.Texture = texList[item.Item2];
            }
            var relativeMousePos = app.MapPixelToCoords(Mouse.GetPosition(app)) - Position - offset;
            if (!oldMouseState && Mouse.IsButtonPressed(Mouse.Button.Left))
            {
                foreach (var box in _hitboxes)
                {
                    if (box.Item1.Contains(relativeMousePos))
                    {
                        switch (box.Item2.Type)
                        {
                            case Trigger.Mode.ACTION:
                                if (box.Item2.Action != null && box.Item2.Action.Length > 0)
                                {
                                    Action action;
                                    _actions.TryGetValue(box.Item2.Action, out action);
                                    action?.Invoke();
                                }
                                break;

                            case Trigger.Mode.URI:
                                if (box.Item2.Uri != null)
                                    Process.Start(box.Item2.Uri.ToString());
                                break;
                        }
                    }
                }
            }
            oldMouseState = Mouse.IsButtonPressed(Mouse.Button.Left);
        }

        #endregion Internal Methods

        #region Private Methods

        private void GenerateSubComponentText(string str, Part part)
        {
            if (str.Length == 0)
                return;
            var result = new Component();
            result.Text = str;
            result.IsDefaultColor = true;
            result.IsImage = false;
            result.IsLine = false;
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
                result.IsDefaultColor = false;
                result.Action = part.Action;
            }
            if ((part.Modifier & URI) != 0)
            {
                result.IsDefaultColor = false;
                result.Link = part.linkURI;
            }
            _components.Add(result);
        }

        private void Parse(string str, ref int index, Part currentPart)
        {
            string currentText = "";
            while (index < str.Length)
            {
                char currChar = str[index];
                if (currChar == '\r') //why on earth is this old character still used ??
                {
                    index++;
                    continue;
                }
                if (currChar == '\\' && index + 1 <= str.Length)
                {
                    currChar = str[index + 1];
                    currentText += currChar;
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
                            if (currentText.Length > 0)
                                GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            index += 2;
                            return;
                        }
                        else if (str.Substring(index, 3) == "[b]")
                        {
                            if (currentText.Length > 0)
                                GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= BOLD;
                            index += 3;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 3) == "[u]")
                        {
                            if (currentText.Length > 0)
                                GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= UNDERLINED;
                            index += 3;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 3) == "[i]")
                        {
                            if (currentText.Length > 0)
                                GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= ITALIC;
                            index += 3;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 4) == "[h1]")
                        {
                            if (currentText.Length > 0)
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
                            if (currentText.Length > 0)
                                GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= HEADLINE2;
                            index += 4;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 4) == "[h3]")
                        {
                            if (currentText.Length > 0)
                                GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= HEADLINE3;
                            index += 4;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 8) == "[strike]")
                        {
                            if (currentText.Length > 0)
                                GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            var pushed = currentPart;
                            pushed.Modifier |= STRIKETHROUGH;
                            index += 8;
                            Parse(str, ref index, pushed);
                        }
                        else if (str.Substring(index, 6) == "[uri=\"")
                        {
                            if (currentText.Length > 0)
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
                            if (currentText.Length > 0)
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
                            _actions.Add(action, null);
                        }
                        else if (str.Substring(index, 7) == "[line/]")
                        {
                            if (currentText.Length > 0)
                                GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            _components.Add(new Component() { IsLine = true, Text = "" });
                            index += 7;
                        }
                        else if (str.Substring(index, 6) == "[img=\"")
                        {
                            if (currentText.Length > 0)
                                GenerateSubComponentText(currentText, currentPart);
                            currentText = "";
                            index += 6;
                            string uriString = "";
                            while (str[index] != '"')
                                uriString += str[index++];
                            index += 2;
                            var uri = GetUri(uriString);
                            Vector2u size;
                            var compo = new Component();
                            compo.Action = currentPart.Action;
                            compo.Link = currentPart.linkURI;
                            compo.IsImage = true;
                            compo.Image = _textures.Count;
                            compo.Text = "";
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
                    catch (Exception e)
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

            public string Action;
            public uint CharSize;
            public int Image;
            public bool IsDefaultColor;
            public bool IsImage;
            public bool IsLine;
            public Uri Link;
            public Text.Styles Style;
            public string Text;

            #endregion Public Fields
        }

        #endregion Private Classes
    }
}