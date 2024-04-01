using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notepad_project
{
    public partial class GoToForm : Form
    {
        public event EventHandler GoToButtonClicked;

        public string LineNumberText => textBox1.Text;

        public GoToForm()
        {
            InitializeComponent();
        }

        private void GoToForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = "1";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // go to button
            GoToButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // cancel button
            this.Close();
        }
    }
}
