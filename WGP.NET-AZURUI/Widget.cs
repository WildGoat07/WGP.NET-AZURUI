using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGP;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace WGP.AZURUI
{
    public abstract class Widget
    {
        #region Public Constructors

        public Widget()
        {
            _chronometer = new Chronometer(Engine.Chronometer);
            Position = new Vector2f();
        }

        #endregion Public Constructors

        #region Public Properties

        public FloatRect GlobalBounds => new FloatRect(LocalBounds.TopLeft() + Position, LocalBounds.Size());
        public abstract FloatRect LocalBounds { get; }
        public Vector2f Position { get; set; }

        #endregion Public Properties

        #region Protected Properties

        protected Chronometer _chronometer { get; private set; }

        #endregion Protected Properties

        #region Public Methods

        public abstract void DrawOn(RenderTarget target);

        public abstract void Update(RenderWindow app);

        #endregion Public Methods
    }
}