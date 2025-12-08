using System;
using System.Windows.Forms;
using VillainLairManager.Repositories;

namespace VillainLairManager.Forms
{
    public partial class SchemeManagementForm : Form
    {
        private readonly IEvilSchemeRepository _schemeRepository;

        public SchemeManagementForm(IEvilSchemeRepository schemeRepository)
        {
            _schemeRepository = schemeRepository ?? throw new ArgumentNullException(nameof(schemeRepository));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Evil Scheme Management";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblStub = new Label
            {
                Text = "TODO: Implement Evil Scheme Management Form\n\n" +
                       "Requirements:\n" +
                       "- DataGridView with color-coded status\n" +
                       "- Success likelihood calculation in UI (anti-pattern)\n" +
                       "- Budget validation in button handler (anti-pattern)\n" +
                       "- Status transition logic in ComboBox event (anti-pattern)",
                Location = new System.Drawing.Point(50, 50),
                Size = new System.Drawing.Size(800, 400),
                Font = new System.Drawing.Font("Arial", 10)
            };
            this.Controls.Add(lblStub);
        }
    }
}
