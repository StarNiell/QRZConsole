namespace QRZLibrary
{
    /// <summary>
    /// Everything that ties the scraper to the current QRZ.com pages: addresses, page titles,
    /// element ids, CSS selectors and site JavaScript functions.
    /// When QRZ changes its pages, start here (use "bd" and "bf" in the console to investigate).
    /// </summary>
    internal static class QrzSite
    {
        public static class Urls
        {
            public const string Home = "https://www.qrz.com/";
            public const string Login = "https://www.qrz.com/login";
            public const string Logbook = "https://logbook.qrz.com/";
            public const string Lookup = "https://www.qrz.com/db/";
            /// <summary>Followed by the callsign: opens the "add QSO" form, which also shows the "seen before" table.</summary>
            public const string AddCall = "https://logbook.qrz.com/logbook/?op=add;addcall=";
        }

        public static class Titles
        {
            public const string Login = "Login - QRZ.com";
            public const string AfterLogin = "Callsign Database - QRZ.com";
            public const string Logbook = "Logbook by QRZ.COM";
            public const string LookupSearch = "QRZ Callsign Database Search by QRZ Ham Radio";
            /// <summary>Shown instead of the lookup page when the daily lookup limit is reached.</summary>
            public const string Home = "The Home for Amateur Radio by QRZ Ham Radio";
        }

        public static class Navigation
        {
            /// <summary>Main menu; contains "Logout" when logged in.</summary>
            public const string Menu = "nav ul.primary-navigation";
            public const string MenuAny = "ul.primary-navigation";
            public const string LogoutOnAddPage = "#qrztop > nav > ul.primary-navigation > li.leaf.last > ul > li:nth-child(7) > a";
            public const string LogoutWithEditMenu = "body > div.qrztop > nav > ul.primary-navigation > li:nth-child(10) > ul > li:nth-child(7) > a";
            public const string LogoutDefault = "body > div.qrztop > nav > ul.primary-navigation > li:nth-child(10) > ul > li:nth-child(6) > a";
        }

        public static class LoginForm
        {
            public const string Username = "username";
            public const string Password = "password";
            /// <summary>Filled by the site after the username step ("Non Ham User" for non-hams).</summary>
            public const string FullName = "fullname";
            public const string UsernameError = "usernameError";
            public const string NextStepScript = "next();";
            public const string InvalidPasswordAlert = "body > div.container-fluid.main-content.theme-showcase > div > div.alert-container > div.alert.alert-warning";
            public const string InvalidPasswordText = "Invalid username or password";
        }

        public static class Lookup
        {
            public const string QueryBox = "tquery";
            public const string QueryInput = "#topcall > div.magic > div.tqueryWrapper > input.tquery";
            public const string Submit = "tsubmit";
            public const string ResultArea = "qrzcenter";
            /// <summary>Callsign data box; its children are: call, country, ?, name+address, ?, email.</summary>
            public const string CallData = "#calldata #csdata";
            public const string CallDataId = "csdata";
            public const string Details = "#t_detail #dt";
            public const string DetailsId = "dt";
            /// <summary>Site function that reveals the e-mail address.</summary>
            public const string ShowEmailScript = "showqem()";
        }

        public static class Logbook
        {
            /// <summary>Present only on the logbook main page (with #addcall).</summary>
            public const string PageMarker = "button-addon4";
            public const string AddCallBox = "addcall";
            public const string Table = "#lblistrs #lbtab";
            public const string TableId = "lbtab";
            public const string DateHeader = "#lblistrs #th_date";
            public const string DateHeaderId = "th_date";
            public const string SortAscClass = "sortAsc";
            public const string SortDescClass = "sortDesc";
            /// <summary>Hidden form used to open/delete a record (inputs: bookid, logpos, op).</summary>
            public const string RecordForm = "lbmenu";
            public const string Body = "lb_body";
            public const string QsoDetailText = "QSO Detail";
            public const string PageInput = "ipage";
            public const string GotoPageScript = "goto('page')";
            public const string RowsPerPage = "dispOpt_rpp";
            public const string RowsPerPageScript = "updateDisplayOptions('rpp')";
            /// <summary>Spinner shown while the logbook reloads via AJAX.</summary>
            public const string Loading = "filterLoading";
            public const string QsoCount = "#lbmenu span.qcnt span.mitm";
            public const string PageCountLabel = "#lbmenu #listnav span.input-group-text.hide-lt-1100";
            public const string SeenBeforeTable = "#lbform #lblist table.styledTable.seenBeforeTable";
            public const string EditScript = "lb_go('edit','')";
            public const string DeleteOperation = "logdel";
        }

        public static class QsoForm
        {
            public const string StartDate = "start_date";
            public const string EndDate = "end_date";
            public const string StartTime = "start_time";
            public const string EndTime = "end_time";
            public const string Freq1 = "freq1";
            public const string Freq2 = "freq2";
            public const string Band1 = "band1";
            public const string Band2 = "band2";
            public const string Mode1 = "mode1";
            public const string Mode2 = "mode2";
            public const string RstSent = "rst_sent";
            public const string RstReceived = "rst_rcvd";
            /// <summary>
            /// Comment textarea: it has no id, only name="comments1", and sits in a collapsed section
            /// (hidden fields are still submitted with the form). The id is kept as a fallback.
            /// </summary>
            public const string Comments = "textarea[name=\"comments1\"], #comments1";
            public const string Save = "savebut";
            public const string DefaultRst = "59";
        }
    }
}
