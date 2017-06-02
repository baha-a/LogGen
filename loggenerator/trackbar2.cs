using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace loggenerator
{
    public partial class trackbar2 : UserControl
    {
        public string LabelName { get { return label2.Text; } set { label2.Text = value; } }
        public int Value { get { return trackBar1.Value; } set { trackBar1.Value = value; trackBar1_Scroll(null, null); } }
        public int MaxValue { get { return trackBar1.Maximum; } set { trackBar1.Maximum = value; } }

        static int index = 0;
        public trackbar2()
        {
            InitializeComponent();

            LabelName = ++index + "";
        }


        private void trackBar1_Scroll(object sender, EventArgs e) { label1.Text = trackBar1.Value + ""; }
    }
}
