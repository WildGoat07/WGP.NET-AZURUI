using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGP;
using SFML.System;
using SFML.Graphics;

namespace WGP.AzurUI
{
    public static class Engine
    {
        #region Internal Fields

        internal static Font BaseFont;
        internal static Color BaseFontColor;
        internal static uint CharacterSize;
        internal static Chronometer Chronometer;

        #endregion Internal Fields

        #region Public Properties

        public static Color DefaultBackgroundColor => new HSVColor(DefaultHue, .26f, .37f);
        public static float DefaultHue => 220;

        #endregion Public Properties

        #region Public Methods

        public static void Initialize()
        {
            BaseFont = new Font(Properties.Resources.tahoma);
            CharacterSize = 12;
            BaseFontColor = new Color(230, 230, 230);
            Chronometer = new Chronometer();
        }

        #endregion Public Methods
    }
}