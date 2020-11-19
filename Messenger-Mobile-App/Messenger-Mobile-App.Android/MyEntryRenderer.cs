using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Messenger_Mobile_App.Droid;
using Messenger_Mobile_App.Models;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MyEntry), typeof(MyEntryRenderer))]
namespace Messenger_Mobile_App.Droid
{
    public class MyEntryRenderer : EntryRenderer
    {
        public MyEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null || e.NewElement == null) return;

            if(e.NewElement is MyEntry customEntry)
            {
                Control.SetHighlightColor(color: customEntry.MyHighlightColor.ToAndroid());

                JNIEnv.SetField(Control.Handle, JNIEnv.GetFieldID(JNIEnv.FindClass(typeof(TextView)), "mCursorDrawableRes", "I"), 0);
                TextView textViewTemplate = new TextView(Control.Context);

                var field = textViewTemplate.Class.GetDeclaredField("mEditor");
                field.Accessible = true;
                var editor = field.Get(Control);

                string[]
                fieldsNames = { "mTextSelectHandleLeftRes", "mTextSelectHandleRightRes", "mTextSelectHandleRes" },
                drawablesNames = { "mSelectHandleLeft", "mSelectHandleRight", "mSelectHandleCenter"}; 
            
                for(int index = 0; index < fieldsNames.Length && index < drawablesNames.Length; index++)
                {
                    field = textViewTemplate.Class.GetDeclaredField(fieldsNames[index]);
                    field.Accessible = true;
                    Drawable handleDrawble = ContextCompat.GetDrawable(Context, field.GetInt(Control));
                    handleDrawble.SetColorFilter(new PorterDuffColorFilter(customEntry.MyHandleColor.ToAndroid(), PorterDuff.Mode.SrcIn));

                    field = editor.Class.GetDeclaredField(drawablesNames[index]);
                    field.Accessible = true;
                    field.Set(editor, handleDrawble);
                }

                if(Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Control.BackgroundTintList = ColorStateList.ValueOf(customEntry.MyTintColor.ToAndroid());
                }else
                {
                    Control.Background.SetColorFilter(new PorterDuffColorFilter(customEntry.MyTintColor.ToAndroid(), PorterDuff.Mode.SrcAtop));
                }
            }
        }
    }
}