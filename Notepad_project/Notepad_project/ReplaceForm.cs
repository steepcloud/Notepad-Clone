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
    public partial class ReplaceForm : Form
    {
        private bool matchCaseEnabled = false;

        private bool wrapAroundEnabled = false;

        public bool MatchCase => matchCaseEnabled;

        public bool WrapAround => wrapAroundEnabled;

        public string FindText => textBox1.Text;

        public string ReplaceText => textBox2.Text;

        public event EventHandler FindNextButtonClicked;

        public event EventHandler ReplaceButtonClicked;

        public event EventHandler ReplaceAllButtonClicked;

        public ReplaceForm()
        {
            InitializeComponent();
        }

        private void ReplaceForm_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // find what text box
            string searchText = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // replace with text box
            string replaceText = textBox2.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // find next button
            FindNextButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // replace button
            ReplaceButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // replace All button
            ReplaceAllButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //  cancel button
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // match case checkbox
            matchCaseEnabled = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            // wrap around checkbox
            wrapAroundEnabled = checkBox2.Checked;
        }
    }
}
