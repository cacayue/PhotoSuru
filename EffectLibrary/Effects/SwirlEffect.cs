﻿//-----------------------------------------------------------------------
// <copyright file="SwirlEffect.cs" company="Microsoft">
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
    public class SwirlEffect : ShaderEffect
    {
        /// <summary>
        /// The explict input for this pixel shader.
        /// </summary>
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(SwirlEffect), 0);
        
        /// <summary>
        /// This property is mapped to the Center variable within the pixel shader.
        /// </summary>
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(Point), typeof(SwirlEffect), new UIPropertyMetadata(new Point(0.5, 0.5), PixelShaderConstantCallback(0)));

        /// <summary>
        /// This property is mapped to the SwirlStrength variable within the pixel shader.
        /// </summary>
        public static readonly DependencyProperty SwirlStrengthProperty = DependencyProperty.Register("SwirlStrength", typeof(double), typeof(SwirlEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(1)));

        /// <summary>
        /// This property is mapped to the AngleFrequency variable within the pixel shader.
        /// </summary>
        public static readonly DependencyProperty AngleFrequencyProperty = DependencyProperty.Register("AngleFrequency", typeof(Vector), typeof(SwirlEffect), new UIPropertyMetadata(new Vector(1, 1), PixelShaderConstantCallback(2)));

        /// <summary>
        /// A refernce to the pixel shader used.
        /// </summary>
        private static PixelShader pixelShader = new PixelShader();

        /// <summary>
        /// The transform used when this Effect is applied.
        /// </summary>
        private SwirlGeneralTransform generalTransform;

        /// <summary>
        /// Creates an instance of the shader from the included pixel shader.
        /// </summary>
        static SwirlEffect()
        {
            pixelShader.UriSource = Global.MakePackUri("ShaderBytecode/swirl.fx.ps");
        }

        /// <summary>
        /// Creates an instance and updates the shader's variables to the default values.
        /// </summary>
        public SwirlEffect()
        {
            this.PixelShader = pixelShader;

            UpdateShaderValue(CenterProperty);
            UpdateShaderValue(SwirlStrengthProperty);
            UpdateShaderValue(AngleFrequencyProperty);
            UpdateShaderValue(InputProperty);

            this.generalTransform = new SwirlGeneralTransform(this);
        }

        /// <summary>
        /// Gets or sets the Center variable within the shader.
        /// </summary>
        public Point Center
        {
            get { return (Point)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        /// <summary>
        /// Gets or sets the SwirlStrength variable within the shader.
        /// </summary>
        public double SwirlStrength
        {
            get { return (double)GetValue(SwirlStrengthProperty); }
            set { SetValue(SwirlStrengthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the AngleFrequency variable within the shader.
        /// </summary>
        public Vector AngleFrequency
        {
            get { return (Vector)GetValue(AngleFrequencyProperty); }
            set { SetValue(AngleFrequencyProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Input shader sampler.
        /// </summary>
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        /// <summary>
        /// Gets the GeneralTransform for this effect.
        /// </summary>
        protected override GeneralTransform EffectMapping
        {
            get
            {
                return this.generalTransform;
            }
        }

        /// <summary>
        /// For transforming input and tree transformations.
        /// </summary>
        private class SwirlGeneralTransform : GeneralTransform
        {
            /// <summary>
            /// The instance of the Effect.
            /// </summary>
            private readonly SwirlEffect theEffect;

            /// <summary>
            /// The inverse of the transform.
            /// </summary>
            private bool transformIsInverse;

            /// <summary>
            /// The inverse of this GeneralTransform.
            /// </summary>
            private SwirlGeneralTransform inverseTransform;

            /// <summary>
            /// Creates an instance of this class.
            /// </summary>
            /// <param name="eff">The effect itself.</param>
            public SwirlGeneralTransform(SwirlEffect eff)
            {
                this.theEffect = eff;
            }

            /// <summary>
            /// Gets a clone of the inverse of the current transform.
            /// </summary>
            public override GeneralTransform Inverse
            {
                get
                {
                    // Cache this since it can get called often
                    if (this.inverseTransform == null)
                    {
                        this.inverseTransform = (SwirlGeneralTransform)this.Clone();
                        this.inverseTransform.transformIsInverse = !this.transformIsInverse;
                    }

                    return this.inverseTransform;
                }
            }

            /// <summary>
            /// For this operation, the bounds is the bounding box of the 4 transformed points. 
            /// Need to transform each of them, and then circumscribe.  This is true for both the 
            /// forward and the inverse.
            /// </summary>
            /// <param name="rect">The input rect.</param>
            /// <returns>The transformed rect.</returns>
            public override Rect TransformBounds(Rect rect)
            {
                Point tl, tr, bl, br;

                if (this.TryTransform(rect.TopLeft, out tl) &&
                    this.TryTransform(rect.TopRight, out tr) &&
                    this.TryTransform(rect.BottomLeft, out bl) &&
                    this.TryTransform(rect.BottomRight, out br))
                {
                    double maxX = Math.Max(tl.X, Math.Max(tr.X, Math.Max(bl.X, br.X)));
                    double minX = Math.Min(tl.X, Math.Min(tr.X, Math.Min(bl.X, br.X)));

                    double maxY = Math.Max(tl.Y, Math.Max(tr.Y, Math.Max(bl.Y, br.Y)));
                    double minY = Math.Min(tl.Y, Math.Min(tr.Y, Math.Min(bl.Y, br.Y)));

                    return new Rect(minX, minY, maxX - minX, maxY - minY);
                }
                else
                {
                    return Rect.Empty;
                }
            }

            /// <summary>
            /// Attempt to transform inPoint with the Swirl effect.
            /// </summary>
            /// <param name="targetPoint">The input point.</param>
            /// <param name="result">The output point after transformed using the Swirl effect.</param>
            /// <returns>True and throws if false.</returns>
            public override bool TryTransform(Point targetPoint, out Point result)
            {
                // Exactly follows what the HLSL shader itself does.
                Vector dir = targetPoint - this.theEffect.Center;
                double l = dir.Length;
                dir.Normalize();

                double angle = Math.Atan2(dir.Y, dir.X);

                double inverseFactor = this.transformIsInverse ? 1 : -1;
                double newAngle = angle + inverseFactor * this.theEffect.SwirlStrength * l;

                Vector angleFrequency = this.theEffect.AngleFrequency;
                double xamt = Math.Cos(angleFrequency.X * newAngle) * l;
                double yamt = Math.Sin(angleFrequency.Y * newAngle) * l;

                result = this.theEffect.Center + new Vector(xamt, yamt);

                return true;
            }

            /// <summary>
            /// Returns a new instance of this.
            /// </summary>
            /// <returns>A new instance.</returns>
            protected override Freezable CreateInstanceCore()
            {
                return new SwirlGeneralTransform(this.theEffect) { transformIsInverse = this.transformIsInverse };
            }
        }
    }
}
