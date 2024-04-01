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
    public partial class FindForm : Form
    {
        private bool matchCaseEnabled = false;
        private bool searchUpwards = false;
        private bool wrapAroundEnabled = false;

        public string SearchText => textBox1.Text;

        public bool MatchCase => matchCaseEnabled;

        public bool WrapAround => wrapAroundEnabled;

        public bool SearchUp => searchUpwards;

        public event EventHandler FindNextButtonClicked;

        public FindForm()
        {
            InitializeComponent();
        }

        private void FindForm_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // match case checkbutton
            matchCaseEnabled = checkBox1.Checked;
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            // wrap around checkbutton
            wrapAroundEnabled = checkBox2.Checked;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Find next button
            FindNextButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Cancel button
            this.Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            // Up radio button
            if (radioButton1.Checked)
            {
                searchUpwards = true;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            // Down radio button
            if (radioButton2.Checked)
            {
                searchUpwards = false;
            }
        }

    }
}
