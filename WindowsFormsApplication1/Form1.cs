using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        double g = 9.81;
        double sigma = 5.67E-8;
        double r0, density, k, c, alpha, e2, T_init;
        double Tc, Ta, T_inf, T_film_free, D_coal, e1;
        double dr, t_total, dt, F, L;
        double[,] T;
        double[] hc, qc, hr, qr;
        int M, N;
        double M_double, N_double;
        double F12, F21;
        double B, rho_free, miu_free, Gr, V;
        double dummy;
        double T_center, T_surface;
        int term;
        double theta;
        bool toggle;
        double T_film_forced, rho_forced, miu_forced, k_forced;
        double progress;
        double h_anl;
        double[] z, C;
        double Re, Pr, Nu, T2_, h_tot, Bi, cp;
        BackgroundWorker bw;
        AboutBox1 AboutBox;
        Form2 ReadMe = new Form2();
        Form3 warning1 = new Form3();
        Form4 warning2 = new Form4();
        Form5 moreplots = new Form5();
        Form6 mallocerr = new Form6();

        public Form1()
        {
            InitializeComponent();
            chart1.Images.Add(new System.Windows.Forms.DataVisualization.Charting.NamedImage("watermark", Properties.Resources.watermark));
            chart1.ChartAreas[0].BackImage = "watermark";
            chart2.Images.Add(new System.Windows.Forms.DataVisualization.Charting.NamedImage("watermark", Properties.Resources.watermark));
            chart2.ChartAreas[0].BackImage = "watermark";

            update_opt_dt();
            toggle = false;
            //chart1
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            numericUpDown1.Enabled = false;
            //chart2
            chart2.Series[0].Points.Clear();
            chart2.Series[1].Points.Clear();
            numericUpDown2.Enabled = false;
            //hotdog properties
            r0 = 0.0;
            density = 0.0;
            k = 0.0;
            c = 0.0;
            alpha = 0.0;
            e2 = 0.0;
            T_init = 0.0;
            //charcoal properties
            Tc = 0.0;
            Ta = 0.0;
            T_inf = 0.0;
            T_film_free = 0.0;
            D_coal = 0.0;
            e1 = 0.0;
            //simulation setting
            dr = 0.0;
            M = 0;
            M_double = 0.0;
            t_total = 0.0;
            dt = 0.0;
            N = 0;
            N_double = 0.0;
            F = 0.0;
            L = 0.0;
            h_anl = 0.0;
            z = new double[1];
            C = new double[1];
            T_center = 0.0;
            T_surface = 0.0;
            term = 1;
            theta = 0.0;
            //view factors
            F12 = 0.0;
            F21 = 0.0;
            //free convection around charcoal
            B = 0.0;
            rho_free = 0.0;
            miu_free = 0.0;
            Gr = 0.0;
            V = 0.0;

            //temperature profile
            T = new double[1, 1];

            //convection profile
            hc = new double[1];
            qc = new double[1];

            //radiation profile
            hr = new double[1];
            qr = new double[1];

            //background thread
            bool cancel = false;
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(
            delegate (object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;
                cancel = false;
                //hotdog properties
                r0 = double.Parse(r_text.Text) / 100.0;
                density = double.Parse(rho_text.Text);
                k = double.Parse(k_text.Text);
                c = double.Parse(c_text.Text);
                alpha = k / density / c;
                e2 = double.Parse(e2_text.Text);
                T_init = double.Parse(T_init_text.Text) + 273.0;
                L = double.Parse(L_text.Text) / 100.0;

                //charcoal properties
                Tc = double.Parse(Tc_text.Text) + 273.0;
                Ta = double.Parse(Ta_text.Text) + 273.0;
                T_inf = double.Parse(T_text.Text) + 273.0;
                T_film_free = (Tc + Ta) / 2.0;
                D_coal = double.Parse(D_coal_text.Text)/100.0;
                e1 = double.Parse(e1_text.Text);
                
                //simulation setting
                dr = double.Parse(dr_text.Text) / 100.0;
                M = System.Convert.ToInt32(r0 / dr + 1.0);
                M_double = System.Convert.ToDouble(M);
                t_total = double.Parse(t_tot_text.Text);
                dt = double.Parse(dt_text.Text);
                N = System.Convert.ToInt32(t_total / dt + 1.0);
                N_double = System.Convert.ToDouble(N);
                F = alpha * dt / dr / dr;
                h_anl = double.Parse(h_text.Text);
                T_center = double.Parse(T_center_text.Text);
                T_surface = double.Parse(T_surface_text.Text);
                term = (int) numericUpDown3.Value;
                z = new double[term];
                C = new double[term];
                
                //view factors
                F12 = r0 / 0.35 * (Math.Atan(0.175 / L) - Math.Atan(-0.175 / L));
                F21 = 0.35 / Math.PI / 2.0 / r0 * F12;
                
                //free convection around charcoal
                B = 1.0 / T_film_free;
                rho_free = 101000.0 / (287.0 * T_film_free);
                miu_free = 0.00001716 * Math.Pow(T_film_free / 273.0, 1.5) * 384.0 / (T_film_free + 111.0);
                Gr = g * B * (Tc - Ta) * Math.Pow(D_coal, 3.0) / Math.Pow(miu_free / rho_free, 2.0);
                V = Math.Sqrt(Gr) * miu_free / rho_free / D_coal / 2.0;

                //temperature profile
                try
                {
                    T = new double[M, N];
                }
                catch
                {
                    cancel = true;
                }
                if (!cancel)
                {
                    for (int i = 0; i < M; ++i)
                    {
                        T[i, 0] = T_init;
                    }

                    //convection profile
                    hc = new double[N - 1];
                    qc = new double[N - 1];

                    //radiation profile
                    hr = new double[N - 1];
                    qr = new double[N - 1];

                    for (int i = 0; i < term; ++i)
                    {
                        z[i] = solve(System.Convert.ToDouble(i) * Math.PI, (System.Convert.ToDouble(i) + 1) * Math.PI);
                        C[i] = 2 * besselJ(1, z[i]) / z[i] / (Math.Pow(besselJ(0, z[i]), 2.0) + Math.Pow(besselJ(1, z[i]), 2.0));
                    }

                    for (int i = 0; i < N - 1; ++i)
                    {
                        //forced convection
                        T_film_forced = (T[M - 1, i] + T_inf) / 2.0;
                        cp = 0.000352 * Math.Pow(T_film_forced, 2.0) - 0.161 * T_film_forced + 1021.48;
                        rho_forced = 101000.0 / (287.0 * T_film_forced);
                        miu_forced = 0.00001716 * Math.Pow(T_film_forced / 273.0, 1.5) * 384.0 / (T_film_forced + 111.0);
                        k_forced = 0.0241 * Math.Pow(T_film_forced / 273.0, 1.5) * 467.0 / (T_film_forced + 194.0);
                        Re = V * r0 * 2.0 / miu_forced * rho_forced;
                        Pr = cp * miu_forced / k_forced;
                        Nu = 0.3 + 0.62 * Math.Pow(Re, 0.5) * Math.Pow(Pr, 1.0 / 3.0) / Math.Pow((1.0 + Math.Pow(0.4 / Pr, 2.0 / 3.0)), 0.25) * Math.Pow((1.0 + Math.Pow(Re / 282000.0, 0.625)), 0.8);
                        hc[i] = Nu * k_forced / 2.0 / r0;
                        qc[i] = -hc[i] * (T[M - 1, i] - T_inf);

                        //radiation
                        T2_ = T[M - 1, i] / Math.Pow(System.Convert.ToDouble(e1 * F21), 0.25);
                        hr[i] = e1 * e2 * F21 * sigma * (Tc + T2_) * (Tc * Tc + T2_ * T2_);
                        qr[i] = -hr[i] * (T[M - 1, i] - T_inf);

                        //total
                        h_tot = hc[i] + hr[i];
                        if (opt1.Checked) { Bi = h_tot * dr / k; }
                        if (opt2.Checked) { Bi = h_anl * dr / k; }

                        //nodal equations
                        T[0, i + 1] = (1.0 - 4.0 * F) * T[0, i] + 4.0 * F * T[1, i];
                        for (int m = 1; m < M - 1; ++m)
                        {
                            T[m, i + 1] = F * (1.0 - 1.0 / 2.0 / (m + 1.0)) * T[m - 1, i] + F * (1.0 + 1.0 / 2.0 / (m + 1.0)) * T[m + 1, i] + (1.0 - 2.0 * F) * T[m, i];
                        }
                        T[M - 1, i + 1] = 2.0 * F * (1.0 - 1.0 / 2.0 / (M - 1.0)) * T[M - 2, i] + (1.0 + 2.0 * F * (1.0 / 2.0 / (M - 1.0) - 1.0 - Bi)) * T[M - 1, i] + 2.0 * F * Bi * T_inf;
                        double i_double = System.Convert.ToDouble(i);
                        progress = (i_double * M_double + M_double) / (N_double * M_double - M_double) * 100.0;
                        b.ReportProgress((int)progress);
                    }
                }
                b.ReportProgress(100);
            });

            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate (object o, ProgressChangedEventArgs args)
            {
                progressBar.Value = args.ProgressPercentage;
                if (cancel)
                {
                    mallocerr.Close();
                    mallocerr = new Form6();
                    mallocerr.Show();
                }
            });

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate (object o, RunWorkerCompletedEventArgs args)
            {
                button1.Enabled = true; 
                if (!cancel)
                {
                    pictureBox1.Enabled = true;
                    button4.Enabled = true;
                    //find solution
                    int opt_n = 0;
                    while ((opt_n < N - 1) && (T[0, opt_n] < T_center + 273)) { opt_n++; }
                    if (opt_n < N - 1)
                    {
                        result.Text = String.Format("Optimal cooking duration: \n{0:0.##} minutes", System.Convert.ToDouble(opt_n) * dt / 60.0);
                        if (T[M - 1, opt_n] > T_surface + 273)
                        {
                            comment.Text = String.Format("Warning -- Burnt surface!\nSurface temperature: \n{0:0.##} °C", T[M - 1, opt_n] - 273.0);
                        }
                    }
                    else { comment.Text = "Undercooked -- Try \nincreasing t_tot :)"; }
                    pictureBox1.Enabled = true;
                    //plot1
                    chart1.ChartAreas["ChartArea1"].AxisX.Maximum = r0*100.0;
                    chart1.ChartAreas["ChartArea1"].AxisX.Interval = r0*20.0;
                    chart1.ChartAreas["ChartArea1"].AxisY.Maximum = 110.0;
                    chart1.ChartAreas["ChartArea1"].AxisY.Interval = 10.0;
                    numericUpDown1.Value = 0;
                    numericUpDown1.Increment = System.Convert.ToDecimal((N - 1) / 50 * dt);
                    numericUpDown1.Maximum = System.Convert.ToDecimal(t_total);
                    makePlot1(T);
                    numericUpDown1.Enabled = true;

                    //plot2
                    chart2.ChartAreas["ChartArea1"].AxisX.Maximum = t_total;
                    chart2.ChartAreas["ChartArea1"].AxisX.Interval = t_total / 5.0;
                    chart2.ChartAreas["ChartArea1"].AxisY.Maximum = 110.0;
                    chart2.ChartAreas["ChartArea1"].AxisY.Interval = 10.0;
                    numericUpDown2.Value = 0;
                    numericUpDown2.Increment = System.Convert.ToDecimal((M - 1) / 50 * dr * 100);
                    numericUpDown2.Maximum = System.Convert.ToDecimal(r0 * 100);
                    makePlot2(T);
                    numericUpDown2.Enabled = true;
                }
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            result.Text = "";
            comment.Text = "";
            bool valid_input = validate();
            if (valid_input == false)
            {
                warning2.Close();
                warning2 = new Form4();
                warning2.Show();
            }
            else
            {
                dt = double.Parse(dt_text.Text);
                if (dt > update_opt_dt())
                {
                    warning1.Close();
                    warning1 = new Form3();
                    warning1.Show();
                }
                else
                {
                    numericUpDown1.Enabled = false;
                    numericUpDown2.Enabled = false;
                    pictureBox1.Enabled = false;
                    button1.Enabled = false;
                    button4.Enabled = false;
                    bw.RunWorkerAsync();
                }
            }
        }

        public void makePlot1(double[,] T)
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();
            chart1.Series[3].Points.Clear();
            double T_anl;
            double time = System.Convert.ToDouble(numericUpDown1.Value);
            int n = (int)(time / dt);
            int di = M / 30;
            if (di < 1) { di = 1; }
            for (int i = 0; i < M; i += di)
            {
                chart1.Series[0].Points.AddXY(i * dr * 100.0, T[i, n] - 273.0);
                if (toggle)
                {
                    theta = 0.0;
                    for (int ii = 0; ii < term; ++ii)
                    {
                        theta += C[ii] * Math.Exp(-Math.Pow(z[ii], 2.0) * alpha * time / Math.Pow(r0, 2.0)) * besselJ(0, z[ii] / r0 * i * dr);
                    }
                    T_anl = (T_init - T_inf) * theta + T_inf - 273.0;
                    chart1.Series[1].Points.AddXY(i * dr * 100.0, T_anl);
                }
                chart1.Series[2].Points.AddXY(i * dr * 100.0, T_center);
                chart1.Series[3].Points.AddXY(i * dr * 100.0, T_surface);
            }

        }

        public void makePlot2(double[,] T)
        {
            chart2.Series[0].Points.Clear();
            chart2.Series[1].Points.Clear();
            chart2.Series[2].Points.Clear();
            chart2.Series[3].Points.Clear();
            double T_anl;
            double radius = System.Convert.ToDouble(numericUpDown2.Value) / 100;
            int m = (int)(radius / dr);
            int di = N / 30;
            if (di < 1) { di = 1; }
            for (int i = 0; i < N; i += di)
            {
                chart2.Series[0].Points.AddXY(i * dt, T[m, i] - 273);
                if (toggle)
                {
                    theta = 0.0;
                    for (int ii = 0; ii < term; ++ii)
                    {
                        theta += C[ii] * Math.Exp(-Math.Pow(z[ii], 2.0) * alpha * i*dt / Math.Pow(r0, 2.0)) * besselJ(0, z[ii] / r0 * radius);
                    }
                    T_anl = (T_init - T_inf) * theta + T_inf - 273.0;
                    chart2.Series[1].Points.AddXY(i * dt, T_anl);
                }
                chart2.Series[2].Points.AddXY(i * dt, T_center);
                chart2.Series[3].Points.AddXY(i * dt, T_surface);
            }
        }

        public double update_opt_dt()
        {
            bool density_ = double.TryParse(rho_text.Text, out density);
            bool k_ = double.TryParse(k_text.Text, out k);
            bool c_ = double.TryParse(c_text.Text, out c);
            bool dr_ = double.TryParse(dr_text.Text, out dr);
            double dt = 0.0;
            if (density_ && k_ && c_ && dr_)
            {
                dr = dr / 100.0;
                alpha = k / density / c;
                dt = dr * dr / 4.0 / alpha;
                opt_dt.Text = string.Format("Maximum dt = {0:0.####} s \nto ensure convergence", dt);
            }
            return dt;
        }

        static double Fact(double n)
        {
            if (n <= 1.0)
                return 1.0;
            return n * Fact(n - 1.0);
        }

        public double besselJ(int n, double x)
        {
            double result;
            double J = 0;
            double J_next = 1.0 / Math.Pow(2.0, System.Convert.ToDouble(n)) / Fact(n);
            int m = 1;
            double e = 1;
            while (e > 1e-14)
            {
                J = J_next;
                J_next = J + Math.Pow(-1.0, System.Convert.ToDouble(m)) * Math.Pow(x, System.Convert.ToDouble(2 * m)) / Math.Pow(2.0, System.Convert.ToDouble(2 * m + n)) / System.Convert.ToDouble(Fact(m) * Fact(m + n));
                e = Math.Abs(J_next - J);
                m++;
            }
            result = J_next * Math.Pow(x, System.Convert.ToDouble(n));
            return result;
        }

        public double solve(double x0, double x1)
        {
            int itr = 1;
            double a = x0;
            double b = x1;
            double d = 0.0;
            double fd = 0.0;
            double fa = 0.0;
            while (itr <= 10000)
            {
                d = (a + b) / 2;
                fd = d * besselJ(1, d) / besselJ(0, d) - h_anl * r0 / k;
                fa = a * besselJ(1, a) / besselJ(0, a) - h_anl * r0 / k;
                if (fd == 0 || (b - a) / 2 < 1e-14)
                {
                    return d;
                }
                itr++;
                if (fd * fa > 0) { a = d; }
                else { b = d; }
            }
            return 0;
        }

        public bool validate()
        {
            if (!double.TryParse(r_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(rho_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(k_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(c_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(e2_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(T_init_text.Text, out dummy)) { return false; }
            if (!double.TryParse(L_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(Tc_text.Text, out dummy)) { return false; }
            if (!double.TryParse(Ta_text.Text, out dummy)) { return false; }
            if (!double.TryParse(T_text.Text, out dummy)) { return false; }
            if (!double.TryParse(D_coal_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(e1_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(dr_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(t_tot_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(dt_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(h_text.Text, out dummy)) { return false; }
            else if (dummy <= 0) { return false; }
            if (!double.TryParse(T_center_text.Text, out dummy)) { return false; }
            if (!double.TryParse(T_surface_text.Text, out dummy)) { return false; }
            return true;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            makePlot2(T);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            makePlot1(T);
        }

        private void exitapp_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (toggle) {
                pictureBox1.Image = Properties.Resources.toggle_off;
                toggle = false;
            }
            else {
                pictureBox1.Image = Properties.Resources.toggle_on;
                toggle = true;
            }
            makePlot1(T);
            makePlot2(T);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            moreplots.Close();
            moreplots = new Form5();
            moreplots.h_conv = hc;
            moreplots.h_rad = hr;
            moreplots.q_conv = qc;
            moreplots.q_rad = qr;
            moreplots.N = N;
            moreplots.dt = dt;
            moreplots.alpha = alpha;
            moreplots.F12 = F12;
            moreplots.F21 = F21;
            moreplots.Show();
        }

        private void button3_CheckedChanged(object sender, EventArgs e)
        {
            if (button3.Checked) { ReadMe.Show(); }
            else { ReadMe.Hide(); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AboutBox = new AboutBox1();
            AboutBox.Show();
        }

        private void k_text_TextChanged(object sender, EventArgs e)
        {
            update_opt_dt();
        }

        private void rho_text_TextChanged(object sender, EventArgs e)
        {
            update_opt_dt();
        }

        private void c_text_TextChanged(object sender, EventArgs e)
        {
            update_opt_dt();
        }

        private void dr_text_TextChanged(object sender, EventArgs e)
        {
            update_opt_dt();
        }

    }
}
