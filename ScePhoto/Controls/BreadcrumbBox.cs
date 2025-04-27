//-----------------------------------------------------------------------
// <copyright file="BreadcrumbBox.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     ComboBox subclass control with extended properties to allow 
//     Vista-like breadcrumb bar behaviour.
// </summary>
//-----------------------------------------------------------------------

namespace ScePhoto.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    /// <summary>
    /// ComboBox subclass control with extended properties to allow Vista-like breadcrumb bar behaviour.
    /// </summary>
    [TemplatePart(Name = "PART_TextAreaButton", Type = typeof(Button)),
     TemplatePart(Name = "PART_DropDownButton", Type = typeof(ToggleButton))]
    public class BreadcrumbBox : ComboBox
    {
        #region Fields
        /// <summary>
        /// DependencyProperty backing store for PreviousBreadcrumbBox.
        /// </summary>
        public static readonly DependencyProperty PreviousBreadcrumbBoxProperty =
            DependencyProperty.Register("PreviousBreadcrumbBox", typeof(BreadcrumbBox), typeof(BreadcrumbBox), new UIPropertyMetadata(null));

        /// <summary>
        /// DependencyProperty backing store for NextBreadcrumbBox.
        /// </summary>
        public static readonly DependencyProperty NextBreadcrumbBoxProperty =
           DependencyProperty.Register("NextBreadcrumbBox", typeof(BreadcrumbBox), typeof(BreadcrumbBox), new UIPropertyMetadata(null));

        /// <summary>
        /// DependencyProperty backing store for TextAreaControl.
        /// </summary>
        public static readonly DependencyProperty TextAreaControlProperty =
           DependencyProperty.Register("TextAreaControl", typeof(Control), typeof(BreadcrumbBox), new UIPropertyMetadata(null));

        /// <summary>
        /// DependencyProperty backing store for DropDownControl.
        /// </summary>
        public static readonly DependencyProperty DropDownControlProperty =
            DependencyProperty.Register("DropDownControl", typeof(Control), typeof(BreadcrumbBox), new UIPropertyMetadata(null));

        /// <summary>
        /// DependencyProperty backing store for Command.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(BreadcrumbBox), new UIPropertyMetadata(null));

        /// <summary>
        /// DependencyProperty backing store for CommandParameter.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(BreadcrumbBox), new UIPropertyMetadata(null));

        /// <summary>
        /// The button inhabiting the text area.
        /// </summary>
        private Button textAreaButton;

        /// <summary>
        /// The button displaying the drop down arrow.
        /// </summary>
        private ToggleButton dropDownButton; 
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the breadcrumb box to the left of this box.
        /// </summary>
        public BreadcrumbBox PreviousBreadcrumbBox
        {
            get { return (BreadcrumbBox)GetValue(PreviousBreadcrumbBoxProperty); }
            set { SetValue(PreviousBreadcrumbBoxProperty, value); }
        }

        /// <summary>
        /// Gets or sets the breadcrumb box to the right of this box.
        /// </summary>
        public BreadcrumbBox NextBreadcrumbBox
        {
            get { return (BreadcrumbBox)GetValue(NextBreadcrumbBoxProperty); }
            set { SetValue(NextBreadcrumbBoxProperty, value); }
        }

        /// <summary>
        /// Gets the control inhabiting the text area.
        /// </summary>
        public Control TextAreaControl
        {
            get { return (Control)GetValue(TextAreaControlProperty); }
            protected set { SetValue(TextAreaControlProperty, value); }
        }

        /// <summary>
        /// Gets the control used to drop the breadcrumb box open.
        /// </summary>
        public Control DropDownControl
        {
            get { return (Control)GetValue(DropDownControlProperty); }
            protected set { SetValue(DropDownControlProperty, value); }
        }

        /// <summary>
        /// Gets or sets the command to execute when the TextAreaControl is clicked.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the command parameter for this control's command.
        /// </summary>
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        } 
        #endregion

        #region Methods
        /// <summary>
        /// Locates the TextAreaControl and the DropDownButton in the currently applied template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.textAreaButton = this.Template.FindName("PART_TextAreaButton", this) as Button;
            this.TextAreaControl = this.textAreaButton;
            this.dropDownButton = this.Template.FindName("PART_DropDownButton", this) as ToggleButton;
            this.DropDownControl = this.dropDownButton;
        } 
        #endregion
    }
}
