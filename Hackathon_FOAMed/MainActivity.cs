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

namespace FOAMfilter
{
    [Activity(Label = "FOAMfilter", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        Spinner topSpinner, bottomSpinner;
        Button searchButton;

        private List<infoBlock> contents = new List<infoBlock>();
        private ListView myListView;
        private Dictionary<string, string> searchTerms = new Dictionary<string, string>();

        private SingleUserAuthorizer auth;
        
        public static string language, currentSearchTermTop, currentSearchTermBottom, defaultSearchTerm;
        public static List<string> listOfSearchTerms = new List<string>();

        string cKey = "";
        string cSecret = "";
        string aToken = "";
        string aSecret = "";

        public struct infoBlock
        {
            public string name;
            public string contents;
            public string link;
            public string imgURL;
            public string tweetURL;
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var prefs = Application.Context.GetSharedPreferences("FOAMfilter", FileCreationMode.Private);
            
            SetContentView(Resource.Layout.Main);

            topSpinner = FindViewById<Spinner>(Resource.Id.spinner);
            bottomSpinner = FindViewById<Spinner>(Resource.Id.spinner2);
            myListView = FindViewById<ListView>(Resource.Id.myListView);
            Button options = FindViewById<Button>(Resource.Id.options);
            searchButton = FindViewById<Button>(Resource.Id.searchButton);

            PopulateListOfSearchTerms();

            var langPref = prefs.GetString("lang", null);
            language = langPref ?? "en";

            var defPref = prefs.GetString("def", null);
            defaultSearchTerm = defPref ?? "General";
            currentSearchTermTop = defaultSearchTerm;
            currentSearchTermBottom = defaultSearchTerm;

            topSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            var topAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, MainActivity.listOfSearchTerms);

            topAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            topSpinner.Adapter = topAdapter;
            topSpinner.SetSelection(listOfSearchTerms.IndexOf(currentSearchTermTop));

            bottomSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner2_ItemSelected);
            var bottomAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, MainActivity.listOfSearchTerms);

            bottomAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            bottomSpinner.Adapter = bottomAdapter;
            bottomSpinner.SetSelection(listOfSearchTerms.IndexOf(currentSearchTermBottom));

            search();

            options.Click += delegate
            {
                StartActivity(typeof(OptionsPage));
            };

            searchButton.Click += delegate
            {
                search();
            };
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            currentSearchTermTop = spinner.SelectedItem.ToString();
        }

        private void spinner2_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            currentSearchTermBottom = spinner.SelectedItem.ToString();
        }

        void PopulateListOfSearchTerms()
        {
            searchTerms.Add("Critical Care", "#FOAMcc");
            searchTerms.Add("Emergency Medicine", "#FOAMems");
            searchTerms.Add("Toxicology", "#FOAMtox");
            searchTerms.Add("Pediatrics", "#FOAMped");
            searchTerms.Add("General", "FOAMed");

            var prefs = Application.Context.GetSharedPreferences("FOAMfilter", FileCreationMode.Private);
            ICollection<string> termsSearch = prefs.GetStringSet("search", null);

            if (termsSearch == null)
            {
                listOfSearchTerms = new List<string>() { "General", "Critical Care", "Emergency Medicine", "Toxicology", "Pediatrics" };
            }
            else
            {
                listOfSearchTerms = termsSearch.Cast<string>().ToList();
                if (listOfSearchTerms.Count > 5)
                {
                    for (int i = 0; i < listOfSearchTerms.Count; i++)
                    {
                        if (!searchTerms.ContainsKey(listOfSearchTerms[i]))
                        {
                            searchTerms.Add(listOfSearchTerms[i], listOfSearchTerms[i]);
                        }
                    }
                }
            }
        }

        void setData(string name, string content, string link, string image, string tweet)
        {
            infoBlock info = new infoBlock();

            content = content.Replace("&amp;", "&");
            content = content.Replace("&lt;", "<");
            content = content.Replace("&gt;", ">");

            if (content.IndexOf("t.co") != -1)
            {
                int i = content.IndexOf("https://t.co");
                content = content.Remove(i);
            }

            info.name = name;
            info.contents = content;
            info.link = link;
            info.imgURL = image;
            info.tweetURL = "https://twitter.com/"+name+"/statuses/"+tweet;
            contents.Add(info);
        }

        private async void search()
        {
            searchButton.Enabled = false;
            topSpinner.Enabled = false;
            bottomSpinner.Enabled = false;

            auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = cKey,
                    ConsumerSecret = cSecret,
                    AccessToken = aToken,
                    AccessTokenSecret = aSecret
                }
            };

            await auth.AuthorizeAsync();

            List<Status> nonRTStat = new List<Status>();
            var twitterCtx = new TwitterContext(auth);

            var searchResponse =
                await
                (from search in twitterCtx.Search
                 where search.Type == SearchType.Search &&
                    search.SearchLanguage == language &&
                    search.ResultType == ResultType.Mixed &&
                    search.Count == 200 &&
                    search.Query == "#FOAMed " + searchTerms[currentSearchTermTop] + " " + searchTerms[currentSearchTermBottom]
                 select search)
                .SingleOrDefaultAsync();
            
            nonRTStat =
                (from tweet in searchResponse.Statuses
                 where tweet.RetweetedStatus.StatusID == 0
                 select tweet)
                .ToList();

            contents = new List<infoBlock>();
            for (int i = 0; i < nonRTStat.Count; i++)
            {
                Array md = nonRTStat[i].Entities.MediaEntities.ToArray();
                string link = null, image = null;

                if (nonRTStat[i].Entities.UrlEntities.ToArray().Length > 0)
                {
                    link = nonRTStat[i].Entities.UrlEntities.ToArray()[0].ExpandedUrl;
                }
                if (nonRTStat[i].Entities.MediaEntities.ToArray().Length > 0)
                {
                    image = nonRTStat[i].Entities.MediaEntities.ToArray()[0].MediaUrlHttps;
                }
                setData(nonRTStat[i].User.ScreenNameResponse, nonRTStat[i].Text, link, image, nonRTStat[i].StatusID.ToString());
            }

            MyListViewAdapter listAdapter = new MyListViewAdapter(this, contents);
            myListView.Adapter = listAdapter;
            myListView.ItemClick += itemTapped;
            myListView.ItemLongClick += itemHeld;

            searchButton.Enabled = true;
            topSpinner.Enabled = true;
            bottomSpinner.Enabled = true;
        }

        void itemTapped(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (contents[e.Position].link != null)
            {
                var uri = Android.Net.Uri.Parse(contents[e.Position].link);
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            }
        }

        void itemHeld(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var uri = Android.Net.Uri.Parse(contents[e.Position].tweetURL);
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            var prefs = Application.Context.GetSharedPreferences("FOAMfilter", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutString("lang", MainActivity.language);
            prefEditor.Commit();
        }
    }
}


