using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace UrbanSpots
{
    [Activity(Label = "Urbane Mreže")]
    public class MainActivity : Activity
    {
        private Button btnAddSpot;
        private TextView txtWelcome;
        private Guid userGuid;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);
            this.Window.SetTitle("Urbane mreže");

            btnAddSpot = FindViewById<Button>(Resource.Id.btnAddSpot);
            btnAddSpot.Click += BtnAddSpot_Click;

            txtWelcome = FindViewById<TextView>(Resource.Id.txtWelcome);
            txtWelcome.Text = "Pozdravljeni, " + Intent.GetStringExtra("UserName") + "!";
            userGuid = new Guid(Intent.GetStringExtra("UserGuid"));
        }

        private void BtnAddSpot_Click(object sender, EventArgs e)
        {
            var spotsActivity = new Intent(this, typeof(SpotsActivity));
            spotsActivity.PutExtra("UserGuid", userGuid.ToString());
            StartActivity(spotsActivity);
        }
    }
}