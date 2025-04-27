//-----------------------------------------------------------------------
// <copyright file="DirectionalBlurEffect.cs" company="Microsoft">
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
    
    /// <summary>
    /// Subclassing ShaderEffect class and implementing a directional blur effect.
    /// </summary>
    public class DirectionalBlurEffect : ShaderEffect
    {
        /// <summary>
        /// Dependency propery for Input - assigning it to sampler register S0.
        /// </summary>
        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(DirectionalBlurEffect), 0);

        /// <summary>
        /// Dependency propery for Angel - assigning it to constant float register C0.
        /// </summary>
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(DirectionalBlurEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));

        /// <summary>
        /// Deependency propery for BlurAmount - assigning it to constant float register C1.
        /// </summary>
        public static readonly DependencyProperty BlurAmountProperty =
            DependencyProperty.Register("BlurAmount", typeof(double), typeof(DirectionalBlurEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(1)));

        /// <summary>
        /// PixelShader for this effect.
        /// </summary>
        private static PixelShader pixelShader;

        /// <summary>
        /// Static contructor - Create a PixelShader for all DirectionalBlurEffect instances. 
        /// </summary>
        static DirectionalBlurEffect()
        {
            pixelShader = new PixelShader();
            pixelShader.UriSource = Global.MakePackUri("ShaderBytecode/DirectionalBlur.fx.ps");
        }

        /// <summary>
        /// Constructor - Assign the PixelShader property and set the shader parameters to default values.
        /// </summary>
        public DirectionalBlurEffect()
        {
            this.PixelShader = pixelShader;
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(AngleProperty);
            UpdateShaderValue(BlurAmountProperty);
        }

        /// <summary>
        /// Gets or sets the Input property.
        /// </summary>
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Angle property.
        /// </summary>
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the BlurAmount property.
        /// </summary>
        public double BlurAmount
        {
            get { return (double)GetValue(BlurAmountProperty); }
            set { SetValue(BlurAmountProperty, value); }
        }
    }
}
