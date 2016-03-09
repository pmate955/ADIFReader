using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;


namespace ADIFReader
{
    public partial class Form1 : Form
    {
        private string location = "";
        private int counter = 0;
        private int[] bands = new int[13];                              //Band count
        private List<int> mode = new List<int>();                       //Mode count
        private List<string> modeL = new List<string>();                //Mode label
        private List<int> yearNum = new List<int>();                    //Year count
        private List<string> yearL = new List<string>();                //Year label   
        private List<int> yearMonthNum = new List<int>();               //Year/Month count
        private List<string> yearMonthL = new List<string>();           //Year/Month label
        private int[] times = new int[24];                              //Time count        
        private int[] rst = new int[620];                               //RST 599        
        private int[] rstJT = new int[40];                              //JT65 -30 - 10
        private DateTime now = DateTime.Today;
       

        public Form1()
        {
            InitializeComponent();
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            this.radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
            this.radioButton4.CheckedChanged += new System.EventHandler(this.radioButton4_CheckedChanged);
        }

        private void button1_Click(object sender, EventArgs e)                       //Import first
        {
            for (int b = 0; b < 13; b++) bands[b] = 0;
            for (int i = 0; i < 24; i++) times[i] = 0;
            for (int i = 0; i < 40; i++) rstJT[i] = 0;
            for (int i = 0; i < rst.Length; i++) rst[i] = 0;
            mode.Clear();
            modeL.Clear();
            yearL.Clear();
            yearNum.Clear();
            yearMonthNum.Clear();
            yearMonthL.Clear();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Adi files|*.adi";
            openFileDialog1.Title = "Select an ADI file";
            //label1.Text = "Reading, please wait!";
                 
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {                
               
                location = openFileDialog1.FileName;
                this.readFile();                
                this.updateCharts();               
            }
        }

        

        private void readFile()
        {
            counter = 0;
            string line;            
            System.IO.StreamReader file =
                new System.IO.StreamReader(location);
            progress.Maximum = TotalLines(location);
            while ((line = file.ReadLine()) != null)
            {
                readData(line);
                progress.Value = counter;
                counter++;                
            }            
            label1.Text = "Read: " + counter + " lines OK";
            label2.Text = location;
            file.Close();
            file = null;
            progress.Value = 0;

        }                                                  //Read the source file

        private int TotalLines(string filePath)                                     //Count lines
        {
            using (System.IO.StreamReader r = new System.IO.StreamReader(filePath))
            {
                int i = 0;
                while (r.ReadLine() != null) { i++; }
                return i;
            }
        }

        private byte b2a(int band)
        {
            byte outN = 0;
            switch (band)
            {
                case 160:
                    outN = 0;
                    break;
                case 80:
                    outN = 1;
                    break;
                case 60:
                    outN = 2;
                    break;
                case 40:
                    outN = 3;
                    break;
                case 30:
                    outN = 4;
                    break;
                case 20:
                    outN = 5;
                    break;
                case 17:
                    outN = 6;
                    break;
                case 15:
                    outN = 7;
                    break;
                case 12:
                    outN = 8;
                    break;
                case 10:
                    outN = 9;
                    break;
                case 6:
                    outN = 10;
                    break;
                case 2:
                    outN = 11;
                    break;
                case 70:
                    outN = 12;
                    break;
            }
            return outN;
        }                                               //Band to array

        private byte freq2a(int band)
        {
            byte outN = 0;
            switch (band)
            {
                case 1:
                    outN = 0;
                    break;
                case 3:
                    outN = 1;
                    break;
                case 5:
                    outN = 2;
                    break;
                case 7:
                    outN = 3;
                    break;
                case 10:
                    outN = 4;
                    break;
                case 14:
                    outN = 5;
                    break;
                case 18:
                    outN = 6;
                    break;
                case 21:
                    outN = 7;
                    break;
                case 24:
                    outN = 8;
                    break;
                case 28:
                    outN = 9;
                    break;
                case 50:
                    outN = 10;
                    break;
                case 145:
                    outN = 11;
                    break;
                case 434:
                    outN = 12;
                    break;
            }
            return outN;
        }                                            //Freq to array

        private void m2a(string mod)                                                //Mode to list
        {
            if (!modeL.Contains(mod))
            {
                modeL.Add(mod);
                mode.Add(1);
            } else
            {
                for(int i = 0; i < modeL.Count; i++)
                {
                    if (modeL[i].Equals(mod)){
                        mode[i]++;
                        return;
                    }
                }
            }
        }

        private void y2a(string year)                                               //Year to list
        {
            if (!yearL.Contains(year))
            {
                yearL.Add(year);
                yearNum.Add(1);
            } else
            {
                for(int i = 0; i < yearL.Count; i++)
                {
                    if (yearL[i].Equals(year))
                    {
                        yearNum[i]++;
                        return;
                    }
                }
            }
        }

        private void yM2a(string yearMonth, bool isLast)                                         //Year and month stat
        {
            if (yearMonthL.Count == 0)
            {
                string[] token = yearMonth.Split('-');
                int yearStart = Int32.Parse(token[0]);
                int monthStart = Int32.Parse(token[1]);
                int yearEnd = Int32.Parse(now.ToString("yyyy"));
                int monthEnd = Int32.Parse(now.ToString("MM"));
                for(int years = yearStart; years <= yearEnd; years++)
                {
                    for(int months = monthStart; months <= 12; months++)
                    {
                        if (years == yearEnd && months == monthEnd + 1) return;
                        if(months < 10) yearMonthL.Add(years + "-0" + months);
                        else yearMonthL.Add(years + "-" + months);
                        if (years == yearStart && months == monthStart) yearMonthNum.Add(1);
                        else yearMonthNum.Add(0);
                    }
                    monthStart = 1;
                }

            } else
            {
                if (!isLast)
                {
                    for (int i = 0; i < yearMonthL.Count; i++)
                    {
                        if (yearMonthL[i].Equals(yearMonth))
                        {
                            yearMonthNum[i]++;
                            return;
                        }
                    }
                }
                else
                {
                    bool deleteAble = false;
                    for (int i = 0; i < yearMonthL.Count; i++)
                    {
                        if (deleteAble)
                        {
                            yearMonthL.RemoveAt(i);
                            yearMonthNum.RemoveAt(i);
                        }
                        else if (yearMonthL[i].Equals(yearMonth))
                        {
                            deleteAble = true;
                        }
                    }
                }
            }            
        }

        private void t2a(string time)                                               //Time to array
        {
            int number = Int16.Parse(time);
            if (number >= 0 && number < 24) times[number]++;
        }
        private bool matchPatter(string line, string patt)
        {            
            Regex pattern = new Regex(patt);
            return pattern.IsMatch(line);
        }

        private void rst2a(string rs)                                               //RST to array
        {
            if (rs.Equals("swl") || rs.Equals("SWL") || rs.Length==0) return;
            int num;
            try
            {
                num = Int16.Parse(rs);
            } catch (Exception)
            {
                return;
            }

            if (num > 100)
            {
                if (num < 620) rst[num]++;
            } else if(num < 20)
            {
                rstJT[30 + num]++;
            }
        }

        private void readData(string line)
        {

            bool numOK = false;
            bool modeOK = false;
            bool rstOK = false;            
            StringBuilder temp2 = new StringBuilder();            
            bool start = false;
            bool end = false;
            for(int i = 0; i < line.Length; i++)
            {
                if(line[i] == '>')
                {
                    end = true;
                    start = false;
                    if (temp2.ToString().Equals("QSO_DATE:8:D") || temp2.ToString().Equals("QSO_DATE:8"))                            //Year
                    {
                        y2a(line.Substring(i + 1, 4));
                        yM2a(line.Substring(i + 1, 6).Insert(4, "-"),(counter+1==progress.Maximum));                   
                        temp2.Clear();
                    }
                    else if (temp2.ToString().Equals("TIME_ON:4") || temp2.ToString().Equals("TIME_ON:6"))                                     //Time
                    {
                        t2a(line.Substring(i + 1, 2));                       
                        temp2.Clear();
                    }
                    else if (matchPatter(temp2.ToString(), "RST_SENT:3*") && !rstOK)                                               //RST
                    {
                        string rs = "";
                        for (int j = i + 1; j < line.Length; j++)
                        {
                            if (line[j] == '<' && !rstOK)
                            {
                                rst2a(rs);
                                rstOK = true;
                            }
                            else rs += line[j];
                        }
                    }
                    else if (matchPatter(temp2.ToString(), "BAND:*") && !numOK)                                                      //Band
                    {
                        string num = "";
                        for (int j = i + 1; j < line.Length; j++)
                        {
                            if ((line[j] == 'm' || line[j] == 'c' || line[j] == 'M') && !numOK)
                            {
                                try
                                {
                                    int band = Int16.Parse(num);
                                    bands[b2a(band)]++;
                                }
                                catch (System.FormatException)
                                {
                                    Console.Out.WriteLine("Error: " + num);
                                }
                                numOK = true;
                            }
                            else num += line[j];
                        }
                        temp2.Clear();
                    }
                    else if (matchPatter(temp2.ToString(), "FREQ:*") && !numOK)                                                      //Freq to Band
                    {
                        string num = "";
                        for (int j = i + 1; j < line.Length; j++)
                        {
                            if (line[j] == '.' && !numOK)
                            {
                                try
                                {
                                    int band = Int16.Parse(num);
                                    bands[freq2a(band)]++;
                                }
                                catch (System.FormatException)
                                {
                                    Console.Out.WriteLine("Error: " + num);
                                }
                                numOK = true;
                            }
                            else num += line[j];
                        }
                        temp2.Clear();
                    }
                    else if (matchPatter(temp2.ToString(), "MODE:*") && !modeOK)                                                           //Mode
                    {

                        string mod = "";
                        for (int j = i + 1; j < line.Length; j++)
                        {
                            if (line[j] == '<' && !modeOK)
                            {
                                m2a(mod);
                                modeOK = true;
                            }
                            else mod += line[j];
                        }
                    }
                    else {
                        temp2.Clear();
                    }
                }
                if (start && !end)
                {
                    temp2.Append(line[i]);
                }
                if (line[i] == '<')
                {
                    start = true;
                    end = false;
                }
            }
        }                                                               //Read datas per line

        private void button2_Click(object sender, EventArgs e)                                              //Exit button
        {
            
            System.Windows.Forms.Application.Exit();
            this.Close();
        }

        private void updateCharts()                                                                         //Chart update
        {
            int maxBand = 0;                                                                                //Maximum of datas
            int maxMode = 0;
            int maxYear = 0;
            int maxYearMonth = 0;
            int maxTime = 0;
            int maxRST = 0;
            int maxJT = 0;
            for(int i = 0; i < bands.Length; i++) if (bands[i] > maxBand) maxBand = bands[i];            
            for (int i = 0; i < mode.Count; i++) if (mode[i] > maxMode) maxMode = mode[i];            
            for(int i = 0; i < yearNum.Count; i++) if (yearNum[i] > maxYear) maxYear = yearNum[i];
            for (int i = 0; i < yearMonthNum.Count; i++) if (yearMonthNum[i] > maxYearMonth) maxYearMonth = yearMonthNum[i];           
            for(int i = 0; i < 24; i++) if (times[i] > maxTime) maxTime = times[i];            
            for (int i = 0; i < rst.Length; i++) if (rst[i] > maxRST) maxRST = rst[i];
            for (int i = 0; i < rstJT.Length; i++) if (rstJT[i] > maxJT) maxJT = rstJT[i];
                                                                                                           //Band
            chart1.ChartAreas[0].AxisY.Maximum = maxBand;
            chart1.ChartAreas[0].AxisY.Interval = round(maxBand/10);
            chart1.Series["Bands"].Points.Clear();
            if (bands[12] > 0) chart1.Series["Bands"].Points.AddXY("70 CM", bands[12]);
            if (bands[11] > 0) chart1.Series["Bands"].Points.AddXY("2 M", bands[11]);
            if (bands[10] > 0) chart1.Series["Bands"].Points.AddXY("6 M", bands[10]);
            if (bands[9] > 0) chart1.Series["Bands"].Points.AddXY("10 M", bands[9]);
            if (bands[8] > 0) chart1.Series["Bands"].Points.AddXY("12 M", bands[8]);
            if (bands[7] > 0) chart1.Series["Bands"].Points.AddXY("15 M", bands[7]);           
            if (bands[6] > 0) chart1.Series["Bands"].Points.AddXY("17 M", bands[6]);
            if (bands[5] > 0) chart1.Series["Bands"].Points.AddXY("20 M", bands[5]);
            if (bands[4] > 0) chart1.Series["Bands"].Points.AddXY("30 M", bands[4]);
            if (bands[3] > 0) chart1.Series["Bands"].Points.AddXY("40 M", bands[3]);
            if (bands[2] > 0) chart1.Series["Bands"].Points.AddXY("60 M", bands[2]);
            if (bands[1] > 0) chart1.Series["Bands"].Points.AddXY("80 M", bands[1]);
            if (bands[0] > 0) chart1.Series["Bands"].Points.AddXY("160 M", bands[0]);
            chart2.ChartAreas[0].AxisY.Maximum = maxMode;                                               //Mode
            chart2.ChartAreas[0].AxisY.Interval = round(maxMode / 10);
            chart2.Series["Modes"].Points.Clear();
            for (int i = 0; i < mode.Count; i++)
            {
                chart2.Series["Modes"].Points.AddXY(modeL[i], mode[i]);
            }
            chart3.ChartAreas[0].AxisY.Maximum = maxYear;                                               //Year
            chart3.ChartAreas[0].AxisY.Interval = round(maxYear / 10);
            chart3.Series["Year"].Points.Clear();
            for (int i = 0; i < yearL.Count; i++)
            {
                chart3.Series["Year"].Points.AddXY(yearL[i], yearNum[i]);
            }
                                                                                                        //Time
            chart4.ChartAreas[0].AxisY.Maximum = maxTime;                                               
            chart4.ChartAreas[0].AxisY.Interval = round(maxTime / 10);
            chart4.Series["Time"].Points.Clear();
            for (int i = 0; i < 24; i++)
            {
                chart4.Series["Time"].Points.AddXY(i, times[i]);
            }
            chart5.ChartAreas[0].AxisY.Maximum = maxRST;                                                //RST
            chart5.ChartAreas[0].AxisY.Interval = round(maxRST / 10);
            chart5.Series["RST"].Points.Clear();
            for (int i = 0; i < rst.Length; i++)
            {
                if(rst[i]>0) chart5.Series["RST"].Points.AddXY(i + " ", rst[i]);
            }
            chart6.ChartAreas[0].AxisY.Maximum = maxJT;                                                //RST JT65
            chart6.ChartAreas[0].AxisY.Interval = round(maxJT / 10);
            chart6.Series["JT65 S"].Points.Clear();
            for (int i = 0; i < rstJT.Length; i++)
            {
                if (rstJT[i] > 0) chart6.Series["JT65 S"].Points.AddXY(i-30, rstJT[i]);
            }
            chart7.ChartAreas[0].AxisY.Maximum = maxYearMonth;
            chart7.ChartAreas[0].AxisY.Interval = round(maxYearMonth / 10);
            chart7.Series["Year/Month"].Points.Clear();
            for (int i = 0; i < yearMonthL.Count; i++) chart7.Series["Year/Month"].Points.AddXY(yearMonthL[i], yearMonthNum[i]);


        }

        private int round(int i)
        {
            return ((int)Math.Round(i / 10.0)) * 10;
        }

        private void radioButton1_CheckedChanged(Object sender,                                             //Column chart
                                         EventArgs e)
        {
            // Change the check box position to be opposite its current position.
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (rb.Checked)
                {
                    chart1.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart2.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart3.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart4.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart5.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart6.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart7.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart1.Series["Bands"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                    chart2.Series["Modes"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                    chart3.Series["Year"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                    chart4.Series["Time"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                    chart5.Series["RST"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                    chart6.Series["JT65 S"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                    chart7.Series["Year/Month"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

                }
            }
            
        }
        private void radioButton2_CheckedChanged(Object sender,                                             //Pie chart
                                        EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (rb.Checked)
                {
                    chart1.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart2.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart3.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart4.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart5.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart6.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart7.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart1.Series["Bands"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
                    chart2.Series["Modes"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
                    chart3.Series["Year"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
                    chart4.Series["Time"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
                    chart5.Series["RST"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
                    chart6.Series["JT65 S"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
                    chart7.Series["Year/Month"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
                }
            }
        }

        private void radioButton3_CheckedChanged(Object sender,                                             //Line chart
                                        EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (rb.Checked)
                {
                    chart1.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart2.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart3.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart4.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart5.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart6.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart7.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart1.Series["Bands"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    chart2.Series["Modes"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    chart3.Series["Year"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    chart4.Series["Time"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    chart5.Series["RST"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    chart6.Series["JT65 S"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    chart7.Series["Year/Month"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                }
            }
        }

        private void radioButton4_CheckedChanged(Object sender,                                             //SPLine chart
                                       EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (rb.Checked)
                {
                    chart1.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart2.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart3.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart4.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart5.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart6.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart7.ChartAreas[0].Area3DStyle.Enable3D = false;
                    chart1.Series["Bands"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                    chart2.Series["Modes"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                    chart3.Series["Year"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                    chart4.Series["Time"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                    chart5.Series["RST"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                    chart6.Series["JT65 S"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                    chart7.Series["Year/Month"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                }
            }
        }

    }
}
