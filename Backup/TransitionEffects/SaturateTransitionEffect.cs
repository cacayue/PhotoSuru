// <copyright file="SaturateTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for saturate transition effect
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
    /// Saturate transition effect.
    /// </summary>
    public class SaturateTransitionEffect : TransitionEffect
    {
        /// <summary>
        /// Constructor - initializes shader instructions for this effect.
        /// </summary>
        public SaturateTransitionEffect()
        {
            PixelShader shader = new PixelShader();
            shader.UriSource = TransitionUtilities.MakePackUri("Shaders/Saturate.fx.ps");
            this.PixelShader = shader;
        }
    }
}
