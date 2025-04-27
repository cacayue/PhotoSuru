//-----------------------------------------------------------------------
// <copyright file="FadeTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for fade transition effect
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
    /// Fade transition effect.
    /// </summary>
    public class FadeTransitionEffect : TransitionEffect
    {
        /// <summary>
        /// Constructor - initializes shader instructions for this effect.
        /// </summary>
        public FadeTransitionEffect()
        {
            PixelShader shader = new PixelShader();
            shader.UriSource = TransitionUtilities.MakePackUri("Shaders/Fade.fx.ps");
            this.PixelShader = shader;
        }
    }
}
