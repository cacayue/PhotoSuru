//-----------------------------------------------------------------------
// <copyright file="LeastBrightTransitionEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Code for least bright transition effect
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
    /// Least bright transition effect.
    /// </summary>
    public class LeastBrightTransitionEffect : TransitionEffect
    {
        /// <summary>
        /// Constructor - initializes shader instructions for this effect.
        /// </summary>
        public LeastBrightTransitionEffect()
        {
            PixelShader shader = new PixelShader();
            shader.UriSource = TransitionUtilities.MakePackUri("Shaders/LeastBright.fx.ps");
            this.PixelShader = shader;
        }
    }
}
