using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Notepad_project
{
    public partial class MainForm : Form
    {
        private string fileName = "";

        private string filePath = "";

        private string searchText = "";

        private bool matchCase = false;

        private bool wrapAround = false;

        private bool matchCaseReplace = false;

        private bool wrapAroundReplace = false;

        private bool searchUp = false;

        private bool exitingByMenuItem = false;

        private int MinimumFontSize = 5;

        private Font defaultFont;

        private FontDialog fontDialog = new FontDialog();

        private Stack<string> undoStack = new Stack<string>();

        private Stack<string> redoStack = new Stack<string>();

        public MainForm()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateWindowTitle();
            defaultFont = textBox1.Font;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!exitingByMenuItem && (string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(textBox1.Text)))
            {
                DialogResult result = MessageBox.Show("Do you want to save changes to Untitled?", "Notepad", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    saveAsToolStripMenuItem_Click(sender, e);
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            else if (AreChangesUnsaved())
            {
                DialogResult result = MessageBox.Show("Do you want to save changes to " + (string.IsNullOrEmpty(fileName) ? "Untitled" : fileName) + "?", "Notepad", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.N: // ctrl + N for New file, ctrl + shift + N for New Window
                    if (e.Control)
                    {
                        newToolStripMenuItem_Click(sender, e);
                    }
                    else if (e.Control && e.Shift)
                    {
                        newWindowToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.S: // ctrl + S for file saving, ctrl + shift + S for Save as...
                    if (e.Control)
                    {
                        saveToolStripMenuItem_Click(sender, e);
                    }
                    else if (e.Control && e.Shift)
                    {
                        saveAsToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.O: // ctrl + O for Open file
                    if (e.Control)
                    {
                        openToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.Z: // ctrl + Z for Undo, ctrl + shift + Z for Redo
                    if (e.Control)
                    {
                        undoToolStripMenuItem_Click(sender, e);
                    }
                    else if (e.Control && e.Shift)
                    {
                        redoToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.X: // ctrl + X for Cut
                    if (e.Control)
                    {
                        cutToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.C: // ctrl + C for Copy
                    if (e.Control)
                    {
                        copyToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.V: // ctrl + V for Paste
                    if (e.Control)
                    {
                        pasteToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.Delete: // Del for Delete
                    deleteToolStripMenuItem_Click(sender, e);
                    break;
                case Keys.F: // ctrl + F for Find
                    if (e.Control)
                    {
                        findToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.F3: // F3 for Find Next, shift + F3 for Find Previous
                    if (e.Shift)
                    {
                        findPreviousToolStripMenuItem_Click(sender, e);
                    }
                    else
                    {
                        findNextToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.A: // ctrl + A for Select All
                    if (e.Control)
                    {
                        selectAllToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.F5: // F5 for Time/Date
                    timeDateToolStripMenuItem_Click(sender, e);
                    break;
                case Keys.Oemplus: // ctrl + for Zoom in
                    if (e.Control)
                    {
                        zoomInToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.OemMinus: // ctrl - for Zoom out
                    if (e.Control)
                    {
                        zoomOuToolStripMenuItem_Click(sender, e);
                    }
                    break;
                case Keys.D0: // ctrl + 0 for Restoring Default Zoom
                    if (e.Control)
                    {
                        restoreDefaultZoomToolStripMenuItem_Click(sender, e);
                    }
                    break;
                default:
                    break;
            }
        }

        private void UpdateWindowTitle()
        {
            if (string.IsNullOrEmpty(fileName))
            {
                this.Text = "Untitled - Notepad";
            } 
            else
            {
                fileName = Path.GetFileNameWithoutExtension(filePath);
                this.Text = fileName + " - Notepad";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            undoStack.Push(textBox1.Text);
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var replaceForm = new ReplaceForm();
            replaceForm.FindNextButtonClicked += ReplaceForm_FindNextButtonClicked;
            replaceForm.ReplaceButtonClicked += ReplaceForm_ReplaceButtonClicked;
            replaceForm.ReplaceAllButtonClicked += ReplaceForm_ReplaceAllButtonClicked;
            replaceForm.MaximizeBox = false;
            replaceForm.MinimizeBox = false;
            replaceForm.FormClosed += (s, args) =>
            {
                this.Focus();
            };
            replaceForm.Show(this);
        }

        private void ReplaceForm_ReplaceButtonClicked(object sender, EventArgs e)
        {
            ReplaceForm replaceForm = (ReplaceForm)sender;

            string findText = replaceForm.FindText;
            string replaceText = replaceForm.ReplaceText;

            matchCaseReplace = replaceForm.MatchCase;
            wrapAroundReplace = replaceForm.WrapAround;

            StringComparison comparisonType = matchCaseReplace ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            int startIndex = textBox1.SelectionStart;
            int index;

            if (wrapAroundReplace)
            {
                index = textBox1.Text.IndexOf(findText, comparisonType);
                if (index == -1)
                {
                    index = 0;
                }
            }
            else
            {
                index = textBox1.Text.IndexOf(findText, startIndex, comparisonType);
            }

            if (index != -1)
            {
                textBox1.Text = textBox1.Text.Remove(index, findText.Length).Insert(index, replaceText);
                textBox1.Select(index, replaceText.Length);
                textBox1.ScrollToCaret();
            }
            else
            {
                MessageBox.Show("Text not found.", "Replace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ReplaceForm_ReplaceAllButtonClicked(object sender, EventArgs e)
        {
            ReplaceForm replaceForm = (ReplaceForm)sender;

            string findText = replaceForm.FindText;
            string replaceText = replaceForm.ReplaceText;

            string mainText = textBox1.Text;

            string modifiedText = mainText.Replace(findText, replaceText);

            textBox1.Text = modifiedText;
        }

        private void zoomOuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float newSize = textBox1.Font.Size - 2;
            if (newSize >= MinimumFontSize)
            {
                textBox1.Font = new Font(textBox1.Font.FontFamily, newSize);
            }
            else
            {
                MessageBox.Show("Font size cannot be reduced further.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(fileName) || !string.IsNullOrEmpty(textBox1.Text))
            {
                DialogResult result = MessageBox.Show("Do you want to save changes to " + (string.IsNullOrEmpty(fileName) ? "Untitled" : fileName) + "?", "Notepad", MessageBoxButtons.YesNoCancel);
                
                if (result == DialogResult.Yes)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();

                    saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog.FileName, textBox1.Text);
                    }

                    fileName = "";

                    filePath = "";

                    textBox1.Text = "";

                    UpdateWindowTitle();
                }
                else if (result == DialogResult.No)
                {
                    fileName = "";
                    filePath = "";
                    textBox1.Text = "";
                    UpdateWindowTitle();
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
            else
            {
                fileName = "";
                filePath = "";
                textBox1.Text = "";
                UpdateWindowTitle();
            }
        }

        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm newform = new MainForm();

            newform.fileName = "";
            newform.filePath = "";
            newform.textBox1.Text = "";

            newform.Show();
        }

        private bool AreChangesUnsaved()
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return !string.IsNullOrEmpty(textBox1.Text);
            }
            else
            {
                string fileContent = File.ReadAllText(filePath);
                return !string.Equals(textBox1.Text, fileContent);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AreChangesUnsaved())
            {
                DialogResult result = MessageBox.Show("Do you want to save changes to " + (string.IsNullOrEmpty(fileName) ? "Untitled" : fileName) + "?", "Notepad", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Open";
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filePath = openFileDialog.FileName;

                    fileName = Path.GetFileName(filePath);

                    string fileContent = File.ReadAllText(filePath);

                    textBox1.Text = fileContent;

                    UpdateWindowTitle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                return;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.DefaultExt = "txt";
                saveFileDialog.AddExtension = true;
                saveFileDialog.Title = "Save As";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                saveFileDialog.FileName = "Untitled.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = saveFileDialog.FileName;
                    fileName = Path.GetFileName(filePath);
                    SaveFile(filePath);
                }
            }
            else
            {
                string fileContent = File.ReadAllText(filePath);
                if (!string.Equals(textBox1.Text, fileContent))
                {
                    SaveFile(filePath);
                }
            }
        }

        private void SaveFile(string filePath)
        {
            try
            {
                File.WriteAllText(filePath, textBox1.Text, Encoding.UTF8);
                UpdateWindowTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.AddExtension = true;
            saveFileDialog.Title = "Save As";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = saveFileDialog.FileName;
                fileName = Path.GetFileName(filePath);
                SaveFile(saveFileDialog.FileName);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exitingByMenuItem = true;
            this.Close();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 1)
            {
                redoStack.Push(textBox1.Text);

                undoStack.Pop();

                string previousText = undoStack.Pop();

                textBox1.Text = previousText;

                textBox1.SelectionStart = textBox1.Text.Length;
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectionLength > 0)
            {
                Clipboard.SetText(textBox1.SelectedText);
                textBox1.SelectedText = "";
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectionLength > 0)
            {
                Clipboard.SetText(textBox1.SelectedText);
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                textBox1.Paste(Clipboard.GetText());
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectionLength > 0)
            {
                textBox1.SelectedText = "";
            }
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var findForm = new FindForm();
            findForm.FindNextButtonClicked += FindForm_FindNextButtonClicked;
            findForm.SearchDirectionChanged += FindForm_SearchDirectionChanged;
            findForm.MaximizeBox = false;
            findForm.MinimizeBox = false;
            findForm.FormClosed += (s, args) =>
            {
                this.Focus();
            };
            findForm.Show(this);
        }

        private void FindForm_FindNextButtonClicked(object sender, EventArgs e)
        {
            FindForm findForm = (FindForm)sender;

            searchText = findForm.SearchText;
            matchCase = findForm.MatchCase;
            wrapAround = findForm.WrapAround;
            searchUp = findForm.SearchUp;

            findNextToolStripMenuItem_Click(sender, e);
        }

        private void FindForm_SearchDirectionChanged(object sender, FindForm.SearchDirectionChangedEventArgs e)
        {
            searchUp = e.SearchUpwards;
        }

        private void ReplaceForm_FindNextButtonClicked(object sender, EventArgs e)
        {
            ReplaceForm replaceForm = (ReplaceForm)sender;

            matchCaseReplace = replaceForm.MatchCase;
            wrapAroundReplace = replaceForm.WrapAround;

            findNextToolStripMenuItem_Click(sender, e);
        }

        private void findNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int startIndex = textBox1.SelectionStart;

            StringComparison comparisonType = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            int index;

            if (searchUp)
            {
                SendKeys.Send("{LEFT}");
                index = textBox1.Text.LastIndexOf(searchText, startIndex, comparisonType);
            }
            else
            {
                SendKeys.Send("{RIGHT}");
                index = textBox1.Text.IndexOf(searchText, startIndex, comparisonType);
            }

            if (index != -1)
            {
                textBox1.ScrollToCaret();
            }
            else if (wrapAround)
            {
                if (searchUp)
                {
                    SendKeys.Send("{LEFT}");
                    index = textBox1.Text.LastIndexOf(searchText, comparisonType);
                }
                else
                {
                    SendKeys.Send("{RIGHT}");
                    index = textBox1.Text.IndexOf(searchText, comparisonType);
                }

                if (index != -1)
                {
                    textBox1.Select(index, searchText.Length);
                    textBox1.ScrollToCaret();
                }
                else
                {
                    MessageBox.Show("Text not found.", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Text not found.", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            this.Focus();
        }

        private void findPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int startIndex = textBox1.SelectionStart;

            StringComparison comparisonType = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            int index;

            SendKeys.Send("{LEFT}");

            index = textBox1.Text.LastIndexOf(searchText, startIndex, comparisonType);
            
            if (index != -1)
            {
                textBox1.ScrollToCaret();
            }
            else
            {
                MessageBox.Show("Text not found.", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            this.Focus();
        }

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var goToForm = new GoToForm())
            {
                if (!textBox1.WordWrap)
                {
                    goToForm.GoToButtonClicked += GoToForm_GoToButtonClicked;
                    goToForm.MaximizeBox = false;
                    goToForm.MinimizeBox = false;
                    goToForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("The 'Go To' feature is available only when 'Word Wrap' is unchecked.", "Notepad - Goto Line", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        private void GoToForm_GoToButtonClicked(object sender, EventArgs e)
        {
            GoToForm goToForm = (GoToForm)sender;

            int lineNumber;

            if (int.TryParse(goToForm.LineNumberText, out lineNumber))
            {
                if (lineNumber >= 1 && lineNumber <= textBox1.Lines.Length)
                {
                    int charIndex = textBox1.GetFirstCharIndexFromLine(lineNumber - 1);
                    textBox1.SelectionStart = charIndex;
                    textBox1.ScrollToCaret();
                    goToForm.Close();
                }
                else
                {
                    MessageBox.Show("The line number is beyond the total number of lines", "Notepad - Goto Line", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid line number", "Notepad - Goto Line", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.SelectAll();
        }

        private void timeDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string currentDateAndTime = DateTime.Now.ToString();

            textBox1.AppendText(currentDateAndTime);
        }

        private void formatToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.WordWrap = !textBox1.WordWrap;

            wordWrapToolStripMenuItem.Checked = textBox1.WordWrap;
        }
        
        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Font = fontDialog.Font;
            }
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Font = new Font(textBox1.Font.FontFamily, textBox1.Font.Size + 2);
        }

        private void restoreDefaultZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Font = defaultFont;
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void viewHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string url = "https://www.bing.com/search?q=get+help+with+notepad+in+windows&filters=guid:\"4466414-en-dia\"%20lang:\"en\"&form=T00032&ocid=HelpPane-BingIA";

            Process.Start(url);
        }

        private void aboutNotepadToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                string currentText = textBox1.Text;
                undoStack.Push(currentText);
                string nextText = redoStack.Pop();
                textBox1.Text = nextText;
                textBox1.SelectionStart = textBox1.Text.Length;
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
