using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using System.Xml;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {
        private Dictionary<string, Cell> cells;
        private DependencyGraph dg;


        public Spreadsheet() : base(((string s) => true), ((string f) => f), "default")
        {
            cells = new Dictionary<string, Cell>();
            dg = new DependencyGraph();
        }


        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            cells = new Dictionary<string, Cell>();
            dg = new DependencyGraph();
        }


        public Spreadsheet(String filename, Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            cells = new Dictionary<string, Cell>();
            dg = new DependencyGraph();
            this.GetSavedVersion(filename);
        }

        // ADDED FOR PS5
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed { get; protected set; }


        // ADDED FOR PS5
        /// <summary>
        /// Returns the version information of the spreadsheetg savd in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override string GetSavedVersion(String filename)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try {
            xmlDoc.Load(filename);
            } catch (Exception e) 
            {
                throw new SpreadsheetReadWriteException("There were problems opening, reading, or closing the specified file.");
            }
            cells = new Dictionary<string, Cell>();
            dg = new DependencyGraph();
            XmlNode spreadsheet = xmlDoc.GetElementsByTagName("spreadsheet")[0];
            foreach (XmlNode cell in spreadsheet.ChildNodes)
            {
                this.SetContentsOfCell(cell.SelectSingleNode("name").InnerText, cell.SelectSingleNode("contents").InnerText);
            }
            this.Changed = false;
            return spreadsheet.Attributes["version"].Value;
        }
        // ADDED FOR PS5
        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>
        /// cell name goes here
        /// </name>
        /// <contents>
        /// cell contents goes here
        /// </contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(String filename)
        {
            //Set up the initial <spreadsheet> node.
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("spreadsheet");
            XmlAttribute versionAttribute = xmlDoc.CreateAttribute("version");
            versionAttribute.Value = this.Version;
            rootNode.Attributes.Append(versionAttribute);
            xmlDoc.AppendChild(rootNode);

            foreach (KeyValuePair<string, Cell> kvp in cells) { //Add each cell (and its information) to the spreadsheet node.
                XmlNode cellNode = xmlDoc.CreateElement("cell");
                XmlNode nameNode = xmlDoc.CreateElement("name");
                XmlNode contentsNode = xmlDoc.CreateElement("contents");
                nameNode.InnerText = kvp.Key;
                contentsNode.InnerText = kvp.Value.getContentsAsString();
                cellNode.AppendChild(nameNode);
                cellNode.AppendChild(contentsNode);
                rootNode.AppendChild(cellNode);
            }
            try
            {
                xmlDoc.Save(filename); //Save the XML 
            }
            catch (Exception e)
            {
                throw new SpreadsheetReadWriteException("There were problems opening, reading, or closing the specified file.");
            }

            this.Changed = false;
        }


        // ADDED FOR PS5
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(String name)
        {
            if (!Cell.nameIsValid(ref name, this.IsValid, this.Normalize))
                throw new InvalidNameException();
            if (!cells.ContainsKey(name))
                return String.Empty;
            return cells[name].getValue();  //What about lookup delegates?
        }


        // ADDED FOR PS5
        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<String> SetContentsOfCell(String name, String content)
        {
            if (content == null)
                throw new ArgumentNullException();
            if (!Cell.nameIsValid(ref name, this.IsValid, this.Normalize))
                throw new InvalidNameException();
            double output;
            if (Double.TryParse(content, out output))   //Note to self - while I don't actually need to parse the double to insert it, I do need to parse it to make sure it is inserted as a double.
                return this.SetCellContents(name, output);
            if (content.StartsWith("="))  //The specification only requires that the content begins with this character. It doesn't mention anything about content that begins with a space.
            {
                Formula f = new Formula(content.Substring(1),this.Normalize); //Will throw exception if the formula can't be parsed correctly.
                return this.SetCellContents(name, f); //Will throw a circular dependency if there is one
            }
            return this.SetCellContents(name, content);

        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet. 
        /// </summary>
        public override IEnumerable<String> GetNamesOfAllNonemptyCells()
        {
            foreach (KeyValuePair<string,Cell> kvp in cells) //All cells in the dictionary should be non-empty.
                yield return kvp.Key;
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object GetCellContents(String name)
        {
            if (!Cell.nameIsValid(ref name, this.IsValid, this.Normalize))
                throw new InvalidNameException();
            if (cells.ContainsKey(name))
                return cells[name].getContents();
            return String.Empty;
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, double number)
        {
            return this.SetCellContentsAsObject(name, number);
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, String text)
        {
            if (text == null)
                throw new ArgumentNullException("String contents were null.");
            return this.SetCellContentsAsObject(name, text);
        }

        //Remember this one is special in that it doesn't utilize the SetCellContents(string, object) method.
        /// <summary>
        /// If formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, Formula formula)
        {
            if (formula == null)
                throw new ArgumentNullException("Formula was null.");
            if (!Cell.nameIsValid(name, this.IsValid))
                throw new InvalidNameException();


            if (formula.GetVariables().Contains(name) || hasCircularDependencies(formula, name)) //The first check is redundant, but conserves time.
                throw new CircularException();

            if (cells.ContainsKey(name))
                cells[name].setContents(formula);
            else
                cells.Add(name, new Cell(this, name, formula));

            dg.ReplaceDependees(name, formula.GetVariables()); //Replace any existing dependees with new ones, since the formula has changed.

            ISet<String> dependents = new HashSet<string>();
            getAllDependents(name, ref dependents);

            this.Changed = true;
            return dependents;
        }



        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override IEnumerable<String> GetDirectDependents(String name)
        {
            if (name == null)
                throw new ArgumentNullException();
            if (!Cell.nameIsValid(name, this.IsValid))
                throw new InvalidNameException();
            return dg.GetDependents(name);
        }


        /// <summary>
        /// This will extract the variables used in a given formula and check our spreadsheet to see if any of 
        /// these variables currently depend on a cell (original) within our spreadsheet.
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="original"></param>
        /// <returns></returns>
        private bool hasCircularDependencies(Formula formula, string original)
        {
            foreach (string s in formula.GetVariables())
                if (hasCircularDependencies(s, original))
                    return true;
            return false;
        }


        /// <summary>
        /// This will check a particular cell to see if there are any EXISTING circular dependencies that this cell
        /// is a part of. Seeing how circular dependencies are checked before allowing a formula cell to be entered
        /// in, calling this method with the cellName and original being equal should result in false every time.
        /// </summary>
        /// <param name="cellName"></param>
        /// <param name="original"></param>
        /// <returns></returns>
        private bool hasCircularDependencies(string cellName, string original)
        {
            foreach (string s in dg.GetDependees(cellName))
            {
                if (s.Equals(original) || hasCircularDependencies(s, original)) //If circular dependency found, return true;
                    return true;
            }
            return false;

        }


        /// <summary>
        /// Gets all the direct and indirect dependents of a cell and puts them in a provided set.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="output"></param>
        private void getAllDependents(String name, ref ISet<String> output)
        {
            foreach (string s in GetDirectDependents(name))
            {
                output.Add(s);
                getAllDependents(s, ref output);
            }
        }


        /// <summary>
        /// Sets the contents of any cell, given an object and the cell's name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        private ISet<String> SetCellContentsAsObject(String name, object contents)
        {
            if (!Cell.nameIsValid(name, this.IsValid))
                throw new InvalidNameException();
            if (cells.ContainsKey(name))
                cells[name].setContents(contents);
            else
                cells.Add(name, new Cell(this, name, contents));

            ISet<String> dependents = new HashSet<string>();
            getAllDependents(name, ref dependents);

            this.Changed = true;

            return dependents;
        }

    }

    /// <summary>
    /// Representation of a spreadsheet cell.
    /// </summary>
    public class Cell
    {
        private object contents;
        private Spreadsheet owner;

        /// <summary>
        /// Name of the cell.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Create a new cell with specified name.
        /// 
        /// 9/30 - Removed a check on the name, deemed redundant and not in the scope of the purpose of Cell.
        /// </summary>
        /// <param name="name"></param>
        public Cell(Spreadsheet owner, String name)
        {
            this.Name = name;
            this.owner = owner;
        }

        /// <summary>
        /// Creates a cell with specified contents.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        public Cell(Spreadsheet owner, String name, object contents)
            : this(owner, name)
        {
            this.setContents(contents);
        }

        /// <summary>
        /// Validates a spreadsheet cell name.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <returns></returns>
        public static bool nameIsValid(ref String s, Func<string, bool> isValid, Func<string, string> normalize)
        {
            if ((s != null) && (Regex.IsMatch(s, @"^[a-zA-Z]+[1-9]+\d*$")) && isValid(s))
            {
                s = normalize(s);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validates a spreadsheet cell name.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <returns></returns>
        public static bool nameIsValid(String s, Func<string, bool> isValid)
        {
            return ((s != null) && (Regex.IsMatch(s, @"^[a-zA-Z]+[1-9]+\d*$")) && isValid(s));
        }

        /// <summary>
        /// Sets the contents of the cell. Contents must be a string, double, or Formula, or nothing will be set.
        /// </summary>
        /// <param name="o">Contents of the cell.</param>
        public void setContents(object o)
        {
            if (o.GetType() == typeof(string) || o.GetType() == typeof(double) || o.GetType() == typeof(Formula))
                contents = o;
        }

        private double spreadsheetLookup(string variable)
        {
            //This is a recursive function in disguise. The components of the recursion, in order are:
            //-This method
            //-Spreadsheet.GetCellValue
            //-Cell.getValue
            //-Formula.Evaluate (possibly)
            //The recursion will attempt to obtain a cell value for the cell "variable", which may be a string, double or Formula. In the
            //first case, an exception will be thrown, as seen below. The second case is our base case, where the cell's content is a double.
            //The third case is our recursive call, where the formula may need to lookup additional variables/cells in order to obtain a double.
            object variableObjectRepresentation = this.owner.GetCellValue(variable);
            if (variableObjectRepresentation.GetType() != typeof(double))
                throw new Exception("Formula contained a reference to a cell that did not contain a number.");   ///// This is a temporary fix!
            return (double) variableObjectRepresentation;
        }
        /// <summary>
        /// Retrieves the value of the cell contents.
        /// </summary>
        /// <returns></returns>
        public object getValue()
        {
            if (contents.GetType() == typeof(Formula))
                return ((Formula)contents).Evaluate(spreadsheetLookup);      //////// This doesn't make sense - we need to use the lookup delegate somewhere, don't we?!??
            return contents;
        }

        /// <summary>
        /// Returns the contents of the cell.
        /// </summary>
        /// <returns></returns>
        public object getContents()
        {
            return contents;
        }

        /// <summary>
        /// Returns the string representation of the cell contents.
        /// </summary>
        /// <returns></returns>
        public String getContentsAsString()
        {
            if (contents.GetType() == typeof(Formula))
                return "=" + contents.ToString();
            return contents.ToString();
        }

    }
}
