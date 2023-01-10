using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using Windows.UI.Popups;

namespace Prospekt_PDF_Designer
{
    /// <summary>
    /// Class for preview in listview
    /// </summary>
    class Preview
    {
        public DataRow Row { get; set; }
        public string Allabolag { get; set; }
        public string Comment { get; set; }
        public string H1 { get; set; }
        public string H2 { get; set; }
    }

    class MyList : IEnumerable
    {
        List<Preview> _List;

        public MyList()
        {
            _List = new List<Preview>();
        }
        public IEnumerator GetEnumerator()
        {
            return _List.GetEnumerator();
        }

        public void Add(Preview prev)
        {
            _List.Add(prev);
        }
    }

    /// <summary>
    /// This class has all connection / serverdata, opens and closes the database
    /// </summary>
    class Database
    {
        static public string connectionstring;
        static public ObservableCollection<Preview> myDataTable;
        static public DataTable dtable;

        static public void SetConnetion(string server,string port,string username, string password)
        {
            connectionstring = string.Format("SERVER={0};PORT={1};USERNAME={2};PASSWORD={3};", server, port, username,password);
        }

        static public async Task<bool> Execute(string mysql)
        {

            //try catch will help to find our errors when we have error in our code
            try
            {
                dtable = new DataTable();
                myDataTable = new ObservableCollection<Preview>();


                //we need to add the connectionstring here
                //here we need our database server name and mysql port no and mysql username and password
                //string connectionstring = "SERVER=localhost;PORT=3306;USERNAME=root;PASSWORD=3884";


                //this is our mysql connection so that we can able to connect to the database and retreive data from there.
                MySqlConnection conn = new MySqlConnection(connectionstring);

                //in here we wíll add two parameters inside this bracket so that we can have access from mysql commands.
                MySqlCommand command = new MySqlCommand(mysql, conn);

                //this will open the connection for us to able to access inside the database
                conn.Open();

                //so now we need mysqldataadapter to view all our records into the listview
                MySqlDataAdapter dtb = new MySqlDataAdapter();
                dtb.SelectCommand = command;

                //in this code i am telling the command t0 fill the listview when the command is executed
                dtb.Fill(dtable);

                return true;
            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog(ex.Message);
                await message.ShowAsync();

                return false;
            }
        }
    }
}
