//-----------------------------------------------------------------------
// <copyright file="RippleTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for ripple transition effect
// </summary>
//-----------------------------------------------------------------------

namespace TransitionEffects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Media.Effects;
    using System.Windows;

    /// <summary>
    /// Ripple transition effect.
    /// </summary>
    public class RippleTransitionEffect : TransitionEffect
    {
        #region Fields

        /// <summary>
        /// DependencyProperty for <see cref="Frequency"/> property
        /// </summary>
        public static readonly DependencyProperty FrequencyProperty = DependencyProperty.Register("Frequency", typeof(double), typeof(RippleTransitionEffect), new UIPropertyMetadata(20.0, PixelShaderConstantCallback(1)));

        #endregion

        #region Methods

        /// <summary>
        /// Initializes effect with specified ripple frequency.
        /// </summary>
        /// <param name="freq">Riiple frequency</param>
        public RippleTransitionEffect(double freq)
            : this()
        {
            this.Frequency = freq;
        }

        /// <summary>
        /// Constructor - initializes shader instructions for this effect, updates shader value for frequency property.
        /// </summary>
        public RippleTransitionEffect()
        {
            this.UpdateShaderValue(FrequencyProperty);

            PixelShader shader = new PixelShader();
            shader.UriSource = TransitionUtilities.MakePackUri("Shaders/Ripple.fx.ps");
            this.PixelShader = shader;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets ripple frequency for this effect.
        /// </summary>
        public double Frequency
        {
            get { return (double)GetValue(FrequencyProperty); }
            set { SetValue(FrequencyProperty, value); }
        }

        #endregion
    }
}
