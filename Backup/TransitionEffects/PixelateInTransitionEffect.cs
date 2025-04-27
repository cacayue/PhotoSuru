//-----------------------------------------------------------------------
// <copyright file="PixelateInTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for pixelate in transition effect
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
    /// Pixelate in transition effect.
    /// </summary>
    public class PixelateInTransitionEffect : TransitionEffect
    {
        /// <summary>
        /// Constructor - initializes shader instructions for this effect.
        /// </summary>
        public PixelateInTransitionEffect()
        {
            PixelShader shader = new PixelShader();
            shader.UriSource = TransitionUtilities.MakePackUri("Shaders/PixelateIn.fx.ps");
            this.PixelShader = shader;
        }
    }
}
