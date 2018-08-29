using System;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace UrbanSpots
{
    public class DialogSignUp : DialogFragment
    {
        private EditText txtEmail;
        private EditText txtPassword;
        private Button btnSignUp;

        public event EventHandler<OnSignUpEventArgs> onSignUpComplete;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            View view = inflater.Inflate(Resource.Layout.SignUpDialog, container, false);

            txtEmail = view.FindViewById<EditText>(Resource.Id.txtEmail);
            txtPassword = view.FindViewById<EditText>(Resource.Id.txtPassword);
            btnSignUp = view.FindViewById<Button>(Resource.Id.btnDialogSubmit);
            btnSignUp.Click += btnSignUp_Click;

            return view;
        }

        private void btnSignUp_Click(object sender, EventArgs e)
        {
            onSignUpComplete.Invoke(this, new OnSignUpEventArgs(txtEmail.Text, txtPassword.Text));
            this.Dismiss(); // Close the dialog
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            base.OnActivityCreated(savedInstanceState);
            Dialog.Window.Attributes.WindowAnimations = Resource.Style.dialogAnim;
        }
    }

    public class OnSignUpEventArgs : EventArgs
    {
        private string email;
        private string password;

        public OnSignUpEventArgs(string email, string password) : base()
        {
            this.email = email;
            this.password = password;
        }

        public string EMail
        {
            get
            {
                return email;
            }
            set
            {
                this.email = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                this.password = value;
            }
        }
    }
}