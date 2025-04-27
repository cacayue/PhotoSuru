// <copyright file="SlideInTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for slide in transition effect
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
    /// Slide in transition effect.
    /// </summary>
    public class SlideInTransitionEffect : TransitionEffect
    {
        #region Fields

        /// <summary>
        /// DependencyProperty for <see cref="SlideAmount"/> property
        /// </summary>
        public static readonly DependencyProperty SlideAmountProperty = DependencyProperty.Register("SlideAmount", typeof(Vector), typeof(SlideInTransitionEffect), new UIPropertyMetadata(new Vector(1.0, 0.0), PixelShaderConstantCallback(1)));

        #endregion

        #region Methods

        /// <summary>
        /// Initializes effect with specified slide amount value.
        /// </summary>
        /// <param name="slideAmount">Vector representing slide amount.</param>
        public SlideInTransitionEffect(Vector slideAmount)
            : this()
        {
            this.SlideAmount = slideAmount;
        }

        /// <summary>
        /// Constructor - initializes shader instructions for this effect, updates shader value for slide amount.
        /// </summary>
        public SlideInTransitionEffect()
        {
            PixelShader shader = new PixelShader();
            shader.UriSource = TransitionUtilities.MakePackUri("Shaders/SlideIn.fx.ps");
            this.PixelShader = shader;

            this.UpdateShaderValue(SlideAmountProperty);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the slide amount for this transition.
        /// </summary>
        public Vector SlideAmount
        {
            get { return (Vector)GetValue(SlideAmountProperty); }
            set { SetValue(SlideAmountProperty, value); }
        }

        #endregion
    }
}
