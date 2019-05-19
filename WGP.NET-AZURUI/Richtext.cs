using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WGP.AzurUI
{
    public class Richtext : Label
    {
        #region Public Properties

        /// <summary>
        /// When enabled, a widget handle events, otherwise it becomes grey and doesn't react.
        /// </summary>
        public new bool Enabled { get => base.Enabled; set => base.Enabled = value; }

        /// <summary>
        /// Hue of the theme.
        /// </summary>
        public new float Hue { get => base.Hue; set => base.Hue = value; }

        /// <summary>
        /// Formatted text.
        /// </summary>
        /// The formatting is based on Markdown, currently supported : headlines, emphasis, ordered
        /// and unordered, links, image link, line break
        public override string Text { get; set; }

        #endregion Public Properties
    }
}