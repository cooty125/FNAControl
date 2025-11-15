namespace FNAWinForms
{
    public partial class Form1 : Form
    {
        FNA fna;
        public Form1()
        {
            InitializeComponent();

            fna = new FNA();
            fna.Dock = DockStyle.Fill;
            this.splitContainer1.Panel2.Controls.Add(fna);
        }
    }
}
