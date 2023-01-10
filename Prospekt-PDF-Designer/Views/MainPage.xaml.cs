using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Data;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.text.pdf.draw;
using Windows.Storage;
using EASendMail;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace Prospekt_PDF_Designer.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Maximized;

            StartListview();
            ShowAndFillListviewMail();

        }

        public async void StartListview()
        {
            bool result = await Database.Execute("SELECT * FROM kundregister.prospekt order by PRO_RDT DESC;");
            ShowAndFillListview(result);
        }

        /// <summary>
        /// This method clears all fields in the input mask
        /// </summary>
        public void ClearAllFields()
        {
            ID.Text = "";
            CreatedDatePro.Text = "";
            EditedDate.Text = "";
            WhoEdited.Text = "";
            UserInitials.Text = "";
            CompanyName.Text = "";
            ProURL.Text = "";
            AllabolagURL.Text = "";
            Email.Text = "";
            Comment.Text = "";
            URLH1.Text = "";
            URLH2.Text = "";
            URLGTM.Text = "";
            URLGA.Text = "";
            URLORGNR.Text = "";
            currentCompany.Text = "";
            currentCompanyScrap.Text = "";
            currentCompanyResult.Text = "";

            SeekAll.Text = "";
            SeekAllScrap.Text = "";
            SeekAllResult.Text = "";
            SeekID.Text = "";
            SeekIDscrap.Text = "";
            SeekIDResult.Text = "";

            if (lvmailadresses.SelectionMode == ListViewSelectionMode.Multiple ||
                lvmailadresses.SelectionMode == ListViewSelectionMode.Extended)
            {
                lvmailadresses.DeselectRange(new ItemIndexRange(0, (uint)lvmailadresses.Items.Count()));
            }

            if (lvmailadressesScrap.SelectionMode == ListViewSelectionMode.Multiple ||
                lvmailadressesScrap.SelectionMode == ListViewSelectionMode.Extended)
            {
                lvmailadressesScrap.DeselectRange(new ItemIndexRange(0, (uint)lvmailadressesScrap.Items.Count()));
            }

            if (lvmailadressesResult.SelectionMode == ListViewSelectionMode.Multiple ||
                lvmailadressesResult.SelectionMode == ListViewSelectionMode.Extended)
            {
                lvmailadressesResult.DeselectRange(new ItemIndexRange(0, (uint)lvmailadressesResult.Items.Count()));
            }
        }

        /// <summary>
        /// Clears all in the scrap result overview
        /// </summary>
        public void ClearAllScrapResultFields()
        {
            txtscrapingResultGTM.Text = "";
            txtscrapingResultUA.Text = "";
            txtscrapingResultH1.Text = "";
            txtscrapingResultH2.Text = "";
            txtscrapingResultOrg.Text = "";

            Items.SelectedIndex = 2;
        }

        public void ClearAllScrapFields()
        {
            ProURL.Text = "";
            URLGTM.Text = "";
            URLGA.Text = "";
            URLH1.Text = "";
            URLH2.Text = "";
            URLORGNR.Text = "";
        }

        public async void ShowAndFillListviewMail()
        {
            bool result = await Database.Execute("SELECT * FROM kundregister.mottagare order by MOT_EPOST;");
            if (true == result)
            {
                // Specify the list view item source
                lvmailadresses.ItemsSource = Database.dtable.DefaultView;
                lvmailadressesScrap.ItemsSource = Database.dtable.DefaultView;
                lvmailadressesResult.ItemsSource = Database.dtable.DefaultView;
            }
            else
            {
                lvmailadresses.ItemsSource = null;
                lvmailadressesScrap.ItemsSource = null;
                lvmailadressesResult.ItemsSource = null;
            }
        }

        /// <summary>
        /// This method fills the listview
        /// </summary>
        /// <param name="result"></param>
        public void ShowAndFillListview(bool result)
        {
            if (true == result)
            {
                //Prepare preview for listview data comment, allabolag, h1 and h2
                foreach (DataRow item in Database.dtable.Rows)
                {
                    string allabolag = item.ItemArray[7].ToString();
                    if (25 < allabolag.Length)
                        allabolag = allabolag.Substring(0, 25) + "...";

                    string comment = item.ItemArray[9].ToString();
                    if( 25 < comment.Length )
                        comment = comment.Substring(0, 25) + "...";

                    string h1 = item.ItemArray[10].ToString();
                    if( 25 < h1.Length )
                        h1 = h1.Substring(0, 25) + "...";

                    string h2 = item.ItemArray[11].ToString();
                    if( 25 < h2.Length )
                        h2 = h2.Substring(0, 25) + "...";
                    Preview preview = new Preview() { Row = item, Allabolag = allabolag, Comment = comment, H1 = h1, H2 = h2 };
                    Database.myDataTable.Add(preview);
                }
                // Specify the list view item source
                lvProData.ItemsSource = Database.myDataTable;
            }
            else
            {
                lvProData.ItemsSource = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// This method is for the search of items in the databasetable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void btnSeek_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                string mysql = "SELECT * FROM kundregister.prospekt WHERE ";

                if (("" != SeekID.Text && 0 < int.Parse(SeekID.Text)))
                {
                    mysql = mysql + "PRO_ID = '" + SeekID.Text + "';";
                }
                else if ("" != SeekIDscrap.Text && 0 < int.Parse(SeekIDscrap.Text))
                {
                    mysql = mysql + "PRO_ID = '" + SeekIDscrap.Text + "';";
                }
                else if ("" != SeekIDResult.Text && 0 < int.Parse(SeekIDResult.Text))
                {
                    mysql = mysql + "PRO_ID = '" + SeekIDResult.Text + "';";
                }
                else if ("" != SeekAll.Text.Trim())
                {
                    mysql = mysql + "PRO_NAMN LIKE '%" + SeekAll.Text + "%'  OR PRO_URL LIKE '%" + SeekAll.Text + "%' OR PRO_URL_ORGNR LIKE '%" + SeekAll.Text + "%';";
                }
                else if ("" != SeekAllScrap.Text.Trim())
                {
                    mysql = mysql + "PRO_NAMN LIKE '%" + SeekAllScrap.Text + "%'  OR PRO_URL LIKE '%" + SeekAllScrap.Text + "%' OR PRO_URL_ORGNR LIKE '%" + SeekAllScrap.Text + "%';";
                }
                else if ("" != SeekAllResult.Text.Trim())
                {
                    mysql = mysql + "PRO_NAMN LIKE '%" + SeekAllResult.Text + "%'  OR PRO_URL LIKE '%" + SeekAllResult.Text + "%' OR PRO_URL_ORGNR LIKE '%" + SeekAllResult.Text + "%';";
                }
                else return;

                bool result = await Database.Execute(mysql);

                ShowAndFillListview(result);
                Items.SelectedIndex = 0;
                ClearAllFields();
                btnGetData.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog(ex.Message);
                await message.ShowAsync();
            }
        }
        /// <summary>
        /// This method shows all data in the databasetable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnShowAll_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                bool result = await Database.Execute("SELECT * FROM kundregister.prospekt order by PRO_RDT DESC;");

                ShowAndFillListview(result);
                Items.SelectedIndex = 0;
                ClearAllFields();
                btnGetData.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog(ex.Message);
                await message.ShowAsync();
            }
        }
        /// <summary>
        /// This method delets items from the databasetable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnDelete_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                //MessageDialog message = new MessageDialog("Are you sure you want to delete company " + CompanyName.Text + "\nwith ID " + ID.Text + " and last edited by: " + WhoEdited.Text + " ?");
                MessageDialog message = new MessageDialog("Är du säkert att du vill radera prospektet " + CompanyName.Text + "\nmed ID " + ID.Text + " och sist ändrat av: " + WhoEdited.Text + " ?");

                //message.Commands.Add(new UICommand("Yes", null));
                //message.Commands.Add(new UICommand("No", null));
                message.Commands.Add(new UICommand("Ja", null));
                message.Commands.Add(new UICommand("Nej", null));

                message.DefaultCommandIndex = 1;
                message.CancelCommandIndex = 0;
                var cmd = await message.ShowAsync();

                if (cmd.Label == "Yes" || cmd.Label == "Ja")
                {
                    bool result = await Database.Execute(
                     "DELETE FROM kundregister.prospekt " +
                     "WHERE PRO_NAMN = '" + CompanyName.Text + "' " +
                     "AND PRO_ID = '" + ID.Text + "';");

                    btnShowAll_Click(sender, e);
                    Items.SelectedIndex = 0;
                    ClearAllFields();
                    btnGetData.IsEnabled = false;
                }

                btnShowAll_Click(sender, e);
                Items.SelectedIndex = 0;
                ClearAllFields();
                btnGetData.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog(ex.Message);
                await message.ShowAsync();
            }
        }

        string username = "";
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                username = e.Parameter.ToString();
                currentLogin.Text = username;
                currentLoginscrap.Text = username;
                currentLoginResult.Text = username;

            }
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// This method inserts data into the databasetable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnInsert_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                string mysql = "SELECT ANV_ID FROM kundregister.användarinfo WHERE ANV_NAMN = '" + username + "';";

                bool resultusername = await Database.Execute(mysql);

                if (0 == Database.dtable.Rows.Count)
                {
                    //throw (new Exception("Username is empty !"));
                    throw (new Exception("Användarnamnet är tom !"));
                }

                UserInitials.Text = Database.dtable.Rows[0].ItemArray[0].ToString();
                WhoEdited.Text = username;


                if (CompanyName.Text.Trim() == "")
                {
                    //throw (new Exception("Please enter valid data for CompanyName !"));
                    throw (new Exception("Snälla mata in en giltig prospekt namn !"));
                }

                bool result = await Database.Execute(
                    "INSERT INTO kundregister.prospekt (" +
                        "PRO_DT, " +
                        "PRO_RDT, " +
                        "PRO_RDT_AV, " +
                        "PRO_SKAPAD_AV, " +
                        "PRO_NAMN, " +
                        "PRO_URL, " +
                        "PRO_URL_ALLABOLAG, " +
                        "PRO_EPOST, " +
                        "PRO_URL_H1, " +
                        "PRO_URL_H2, " +
                        "PRO_URL_GTM, " +
                        "PRO_URL_GA, " +
                        "PRO_URL_ORGNR, " +
                        "PRO_KOM) " +

                    "VALUES (" +
                        "'" + DateTime.Now.GetDateTimeFormats('G')[23] + "', " +
                        "'" + DateTime.Now.GetDateTimeFormats('G')[23] + "', " +
                        "'" + WhoEdited.Text + "', " +
                        "'" + UserInitials.Text + "', " +
                        "'" + CompanyName.Text + "', " +
                        "'" + ProURL.Text + "', " +
                        "'" + AllabolagURL.Text + "', " +
                        "'" + Email.Text + "', " +
                        "'" + URLH1.Text + "', " +
                        "'" + URLH2.Text + "', " +
                        "'" + URLGTM.Text + "', " +
                        "'" + URLGA.Text + "', " +
                        "'" + URLORGNR.Text + "'," +
                        "'" + Comment.Text + "'); ");

                if (true == result)
                {
                    SeekAll.Text = CompanyName.Text;
                    btnSeek_Click(sender, e);
                    Items.SelectedIndex = 0;
                    ClearAllFields();
                    btnGetData.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog(ex.Message);
                await message.ShowAsync();
            }
        }
        /// <summary>
        /// This method modifies items in the databasetable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnModify_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                WhoEdited.Text = username;

                if (ID.Text == "" || UserInitials.Text == "" || CompanyName.Text.Trim() == "")
                {
                    //throw (new Exception("Please enter valid ID, User Initials and Company name!"));
                    throw (new Exception("Snälla mata in giltig ID, användar initialer och prospekt namn!"));
                }

                bool result = await Database.Execute("UPDATE kundregister.prospekt SET" +
                    "                  PRO_ID = '" + ID.Text + "', " +
                    "                  PRO_RDT = '" + DateTime.Now.GetDateTimeFormats('G')[23] + "', " +
                    "                  PRO_RDT_AV = '" + WhoEdited.Text + "', " +
                    "                  PRO_SKAPAD_AV = '" + UserInitials.Text + "', " +
                    "                  PRO_NAMN = '" + CompanyName.Text.Trim() + "', " +
                    "                  PRO_URL = '" + ProURL.Text + "', " +
                    "                  PRO_URL_ALLABOLAG = '" + AllabolagURL.Text + "', " +
                    "                  PRO_EPOST = '" + Email.Text + "', " +
                    "                  PRO_URL_H1 = '" + URLH1.Text + "', " +
                    "                  PRO_URL_H2 = '" + URLH2.Text + "', " +
                    "                  PRO_URL_GTM = '" + URLGTM.Text + "', " +
                    "                  PRO_URL_GA = '" + URLGA.Text + "', " +
                    "                  PRO_URL_ORGNR = '" + URLORGNR.Text + "',  " +
                    "                  PRO_KOM = '" + Comment.Text + "' " +
                    "   WHERE PRO_ID = '" + ID.Text + "';");

                SeekID.Text = ID.Text;
                btnSeek_Click(sender, e);
                Items.SelectedIndex = 0;
                ClearAllFields();
                btnGetData.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog(ex.Message);
                await message.ShowAsync();
            }
        }

        /// <summary>
        /// This method fills the input mask with data from the databasetable after double tapping an item in the listview
        /// You have to dubbletab ON the item!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_DoubleTapped_1(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            //DataRowView Item = (DataRowView)lvProData.SelectedItem;
            Preview Item = lvProData.SelectedItem as Preview;

            if (null != Item)
            {
                ID.Text = Item.Row.ItemArray[0].ToString();
                CreatedDatePro.Text = Item.Row.ItemArray[1].ToString();
                EditedDate.Text = Item.Row.ItemArray[2].ToString();
                WhoEdited.Text = Item.Row.ItemArray[3].ToString();
                UserInitials.Text = Item.Row.ItemArray[4].ToString();
                CompanyName.Text = Item.Row.ItemArray[5].ToString();
                ProURL.Text = Item.Row.ItemArray[6].ToString();
                AllabolagURL.Text = Item.Row.ItemArray[7].ToString();
                Email.Text = Item.Row.ItemArray[8].ToString();
                Comment.Text = Item.Row.ItemArray[9].ToString();
                URLH1.Text = Item.Row.ItemArray[10].ToString();
                URLH2.Text = Item.Row.ItemArray[11].ToString();
                URLGTM.Text = Item.Row.ItemArray[12].ToString();
                URLGA.Text = Item.Row.ItemArray[13].ToString();
                URLORGNR.Text = Item.Row.ItemArray[14].ToString();

                currentCompany.Text = CompanyName.Text;
                currentCompanyScrap.Text = CompanyName.Text;
                currentCompanyResult.Text = CompanyName.Text;

                Items.SelectedIndex = 1;
                ProURL.IsEnabled = true;
                btnGetData.IsEnabled = true;
            }
        }

        private void btnCancel_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ClearAllFields();
            ClearAllScrapFields();
            ClearAllScrapResultFields();
            ProURL.IsEnabled = true;
            btnGetData.IsEnabled = false;

        }

        private void lvProData_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.ItemIndex % 2 == 0)
            {
                args.ItemContainer.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                args.ItemContainer.Background = new SolidColorBrush(Colors.LightGray);
            }
        }

        public object WindowState { get; private set; }

        private async void btnGetData_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                MessageDialog message = new MessageDialog("Appen börjar skrapa nu. Tack för ditt tålamod!");
                message.Commands.Add(new UICommand("OK", null));
                message.Commands.Add(new UICommand("Avbryt", null));

                message.DefaultCommandIndex = 1;
                message.CancelCommandIndex = 0;
                var cmd = await message.ShowAsync();

                if (cmd.Label == "OK")
                {
                    Listview_Pivot.IsEnabled = false;
                    Inputmask_Pivot.IsEnabled = false;
                    Scrap_Pivot.IsEnabled = false;
                    Result_Pivot.IsEnabled = false;

                    //kontroll om url har en giltig längd och börjar med https:// eller www. I fall att den bara börja med www sätt https framför.

                    var webpageFromTextfield = ProURL.Text.ToLower().Trim();
                    string url = "";

                    if ((8 <= webpageFromTextfield.Length && webpageFromTextfield.Substring(0, 8) == "https://"))
                    {
                        url = webpageFromTextfield;
                    }
                    else if ((4 <= webpageFromTextfield.Length && webpageFromTextfield.Substring(0, 4) == "www.") && (webpageFromTextfield.Substring(0, 8) != "https://"))
                    {
                        string korrekturlListLine = "https://" + webpageFromTextfield;
                        url = korrekturlListLine;
                    }

                    if (12 > url.Length)
                    {
                        //exception handling if adress < 12 char. https://www. is already 12 char long
                        throw new Exception("Your webadress is too short. It must be at least 12 characters long.");
                    }

                    string[] domainSplit = url.Split('.');
                    string domain_prefix = domainSplit[1];

                    string domain_suffix = "";
                    for (int suffixIndex = 0; suffixIndex < domainSplit[2].Length; suffixIndex++)
                    {
                        if ('a' > domainSplit[2][suffixIndex] || 'z' < domainSplit[2][suffixIndex])
                            break;

                        domain_suffix += domainSplit[2][suffixIndex];
                    }

                    string domain = domain_prefix + "." + domain_suffix;

                    SitemapGenerator.Sitemap.Sitemapper siteMapper = new SitemapGenerator.Sitemap.Sitemapper(domain, url);

                    await siteMapper.GenerateSitemap();

                    txtscrapingResultOrg.Text = siteMapper.orgString;
                    txtscrapingResultGTM.Text = siteMapper.gtmString;
                    txtscrapingResultUA.Text = siteMapper.uaString;
                    txtscrapingResultH1.Text = siteMapper.GetUniqueH1();
                    txtscrapingResultH2.Text = siteMapper.GetUniqueH2();

                    Items.SelectedIndex = 3;
                    btnGetData.IsEnabled = false;
                }
                
            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog(ex.Message);
                await message.ShowAsync();
            }

            Result_Pivot.IsEnabled = true;

            Listview_Pivot.IsEnabled = true;
            Inputmask_Pivot.IsEnabled = true;
            Scrap_Pivot.IsEnabled = true;
        }

        private async void btnApplyScrap_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (URLGTM.Text != txtscrapingResultGTM.Text)
            {
                MessageDialog message = new MessageDialog("Är du säkert att du vill ersätta befintligt data för URL GTM med den nya du hittade?");

                message.Commands.Add(new UICommand("Ja", null));
                message.Commands.Add(new UICommand("Nej", null));
                message.DefaultCommandIndex = 1;
                message.CancelCommandIndex = 0;
                IUICommand cmd = await message.ShowAsync();

                if (cmd.Label == "Yes" || cmd.Label == "Ja")
                {
                    URLGTM.Text = txtscrapingResultGTM.Text;
                }
            }

            if (URLGA.Text != txtscrapingResultUA.Text)
            {
                MessageDialog message = new MessageDialog("Är du säkert att du vill ersätta befintligt data för URL GA med den nya du hittade?");

                message.Commands.Add(new UICommand("Ja", null));
                message.Commands.Add(new UICommand("Nej", null));

                message.DefaultCommandIndex = 1;
                message.CancelCommandIndex = 0;
                IUICommand cmd = await message.ShowAsync();

                if (cmd.Label == "Yes" || cmd.Label == "Ja")
                {
                    URLGA.Text = txtscrapingResultUA.Text;
                }
            }

            if (URLH1.Text != txtscrapingResultH1.Text.TrimStart())
            {
                //MessageDialog message = new MessageDialog("Are you sure you want to modify content for the field URL H1?");
                MessageDialog message = new MessageDialog("Är du säkert att du vill ersätta befintligt data för alla H1 med den nya du hittade?");

                //message.Commands.Add(new UICommand("Yes", null));
                //message.Commands.Add(new UICommand("No", null));
                message.Commands.Add(new UICommand("Ja", null));
                message.Commands.Add(new UICommand("Nej", null));

                message.DefaultCommandIndex = 1;
                message.CancelCommandIndex = 0;
                IUICommand cmd = await message.ShowAsync();

                if (cmd.Label == "Yes" || cmd.Label == "Ja")
                {
                    URLH1.Text = txtscrapingResultH1.Text.TrimStart();
                }
            }

            if (URLH2.Text != txtscrapingResultH2.Text.TrimStart())
            {
                //MessageDialog message = new MessageDialog("Are you sure you want to modify content for the field URL H2?");
                MessageDialog message = new MessageDialog("Är du säkert att du vill ersätta befintligt data för alla H2 med den nya du hittade?");

                //message.Commands.Add(new UICommand("Yes", null));
                //message.Commands.Add(new UICommand("No", null));
                message.Commands.Add(new UICommand("Ja", null));
                message.Commands.Add(new UICommand("Nej", null));

                message.DefaultCommandIndex = 1;
                message.CancelCommandIndex = 0;
                IUICommand cmd = await message.ShowAsync();

                if (cmd.Label == "Yes" || cmd.Label == "Ja")
                {
                    URLH2.Text = txtscrapingResultH2.Text.TrimStart();
                }
            }

            if (URLORGNR.Text != txtscrapingResultOrg.Text.TrimStart())
            {
                //MessageDialog message = new MessageDialog("Are you sure you want to modify content for the field URL Organisation number?");
                MessageDialog message = new MessageDialog("Är du säkert att du vill ersätta befintligt data för organisationsnummer med den nya du hittade?");

                //message.Commands.Add(new UICommand("Yes", null));
                //message.Commands.Add(new UICommand("No", null));
                message.Commands.Add(new UICommand("Ja", null));
                message.Commands.Add(new UICommand("Nej", null));

                message.DefaultCommandIndex = 1;
                message.CancelCommandIndex = 0;
                IUICommand cmd = await message.ShowAsync();

                if (cmd.Label == "Yes" || cmd.Label == "Ja")
                {
                    URLORGNR.Text = txtscrapingResultOrg.Text.TrimStart();
                }
            }

            Items.SelectedIndex = 2;
            ClearAllScrapResultFields();
            btnGetData.IsEnabled = false;
        }

        private void btnCancelScrapResult_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ClearAllScrapResultFields();
            ProURL.IsEnabled = true;
            btnGetData.IsEnabled = true;
        }

        private void btnCancelScrap_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ClearAllScrapFields();
            ProURL.IsEnabled = true;
            btnGetData.IsEnabled = true;
        }

        //create pdf

        public async void ExportDataTableToPdf(DataTable dtblTable, string strHeader, List<string> mailadresses)
        {
            //File1 path = place from saved pdf
            StorageFolder folder = ApplicationData.Current.TemporaryFolder;
            StorageFile file = await folder.CreateFileAsync("Prospekt.pdf", CreationCollisionOption.ReplaceExisting);
            string myFile = file.Path;
            if (file != null)
            {
                //Create a file object
                Stream s = await file.OpenStreamForWriteAsync();

                //Create a document object
                Document document = new Document();

                //Take page size from in-built PageSize class
                document.SetPageSize(PageSize.A4);

                //Create a PdfWriter object. It helps to write the document to the specified stream
                PdfWriter writer = PdfWriter.GetInstance(document, s);

                //Open the document
                document.Open();

                //Report Header
                BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Paragraph prgHeading = new Paragraph();
                prgHeading.Alignment = Element.ALIGN_CENTER;
                prgHeading.Add(new Chunk(strHeader.ToUpper()));
                document.Add(prgHeading);

                //Author
                Paragraph prgAuthor = new Paragraph();
                BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                prgAuthor.Alignment = Element.ALIGN_RIGHT;
                prgAuthor.Add(new Chunk("Aktuell användare : " + currentLogin.Text));
                prgAuthor.Add(new Chunk("\nDatum : " + DateTime.Now.ToShortDateString()));
                document.Add(prgAuthor);

                //Add a line seperation
                Paragraph p = new Paragraph(new Chunk(new LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 0)));
                document.Add(p);

                //Add line break
                document.Add(new Chunk("\n"));

                //Write the table
                PdfPTable table = new PdfPTable(2);

                table.TotalWidth = document.PageSize.Width - 650;
                table.SetTotalWidth(new float[] { 150f, 500f });

                //table Data
                for (int i = 0; i < dtblTable.Columns.Count; i++)
                {
                    table.AddCell(dtblTable.Columns[i].ColumnName);
                    table.AddCell(dtblTable.Rows[0][i].ToString());
                }

                document.Add(table);
                document.Close();
                writer.Close();
                s.Close();

                //preview on the screen for testing
                //{
                //    // Launch the retrieved file
                //    await Windows.System.Launcher.LaunchFileAsync(file);
                //}

                await Send_Email(file.Path, mailadresses);

                //File.Delete(file.Path);

            }
        }

        public async Task<DataTable> MakeDataTable()
        {
            string pdfquery = "SELECT " +
                "PRO_ID AS 'ID'," +
                "PRO_DT AS 'Skapad datum'," +
                "PRO_RDT AS 'Senast ändrat'," +
                "PRO_RDT_AV AS 'Vem ändrade sist'," +
                "PRO_SKAPAD_AV AS 'Användare'," +
                "PRO_NAMN AS 'Prospektnamn'," +
                "PRO_URL_ALLABOLAG AS 'Allabolag URL'," +
                "PRO_EPOST AS 'Email'," +
                "PRO_KOM AS 'Kommentar'," +
                "PRO_URL AS 'Pro URL'," +
                "PRO_URL_H1 AS 'URL H1'," +
                "PRO_URL_H2 AS 'URL H2'," +
                "PRO_URL_GTM AS 'URL GTM'," +
                "PRO_URL_GA AS 'URL GA'," +
                "PRO_URL_ORGNR AS 'URL Orgnr' FROM kundregister.prospekt WHERE PRO_NAMN = '" + CompanyName.Text + "';";


            bool result = await Database.Execute(pdfquery);

            if (result != true)
            {
                return null;
            }

            return Database.dtable;
        }

        public async void CheckMailAdresses(List<string> mailadresses)
        {
            try
            {
                if (CompanyName.Text.Trim() == "")
                {

                    throw new ArgumentNullException("Snälla välj ett prospekt.");

                }

                if (mailadresses.Count == 0)
                {
                    throw new ArgumentNullException("Snälla välj minst en mailadress.");
                }

                DataTable dtbl = await MakeDataTable();

                if (null != dtbl && dtbl.Rows.Count > 0)
                {
                    ExportDataTableToPdf(dtbl, "Kundprospekt \n" + CompanyName.Text, mailadresses);

                    ClearAllFields();
                    ClearAllScrapFields();
                    ClearAllScrapResultFields();
                    ProURL.IsEnabled = true;
                    btnGetData.IsEnabled = false;
                    Items.SelectedIndex = 0;
                }
                else
                {
                    throw new ArgumentNullException("Data kan inte läsas!");
                }

            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog(ex.Message, "Error Message");
                await message.ShowAsync();
            }

        }


        private void btnSentMail_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            List<string> mailadresses = new List<string>();

            foreach (var selectedItem in lvmailadresses.SelectedItems)
            {
                mailadresses.Add(((DataRowView)selectedItem).Row.ItemArray[2].ToString());
            }

            CheckMailAdresses(mailadresses);
        }

        private void btnSentMailScrap_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            List<string> mailadresses = new List<string>();

            foreach (var selectedItem in lvmailadressesScrap.SelectedItems)
            {
                mailadresses.Add(((DataRowView)selectedItem).Row.ItemArray[2].ToString());
            }

            CheckMailAdresses(mailadresses);

        }

        private void btnSentMailResult_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            List<string> mailadresses = new List<string>();

            foreach (var selectedItem in lvmailadressesResult.SelectedItems)
            {
                mailadresses.Add(((DataRowView)selectedItem).Row.ItemArray[2].ToString());
            }

            CheckMailAdresses(mailadresses);

        }


        private async Task Send_Email(string attachmentPath, List<string> mailadresses)
        {
            String Result = "";
            try
            {
                SmtpMail oMail = new SmtpMail("TryIt");
                SmtpClient oSmtp = new SmtpClient();

                // Set sender email address, please change it to yours
                oMail.From = new MailAddress("mattias.kullberg@finnmera.se");
                //oMail.From = new MailAddress("nicole@die-walter-familie.de");


                //lvmailadresses.Visibility = Visibility;

                // Add recipient email address, please change it to yours
                //oMail.To.Add(new MailAddress("mattias.kullberg@finnmera.se"));

                foreach (var mailadress in mailadresses)
                {
                    oMail.To.Add(new MailAddress(mailadress));
                }

                // Set email subject
                oMail.Subject = "Kund prospekt " + CompanyName.Text;

                // Set Html body
                oMail.HtmlBody = "<font size=5>Dethär är en automatisk genererat email. Du kan inte svara på den.</font>";

                Attachment oAttachment = await oMail.AddAttachmentAsync(attachmentPath);

                // Your SMTP server address
                //SmtpServer oServer = new SmtpServer("send.one.com");
                SmtpServer oServer = new SmtpServer("pro.turbo-smtp.com");

                // Server name (recommended for EU users): pro.eu.turbo-smtp.com

                // User and password for ESMTP authentication
                //oServer.User = "nicole@die-walter-familie.de";
                //oServer.Password = "superkalifragelistigexpialigetisch";
                oServer.User = " mattias.kullberg@finnmera.se";
                oServer.Password = "ozdWLdwF";

                // If your SMTP server requires TLS connection on 25 port, please add this line
                //oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

                // If your SMTP server requires SSL connection on 465 port, please add this line
                oServer.Port = 465;
                oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

                await oSmtp.SendMailAsync(oServer, oMail);
                //Result = "Email was sent successfully!";
                Result = "Mail skickat framgångsrikt!";

            }
            catch (Exception ep)
            {
                //Result = String.Format("Failed to send email with the following error: {0}", ep.Message);
                Result = String.Format("Det gick inte att skicka mailet på grund av följande fel: {0}", ep.Message);
            }

            // Display Result by Diaglog box
            Windows.UI.Popups.MessageDialog dlg = new
                Windows.UI.Popups.MessageDialog(Result);

            await dlg.ShowAsync();
            File.Delete(attachmentPath);
            ClearAllFields();
        }

    }
}


