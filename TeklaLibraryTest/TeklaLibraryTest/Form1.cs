using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using static TeklaLibrary;

namespace TeklaLibraryTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void TestBtn_Click(object sender, EventArgs e)
        {
            ResetWorkPlane();

            var beam = PickOneObject<Beam>("Pick beam...");

            var coordSys = beam.GetCoordinateSystem();


            var vector = coordSys.AxisX;


        }
    }
}
