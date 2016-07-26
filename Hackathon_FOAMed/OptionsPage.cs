using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using LinqToTwitter.Json;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LinqToTwitter;
using System.IO;
using System.Xml;

namespace FOAMfilter
{
    [Activity(Label = "Options", MainLauncher = false, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class OptionsPage : Activity
    {
        Spinner langSpinner, defSpinner;
        EditText textEdit;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "Main" layout resource
            SetContentView(Resource.Layout.Options);

            Button tweets = FindViewById<Button>(Resource.Id.tweets);
            langSpinner = FindViewById<Spinner>(Resource.Id.langSpinner);
            textEdit = FindViewById<EditText>(Resource.Id.editFilters);
            defSpinner = FindViewById<Spinner>(Resource.Id.defaultSpinner);
            Button add = FindViewById<Button>(Resource.Id.addFilters);
            Button clear = FindViewById<Button>(Resource.Id.clearFilters);

            langSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(langSpinner_ItemSelected);
            var langAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.langs, Android.Resource.Layout.SimpleSpinnerItem);

            langAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            langSpinner.Adapter = langAdapter;
            langSpinner.SetSelection(Helper.getLangId());

            defSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(defSpinner_ItemSelected);
            var defAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, MainActivity.listOfSearchTerms);

            defAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            defSpinner.Adapter = defAdapter;

            var prefs = Application.Context.GetSharedPreferences("FOAMfilter", FileCreationMode.Private);
            var defPref = prefs.GetString("def", null);

            string defaultSearch = defPref ?? "General";
            defSpinner.SetSelection(MainActivity.listOfSearchTerms.IndexOf(MainActivity.defaultSearchTerm));

            tweets.Click += delegate
            {
                StartActivity(typeof(MainActivity));
                Finish();
            };

            add.Click += delegate
            {
                AddNewFilter();
            };

            clear.Click += delegate
            {
                ClearFilters();
            };
        }

        private void langSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner langSpinner = (Spinner)sender;
            MainActivity.language = langSpinner.SelectedItem.ToString();

            var prefs = Application.Context.GetSharedPreferences("FOAMfilter", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutString("lang", MainActivity.language);
            prefEditor.Commit();
        }
        
        private void defSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner defSpinner = (Spinner)sender;
            MainActivity.currentSearchTerm = defSpinner.SelectedItem.ToString();

            var prefs = Application.Context.GetSharedPreferences("FOAMfilter", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutString("def", MainActivity.currentSearchTerm);
            prefEditor.Commit();
        }

        private void AddNewFilter()
        {
            //Add custom search terms
            if(textEdit.Text.Length != 0)
            {
                MainActivity.listOfSearchTerms.Add(textEdit.Text);
                textEdit.Text = string.Empty;

                var prefs = Application.Context.GetSharedPreferences("FOAMfilter", FileCreationMode.Private);
                var prefEditor = prefs.Edit();
                prefEditor.Remove("search");
                prefEditor.PutStringSet("search", MainActivity.listOfSearchTerms);
                prefEditor.Commit();
            }
        }

        private void ClearFilters()
        {
            var prefs = Application.Context.GetSharedPreferences("FOAMfilter", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.Clear();
            prefEditor.Commit();
            this.FinishAffinity();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            var prefs = Application.Context.GetSharedPreferences("FOAMfilter", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutString("lang", MainActivity.language);
            prefEditor.PutString("def", MainActivity.currentSearchTerm);
            prefEditor.Commit();
        }
    }
}


