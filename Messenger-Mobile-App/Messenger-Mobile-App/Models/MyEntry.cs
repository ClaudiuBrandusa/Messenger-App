using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Messenger_Mobile_App.Models
{
    public class MyEntry : Entry
    {
        #region Bindable Properties
        public static readonly BindableProperty MyHighlightColorProperty = BindableProperty.Create(nameof(MyHighlightColor), typeof(Color), typeof(MyEntry));
        public static readonly BindableProperty MyHandleColorProperty = BindableProperty.Create(nameof(MyHandleColor), typeof(Color), typeof(MyEntry));
        public static readonly BindableProperty MyTintColorProperty = BindableProperty.Create(nameof(MyTintColor), typeof(Color), typeof(MyEntry));
        #endregion

        #region Properties
        public Color MyHighlightColor
        {
            get => (Color)GetValue(MyHighlightColorProperty);
            set => SetValue(MyHighlightColorProperty, value);
        }
        public Color MyHandleColor
        {
            get => (Color)GetValue(MyHandleColorProperty);
            set => SetValue(MyHandleColorProperty, value);
        }
        public Color MyTintColor
        {
            get => (Color)GetValue(MyTintColorProperty);
            set => SetValue(MyTintColorProperty, value);
        }
        #endregion
    }
}
