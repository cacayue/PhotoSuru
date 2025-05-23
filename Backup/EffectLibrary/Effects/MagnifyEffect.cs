﻿//-----------------------------------------------------------------------
// <copyright file="MagnifyEffect.cs" company="Microsoft">
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
    public class MagnifyEffect : ShaderEffect
    {
        /// <summary>
        /// Gets or sets the Radii variable within the shader.
        /// </summary>
        public static readonly DependencyProperty RadiiProperty = DependencyProperty.Register("Radii", typeof(Size), typeof(MagnifyEffect), new UIPropertyMetadata(new Size(0.2, 0.2), PixelShaderConstantCallback(0)));

        /// <summary>
        /// Gets or sets the center variable within the shader.
        /// </summary>
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(Point), typeof(MagnifyEffect), new UIPropertyMetadata(new Point(0.25, 0.25), PixelShaderConstantCallback(1)));

        /// <summary>
        /// Gets or sets the amount variable within the shader.
        /// </summary>
        public static readonly DependencyProperty ShrinkFactorProperty = DependencyProperty.Register("Amount", typeof(double), typeof(MagnifyEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(2)));

        /// <summary>
        /// Gets or sets the input used in the shader.
        /// </summary>
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(MagnifyEffect), 0);

        /// <summary>
        /// The pixel shader instance.
        /// </summary>
        private static PixelShader pixelShader = new PixelShader();

        /// <summary>
        /// The transform used for this shader.
        /// </summary>
        private MagnifyGeneralTransform generalTransform;

        /// <summary>
        /// Creates an instance of the shader from the included pixel shader.
        /// </summary>
        static MagnifyEffect()
        {
            pixelShader.UriSource = Global.MakePackUri("ShaderBytecode/magnify.fx.ps");
        }

        /// <summary>
        /// Creates an instance and updates the shader's variables to the default values.
        /// </summary>
        public MagnifyEffect()
        {
            this.PixelShader = pixelShader;

            UpdateShaderValue(RadiiProperty);
            UpdateShaderValue(CenterProperty);
            UpdateShaderValue(ShrinkFactorProperty);
            UpdateShaderValue(InputProperty);

            this.generalTransform = new MagnifyGeneralTransform(this);
        }

        /// <summary>
        /// Gets or sets the Radii variable within the shader.
        /// </summary>
        public Size Radii
        {
            get { return (Size)GetValue(RadiiProperty); }
            set { SetValue(RadiiProperty, value); }
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
        /// Gets or sets the ShrinkFactor: 
        /// The higher the shrink factor the "smaller" the content inside the ellipse will appear. 
        /// </summary>
        public double ShrinkFactor
        {
            get { return (double)GetValue(ShrinkFactorProperty); }
            set { SetValue(ShrinkFactorProperty, value); }
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
        /// Gets the EffectMapping.
        /// </summary>
        protected override GeneralTransform EffectMapping
        {
            get
            {
                return this.generalTransform;
            }
        }

        /// <summary>
        /// The GeneralTransform corrosponding to the Magnify effect.
        /// </summary>
        private class MagnifyGeneralTransform : GeneralTransform
        {
            /// <summary>
            /// The effect instance.
            /// </summary>
            private readonly MagnifyEffect effect;

            /// <summary>
            /// If the transform is an inverse.
            /// </summary>
            private bool transformIsInverse;

            /// <summary>
            /// The transform specific to this Effect.
            /// </summary>
            private MagnifyGeneralTransform inverseTransform;

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="fx">The source effect.</param>
            public MagnifyGeneralTransform(MagnifyEffect fx)
            {
                this.effect = fx;
            }

            /// <summary>
            /// Gets the inverse transform.
            /// </summary>
            public override GeneralTransform Inverse
            {
                get
                {
                    // Cache this since it can get called often
                    if (this.inverseTransform == null)
                    {
                        this.inverseTransform = (MagnifyGeneralTransform)this.Clone();
                        this.inverseTransform.transformIsInverse = !this.transformIsInverse;
                    }

                    return this.inverseTransform;
                }
            }

            /// <summary>
            /// This particular effect keeps axis aligned lines axis aligned, so transformation of the rect is just
            /// transformation of its corner points.
            /// </summary>
            /// <param name="rect">The input rect.</param>
            /// <returns>The output rect.</returns>
            public override Rect TransformBounds(Rect rect)
            {
                Point tl, br;
                Rect result;
                bool ok1 = this.TryTransform(rect.TopLeft, out tl);
                bool ok2 = this.TryTransform(rect.BottomRight, out br);
                if (ok1 && ok2)
                {
                    result = new Rect(tl, br);
                }
                else
                {
                    result = Rect.Empty;
                }

                return result;
            }

            /// <summary>
            /// Attempt to transform a point based on 
            /// the effect.
            /// </summary>
            /// <param name="targetPoint">The point to transform.</param>
            /// <param name="result">The result if available.</param>
            /// <returns>Return true if successful.</returns>
            public override bool TryTransform(Point targetPoint, out Point result)
            {
                // In this particular case, the inverse transform is the same as the forward
                // transform.
                if (!PointIsInEllipse(targetPoint, this.effect.Center, this.effect.Radii))
                {
                    // If outside the ellipse, just the identity.
                    result = targetPoint;
                }
                else
                {
                    // If inside the ellipse, calculate that magnification/minification
                    Point center = this.effect.Center;
                    Vector ray = targetPoint - center;

                    // Inverse maps a point from after the effect was applied to the point that it came from before the effect.
                    // Non-inverse maps where a point before the effect is applied goes after the effect is applied.
                    // The operation the shader itself performs should match up with the "inverse" operation here.
                    double scaleFactor = this.transformIsInverse ? this.effect.ShrinkFactor : 1.0 / this.effect.ShrinkFactor;

                    result = center + scaleFactor * ray;
                }

                return true;
            }

            /// <summary>
            /// Creats a new instance.
            /// </summary>
            /// <returns>A new instance of this.</returns>
            protected override Freezable CreateInstanceCore()
            {
                return new MagnifyGeneralTransform(this.effect) { transformIsInverse = this.transformIsInverse };
            }

            /// <summary>
            /// Determines if a point is within an ellipse.
            /// </summary>
            /// <param name="pt">The test point.</param>
            /// <param name="center">The center point of the ellipse.</param>
            /// <param name="radii">The radii of the ellipse.</param>
            /// <returns>True if successful.</returns>
            private static bool PointIsInEllipse(Point pt, Point center, Size radii)
            {
                Vector ray = pt - center;
                double rayPctX = ray.X / radii.Width;
                double rayPctY = ray.Y / radii.Height;

                // Normally would take sqrt() for length, but since we're comparing 
                // to 1.0, it doesn't matter.
                double pctLength = rayPctX * rayPctX + rayPctY * rayPctY;

                return pctLength <= 1.0;
            }
        }
    }
}
