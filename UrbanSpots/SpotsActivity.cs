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
using Android.Locations;
using Android.Util;
using Android.Content.PM;
using Android.Provider;
using Android.Graphics;
using Java.IO;
using Java.Nio;
using Android;
using Android.Support.V4.Content;

namespace UrbanSpots
{
    [Activity(Label = "Urbane Mreže")]
    public class SpotsActivity : Activity, ILocationListener
    {
        private UrbanSpotsWcfClient client;

        private Button btnAddLocation;
        private Button btnTakePhoto;
        private ImageView imgView;
        private LocationManager locMgr;
        private TextView txtCoordinates;
        private TextView txtName;
        private TextView txtDescription;
        private TextView txtMessage;
        private string lat, lng;
        private Bitmap currentPhoto;

        private Guid userGuid;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Spots);
            this.Window.SetTitle("Urbane mreže");

            userGuid = new Guid(Intent.GetStringExtra("UserGuid"));
            txtCoordinates = FindViewById<TextView>(Resource.Id.txtCoordinates);
            txtMessage = FindViewById<TextView>(Resource.Id.txtMessage);

            int MY_LOCATION_REQUEST_CODE = 100;
            if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
            {
                RequestPermissions(new String[] { Manifest.Permission.AccessFineLocation }, MY_LOCATION_REQUEST_CODE);
            }

            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();
                locMgr = GetSystemService(Context.LocationService) as LocationManager;
                HandleLocationManager();

                imgView = FindViewById<ImageView>(Resource.Id.imgView);
                btnTakePhoto = FindViewById<Button>(Resource.Id.btnTakePhoto);
                btnTakePhoto.Click += TakePicture;

                btnAddLocation = FindViewById<Button>(Resource.Id.btnAddLocation);
                btnAddLocation.Click += BtnAddLocation_Click;

                int MY_CAMERA_REQUEST_CODE = 100;
                if (CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted)
                {
                    RequestPermissions(new String[] { Manifest.Permission.Camera }, MY_CAMERA_REQUEST_CODE);
                }
            }
        }

        private void BtnAddLocation_Click(object sender, EventArgs e)
        {
            if (currentPhoto != null)
            {
                System.IO.MemoryStream ms = null;
                try
                {
                    txtName = FindViewById<TextView>(Resource.Id.txtSpotName);
                    txtDescription = FindViewById<TextView>(Resource.Id.txtSpotDescription);

                    client = WCFFunctions.InitializeWcfServiceClient();

                    int bytes = currentPhoto.ByteCount;
                    ByteBuffer buffer = ByteBuffer.Allocate(bytes);
                    currentPhoto.CopyPixelsToBuffer(buffer);

                    buffer.Position(0);
                    byte[] photo = new byte[buffer.Remaining()];
                    buffer.Get(photo);

                    ms = new System.IO.MemoryStream();
                    currentPhoto.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 100, ms);

                    string result = client.AddLocation(this.userGuid.ToString(), txtName.Text, txtDescription.Text, double.Parse(lng), double.Parse(lat), ms.ToArray());

                    if (result == "0")
                    {
                        txtMessage.Text = "Točka je bila uspešno dodana!";
                        txtName.Text = "";
                        txtDescription.Text = "";
                        txtCoordinates.Text = "Koordinate:";
                        imgView.SetImageBitmap(null);
                    }
                    else
                    {
                        txtMessage.Text = result;
                    }
                }
                catch (Exception ex)
                {
                    txtMessage.Text = ms.Length + "" + ex.Message;
                }
            }
            else
            {
                // Tell the user to load the image!
            }
        }

        private void HandleLocationManager()
        {
            Criteria locationCriteria = new Criteria
            {
                Accuracy = Accuracy.Fine,
                PowerRequirement = Power.Medium
            };
            /*var providers = locMgr.GetProviders(false);

            foreach (var provider in providers)
                locMgr.RequestLocationUpdates(provider, 0, 0, this);*/

            string locationProvider = locMgr.GetBestProvider(locationCriteria, true);
            //string locationProvider = LocationManager.GpsProvider;

            if (locationProvider != null)
            {
                if (locMgr.IsProviderEnabled(locationProvider))
                {
                    Location lastKnownLocation = locMgr.GetLastKnownLocation(locationProvider);

                    locMgr.RequestLocationUpdates(locationProvider, 2000, 1, this);

                    if (lastKnownLocation != null)
                    {
                        lat = Math.Round(lastKnownLocation.Latitude, 6).ToString();
                        lng = Math.Round(lastKnownLocation.Longitude, 3).ToString();

                        txtCoordinates.Text = "Koordinate: " + lat + "," + lng;
                    }
                }
                else
                {
                    Log.Info("Urbane mreže", locationProvider + " ni na voljo. Ali ima naprava omogočene storitve določanja lokacije?");
                }
            }
            else
            {
                txtMessage.Text = "LOCATION NULL!";
            }

            /*try
            {
                locationProvider = locMgr.GetBestProvider(locationCriteria, true);
                if (locationProvider != null)
                {
                    locMgr.RequestLocationUpdates(locationProvider, 0, 0, this);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No (enabled) location provider available.");
                }
                Location currentLocation = locMgr.GetLastKnownLocation(locationProvider);
                if (currentLocation != null)
                {
                    
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }*/

        }

        protected override void OnResume()
        {
            base.OnResume();

            HandleLocationManager();
        }

        protected override void OnPause()
        {
            base.OnPause();
            locMgr.RemoveUpdates(this);
        }


        #region LocationListener functions

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnProviderDisabled(string provider)
        {

        }
        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {

        }
        public void OnLocationChanged(Android.Locations.Location location)
        {
            txtCoordinates.Text = "Koordinate: " + lat + "," + lng;
        }

        #endregion


        private void CreateDirectoryForPictures()
        {
            App._dir = new File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), "CameraAppUrbane");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        private void TakePicture(object sender, EventArgs eventArgs)
        {
            HandleLocationManager();

            File tempFile = File.CreateTempFile("myPhoto_", ".jpg", GetExternalFilesDir("images"));
            if (tempFile != null)
            {
                Intent pictureIntent = new Intent(MediaStore.ActionImageCapture);
                if (pictureIntent.ResolveActivity(PackageManager) != null)
                {
                    Android.Net.Uri photoURI = FileProvider.GetUriForFile(this, Application.Context.PackageName + ".fileprovider", tempFile);
                    pictureIntent.PutExtra(MediaStore.ExtraOutput, photoURI);
                    StartActivityForResult(pictureIntent, 100);
                }
            }
            App._file = tempFile;
            //Intent intent = new Intent(MediaStore.ActionImageCapture);
            //App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            //intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App._file));
            //StartActivityForResult(intent, 0);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Make it available in the gallery

            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Android.Net.Uri contentUri = Android.Net.Uri.FromFile(App._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            // Display in ImageView. We will resize the bitmap to fit the display.
            // Loading the full sized image will consume to much memory
            // and cause the application to crash.

            int height = Resources.DisplayMetrics.HeightPixels;
            int width = imgView.Height;
            App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
            if (App.bitmap != null)
            {
                // For now, thumbnails are of size 300 x prop                
                //int x = thumbDimen * Math.Min(Resources.DisplayMetrics.HeightPixels, Resources.DisplayMetrics.WidthPixels) / Math.Max(Resources.DisplayMetrics.HeightPixels, Resources.DisplayMetrics.WidthPixels);
                currentPhoto = App._file.Path.LoadAndResizeBitmap(500, 500 * Math.Min(Resources.DisplayMetrics.HeightPixels, Resources.DisplayMetrics.WidthPixels) / Math.Max(Resources.DisplayMetrics.HeightPixels, Resources.DisplayMetrics.WidthPixels));

                imgView.SetImageBitmap(App.bitmap);
                App.bitmap = null;
            }
            else
            {
                txtMessage.Text = "Slika se ni naložila.";
            }

            // Dispose of the Java side bitmap.
            GC.Collect();
        }
    }

    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
    }

    public static class BitmapHelpers
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            return resizedBitmap;
        }
    }
}