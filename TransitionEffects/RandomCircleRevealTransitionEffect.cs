//-----------------------------------------------------------------------
// <copyright file="RandomCircleRevealTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for random circle reveal transition effect
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
    /// Random circle reveal transition effect.
    /// </summary>
    public class RandomCircleRevealTransitionEffect : CloudyTransitionEffect
    {
        /// <summary>
        /// Constructor - initializes shader instructions for this effect.
        /// </summary>
        public RandomCircleRevealTransitionEffect()
        {
            PixelShader shader = new PixelShader();
            shader.UriSource = TransitionUtilities.MakePackUri("Shaders/RandomCircleReveal.fx.ps");
            this.PixelShader = shader;
        }
    }
}
