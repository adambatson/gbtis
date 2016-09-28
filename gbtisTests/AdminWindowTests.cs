using Microsoft.VisualStudio.TestTools.UnitTesting;
using gbtis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Diagnostics;
using System.Threading;

namespace gbtis.Tests {
    [TestClass()]
    public class AdminWindowTests {        
        [TestMethod()]
        public void ExitBtnTest() {
            // Start the program
            Process p = Process.Start("gbtis.exe");
            do {
                Thread.Sleep(100);
            } while (p == null);

            // Get window element
            AutomationElement root = AutomationElement.RootElement.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, "adminWindow")
            );
            AutomationElement btn = AutomationElement.RootElement.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, "fileExitButton")
            );

            if (btn == null) Assert.Fail();
            InvokePattern ipFileExit = (InvokePattern)btn.GetCurrentPattern(InvokePattern.Pattern);
            ipFileExit.Invoke();
        }
    }
}