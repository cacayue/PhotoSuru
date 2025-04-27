//-----------------------------------------------------------------------
// <copyright file="CircleRevealTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for circle reveal transition effect
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
    /// Circle reveal transition effect
    /// </summary>
    public class CircleRevealTransitionEffect : TransitionEffect
    {
        #region Fields

        /// <summary>
        /// DependencyProperty for <see cref="FuzzyAmount"/> property
        /// </summary>
        public static readonly DependencyProperty FuzzyAmountProperty = DependencyProperty.Register("FuzzyAmount", typeof(double), typeof(CircleRevealTransitionEffect), new UIPropertyMetadata(0.1, PixelShaderConstantCallback(1)));

        #endregion

        #region Methods

        /// <summary>
        /// Constructor - gets shader instructions for the effect, initializes Fuzzyness value.
        /// </summary>
        /// <param name="fuzzyness">Amount of fuzziness to be associated with the effect</param>
        public CircleRevealTransitionEffect(double fuzziness)
            : this()
        {
            this.FuzzyAmount = fuzziness;
        }

        /// <summary>
        /// Constructor - gets shader instructions for the effect, initializes Fuzzyness value.
        /// </summary>
        public CircleRevealTransitionEffect()
        {
            this.UpdateShaderValue(FuzzyAmountProperty);

            PixelShader shader = new PixelShader();
            shader.UriSource = TransitionUtilities.MakePackUri("Shaders/CircleReveal.fx.ps");
            this.PixelShader = shader;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets fuzziness factor for the effect.
        /// </summary>
        public double FuzzyAmount
        {
            get { return (double)GetValue(FuzzyAmountProperty); }
            set { SetValue(FuzzyAmountProperty, value); }
        }

        #endregion
    }
}
