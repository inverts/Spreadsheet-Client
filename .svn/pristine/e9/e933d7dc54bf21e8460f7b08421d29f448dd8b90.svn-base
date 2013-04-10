using SS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SpreadsheetUtilities;

namespace SpreadsheetTester
{
    
    
    /// <summary>
    ///This is a test class for SpreadsheetTest and is intended
    ///to contain all SpreadsheetTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SpreadsheetTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
        public void SaveSpreadsheetTest()
        {
            Spreadsheet target = new Spreadsheet();
            target.SetContentsOfCell("aa1", "1.0");
            target.SetContentsOfCell("A1", "dogs");
            target.SetContentsOfCell("B1", "=1");
            target.SetContentsOfCell("c1", "=1+B1");
            target.SetContentsOfCell("d1", "=1-8");
            target.Save("testSpreadsheet.xml");
            Spreadsheet target2 = new Spreadsheet(s=>true,((string r) => r.ToUpper()),"Version 2");
            target2.SetContentsOfCell("aa1", "1.0");
            target2.SetContentsOfCell("A1", "dogs");
            target2.SetContentsOfCell("B1", "=1");
            target2.SetContentsOfCell("c1", "=1+B1");
            target2.SetContentsOfCell("d1", "=1-8");
            target2.Save("testSpreadsheet2.xml");
            try
            {
                target2.Save(@"invalidfilename\/$*&@^!..");
                Assert.Fail();
            }
            catch (Exception e) { }

        }
        [TestMethod()]
        public void LoadSpreadsheetTest()
        {
            Spreadsheet target = new Spreadsheet();
            target.SetContentsOfCell("aa1", "1.20");
            target.SetContentsOfCell("A1", "dogs");
            target.SetContentsOfCell("B1", "=1");
            target.SetContentsOfCell("c1", "=1+B1");
            target.SetContentsOfCell("d1", "=1-8");
            target.Save("testLoadSpreadsheet.xml");

            Spreadsheet target2 = new Spreadsheet();
            Assert.AreEqual("default", target2.GetSavedVersion("testLoadSpreadsheet.xml"));
            target2.Save("testLoadSpreadsheet2.xml");

            Spreadsheet target3 = new Spreadsheet("testLoadSpreadsheet.xml", x=>true, ((string s) => s.ToUpper() + "99"), "v2");
            Assert.AreEqual("default", target3.GetSavedVersion("testLoadSpreadsheet.xml"));
            target3.SetContentsOfCell("new1", "this is the first new cell");
            target3.SetContentsOfCell("new2", "here is the second new cell");
            target3.Save("testLoadSpreadsheet3.xml");

            try
            {
                target2.GetSavedVersion("invalidfile.xml");
                Assert.Fail(); //Fails if no exception is thrown.
            }
            catch (SpreadsheetReadWriteException e)
            {
            }

        }


        [TestMethod()]
        public void GetCellValueTest()
        {
            Spreadsheet target = new Spreadsheet();
            target.SetContentsOfCell("c1", "=12*8+4");
            Assert.AreEqual(100.0, target.GetCellValue("c1"));
            target.SetContentsOfCell("c1", "dogs");
            Assert.AreEqual("dogs", target.GetCellValue("c1"));
            target.SetContentsOfCell("c1", "12*8+4");
            Assert.AreEqual("12*8+4", target.GetCellValue("c1"));
            target.SetContentsOfCell("c1", "3.141592653");
            Assert.AreEqual(3.141592653, target.GetCellValue("c1"));
            Assert.AreEqual(String.Empty, target.GetCellValue("aaaaa99999"));
            try
            {
                target.GetCellValue("invalid name");
                Assert.Fail();
            } catch (InvalidNameException e) { }
        
        }



        /// <summary>
        ///A test for Spreadsheet Constructor
        ///</summary>
        [TestMethod()]
        public void SpreadsheetConstructorTest()
        {
            Spreadsheet target = new Spreadsheet();
            try
            {
                target = new Spreadsheet("invalid file name", null, null, null);
                Assert.Fail();
            }
            catch (SpreadsheetReadWriteException e) { }
        }

        /// <summary>
        ///A test for GetCellContents
        ///</summary>
        [TestMethod()]
        public void GetCellContentsTest()
        {
            Spreadsheet target = new Spreadsheet();
            target.SetContentsOfCell("A1", "1.0");
            Assert.AreEqual(1.0, target.GetCellContents("A1"));
            target.SetContentsOfCell("b1", "dogs");
            Assert.AreEqual("dogs", target.GetCellContents("b1"));
            try {
                target.GetCellContents("b0");
                Assert.Fail();
            } catch (InvalidNameException e) {

            }try {
                target.GetCellContents(null);
                Assert.Fail();
            } catch (InvalidNameException e) {

            }
            Formula f = new Formula("1+2-3");
            target.SetContentsOfCell("b11", "=1+2-3");
            Assert.AreEqual(f, target.GetCellContents("b11"));


            Assert.AreEqual(String.Empty,target.GetCellContents("X12"));
        }

        /// <summary>
        ///A test for GetNamesOfAllNonemptyCells
        ///</summary>
        [TestMethod()]
        public void GetNamesOfAllNonemptyCellsTest()
        {
            Spreadsheet target = new Spreadsheet();
            target.SetContentsOfCell("A1", "1.0");
            target.SetContentsOfCell("b1", "dogs");
            Formula f = new Formula("1+2-3");
            target.SetContentsOfCell("b11", "=1+2-3");
            foreach (string s in target.GetNamesOfAllNonemptyCells())
                if (s != "A1" && s != "b1" && s != "b11")
                    Assert.Fail();
        }

        /// <summary>
        ///A test for SetCellContents
        ///</summary>
        [TestMethod()]
        public void SetCellContentsTest()
        {
            Spreadsheet target = new Spreadsheet();
            target.SetContentsOfCell("A1", "1.0");
            target.SetContentsOfCell("A1", "dogs");
            target.SetContentsOfCell("A1", "=1");

            try { target.SetContentsOfCell("notvalid", "p"); Assert.Fail(); }
            catch (InvalidNameException e) { }
            try { target.SetContentsOfCell(null, "p"); Assert.Fail(); }
            catch (InvalidNameException e) { }
            try { target.SetContentsOfCell("normal1", null); Assert.Fail(); }
            catch (ArgumentNullException e) { }


            target.SetContentsOfCell("B1", "=1");
            target.SetContentsOfCell("c1", "=1+B1");
            target.SetContentsOfCell("d1", "=1-c1");

            target.SetContentsOfCell("d1", "=1-8");
            target.SetContentsOfCell("e1", "=1-d1");

            target.SetContentsOfCell("x2", "=1-y2");
            target.SetContentsOfCell("y2", "=1-z2");
            try
            {
                target.SetContentsOfCell("z2", "=1-x2");
                Assert.Fail();
            }
            catch (CircularException ce) { }


            target.SetContentsOfCell("W2", "=W3");
            try
            {
                target.SetContentsOfCell("W3", "=W2");
                Assert.Fail();
            }
            catch (CircularException ce) { }
            try
            {
                target.SetContentsOfCell("W3", "=W3+1");
                Assert.Fail();
            }
            catch (CircularException ce) { }

        }

        // I deleted the other two auto created methods and decided to group the setcellcontents tests together.
    }
}
