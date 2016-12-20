using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form5 : Form
    {
        public double[] h_conv, h_rad, q_conv, q_rad;
        public double dt,alpha,F12,F21;
        public int N;

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        public Form5()
        {
            InitializeComponent();
            h_conv = new double[1];
            h_rad = new double[1];
            q_conv = new double[1];
            q_rad = new double[1];
            N = 1;
            dt = 1.0;
            alpha = 0.0;
            F12 = 0.0;
            F21 = 0.0;
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            plot();
        }

        public void plot()
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart2.Series[0].Points.Clear();
            chart2.Series[1].Points.Clear();
            int di = N / 30;
            if (di < 1) { di = 1; }
            for (int i = 0; i < N - 1; i += di)
            {
                chart1.Series[0].Points.AddXY(i * dt, h_conv[i]);
                chart1.Series[1].Points.AddXY(i * dt, h_rad[i]);
                chart2.Series[0].Points.AddXY(i * dt, q_conv[i]);
                chart2.Series[1].Points.AddXY(i * dt, q_rad[i]);
            }
            label1.Text = string.Format("Hotdog thermal diffusivity alpha = {0:0.###E-00} [m^2/s]", alpha);
            label2.Text = string.Format("View factor from hotdog to coal bed F21 = {0:0.##}", F21);
            label3.Text = string.Format("View factor from coal bed to hotdog F12 = {0:0.##}", F12);
        }
    }
}
