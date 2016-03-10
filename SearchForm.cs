using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADIFReader
{
    public partial class SearchForm : Form
    {
        private string location;
        private List<string> found = new List<string>();
        private int maximumLines;
        private int maxL;
        private int results = 0;
        private bool generated = false;

        public SearchForm(string location, int maximumLines)
        {
            InitializeComponent();
            this.location = location;
            this.maximumLines = maximumLines;
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            if (searchBox.Text.Length > 0)
            {
                errorLabel.Text = "Please wait, searching...";
                readFile(searchBox.Text);
                for(int i = 0; i < listView1.Columns.Count;i++)
                    listView1.Columns[i].Width = -2;
            }
        }

        private void readFile(string text)
        {
            listView1.Items.Clear();
            results = 0;
            string line;
            maxL = 0;
            bool started = false;
            System.IO.StreamReader file =
                new System.IO.StreamReader(location);
            generated = false;
            while ((line = file.ReadLine()) != null)
            {

                if (line.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 && started)
                {

                    formatLine(line);
                    results++;                    
                }
                if (started) maxL++;
                if (line.Equals("<EOH>")) started = true;
            }
            errorLabel.Text = "Found items: " + results + " of " + maxL;
            file.Close();
        }

        private void formatLine(string line)
        {
            string output = "";
            string command = "";
            int position = 0;
            string[,] subitems = new string[2,18];
            for (int i = 0; i < 18; i++) subitems[0, i] = "";
            bool needChar = false;
            bool needCommand = false;
            int index = 0;
            for(int i = 0; i < line.Length; i++)
            {
                if (line[i] == '<')
                {
                    needChar = false;
                    needCommand = true;
                    //output += " | ";
                    if (output.Length > 0 && index < subitems.Length)
                    {
                        subitems[1,index] = output;
                        index++;
                        output = "";
                    }
                }
                else if (line[i] == '>')
                {
                    needChar = true;
                    needCommand = false;
                    if (position < listView1.Columns.Count && command != "EOR" && command !="part")
                    {
                        subitems[0, index] = command;
                        if(!generated) listView1.Columns[position].Text = command;

                        position++;
                    }
                    command = "";
                }
                else if (needChar) output += line[i];
                else if (needCommand)
                {
                    if (line[i] != ':') command += line[i];
                    else needCommand = false;
                }

            }
            listView1.View = View.Details;
            ListViewItem x = new ListViewItem(subitems[1,0]);
            if (subitems.Length != 0) generated = true;
            else return;
            for (int i = 1; i < listView1.Columns.Count; i++) {
                if (subitems[0, i].Equals(listView1.Columns[i].Text)) x.SubItems.Add(subitems[1, i]);
                else {
                    for (int j = i; j < listView1.Columns.Count - 1; j++)
                    {
                        subitems[0, i] = subitems[0, i + 1];
                        subitems[1, i] = subitems[1, i + 1];
                    }
                    if (subitems[0, i].Equals(listView1.Columns[i].Text)) x.SubItems.Add(subitems[1, i]);
                }
            }
            for (int i = 0; i < 16; i++) Console.Out.WriteLine(subitems[0, i]);
            listView1.Items.Add(x);
        }

        private void SearchForm_Load(object sender, EventArgs e)
        {
            this.AcceptButton = searchButton;
        }
    }
}
