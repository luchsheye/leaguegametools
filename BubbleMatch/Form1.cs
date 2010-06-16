using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BubbleMatch
{
    public partial class Form1 : Form
    {
        Arithmetic ms = new Arithmetic();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "") return;
            textBox2.Text = ms.createNewForm(Convert.ToInt32(textBox1.Text), 1);
        }
    }
}
