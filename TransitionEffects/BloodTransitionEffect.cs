//-----------------------------------------------------------------------
// <copyright file="BloodTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for blood transition effect
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
    /// Blood transition effect.
    /// </summary>
    public class BloodTransitionEffect : CloudyTransitionEffect
    {
        /// <summary>
        /// Constructor - gets shader instructions for this effect.
        /// </summary>
        public BloodTransitionEffect()
        {
            PixelShader shader = new PixelShader();
            shader.UriSource = TransitionUtilities.MakePackUri("Shaders/Blood.fx.ps");
            this.PixelShader = shader;
        }
    }
}
