using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text.Method;
using Android.Views;
using Android.Widget;

namespace UrbanSpots
{
    [Activity(Label = "Urbane Mreže", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        private UrbanSpotsWcfClient client;

        private Button btnSignUp;
        private TextView txtNotSuccessful;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Login);

            this.Window.SetTitle("Urbane mreže");

            this.txtNotSuccessful = FindViewById<TextView>(Resource.Id.txtNotSuccess);
            this.txtNotSuccessful.MovementMethod = new ScrollingMovementMethod();
            this.btnSignUp = FindViewById<Button>(Resource.Id.btnSignUp);
            this.btnSignUp.Click += BtnSignUp_Click;
        }

        private void BtnSignUp_Click(object sender, EventArgs e)
        {
            client = WCFFunctions.InitializeWcfServiceClient();

            FragmentTransaction ftrans = FragmentManager.BeginTransaction();
            DialogSignUp dlgSignUp = new DialogSignUp();
            dlgSignUp.Show(ftrans, "dialog fragment");

            dlgSignUp.onSignUpComplete += dlgSignUp_onSignUpComplete;
        }

        private void dlgSignUp_onSignUpComplete(object sender, OnSignUpEventArgs e)
        {
            string errorMsg = "";
            try
            {
                // Check login            
                client = WCFFunctions.InitializeWcfServiceClient();
                if (client != null)
                {
                    //var user = client.LoginUser(e.EMail, e.Password);
                    var user = client.LoginUser("borut.luzar@fis.unm.si", "Test01.");

                    errorMsg += "8";
                    if (user.UserGuid != Guid.Empty)
                    {
                        var mainActivity = new Intent(this, typeof(MainActivity));
                        mainActivity.PutExtra("UserName", user.UserName);
                        mainActivity.PutExtra("UserGuid", user.UserGuid.ToString());
                        StartActivity(mainActivity);
                    }
                    else
                    {
                        this.txtNotSuccessful.Visibility = Android.Views.ViewStates.Visible;
                        this.txtNotSuccessful.Text = user.UserName;
                    }
                }
                else
                {
                    this.txtNotSuccessful.Visibility = Android.Views.ViewStates.Visible;
                    this.txtNotSuccessful.Text = "Povezava ni bila vzpostavljena!";
                }
            }
            catch (Exception ex)
            {
                this.txtNotSuccessful.Visibility = Android.Views.ViewStates.Visible;
                this.txtNotSuccessful.Text = errorMsg + " - " + ex.Message + " -- " + ex.StackTrace;
            }
        }
    }
}