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

namespace FOAMfilter
{
    class Helper
    {
        public static int getLangId()
        {
            if (MainActivity.language.Equals("en"))
            {
                return 0;
            }
            else if (MainActivity.language.Equals("fr"))
            {
                return 1;
            }
            else if (MainActivity.language.Equals("es"))
            {
                return 2;
            }
            else if (MainActivity.language.Equals("de"))
            {
                return 3;
            }
            return 4;
        }
    }

}