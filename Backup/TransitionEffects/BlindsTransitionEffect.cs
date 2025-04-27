//-----------------------------------------------------------------------
// <copyright file="BlindsTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for blinds transition effect
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
    /// Blinds transition effect
    /// </summary>
    public class BlindsTransitionEffect : TransitionEffect
    {
        /// <summary>
        /// Constructor - gets shader instructions for this effect.
        /// </summary>
        public BlindsTransitionEffect()
        {
            PixelShader shader = new PixelShader();
            shader.UriSource = TransitionUtilities.MakePackUri("Shaders/Blinds.fx.ps");
            this.PixelShader = shader;
        }
    }
}
