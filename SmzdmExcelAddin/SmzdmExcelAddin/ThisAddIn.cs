using Microsoft.Office.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SmzdmExcelAddin
{
    class TaskpaneManager
    {
        static Dictionary<string, CustomTaskPane> _createdPanes = new Dictionary<string, CustomTaskPane>();

        /// <summary>
        /// Gets the taskpane by name (if exists for current excel window then returns existing instance, otherwise uses taskPaneCreatorFunc to create one). 
        /// </summary>
        /// <param name="taskPaneId">Some string to identify the taskpane</param>
        /// <param name="taskPaneTitle">Display title of the taskpane</param>
        /// <param name="taskPaneCreatorFunc">The function that will construct the taskpane if one does not already exist in the current Excel window.</param>
        public static CustomTaskPane GetTaskPane(string taskPaneId, string taskPaneTitle, Func<SmzdmUserControl> taskPaneCreatorFunc)
        {
            string key = string.Format("{0}({1})", taskPaneId, Globals.ThisAddIn.Application.Hwnd);
            if (!_createdPanes.ContainsKey(key))
            {
                var pane = Globals.ThisAddIn.CustomTaskPanes.Add(taskPaneCreatorFunc(), taskPaneTitle);
                _createdPanes[key] = pane;
            }
            return _createdPanes[key];
        }
    }
    public partial class ThisAddIn
    {
        private SmzdmUserControl control;
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            control = new SmzdmUserControl();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
