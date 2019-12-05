using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;

namespace SmzdmExcelAddin
{
    public partial class Ribbon1
    {
        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {

        }
        private void LaunchBtn_Click(object sender, RibbonControlEventArgs e)
        {
            var taskpane = TaskpaneManager.GetTaskPane("Alpha", "Smzdm Excel Addin Alpha", () => new SmzdmUserControl());
            taskpane.Visible = true;
        }
    }
}
