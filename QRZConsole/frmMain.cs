﻿using Microsoft.Win32;
using QRZLibrary;
using QRZLibrary.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Winsock_Orcas;
using Newtonsoft.Json;

namespace QRZConsole
{
    public partial class frmMain : Form
    {
        Logbook qrz;

        private delegate void SafeCallDelegate(string log, bool appendNewLine);
        private string lastQRZ = string.Empty;
        private string lastFreq = "7100";
        private string lastMode = "SSB";
        private string currentViewMode = "raw";

        private bool isTab = false;
        private bool isShiftTab = false;

        private Winsock ws;

        List<string> cmdHistory = new List<string>();
        int cmdHistoryIndex = 0;


        string ClusterAddr = "dxfun.com";
        int ClusterPort = 8000;
        private bool ClusterDebug = false;
        string ClusterDXprompt = "ClusterDX>";
        private bool clusterConnected = false;
        private int clusterDxItems = 30;

        private int splitterExpand = 225;
        private int splittercompress = 25;
        private int pixelsResize = 2;
        private bool SendCompletedRequest = false;
        private bool ClusterDXReconnectPending = false;

        private List<KeyValuePair<string, bool>> Locators = new List<KeyValuePair<string, bool>>();


        public frmMain()
        {
            InitializeComponent();

            qrz = new Logbook();
        }


        private void addMonitor(string log, bool appendNewLine = true)
        {
            if (txtMonitor.InvokeRequired)
            {
                var d = new SafeCallDelegate(addMonitor);
                txtMonitor.Invoke(d, new object[] { log });
            }
            else
            {
                txtMonitor.AppendText(log + (appendNewLine ? Environment.NewLine : string.Empty));
                txtMonitor.SelectionStart = txtMonitor.Text.Length;
                txtMonitor.ScrollToCaret();
            }
        }

        private void btnIsLogged_Click(object sender, EventArgs e)
        {
            IsLogged();
        }

        private void IsLogged()
        {
            addMonitor($"Start IsLogged request...");
            bool il = qrz.IsLogged();
            if (il)
                addMonitor($"IsLogged = {il.ToString()} by [{txtUsername.Text}]");
            else
                addMonitor($"IsLogged = {il.ToString()}");
            SaveRegKey("IsLoggedIn",  il?"1":"0");
        }

        private void IsLoggedInLastStatus()
        {
            bool loggedin = (GetRegKeyValue("IsLoggedIn") == "1");
            if (loggedin)
                addMonitor($"IsLogged (last status) = {loggedin.ToString()} by [{txtUsername.Text}]");
            else
                addMonitor($"IsLogged (last status) = {loggedin.ToString()}");
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogOut();
        }

        private void LogOut()
        {
            if (GetRegKeyValue("IsLoggedIn") != "1")
            {
                addMonitor($"Warning! IsLogged (last status) = False");
                addMonitor($"Before Logout start IsLogged request with il command]");
            }
            else
            {
                addMonitor($"Start Log Out...");
                bool lo = qrz.LogOut();
                addMonitor($"Logout = {lo.ToString()}");
                if (lo)
                {
                    SaveRegKey("IsLoggedIn", "0");

                    txtUsername.Text = string.Empty;
                    txtPassword.Text = string.Empty;
                    txtQRZLookup.Text = string.Empty;
                    txtLogbookPage.Text = "1";

                    qrz.Qrz = string.Empty;
                    qrz.Password = string.Empty;
                    
                    SaveRegKey("username", qrz.Qrz);
                    SaveRegKey("password", qrz.Password);
                    SaveRegKey("lookup", string.Empty);

                }
            }
        }

        private void btnQRZHome_Click(object sender, EventArgs e)
        {
            QRZHome();
        }

        private void QRZHome()
        {
            addMonitor($"Load QRZ Home...");
            addMonitor($"Load QRZ Home = {qrz.GotoQrzHome()}");
        }

        private void btnGotoLookbook_Click(object sender, EventArgs e)
        {
            GotoLogbook();
        }

        private void LoadLocators()
        {
            addMonitor($"Load Grid Locators Worked... It can take a long time");
            Locators = qrz.GetLocatorWorked();
            addMonitor($"Total Grid Locators Worked      = {Locators.Count.ToString()}");
            addMonitor($"Total Grid Locators Confirmed   = {Locators.Where(x => x.Value).Count().ToString()}");
            addMonitor($"Total Grid Locators Unconfirmed = {Locators.Where(x => !x.Value).Count().ToString()}");

            string locatorsSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(Locators);

            SaveRegKey($"{txtUsername.Text}_Locators", locatorsSerialized);

        }

        private void GotoLogbook()
        {
            addMonitor($"Load Logbook...");
            addMonitor($"Load Logbook = {qrz.GotoLogbook()}");

            int page = 1;
            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            if (currentViewMode == "raw")
                GetTableContenteRawView(page);
            else
                GetTableContentTextView(page);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            qrz.Qrz = txtUsername.Text;
            qrz.Password = txtPassword.Text;
            Login();
        }

        private void Login(string prm1 = "", string prm2 = "")
        {
            if (GetRegKeyValue("IsLoggedIn")=="1")
            {
                addMonitor($"Warning! IsLogged (last status) = True by [{GetRegKeyValue("username")}]");
                addMonitor($"Before new Login start IsLogged request with il command]");
            }
            else
            {
                if (prm1 != "")
                    qrz.Qrz = prm1.ToUpper();

                if (prm2 != "")
                    qrz.Password = prm2;

                if (qrz.Qrz != "" && qrz.Password != "")
                {
                    SaveRegKey("username", qrz.Qrz);
                    SaveRegKey("password", qrz.Password);

                    txtUsername.Text = qrz.Qrz;
                    txtPassword.Text = qrz.Password;

                    addMonitor($"Login... QRZ=[{qrz.Qrz}] - Password=[{new string('*', qrz.Password.Length)}]");

                    string errorMessage = string.Empty;
                    bool logged = qrz.Login(out errorMessage);
                    if (logged)
                    {
                        SaveRegKey("IsLoggedIn", "1");
                        addMonitor($"Logged in QRZ=[{qrz.Qrz}]");
                    }
                    else
                        addMonitor(errorMessage);
                }
                else
                {
                    addMonitor($"Unable to Login: username and password are required fields!");
                }
            }
        }

        private void GetLocators(string prm1 = "", string prm2 = "")
        {
            string Loc = string.Empty;
            string uc = string.Empty;
            
            if (prm1 != "")
                Loc = prm1.ToUpper().Trim();

            if (prm2 != "")
                uc = prm2.Trim();

            List<KeyValuePair<string, bool>> LocatorsFiltered = Locators;

            if (Loc != "" && Loc != "*")
            {
                LocatorsFiltered = Locators.Where(x => x.Key.StartsWith(Loc)).ToList();
            }

            if (uc.Equals("c"))
            {
                LocatorsFiltered = LocatorsFiltered.Where(x => x.Value).ToList();
            }

            if (uc.Equals("u"))
            {
                LocatorsFiltered = LocatorsFiltered.Where(x => !x.Value).ToList();
            }

            if (LocatorsFiltered.Count > 0)
            {
                foreach (KeyValuePair<string, bool> l in LocatorsFiltered)
                {
                    string confirmed = l.Value ? "Confirmed" : "Unconfirmed";
                    addMonitor($"{l.Key}   {confirmed}");
                }
                addMonitor("");
                addMonitor($"{LocatorsFiltered.Count} Items found");
            }
            else
            {
                addMonitor("0 items Found (try to load your Grid Locator List Worked using the [lc] command)");
            }
        }

        private void btnLookup_Click(object sender, EventArgs e)
        {
            string QRZtoSearch = txtQRZLookup.Text;

            QRZtoSearch = QRZtoSearch.ToUpper();

            Lookup(QRZtoSearch);
            lastQRZ = QRZtoSearch;
        }

        private void SetCurrentQRZ(string QRZ)
        {
            SaveRegKey("lookup", QRZ);
            lastQRZ = QRZ;
            addMonitor("");
            addMonitor($"Current QRZ: {QRZ}");
        }

        private void Lookup(string QRZtoSearch = "")
        {
            if (QRZtoSearch != "")
            {
                QRZtoSearch = QRZtoSearch.ToUpper();
                lastQRZ = QRZtoSearch;
            }
            else
                QRZtoSearch = lastQRZ;

            if (QRZtoSearch != "")
            {
                SaveRegKey("lookup", QRZtoSearch);
                txtQRZLookup.Text = QRZtoSearch;

                string LocatorWorked = string.Empty;
                string LocToSearch = string.Empty;

                addMonitor("");
                addMonitor($"Lookup {QRZtoSearch}...");
                LookupEntry entry = qrz.ExecQuery(QRZtoSearch);

                if (!string.IsNullOrEmpty(entry?.GridSquare))
                {
                    if (entry.GridSquare.Length >= 4)
                    {
                        LocToSearch = entry.GridSquare.Substring(0, 4);
                        if (Locators.Count > 0)
                        {
                            var l = Locators.Where(x => x.Key == LocToSearch)?.FirstOrDefault();
                            if (l.Value.Key != null)
                            {
                                string tmp = (l.Value.Value ? "Confirmed" : "Unconfirmed");
                                LocatorWorked = $"  (Worked and {tmp})";
                            }
                        }
                    }
                    
                }

                if (entry != null)
                {
                    addMonitor($"Resut = --->");
                    addMonitor($"   QRZ          = {entry.QRZ}");
                    addMonitor($"   Name         = {entry.Name}");
                    addMonitor($"   DXCC         = {entry.DXCC}");
                    addMonitor($"   Country      = {entry.Country}");
                    addMonitor($"   Grid Locator = {entry.GridSquare} {LocatorWorked}");
                    addMonitor($"   Address 1    = {entry.Address1}");
                    addMonitor($"   Address 2    = {entry.Address2}");
                    if (!string.IsNullOrEmpty(entry.Address3))
                        addMonitor($"   Address 3    = {entry.Address3}");
                    if (!string.IsNullOrEmpty(entry.UsState))
                        addMonitor($"   US State     = {entry.UsState}");
                    if (!string.IsNullOrEmpty(entry.UsCounty))
                        addMonitor($"   US County    = {entry.UsCounty}");
                    if (!string.IsNullOrEmpty(entry.Distance))
                        addMonitor($"   Distance     = {entry.Distance}");
                    addMonitor($"   Email        = {entry.Email}");
                    addMonitor($"------------------------------------");
                }
                else
                {
                    addMonitor("An Error cause unable to lookup your request!");
                }
            }
            else
                addMonitor($"No QRZ to search");
        }

        private void btnCheckWorked_Click(object sender, EventArgs e)
        {
            string QRZtoSearch = txtQRZLookup.Text;

            QRZtoSearch = QRZtoSearch.ToUpper();

            GetWorked(QRZtoSearch);

        }

        private void GetWorked(string QRZtoSearch = "")
        {
            if (QRZtoSearch != "")
            {
                QRZtoSearch = QRZtoSearch.ToUpper();
                lastQRZ = QRZtoSearch;
            }
            else
                QRZtoSearch = lastQRZ;

            if (QRZtoSearch != "")
            {
                SaveRegKey("lookup", QRZtoSearch);

                addMonitor("");
                addMonitor($"Check Worked {QRZtoSearch}...");

                string ret = qrz.CheckWorkedRaw(QRZtoSearch);
                if (ret != "")
                    addMonitor(ret);
                else
                    addMonitor("No result found!");
            }
            else
                addMonitor("No QRZ to check");
        }

        private void btnLookupAndCheck_Click(object sender, EventArgs e)
        {
            btnLookup.PerformClick();
            btnCheckWorked.PerformClick();
            txtQRZLookup.SelectAll();
            txtQRZLookup.Focus();
        }

        private void btnQSOCount_Click(object sender, EventArgs e)
        {
            QSOCount();
        }

        private void QSOCount()
        {
            addMonitor($"Get QSOs Count... ");
            addMonitor($"QSOs Count = {qrz.GetQSOsCount()}");
        }

        private void btnLogbookPages_Click(object sender, EventArgs e)
        {
            LogbookPages();
        }

        private void LogbookPages()
        {
            addMonitor($"Get Loogbook Pages... ");
            addMonitor($"Loogbook Pages = {qrz.GetLogbookPages()}");
        }

        private void btnCurrentPage_Click(object sender, EventArgs e)
        {
            CurrentPage();
        }

        private void CurrentPage()
        {
            addMonitor($"Get Current Loogbook Page... ");
            int currentPage = qrz.GetCurrentLogbookPage();
            if (currentPage <= 0)
            {
                addMonitor($"Unable to get Current Loogbook Page! Are you logged in?");
            }
            else
                addMonitor($"Current Loogbook Page = {currentPage}");

            if (currentPage > 0)
                txtLogbookPage.Text = currentPage.ToString();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Text = $"{Application.ProductName} v.{System.Windows.Forms.Application.ProductVersion}-beta";

            txtUsername.Text = GetRegKeyValue("username");
            txtPassword.Text = GetRegKeyValue("password");
            txtQRZLookup.Text = GetRegKeyValue("lookup");
            lastFreq = GetRegKeyValue("lastFreq");
            lastQRZ = txtQRZLookup.Text;
            if (!string.IsNullOrEmpty(GetRegKeyValue("currentViewMode")))
                currentViewMode = GetRegKeyValue("currentViewMode");

            string LocatorsList = (GetRegKeyValue($"{txtUsername.Text}_Locators"));
            if (!string.IsNullOrEmpty(LocatorsList))
            {
                Locators = JsonConvert.DeserializeObject<List<KeyValuePair<string, bool>>>(LocatorsList);
            }

            this.Show();

            chkIsLogged.Checked = (GetRegKeyValue("CheckIsLoggedIn") == "1");
            addMonitor("************************************************************************************************************************");

            addMonitor($"Welcome to {this.Text}");
            addMonitor("");
            addMonitor($"QRZ Console is a \"unofficial command-line interface\" to the QRZ.COM website and require a personal account.\r\nFor more information about the website, go to: https://www.qrz.com");
            addMonitor("");
            addMonitor($"This program require Microsoft Windows EDGE Browser (pre-installed since Windows 8.1)");
            addMonitor("");

            SetCurrentViewMode("");
            addMonitor("");
            chkShowHeader.Checked = (GetRegKeyValue("ShowHeader") == "1");

            if (!chkShowHeader.Checked)
                SwitchView();

            addMonitor("************************************************************************************************************************");
            addMonitor("");

            if (chkIsLogged.Checked)
                IsLogged();
            else
                IsLoggedInLastStatus();

            addMonitor("");
            txtCommand.Text = "insert command here (enter \"cm\" and press ENTER key for Command List)";
            txtCommand.SelectAll();
            txtCommand.Focus();
        }

        private void btnGotoPage_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            GotoPage(page);
        }

        private void SetCurrentViewMode(string mode = "")
        {
            if (mode != "")
            {
                if ((mode.ToUpper() != "RAW") && (mode.ToUpper() != "TEXT") && (mode.ToUpper() != "ADIF"))
                {
                    addMonitor($"Invalid View Mode! (mode is [raw], [text] or [adif])");
                }
                else
                {
                    addMonitor($"Set current Logbook View Mode: {mode}");
                    SaveRegKey("currentViewMode", mode.ToLower());
                    currentViewMode = mode;
                }
            }
            else
            {
                addMonitor($"Current Logbook View Mode is: {currentViewMode}");
            }
        }

        private void GotoPage(int page)
        {
            addMonitor($"Goto Page {page}... ");
            int currentPage = qrz.GotoLoogbookPage(page);
            addMonitor($"Current Page = {currentPage}");
            txtLogbookPage.Text = currentPage.ToString();

            if (currentPage > 0)
            {
                if (currentViewMode == "raw")
                    GetTableContenteRawView(currentPage);
                else if (currentViewMode == "text")
                    GetTableContentTextView(currentPage);
                else if (currentViewMode == "adif")
                    GetTableContentAdifView(currentPage);

            }

        }

        private void btnGetTableContentRaw_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            GetTableContenteRawView(page);
        }

        private void GetTableContenteRawView(int page)
        {
            if (page < 1)
                int.TryParse(txtLogbookPage.Text, out page);

            addMonitor($"Get Table content Raw of page {page}... ");
            addMonitor($"{qrz.GetLogbookPageContentRaw(page)}");
        }

        private void btnGetTableContentXML_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            GetTableContentXML(page);
        }

        private void GetTableContentXML(int page)
        {
            if (page < 1)
                int.TryParse(txtLogbookPage.Text, out page);

            addMonitor($"Get Table content XML of page {page}... ");

            List<LogbookEntry> lbentries = qrz.GetLogbookPageContent(page);
            if (lbentries != null)
            {
                if (lbentries.Count > 0)
                {
                    string xml = QRZHelper.SerializeLogbookEntryCollection(lbentries);
                    addMonitor("");
                    addMonitor($"************************************************************************************************");
                    addMonitor(xml);
                    addMonitor($"************************************************************************************************");
                    addMonitor("");
                }
                addMonitor($"Found {lbentries.Count} entries");
            }
        }

        private void GetQSObyRangeTextView(int start, int end)
        {
            if (end == 0)
                end = start;

            addMonitor($"Get QSOs by range (Text View): from position {start} to {end}... ");

            List<LogbookEntry> lbentries = qrz.GetLogbookEntriesByRange(start, end);
            if (lbentries != null)
            {
                if (lbentries.Count > 0)
                {
                    string headerLine = new string('-', 229);
                    addMonitor("");
                    addMonitor(headerLine);

                    string headerText = "";
                    headerText += "| ";
                    headerText += GetFixedString("Num.", 5);
                    headerText += " | ";
                    headerText += GetFixedString("Date - Time", 16);
                    headerText += " | ";
                    headerText += GetFixedString("Call", 13);
                    headerText += " | ";
                    headerText += GetFixedString("Band", 5);
                    headerText += " | ";
                    headerText += GetFixedString("Freq.", 7);
                    headerText += " | ";
                    headerText += GetFixedString("Mode", 6);
                    headerText += " | ";
                    headerText += GetFixedString("GridLoc", 8);
                    headerText += " | ";
                    headerText += GetFixedString("Country", 20);
                    headerText += " | ";
                    headerText += GetFixedString("Operator Name", 50);
                    headerText += " | ";
                    headerText += GetFixedString("Comments", 50);
                    headerText += " |";
                    headerText += "Confirmed";
                    headerText += " | ";
                    headerText += "LoTw";
                    headerText += " | ";
                    addMonitor(headerText);
                    addMonitor(headerLine);

                    foreach (LogbookEntry entry in lbentries)
                    {
                        string rowText = "";
                        rowText += "| ";
                        rowText += GetFixedString(entry.position.ToString().PadLeft(5), 5);
                        rowText += " | ";
                        rowText += GetFixedString(entry.QSODateTime.ToString("yyyy-MM-dd HH:mm"), 16);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Call, 13);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Band, 5);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Frequency, 7);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Mode, 6);
                        rowText += " | ";
                        rowText += GetFixedString(entry.GridLocator, 8);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Country, 20);
                        rowText += " | ";
                        rowText += GetFixedString(entry.OperatorName, 50);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Comments, 50);
                        rowText += " |";
                        rowText += entry.Confirmed ? "    *    " : "         ";
                        rowText += " | ";
                        rowText += entry.LoTWSent ? "  * " : "    ";
                        rowText += " | ";
                        addMonitor(rowText);
                    }
                    addMonitor(headerLine);

                }
                addMonitor($"Found {lbentries.Count} entries");
            }
        }

        
        private void GetQSObyRangeADIF(int start, int end)
        {
            if (end == 0)
                end = start;

            addMonitor($"Get QSOs by range (ADIF): from position {start} to {end}... ");

            List<LogbookEntry> lbentries = qrz.GetLogbookEntriesByRange(start, end);
            if (lbentries != null)
            {
                if (lbentries.Count > 0)
                {
                    addMonitor("");
                    foreach (LogbookEntry entry in lbentries)
                    {
                        string qsodate = entry.QSODateTime.ToString("yyyyMMdd");
                        string qsotime = entry.QSODateTime.ToString("HHmmss");

                        string rowText = "";
                        rowText += $"<CALL:{entry.Call.Length}>{entry.Call}{Environment.NewLine}";
                        rowText += $"   <BAND:{entry.Band.Length}>{entry.Band}{Environment.NewLine}";
                        rowText += $"   <QSO_DATE:{qsodate.Length}>{qsodate}{Environment.NewLine}";
                        rowText += $"   <TIME_ON:{qsotime.Length}>{qsotime}{Environment.NewLine}";
                        rowText += $"   <FREQ:{entry.Frequency.Length}>{entry.Frequency}{Environment.NewLine}";
                        rowText += $"   <MODE:{entry.Mode.Length}>{entry.Mode}{Environment.NewLine}";
                        rowText += $"   <DXCC:{entry.DXCC.Length}>{entry.DXCC}{Environment.NewLine}";
                        rowText += $"   <COUNTRY:{entry.Country.Length}>{entry.Country}{Environment.NewLine}";
                        rowText += $"   <NAME:{entry.OperatorName.Length}>{entry.OperatorName}{Environment.NewLine}";
                        rowText += $"   <GRIDSQUARE:{entry.GridLocator.Length}>{entry.GridLocator}{Environment.NewLine}";
                        rowText += $"   <COMMENT:{(entry.Comments ?? "").Length}>{entry.Comments ?? ""}{Environment.NewLine}";
                        rowText += $"<EOR>";

                        addMonitor(rowText);
                    }
                }
                addMonitor("");
                addMonitor($"Found {lbentries.Count} entries");
            }
        }

        private void SetCurrentFreq(string freq)
        {
            if (freq.Length > 3)
            {
                lastFreq = freq.Replace(".", "");
                SaveRegKey("lastFreq", freq);
                addMonitor("");
                addMonitor($"Current Frequency: {freq}");
            }
        }

        private void AddQSO(string qrzToAdd, string freq, string mode, string date, string time, string comment)
        {
            if (qrzToAdd != "" && qrzToAdd != "[CALL]" && freq != "" && mode != "" && date != "" && time != "")
            {
                if (freq.Length > 3)
                {
                    if (freq.IndexOf(".") < 0)
                    {
                        freq = freq.Insert(freq.Length - 3, ".");
                    }
                }

                qrzToAdd = qrzToAdd.ToUpper();
                mode = mode.ToUpper();

                bool QsoAdded = qrz.AddQSOToLogbook(qrzToAdd, freq, mode, date, time, comment);

                if (QsoAdded)
                {
                    lastMode = mode;
                    lastQRZ = qrzToAdd;
                    lastFreq = freq.Replace(".", "");
                    addMonitor("QSO added succesfully!");
                }
                else
                {
                    addMonitor("Unable to Add QSO on QRZ.COM. Please check the command format");
                    addMonitor("Remember that the frequencies must be entered in MHz and contain only one decimal point. (eg 7.076 or 7.07615)");
                    addMonitor("aq [qrz] [freq] [mode] [date] [time] [comment]");
                }
            }
            else
            {
                addMonitor("Invalid command! Please check the format:");
                addMonitor("aq [qrz] [freq] [mode] [date] [time] [comment]");
            }
        }

        private void DeleteQSO(int position1, int pos2, int pos3)
        {
            if (position1 > 0 && position1 == pos2 && pos2 == pos3)
            {
                addMonitor($"Deleting QSO from Logbook at position: {position1}...");
                bool QsoDeleted = qrz.DeleteQSOFromLogbook(position1);
                if (QsoDeleted)
                {
                    addMonitor("QSO deleted succesfully!");
                }
                else
                {
                    addMonitor("Unable to delete the QSO!");
                }
            }
            else
            {
                addMonitor("Invalid command! Please check the format:");
                addMonitor("Enter 3 times the param position after the dq command (for secuirity reasons!)");
                addMonitor("dq [position] [position] [position] ");
            }
        }

        private void EditQSO(string position, string qrzToAdd, string freq, string mode, string date, string time, string comment)
        {
            int pos = 0;
            if (int.TryParse(position, out int tmp))
            {
                pos = tmp;
            }

            if (pos > 0 && qrzToAdd != "" && freq != "" && mode != "" && date != "" && time != "")
            {
                if (freq.Length > 3)
                {
                    if (freq.IndexOf(".") < 0)
                    {
                        freq = freq.Insert(freq.Length - 3, ".");
                    }
                }

                qrzToAdd = qrzToAdd.ToUpper();
                mode = mode.ToUpper();

                bool QsoEdited = qrz.EditQSOToLogbook(pos, qrzToAdd, freq, mode, date, time, comment);

                if (QsoEdited)
                {
                    lastMode = mode;
                    lastQRZ = qrzToAdd;
                    lastFreq = lastFreq = freq.Replace(".", "");
                    addMonitor("QSO edited succesfully!");
                }
                else
                {
                    addMonitor("Unable to Edit QSO on QRZ.COM. Please check the command format");
                    addMonitor("Remember that the frequencies must be entered in MHz and contain only one decimal point. (eg 7.076 or 7.07615)");
                    addMonitor("eq [position] [qrz] [freq] [mode] [date] [time] [comment]");
                }
            }
            else
            {
                addMonitor("Invalid command! Please check the format:");
                addMonitor("eq [position] [qrz] [freq] [mode] [date] [time] [comment]");
            }
        }

        private void GetQSObyRangeTextRaw(int start, int end)
        {
            if (end == 0)
                end = start;

            addMonitor($"Get QSOs by range (Text Raw): from position {start} to {end}... ");

            List<LogbookEntry> lbentries = qrz.GetLogbookEntriesByRange(start, end);
            if (lbentries != null)
            {
                if (lbentries.Count > 0)
                {

                    string headerText = "";
                    headerText += "Num.";
                    headerText += "\t";
                    headerText += "Date Time";
                    headerText += "\t";
                    headerText += "Call";
                    headerText += "\t";
                    headerText += "Band";
                    headerText += "\t";
                    headerText += "Frequency";
                    headerText += "\t";
                    headerText += "Mode";
                    headerText += "\t";
                    headerText += "GridLocator";
                    headerText += "\t";
                    headerText += "Country";
                    headerText += "\t";
                    headerText += "Operator Name";
                    headerText += "\t";
                    headerText += "Comments";
                    headerText += "\t";
                    headerText += "Confirmed";
                    headerText += "\t";
                    headerText += "LoTW";
                    addMonitor(headerText);

                    foreach (LogbookEntry entry in lbentries)
                    {
                        string rowText = "";
                        rowText += entry.position;
                        rowText += "\t";
                        rowText += entry.QSODateTime.ToString("yyyy-MM-dd HH:mm");
                        rowText += "\t";
                        rowText += entry.Call;
                        rowText += "\t";
                        rowText += entry.Band;
                        rowText += "\t";
                        rowText += entry.Frequency;
                        rowText += "\t";
                        rowText += entry.Mode;
                        rowText += "\t";
                        rowText += entry.GridLocator;
                        rowText += "\t";
                        rowText += entry.Country;
                        rowText += "\t";
                        rowText += entry.OperatorName;
                        rowText += "\t";
                        rowText += entry.Comments;
                        rowText += "\t";
                        rowText += entry.Confirmed ? "*" : "";
                        rowText += "\t";
                        rowText += entry.LoTWSent ? "*" : "";
                        addMonitor(rowText);
                    }
                    addMonitor("");
                }
                addMonitor($"Found {lbentries.Count} entries");
            }
        }

        private void GetTableContentTextView(int page)
        {
            if (page < 1)
                int.TryParse(txtLogbookPage.Text, out page);

            addMonitor($"Get Table content text of page {page}... ");

            List<LogbookEntry> lbentries = qrz.GetLogbookPageContent(page);
            if (lbentries != null)
            {
                if (lbentries.Count > 0)
                {
                    string headerLine = new string('-', 223);
                    addMonitor("");
                    addMonitor(headerLine);

                    string headerText = "";
                    headerText += "| ";
                    headerText += GetFixedString("Num.", 5);
                    headerText += " | ";
                    headerText += GetFixedString("Date - Time", 16);
                    headerText += " | ";
                    headerText += GetFixedString("Call", 13);
                    headerText += " | ";
                    headerText += GetFixedString("Band", 5);
                    headerText += " | ";
                    headerText += GetFixedString("Freq.", 7);
                    headerText += " | ";
                    headerText += GetFixedString("Mode", 6);
                    headerText += " | ";
                    headerText += "QSL";
                    headerText += " | ";
                    headerText += GetFixedString("GridLoc", 8);
                    headerText += " | ";
                    headerText += GetFixedString("Country", 20);
                    headerText += " | ";
                    headerText += GetFixedString("Operator Name", 50);
                    headerText += " | ";
                    headerText += GetFixedString("Comments", 50);
                    headerText += " |";
                    headerText += "LoTW";
                    headerText += " | ";
                    addMonitor(headerText);
                    addMonitor(headerLine);

                    foreach (LogbookEntry entry in lbentries)
                    {
                        string rowText = "";
                        rowText += "| ";
                        rowText += GetFixedString(entry.position.ToString().PadLeft(5), 5);
                        rowText += " | ";
                        rowText += GetFixedString(entry.QSODateTime.ToString("yyyy-MM-dd HH:mm"), 16);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Call, 13);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Band, 5);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Frequency, 7);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Mode, 6);
                        rowText += " | ";
                        rowText += entry.Confirmed ? " * " : "   ";
                        rowText += " | ";
                        rowText += GetFixedString(entry.GridLocator, 8);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Country, 20);
                        rowText += " | ";
                        rowText += GetFixedString(entry.OperatorName, 50);
                        rowText += " | ";
                        rowText += GetFixedString(entry.Comments, 50);
                        rowText += " |";
                        rowText += entry.LoTWSent ? "  * " : "    ";
                        rowText += " | ";
                        addMonitor(rowText);
                    }
                    addMonitor(headerLine);

                }
                addMonitor($"Found {lbentries.Count} entries");
            }
        }
        private void GetTableContentAdifView(int page)
        {
            if (page < 1)
                int.TryParse(txtLogbookPage.Text, out page);

            addMonitor($"Get Table content ADIF of page {page}... ");

            List<LogbookEntry> lbentries = qrz.GetLogbookPageContent(page);
            if (lbentries != null)
            {
                if (lbentries.Count > 0)
                {
                    addMonitor("");
                    foreach (LogbookEntry entry in lbentries)
                    {
                        string qsodate = entry.QSODateTime.ToString("yyyyMMdd");
                        string qsotime = entry.QSODateTime.ToString("HHmmss");

                        string rowText = "";
                        rowText += $"<CALL:{entry.Call.Length}>{entry.Call}{Environment.NewLine}";
                        rowText += $"   <BAND:{entry.Band.Length}>{entry.Band}{Environment.NewLine}";
                        rowText += $"   <QSO_DATE:{qsodate.Length}>{qsodate}{Environment.NewLine}";
                        rowText += $"   <TIME_ON:{qsotime.Length}>{qsotime}{Environment.NewLine}";
                        rowText += $"   <FREQ:{entry.Frequency.Length}>{entry.Frequency}{Environment.NewLine}";
                        rowText += $"   <MODE:{entry.Mode.Length}>{entry.Mode}{Environment.NewLine}";
                        rowText += $"   <DXCC:{entry.DXCC.Length}>{entry.DXCC}{Environment.NewLine}";
                        rowText += $"   <COUNTRY:{entry.Country.Length}>{entry.Country}{Environment.NewLine}";
                        rowText += $"   <NAME:{entry.OperatorName.Length}>{entry.OperatorName}{Environment.NewLine}";
                        rowText += $"   <GRIDSQUARE:{entry.GridLocator.Length}>{entry.GridLocator}{Environment.NewLine}";
                        rowText += $"   <COMMENT:{(entry.Comments ?? "").Length}>{entry.Comments??""}{Environment.NewLine}";
                        rowText += $"<EOR>";

                        addMonitor(rowText);
                    }
                }
                addMonitor("");
                addMonitor($"Found {lbentries.Count} entries");
            }
        }

        private void btnPageDown_Click(object sender, EventArgs e)
        {
            PageDown();
        }

        private void PageDown()
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            if (page > 1)
                page = page - 1;

            txtLogbookPage.Text = page.ToString();

            GotoPage(page);
        }

        private void btnPageUp_Click(object sender, EventArgs e)
        {
            PageUp();
        }

        private void PageUp()
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            page++;

            txtLogbookPage.Text = page.ToString();

            GotoPage(page);
        }

        private void SaveRegKey(string key, string value)
        {
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\{Application.ProductName}");

            regkey.SetValue(key, value);
            regkey.Close();
        }

        private string GetRegKeyValue(string key)
        {
            string ret = string.Empty;

            RegistryKey regkey = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\{Application.ProductName}");

            //if it does exist, retrieve the stored values  
            if (regkey != null)
            {
                ret = regkey.GetValue(key)?.ToString();
                regkey.Close();
            }
            return ret;
        }

        private void txtQRZLookup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                btnLookupAndCheck.PerformClick();
            }
        }

        private void btnClearMonitor_Click(object sender, EventArgs e)
        {
            ClearMonitor();
        }

        private void ClearMonitor()
        {
            txtMonitor.Text = string.Empty;
        }

        private void GetLastData()
        {
            int dataLen = 20;
            addMonitor("Last data in memory:");
            addMonitor(GetFixedString($"  QRZ", dataLen) + lastQRZ);
            addMonitor(GetFixedString($"  Freq.", dataLen) + lastFreq);
            addMonitor(GetFixedString($"  Mode", dataLen) + lastMode);
        }

        private void btnOrderDateAsc_Click(object sender, EventArgs e)
        {
            OrderDateAsc();
        }

        private void OrderDateAsc()
        {
            addMonitor($"Set Logbook Order Date Asc... ");
            int currOrder = qrz.SetLogbookDateOrder(0);
            if (currOrder == 0)
                addMonitor($"Logbook Order Date is \"Asc\"");
            else
                addMonitor($"Unable to Set Logbook Order Date Asc");
        }

        private void GetEntriesForPage()
        {
            addMonitor($"Get QSO for page... ");
            addMonitor($"QSO for page = {qrz.GetEntriesForPage()}");
        }

        private void SetEntriesForPage(int entries)
        {
            addMonitor($"Set QSO for page to {entries}... ");
            string msg = string.Empty;
            int ret = qrz.SetEntriesForPage(entries, out msg);

            if (!string.IsNullOrEmpty(msg))
                addMonitor(msg);
            else
            {
                if (ret == entries)
                    addMonitor($"QSO for page = {entries}");
                else
                    addMonitor($"Unable to set QSO for page = {entries}");
            }
        }

        private void btnOrderDateDesc_Click(object sender, EventArgs e)
        {
            OrderDateDesc();
        }

        private void OrderDateDesc()
        {
            addMonitor($"Set Logbook Order Date Desc... ");
            int currOrder = qrz.SetLogbookDateOrder(1);
            if (currOrder == 1)
                addMonitor($"Logbook Order Date is \"Desc\"");
            else
                addMonitor($"Unable to Set Logbook Order Date Desc");
        }

        private void txtCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Up)
            {
                if (cmdHistory.Count > 0)
                {
                    if ((cmdHistoryIndex + 1) <= cmdHistory.Count && cmdHistoryIndex > 0)
                    {
                        cmdHistoryIndex--;
                    }
                    txtCommand.Text = cmdHistory.ElementAt(cmdHistoryIndex);
                    e.SuppressKeyPress = true;
                    txtCommand.SelectAll();
                }
            }

            if (e.KeyCode == Keys.Down)
            {
                if (cmdHistory.Count > 0)
                {
                    if ((cmdHistoryIndex + 1) < cmdHistory.Count)
                    {
                        cmdHistoryIndex++;
                    }
                    txtCommand.Text = cmdHistory.ElementAt(cmdHistoryIndex);
                    e.SuppressKeyPress = true;
                    txtCommand.SelectAll();
                }
            }

        }

        private void btnDing_Click(object sender, EventArgs e)
        {

        }

        private void txtCommand_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

                elabCommand(txtCommand.Text);
                e.Handled = true;
            }
        }

        private void txtCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void elabCommand(string command)
        {
            if (command.Trim() != "")
            {
                string cmd = string.Empty;
                string[] param;
                if (command.IndexOf(" ") > 0)
                {
                    param = command.Split(" ".ToCharArray()[0]);
                    cmd = param[0];
                }
                else
                {
                    cmd = command;
                }

                string prm1 = string.Empty;
                string prm2 = string.Empty;
                string prm3 = string.Empty;
                string prm4 = string.Empty;
                string prm5 = string.Empty;
                string prm6 = string.Empty;
                string prm7 = string.Empty;
                if (command.IndexOf(" ") > 0)
                {
                    if (command.Split(" ".ToCharArray()[0]).Length > 1)
                        prm1 = command.Split(" ".ToCharArray()[0])[1];
                    if (command.Split(" ".ToCharArray()[0]).Length > 2)
                        prm2 = command.Split(" ".ToCharArray()[0])[2];
                    if (command.Split(" ".ToCharArray()[0]).Length > 3)
                        prm3 = command.Split(" ".ToCharArray()[0])[3];
                    if (command.Split(" ".ToCharArray()[0]).Length > 4)
                        prm4 = command.Split(" ".ToCharArray()[0])[4];
                    if (command.Split(" ".ToCharArray()[0]).Length > 5)
                        prm5 = command.Split(" ".ToCharArray()[0])[5];
                    if (command.Split(" ".ToCharArray()[0]).Length > 6)
                        prm6 = command.Split(" ".ToCharArray()[0])[6];
                    if (command.Split(" ".ToCharArray()[0]).Length > 7)
                        prm7 = command.Split(" ".ToCharArray()[0])[7];
                }

                cmd = cmd.ToLower();

                addMonitor("");
                addMonitor($">{command}");

                int iprm1 = 0;
                if (int.TryParse(prm1, out int tmp1))
                    iprm1 = tmp1;

                int iprm2 = 0;
                if (int.TryParse(prm2, out int tmp2))
                    iprm2 = tmp2;

                int iprm3 = 0;
                if (int.TryParse(prm3, out int tmp3))
                    iprm3 = tmp3;

                int iprm4 = 0;
                if (int.TryParse(prm4, out int tmp4))
                    iprm4 = tmp4;

                int iprm5 = 0;
                if (int.TryParse(prm5, out int tmp5))
                    iprm5 = tmp5;

                int iprm6 = 0;
                if (int.TryParse(prm6, out int tmp6))
                    iprm6 = tmp6;

                int iprm7 = 0;
                if (int.TryParse(prm7, out int tmp7))
                    iprm7 = tmp7;

                switch (cmd)
                {
                    case "cm":
                        CommandList();
                        break;
                    case "sl":
                        ShortcutList();
                        break;
                    case "il":
                        IsLogged();
                        break;
                    case "ls":
                        IsLoggedInLastStatus();
                        break;
                    case "li":
                        Login(prm1, prm2);
                        break;
                    case "lo":
                        LogOut();
                        break;
                    case "lu":
                        Lookup(prm1);
                        break;
                    case "qq":
                        SetCurrentQRZ(prm1);
                        break;
                    case "cw":
                        GetWorked(prm1);
                        break;
                    case "lw":
                        Lookup(prm1);
                        GetWorked(prm1);
                        break;
                    case "wl":
                        GetWorked(prm1);
                        Lookup(prm1);
                        break;
                    case "hm":
                        QRZHome();
                        break;
                    case "lb":
                        GotoLogbook();
                        break;
                    case "lc":
                        LoadLocators();
                        break;
                    case "gl":
                        GetLocators(prm1, prm2);
                        break;
                    case "qc":
                        QSOCount();
                        break;
                    case "qp":
                        if (iprm1 > 0)
                            SetEntriesForPage(iprm1);
                        else
                            GetEntriesForPage();
                        break;
                    case "lp":
                        LogbookPages();
                        break;
                    case "cv":
                        SetCurrentViewMode(prm1);
                        break;
                    case "gp":
                        GotoPage(iprm1);
                        break;
                    case "pd":
                        PageDown();
                        break;
                    case "pu":
                        PageUp();
                        break;
                    case "cp":
                        CurrentPage();
                        break;
                    case "da":
                        OrderDateAsc();
                        break;
                    case "dd":
                        OrderDateDesc();
                        break;
                    case "tt":
                        GetTableContentTextView(iprm1);
                        break;
                    case "tr":
                        GetTableContenteRawView(iprm1);
                        break;
                    case "tx":
                        GetTableContentXML(iprm1);
                        break;
                    case "qt":
                        GetQSObyRangeTextView(iprm1, iprm2);
                        break;
                    case "qr":
                        GetQSObyRangeTextRaw(iprm1, iprm2);
                        break;
                    case "qa":
                        GetQSObyRangeADIF(iprm1, iprm2);
                        break;
                    case "aq":
                        if (prm6 != string.Empty)
                        {
                            string tmp = (cmd + " " + prm1 + " " + prm2 + " " + prm3 + " " + prm4 + " " + prm5 + " ");
                            prm6 = command.Substring(tmp.Length, (command.Length - tmp.Length));
                        }
                        AddQSO(prm1, prm2, prm3, prm4, prm5, prm6);
                        break;
                    case "ff":
                        SetCurrentFreq(prm1);
                        break;
                    case "eq":
                        if (prm7 != string.Empty)
                        {
                            string tmp = (cmd + " " + prm1 + " " + prm2 + " " + prm3 + " " + prm4 + " " + prm5 + " " + prm6 + " ");
                            prm7 = command.Substring(tmp.Length, (command.Length - tmp.Length));
                        }
                        EditQSO(prm1, prm2, prm3, prm4, prm5, prm6, prm7);
                        break;
                    case "dq":
                        DeleteQSO(iprm1, iprm2, iprm3);
                        break;
                    case "cl":
                        ClearMonitor();
                        break;
                    case "sw":
                        SwitchView();
                        break;
                    case "sc":
                        SwitchCheckIsLoggedAtStartup();
                        break;
                    case "fs":
                        SwitchFullScreen();
                        break;
                    case "dx":
                        ClusterDxOpen(prm1, prm2);
                        break;
                    case "cc":
                        ClusterDxClose();
                        break;
                    case "ss":
                        if (prm3 != string.Empty)
                        {
                            string tmp = (cmd + " " + prm1 + " " + prm2 + " ");
                            prm3 = command.Substring(tmp.Length, (command.Length - tmp.Length));
                        }
                        ClusterDxSendSpot(prm1, prm2, prm3);
                        break;
                    case "sb":
                        ClusterDxSelectBand(prm1, iprm2);
                        break;
                    case "sf":
                        if (string.IsNullOrEmpty(prm1.Trim()))
                        {
                            prm1 = lastFreq;
                        }
                        ClusterDxSelectBand(prm1 + "/" + prm1, iprm2);
                        break;
                    case "ds":
                        ClusterDxSearchCallsign(prm1);
                        break;
                    case "di":
                        ClusterDxItems(iprm1);
                        break;
                    case "cd":
                        ClusterDebugSwitch();
                        break;
                    case "dc":
                        ClusterDxCmd(command);
                        break;
                    case "ld":
                        GetLastData();
                        break;
                    case "fc":
                        if (prm1 != string.Empty)
                        {
                            string tmp = (cmd + " ");
                            prm1 = command.Substring(tmp.Length, (command.Length - tmp.Length));
                        }
                        SearchDCXXByCountry(prm1);
                        break;
                    case "qi":
                    case "exit":
                        this.Close();
                        break;
                    default:
                        addMonitor($"invalid comamnd: {command}");
                        break;
                }
            }

            cmdHistory.Add(command);
            cmdHistoryIndex = cmdHistory.Count - 1;

            txtCommand.SelectAll();
            txtCommand.Focus();
        }

        private void SearchDCXXByCountry(string country)
        {
            addMonitor("Search DCXX by country...");
            List<KeyValuePair<int, string>> clist = new List<KeyValuePair<int, string>>();

            clist = QRZHelper.GetListDXCCByCountry(country);
            if (clist.Count > 0)
            {
                foreach (KeyValuePair<int, string> c in clist)
                {
                    addMonitor($"{GetFixedString(c.Key.ToString().PadLeft(4), 4)} {c.Value}");
                }
                addMonitor("");
            }
            addMonitor($"Found {clist.Count} DXCC");

        }

        private void SwitchFullScreen()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                addMonitor("Console switched to normal screen");
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                addMonitor("Console switched to full screen");
            }
        }
        private void SwitchCheckIsLoggedAtStartup()
        {
            if (chkIsLogged.Checked)
                chkIsLogged.Checked = false;
            else
                chkIsLogged.Checked = true;
        }

        private void btnSwitchView_Click(object sender, EventArgs e)
        {
            SwitchView();
        }

        private void SwitchView()
        {
            string extra = "";
            if (btnSwitchView.Text == "-")
            {
                splitContainer1.SplitterDistance = splittercompress;
                btnSwitchView.Text = "+";
                extra = " (digit sw and press ENTER for show header)";

            }
            else
            {
                splitContainer1.SplitterDistance = splitterExpand;
                btnSwitchView.Text = "-";
            }
            addMonitor($"Console View switched{extra}");
        }

        private void resetSplitter()
        {
            if (btnSwitchView.Text == "-")
            {
                splitContainer1.SplitterDistance = splitterExpand;
            }
            else
            {
                splitContainer1.SplitterDistance = splittercompress;
            }
        }

        private void btnCommandList_Click(object sender, EventArgs e)
        {
            CommandList();
        }

        private void ShortcutList()
        {
            int cmdlen = 40;
            addMonitor($"Shortcut List");
            addMonitor($"-------------------------------------------------------------------------");
            addMonitor(GetFixedString($"  Description", cmdlen) + "Keys...");
            addMonitor($"-------------------------------------------------------------------------");
            addMonitor("View:");
            addMonitor(GetFixedString($"  Zoom in", cmdlen) + "CTRL++");
            addMonitor(GetFixedString($"  Zoom out", cmdlen) + "CTRL+-");
            addMonitor(GetFixedString($"  Zoom reset", cmdlen) + "CTRL+0");
            addMonitor("Logbook:");
            addMonitor(GetFixedString($"  Page Up (pu)", cmdlen) + "CTRL+PagUp");
            addMonitor(GetFixedString($"  Page Down (pd)", cmdlen) + "CTRL+PagDown");
            addMonitor("Behaviors:");
            addMonitor(GetFixedString($"  Set the cursor in the command field", cmdlen) + "F2");
            addMonitor(GetFixedString($"  Moving back the command list", cmdlen) + "Arrow Key Up");
            addMonitor(GetFixedString($"  Moving forward the command list", cmdlen) + "Arrow Key Down");
            addMonitor($"-------------------------------------------------------------------------");

        }

        private void CommandList()
        {
            int cmdlen = 40;
            addMonitor($"Command List");
            addMonitor($"-------------------------------------------------------------------------");
            addMonitor(GetFixedString($"  Description", cmdlen) + "command [param] ...");
            addMonitor($"-------------------------------------------------------------------------");
            addMonitor("Credentials:");
            addMonitor(GetFixedString($"  Is Logged", cmdlen) + "il");
            addMonitor(GetFixedString($"  Is Logged (last status)", cmdlen) + "ls");
            addMonitor(GetFixedString($"  Login", cmdlen) + "li [username] [password]");
            addMonitor(GetFixedString($"  LogOut", cmdlen) + "lo");
            addMonitor("Search:");
            addMonitor(GetFixedString($"  Lookup", cmdlen) + "lu [qrz]");
            addMonitor(GetFixedString($"  Check Worked", cmdlen) + "cw [qrz]");
            addMonitor(GetFixedString($"  Lookup and Check Worked", cmdlen) + "lw [qrz]");
            addMonitor(GetFixedString($"  Check Worked and Lookup", cmdlen) + "wl [qrz]");
            addMonitor("Logbook:");
            addMonitor(GetFixedString($"  Add QSO", cmdlen) + "aq [qrz] [freq] [mode] [date] [time] [comment]");
            addMonitor(GetFixedString($"  Edit QSO", cmdlen) + "eq [position] [qrz] [freq] [mode] [date] [time] [comment]");
            addMonitor(GetFixedString($"  Delete QSO", cmdlen) + "dq [position] [position] [position] (enter 3 times the same position to delete for security reasons)");
            addMonitor(GetFixedString($"  Open Logbook", cmdlen) + "lb");
            addMonitor(GetFixedString($"  QSO Count", cmdlen) + "qc");
            addMonitor(GetFixedString($"  Logbook pages", cmdlen) + "lp");
            addMonitor(GetFixedString($"  Get/Set QSO for page", cmdlen) + "qp [entries] (valid values: 5, 10, 15, 20, 25, 50, 100, 200)");
            addMonitor(GetFixedString($"  Get/Set current view mode", cmdlen) + "cv [mode] (mode: [raw], [text] OR [adif])");
            addMonitor(GetFixedString($"  Goto Page", cmdlen) + "gp [page]");
            addMonitor(GetFixedString($"  Page Down", cmdlen) + "pd");
            addMonitor(GetFixedString($"  Page Up", cmdlen) + "pu");
            addMonitor(GetFixedString($"  Current Page", cmdlen) + "cp");
            addMonitor(GetFixedString($"  Order Date Asc", cmdlen) + "da");
            addMonitor(GetFixedString($"  Order Date Desc", cmdlen) + "dd");
            addMonitor(GetFixedString($"  Get Table Contente Text", cmdlen) + "tt [page]");
            addMonitor(GetFixedString($"  Get Table Contente Raw", cmdlen) + "tr [page]");
            addMonitor(GetFixedString($"  Get Table Content XML", cmdlen) + "tx [page]");
            addMonitor(GetFixedString($"  Get QSOs by range Text View", cmdlen) + "qt [start position] [end position]");
            addMonitor(GetFixedString($"  Get QSOs by range Text Raw", cmdlen) + "qr [start position] [end position]");
            addMonitor(GetFixedString($"  Get QSOs by range ADIF", cmdlen) + "qa [start position] [end position]");
            addMonitor("Cluster DX:");
            addMonitor(GetFixedString($"  Open Cluster DX", cmdlen) + "dx");
            addMonitor(GetFixedString($"  Show DX on Band", cmdlen) + "sb [band] [items] (band example: [40m] [10m] or [hf] [vhf] [uhf])");
            addMonitor(GetFixedString($"  Show DX on Freq", cmdlen) + "sf [freq] [items] (freq example: [7155] [14200] ...)");
            addMonitor(GetFixedString($"  Search Callsign on Cluster DX", cmdlen) + "ds [callsign]");
            addMonitor(GetFixedString($"  DXSpider command", cmdlen) + "dc [DXSpider command: http://www.dxcluster.org/main/usermanual_en-12.html]");
            addMonitor(GetFixedString($"  Send Spot", cmdlen) + "ss [call] [freq] [comment]");
            addMonitor(GetFixedString($"  Close Cluster DX", cmdlen) + "cc");
            addMonitor("General/Utility:");
            addMonitor(GetFixedString($"  Clear Monitor", cmdlen) + "cl");
            addMonitor(GetFixedString($"  Find DXCC Countries", cmdlen) + "fc [search]");
            addMonitor(GetFixedString($"  Load Your Grid Locator", cmdlen) + "lc");
            addMonitor(GetFixedString($"  Get Grid Locator List", cmdlen) + "gl [loc] [c or u] (example: gl [*] [c] - gl [EN70]");
            addMonitor(GetFixedString($"  Set current Frequency", cmdlen) + "ff [freq]");
            addMonitor(GetFixedString($"  Set current QRZ", cmdlen) + "qq [qrz]");
            addMonitor(GetFixedString($"  Switch View", cmdlen) + "sw");
            addMonitor(GetFixedString($"  Switch Check Is Logged at startup", cmdlen) + "sc");
            addMonitor(GetFixedString($"  Switch screen (normal/fullsize)", cmdlen) + "fs");
            addMonitor(GetFixedString($"  Last data in memory", cmdlen) + "ld");
            addMonitor(GetFixedString($"  Command List", cmdlen) + "cm");
            addMonitor(GetFixedString($"  Shortcut List", cmdlen) + "sl");
            addMonitor(GetFixedString($"  Quit", cmdlen) + "qi");
            addMonitor($"-------------------------------------------------------------------------");
        }

        private void frmTest_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                e.Handled = true;
                txtCommand.SelectAll();
                txtCommand.Focus();
            }

            if (e.Control && (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus))
            {
                e.Handled = true;

                if (txtMonitor.Font.Size <= 40)
                {
                    txtMonitor.Font = new Font(txtMonitor.Font.FontFamily, txtMonitor.Font.Size + 1);
                    txtCommand.Font = txtMonitor.Font;
                    splitterExpand = splitterExpand + pixelsResize;
                    splittercompress = splittercompress + pixelsResize;
                    btnSwitchView.Height = btnSwitchView.Height + pixelsResize;
                    btnSwitchView.Width = btnSwitchView.Width + pixelsResize;
                    btnSwitchView.Top = btnSwitchView.Top - pixelsResize;
                    btnSwitchView.Left = btnSwitchView.Left - pixelsResize;
                    resetSplitter();
                }
            }

            if (e.Control && (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus))
            {
                e.Handled = true;

                if (txtMonitor.Font.Size >= 6)
                {
                    txtMonitor.Font = new Font(txtMonitor.Font.FontFamily, txtMonitor.Font.Size - 1);
                    txtCommand.Font = txtMonitor.Font;
                    splitterExpand = splitterExpand - pixelsResize;
                    splittercompress = splittercompress - pixelsResize;
                    btnSwitchView.Height = btnSwitchView.Height - pixelsResize;
                    btnSwitchView.Width = btnSwitchView.Width - pixelsResize;
                    btnSwitchView.Top = btnSwitchView.Top + pixelsResize;
                    btnSwitchView.Left = btnSwitchView.Left + pixelsResize;
                    resetSplitter();

                }
            }

            if (e.Control && (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0))
            {
                e.Handled = true;

                txtMonitor.Font = new Font(txtMonitor.Font.FontFamily, 8.25f);
                txtCommand.Font = txtMonitor.Font;
                splitterExpand = 225;
                splittercompress = 25;
                btnSwitchView.Height = 20;
                btnSwitchView.Width = 20;
                btnSwitchView.Top = txtCommand.Top;
                btnSwitchView.Left = txtCommand.Width - 20;
                resetSplitter();
            }

            if (e.KeyCode == Keys.PageDown)
            {

                if (e.Control)
                {
                    e.Handled = true;
                    txtCommand.Text = "pd";
                    elabCommand(txtCommand.Text);

                }
                else
                {
                    if (!txtMonitor.Focused)
                    {
                        txtMonitor.Focus();
                        SendKeys.Send("{PGDN}");
                    }
                }
            }

            if (e.KeyCode == Keys.PageUp)
            {

                if (e.Control)
                {
                    e.Handled = true;
                    txtCommand.Text = "pu";
                    elabCommand(txtCommand.Text);

                }
                else
                {
                    if (!txtMonitor.Focused)
                    {
                        txtMonitor.Focus();
                        SendKeys.Send("{PGUP}");
                    }
                }
            }

            //if (e.KeyCode == Keys.Up)
            //{
            //    if (!txtMonitor.Focused)
            //    {
            //        txtMonitor.Focus();
            //        SendKeys.Send("{UP}");
            //    }
            //}

            //if (e.KeyCode == Keys.Down)
            //{
            //    if (!txtMonitor.Focused)
            //    {
            //        txtMonitor.Focus();
            //        SendKeys.Send("{DOWN}");
            //    }
            //}

        }

        private string GetFixedString(string str, int len)
        {
            if (string.IsNullOrEmpty(str))
                str = "";
            //return str.PadRight(len);
            return str.PadRight(len).Substring(0, len);
        }

        private void chkIsLogged_CheckedChanged(object sender, EventArgs e)
        {
            SaveRegKey("CheckIsLoggedIn", chkIsLogged.Checked?"1":"0");
            string cilasu = "disabled";
            if (chkIsLogged.Checked)
                cilasu = "enabled";
            addMonitor($"Check Is Logged at startup = {cilasu}");
        }

        private void chkShowHeader_CheckedChanged(object sender, EventArgs e)
        {
            SaveRegKey("ShowHeader", chkShowHeader.Checked ? "1" : "0");
            string cilasu = "disabled";
            if (chkShowHeader.Checked)
                cilasu = "enabled";
            addMonitor($"Show Header at startup = {cilasu}");
        }

        private void GetTableContentText_Click(object sender, EventArgs e)
        {
            int page = 1;

            if (int.TryParse(txtLogbookPage.Text, out int tmp))
                page = tmp;

            GetTableContentTextView(page);
        }

        private void btnQSOforPage_Click(object sender, EventArgs e)
        {
            GetEntriesForPage();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            disposeSocket();
        }

        #region ClusterDX
        private void Ws_SendComplete(object sender, WinsockSendEventArgs e)
        {
            if (SendCompletedRequest)
            {
                addMonitor($"Cluster DX Send Complete");
                SendCompletedRequest = false;
            }
        }

        private void Ws_DataArrival(object sender, WinsockDataArrivalEventArgs e)
        {
            string dataArrival = "";
            object obj = ws.Get();
            if (obj.GetType() == Type.GetType("string"))
                dataArrival = obj.ToString();
            else if (obj.GetType() == Type.GetType("System.Byte[]"))
                dataArrival = Encoding.Default.GetString((byte[])obj);

            dataArrival = dataArrival.Replace($"{ClusterDXprompt}\r\n", string.Empty);

            if (dataArrival != string.Empty && (clusterConnected || ClusterDebug))
            {
                addMonitor(dataArrival, false);
            }

            if (!clusterConnected)
            {
                clusterConnected = (dataArrival.IndexOf("WX disabled for ") >= 0);
                if (clusterConnected)
                {
                    addMonitor("Cluster DX Logged ln");
                    if (ClusterDXReconnectPending)
                    {
                        ClusterDXReconnectPending = false;
                        elabCommand(txtCommand.Text);
                    }
                }
            }


        }

        private void Ws_Disconnected(object sender, EventArgs e)
        {
            addMonitor($"Cluster DX disconnected: {ws.RemoteHost}:{ws.RemotePort}");
            clusterConnected = false;
        }

        private void Ws_ErrorReceived(object sender, WinsockErrorReceivedEventArgs e)
        {
            if (e.ErrorCode == System.Net.Sockets.SocketError.TimedOut)
            {
                addMonitor($"Cluster DX Timeout... try to reconnect");
                ClusterDXReconnectPending = true;
                ClusterDxOpen("", "");
            }
            else
            {
                addMonitor($"#Error: {e.Message}");
            }
        }

        private void Ws_Connected(object sender, WinsockConnectedEventArgs e)
        {
            addMonitor($"Clusterd DX connected: {ws.RemoteHost}:{ws.RemotePort}");
            LoginClusterDX();
        }

        private void ClusterDxItems(int items)
        {
            if (items > 0)
            {
                clusterDxItems = items;
                addMonitor($"Set Items result for Clusterd DX: {items}");
            }
            else
                addMonitor($"Invalid Items value for Clusterd DX. items: the number must be greater than 0");
        }

        private void ClusterDxSearchCallsign(string callsing)
        {
            if (!clusterConnected)
            {
                addMonitor("Cluster DX not available. Digit dx for login Cluster DX");
            }
            else
            {
                Send($"show/dx {callsing}");
            }
        }

        private void ClusterDxSelectBand(string band, int items = 0)
        {
            if (!clusterConnected)
            {
                addMonitor("Cluster DX not available. Digit dx for login Cluster DX");
            }
            else
            {
                if (items == 0)
                    items = clusterDxItems;
                if (band != "")
                    Send($"show/dx {items} on {band}");
                else
                    Send($"show/dx {items}");
            }
        }

        private void ClusterDebugSwitch()
        {
            ClusterDebug = !ClusterDebug;
            addMonitor($"Cluster Debug active: {ClusterDebug.ToString()}");
        }

        private void ClusterDxCmd(string command)
        {
            if (!clusterConnected)
            {
                addMonitor("Cluster DX not available. Digit dx for login Cluster DX");
            }
            else
            {
                if (command.Length > 3)
                {
                    string cmd = command.Substring(3, command.Length - 3);
                    Send($"{cmd}");
                }
                else
                    addMonitor("Invalid DXSpider command!");
            }
        }

        private void ClusterDxClose()
        {
            if (ws.State == WinsockStates.Connected)
            {
                addMonitor("Disconnecting Cluster DX....");
                ws.Close();
            }
        }

        private void ClusterDxSendSpot(string qrzToSpot, string freq, string comment)
        {
            if (!clusterConnected)
            {
                addMonitor("Cluster DX not available. Digit dx for login Cluster DX");
            }
            else
            {
                if (qrzToSpot != "" && freq != "")
                {
                    addMonitor($"Send spot to {qrzToSpot} on freq {freq}");
                    SendCompletedRequest = true;
                    Send($"DX {qrzToSpot} {freq} {comment}");
                }
                else
                    addMonitor($"Invalid Spot command for Cluster DX!");
            }
        }

        private void ClusterDxOpen(string usrename = "", string passowrd = "")
        {

            if (txtUsername.Text != "")
            {
                if (ws == null)
                {
                    ws = new Winsock();
                    ws.LegacySupport = true;

                    ws.Connected += Ws_Connected;
                    ws.ErrorReceived += Ws_ErrorReceived;
                    ws.Disconnected += Ws_Disconnected;
                    ws.DataArrival += Ws_DataArrival;
                    ws.SendComplete += Ws_SendComplete;

                }
                if (ws.State != WinsockStates.Connected)
                {
                    ws.Connect(ClusterAddr, ClusterPort);
                }

                if (clusterConnected)
                    addMonitor("Cluster already connected");
            }
            else
                addMonitor("Please! Befor connect Cluster DX Login in QRZ.com using: li [username] [password] command");


        }

        private void LoginClusterDX()
        {

            addMonitor("Login Cluster DX...");
            Send(txtUsername.Text);
            Thread.Sleep(50);
            Send($"set/prompt {ClusterDXprompt}");
            Thread.Sleep(50);
            Send($"unset/echo");
            Thread.Sleep(50);
            //SendWSCommandByQueue($"set/qth {City} {StateProv}, {Country}");
            //Thread.Sleep(50);
            Send("unset/announce");
            Thread.Sleep(50);
            Send("unset/anntalk");
            Thread.Sleep(50);
            Send("unset/dx");
            Thread.Sleep(50);
            Send("unset/talk");
            Thread.Sleep(50);
            Send("unset/wcy");
            Thread.Sleep(50);
            Send("unset/wwv");
            Thread.Sleep(50);
            Send("unset/wx");
            Thread.Sleep(50);
        }

        private void Send(string request)
        {
            if (ClusterDebug)
                addMonitor($"Send: {request}");

            ws.Send($"{request}\n");
        }

        private void disposeSocket()
        {
            if (ws != null)
            {
                ws.Connected -= Ws_Connected;
                ws.ErrorReceived -= Ws_ErrorReceived;
                ws.Disconnected -= Ws_Disconnected;
                ws.DataArrival -= Ws_DataArrival;
                ws.SendComplete -= Ws_SendComplete;

                ws.Close();

                ws.Dispose();

                ws = null;

            }
        }
        #endregion

        private void txtMonitor_KeyPress(object sender, KeyPressEventArgs e)
        {
            txtCommand.Text = e.KeyChar.ToString();
            txtCommand.SelectionStart = txtCommand.Text.Length;
            txtCommand.Focus();
        }

        private void txtCommand_TextChanged(object sender, EventArgs e)
        {
            if (txtCommand.TextLength > 2)
            {
                if (txtCommand.Text.Substring(0, 3).ToUpper() == "AQ ")
                {
                    if (txtCommand.SelectionStart == 3 && txtCommand.Text.Length == 3)
                    {
                        if (string.IsNullOrEmpty(lastQRZ))
                            lastQRZ = "[CALL]";

                        txtCommand.Text = txtCommand.Text + lastQRZ + " " + lastFreq + " " + lastMode + " " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
                        txtCommand.SelectionStart = 3;
                        txtCommand.SelectionLength = lastQRZ.Length;
                    }
                }
                else if (txtCommand.Text.Substring(0, 3).ToUpper() == "SS ")
                {
                    if (txtCommand.SelectionStart == 3 && txtCommand.Text.Length == 3)
                    {
                        if (string.IsNullOrEmpty(lastQRZ))
                            lastQRZ = "[CALL]";

                        txtCommand.Text = txtCommand.Text + lastQRZ + " " + lastFreq;
                        txtCommand.SelectionStart = 3;
                        txtCommand.SelectionLength = lastQRZ.Length;
                    }
                }
            }
        }

        private void txtCommand_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                string smd = txtCommand.Text;
                int startPos = -1;
                int nextSpace = -1;
                if (!e.Shift)
                {
                    startPos = txtCommand.SelectionStart + txtCommand.SelectionLength;

                    if (startPos > 0)
                    {
                        if (smd.Substring(startPos - 1, 1) != " ")
                            startPos++;
                    }

                    if (txtCommand.TextLength > startPos + 1)
                    {
                        nextSpace = smd.IndexOf(' ', startPos);
                        if (nextSpace < 0)
                            nextSpace = txtCommand.TextLength;

                        if (nextSpace >= 0)
                        {
                            startPos = smd.LastIndexOf(' ', startPos);
                            if (startPos < 0)
                                startPos = 0;
                            else
                                startPos++;

                            txtCommand.SelectionStart = startPos;
                            if (nextSpace >= startPos)
                                txtCommand.SelectionLength = (nextSpace - startPos);
                        }
                    }
                }
                else
                {
                    startPos = txtCommand.SelectionStart - 2;
                    if (startPos > 0)
                    {
                        if (txtCommand.SelectionStart < txtCommand.TextLength)
                        {
                            if (smd.Substring(txtCommand.SelectionStart, 1) == " ")
                            {
                                startPos++;
                            }
                        }

                        nextSpace = smd.LastIndexOf(' ', startPos);

                        if (nextSpace < 0)
                            nextSpace = 0;

                        if (nextSpace >= 0)
                        {
                            startPos = smd.IndexOf(' ', nextSpace + 1);
                            if (startPos < 0)
                                startPos = txtCommand.TextLength;

                            if (nextSpace > 0)
                                txtCommand.SelectionStart = nextSpace + 1;
                            else
                            {
                                txtCommand.SelectionStart = nextSpace;
                                startPos++;
                            }

                            txtCommand.SelectionLength = (startPos - nextSpace - 1);
                        }
                    }
                }
                e.IsInputKey = true;


            }

        }
    }
}
