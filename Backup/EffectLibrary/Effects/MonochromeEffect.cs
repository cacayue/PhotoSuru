//-----------------------------------------------------------------------
// <copyright file="MonochromeEffect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     WPF Extensible Effect
// </summary>
//-----------------------------------------------------------------------
namespace EffectLibrary
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Diagnostics;

    /// <summary>
    /// This is the implementation of an extensible framework ShaderEffect which loads
    /// a shader model 2 pixel shader. Dependecy properties declared in this class are mapped
    /// to registers as defined in the *.ps file being loaded below.
    /// </summary>
    public class MonochromeEffect : ShaderEffect
    {
        /// <summary>
        /// Gets or sets the Input of the shader.
        /// </summary>
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(MonochromeEffect), 0);
        
        /// <summary>
        /// The shader instance.
        /// </summary>
        private static PixelShader pixelShader = new PixelShader();

        /// <summary>
        /// Creates an instance of the shader from the included pixel shader.
        /// </summary>
        static MonochromeEffect()
        {
            pixelShader.UriSource = Global.MakePackUri("ShaderBytecode/monochrome.fx.ps");
        }

        /// <summary>
        /// Creates an instance and updates the shader's variables to the default values.
        /// </summary>
        public MonochromeEffect()
        {
            this.PixelShader = pixelShader;

            UpdateShaderValue(InputProperty);
        }

        /// <summary>
        /// Gets or sets the input used in the shader.
        /// </summary>
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }
    }
}
