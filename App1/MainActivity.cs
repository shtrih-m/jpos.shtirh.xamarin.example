using Android.App;
using Android.OS;
using Android.Util;
using Android.Widget;
using Com.Shtrih.Fiscalprinter;
using Com.Shtrih.Util;
using Jpos;
using System;
using System.Threading.Tasks;
using Android.Content;

namespace App1
{
    [Activity(Label = "App1", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private readonly string TAG = "MainActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var preferences = GetSharedPreferences("MainActivity", FileCreationMode.Append);

            RestoreAndSaveEditValue(Resource.Id.tbNetworkAddress, preferences, "NetworkAddress", "192.168.1.39:7778");

            var btn = FindViewById<Button>(Resource.Id.btnConnect);

            btn.Click += (o, e) =>
            {
                ConnectMePlz();
            };
        }

        private void RestoreAndSaveEditValue(int editId, ISharedPreferences preferences, string key, string defaultValue)
        {
            var tbNetworkAddress = FindViewById<EditText>(editId);
            tbNetworkAddress.Text = preferences.GetString(key, defaultValue);

            tbNetworkAddress.TextChanged += (o, e) =>
            {
                var editor = preferences.Edit();
                editor.PutString(key, tbNetworkAddress.Text);
                editor.Apply();
            };
        }

        public async void ConnectMePlz()
        {
            var dialog = ProgressDialog.Show(this, "Connecting to device", "Please wait...", true);
            dialog.Show();

            var tbNetworkAddress = FindViewById<EditText>(Resource.Id.tbNetworkAddress);

            try
            {
                SysUtils.FilesPath = ApplicationContext.FilesDir.AbsolutePath;
                JposConfig.configure("ShtrihFptr", tbNetworkAddress.Text, ApplicationContext, "2", "0");

                var sw = Stopwatch.StartNew();

                var serial = await Task.Run(() =>
                {
                    using (var printer = new ShtrihFiscalPrinter(new FiscalPrinter()))
                    {
                        printer.Open("ShtrihFptr");
                        printer.Claim(3000);
                        printer.DeviceEnabled = true;

                        return printer.ReadFullSerial();
                    }
                });

                sw.Stop();

                ShowMessage($"Connected to {serial} in {sw.ElapsedMilliseconds()} мс");
            }
            catch (Java.Lang.Exception e)
            {
                Log.Error(TAG, e.ToString());
                ShowMessage(e.Message);
            }
            catch (Exception e)
            {
                Log.Error(TAG, e.ToString());
                ShowMessage(e.Message);
            }
            finally
            {
                dialog.Dismiss();
            }
        }

        private void ShowMessage(string message)
        {
            Toast.MakeText(ApplicationContext, message, ToastLength.Long).Show();
            Log.Debug(TAG, message);
        }

    }

    public static class ShtrihFiscalPritnerEx
    {
        public static string ReadFullSerial(this ShtrihFiscalPrinter printer)
        {
            String[] lines = new String[1];
            printer.GetData(FiscalPrinterConst.FptrGdPrinterId, null, lines);

            return lines[0];
        }
    }
}

