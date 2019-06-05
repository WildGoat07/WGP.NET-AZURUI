using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGP;
using SFML.System;
using SFML.Graphics;
using System.Net;

namespace WGP.AzurUI
{
    /// <summary>
    /// Main class to initialize the widgets.
    /// </summary>
    public static class Engine
    {
        #region Public Fields

        /// <summary>
        /// The default recommended background color.
        /// </summary>
        public static Color DefaultBackgroundColor;

        #endregion Public Fields

        #region Internal Fields

        internal static Font BaseFont;
        internal static Color BaseFontColor;
        internal static uint CharacterSize;
        internal static Chronometer Chronometer;
        internal static WebClient Client;

        internal static Theme Mode;

        #endregion Internal Fields

        #region Public Enums

        /// <summary>
        /// The orientation of some widgets.
        /// </summary>
        public enum Orientation
        {
            /// <summary>
            /// The widget is horizontally displayed.
            /// </summary>
            HORIZONTAL,

            /// <summary>
            /// The widget is vertically displayed.
            /// </summary>
            VERTICAL
        }

        /// <summary>
        /// The default theme of the GUI
        /// </summary>
        public enum Theme
        {
            /// <summary>
            /// Normal, classic, trivial
            /// </summary>
            AZUR,

            /// <summary>
            /// For the degenerates
            /// </summary>
            WHITE
        }

        #endregion Public Enums

        #region Public Properties

        /// <summary>
        /// The default hue of newly created widgets.
        /// </summary>
        public static float DefaultHue => 220;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Initialize the engine. Must be called before creating any widget to not cause troubles.
        /// </summary>
        public static void Initialize(Theme theme = Theme.AZUR)
        {
            Mode = theme;
            Client = new WebClient();
            BaseFont = new Font(Properties.Resources.tahoma);
            CharacterSize = 12;
            if (theme == Theme.AZUR)
            {
                BaseFontColor = new Color(230, 230, 230);
                DefaultBackgroundColor = new HSVColor(DefaultHue, .26f, .37f);
            }
            else
            {
                DefaultBackgroundColor = new HSVColor(DefaultHue, 0, .9f);
                BaseFontColor = new Color(15, 15, 15);
            }

            Chronometer = new Chronometer();
        }

        #endregion Public Methods
    }
}