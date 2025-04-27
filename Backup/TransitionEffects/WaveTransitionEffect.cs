// <copyright file="WaveTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for wave transition effect
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
    /// Wave transition effect.
    /// </summary>
    public class WaveTransitionEffect : TransitionEffect
    {
        /// <summary>
        /// Constructor - initializes shader instructions for this effect.
        /// </summary>
        public WaveTransitionEffect()
        {
            PixelShader shader = new PixelShader();
            shader.UriSource = TransitionUtilities.MakePackUri("Shaders/Wave.fx.ps");
            this.PixelShader = shader;
        }
    }
}
