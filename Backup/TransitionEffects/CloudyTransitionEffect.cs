//-----------------------------------------------------------------------
// <copyright file="CloudyTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for cloudy transition effect
// </summary>
//-----------------------------------------------------------------------

namespace TransitionEffects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Media;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Effects;

    /// <summary>
    /// Cloudy transition effect.
    /// </summary>
    public abstract class CloudyTransitionEffect : RandomizedTransitionEffect
    {
        #region Fields

        /// <summary>
        /// DependencyProperty for <see cref="CloudImage"/> property.
        /// </summary>
        protected static readonly DependencyProperty CloudImageProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("CloudImage", typeof(CloudyTransitionEffect), 2, SamplingMode.Bilinear);

        #endregion

        #region Methods

        /// <summary>
        /// Constructor - performs initialization.
        /// </summary>
        protected CloudyTransitionEffect()
        {
            this.CloudImage = new ImageBrush(new BitmapImage(TransitionUtilities.MakePackUri("Images/clouds.png")));
            this.UpdateShaderValue(CloudImageProperty);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Brush to be used as cloud image for this effect.
        /// </summary>
        protected Brush CloudImage
        {
            get { return (Brush)GetValue(CloudImageProperty); }
            set { SetValue(CloudImageProperty, value); }
        }

        #endregion
    }
}
