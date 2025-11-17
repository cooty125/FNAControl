namespace FNAWinForms
{
    public partial class Form1 : Form
    {
        FNA fna;
        public Form1()
        {
            InitializeComponent();

            this.fna = new FNA();
            this.fna.FPSMax = 60;
            this.fna.Dock = DockStyle.Fill;
            this.splitContainer1.Panel2.Controls.Add(this.fna);
        }
    }
}

